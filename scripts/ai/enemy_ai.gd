class_name EnemyAI
extends CharacterBody3D
enum State { PATROL, INVESTIGATE, CHASE, ATTACK, SEARCH }
var state := State.PATROL
var hp := 100.0
var max_hp := 100.0
var speed := 3.0
var damage := 15.0
var vision_range := 15.0
var vision_angle := 60.0
var boss := false
var phase := 1
var shielded := false
var world: Node3D
var model: Node3D
var nav: NavigationAgent3D
var patrol: Array[Vector3] = []
var patrol_index := 0
var destination := Vector3.ZERO
var last_seen := Vector3.ZERO
var suspicion := 0.0
var memory := 0.0
var shoot_timer := 1.2
var think_timer := 0.0
var anim_time := 0.0
var stun := 0.0
var indicator: Label3D

func _ready() -> void:
	collision_layer = 4
	collision_mask = 1|2|4
	var c := CollisionShape3D.new()
	var capsule := CapsuleShape3D.new()
	capsule.radius = .32
	capsule.height = 1.8
	c.shape = capsule
	c.position.y = .9
	add_child(c)
	model = CharacterVisual.new()
	add_child(model)
	model.setup(true)
	if boss: model.scale = Vector3.ONE*1.08
	nav = NavigationAgent3D.new()
	nav.path_desired_distance = .35
	nav.target_desired_distance = .5
	add_child(nav)
	indicator = Visuals.label(self,"",Vector3(0,2.3,0),32,Color("f39862"))
	indicator.billboard = BaseMaterial3D.BILLBOARD_ENABLED
	if patrol.is_empty(): patrol = [position,position+Vector3(0,0,3)]
	destination = patrol[0]

func can_see_player() -> bool:
	var player = world.player
	var diff: Vector3 = player.global_position-global_position
	var sight := vision_range * (.48 if world.security.dark else 1.0) * (.65 if player.crouching else 1.0)
	if diff.length() > sight: return false
	if diff.length() > 2.0 and -model.global_basis.z.dot(diff.normalized()) < cos(deg_to_rad(vision_angle*.5)): return false
	var eye := global_position+Vector3.UP*1.5
	var ray := PhysicsRayQueryParameters3D.create(eye,player.global_position+Vector3.UP*(.95 if player.crouching else 1.3),3)
	var hit := get_world_3d().direct_space_state.intersect_ray(ray)
	return not hit.is_empty() and hit.collider == player

func hear(pos: Vector3, radius: float) -> void:
	if hp <= 0 or global_position.distance_to(pos) > radius: return
	if state == State.ATTACK or state == State.CHASE: return
	destination = pos
	state = State.INVESTIGATE
	memory = 7

func _physics_process(delta: float) -> void:
	if Game.blocked or not Game.active or hp <= 0: return
	anim_time += delta
	stun = maxf(0,stun-delta)
	if stun > 0:
		indicator.text = "EMP"
		return
	shoot_timer -= delta
	think_timer -= delta
	memory -= delta
	if think_timer <= 0:
		think_timer = .16
		var sees := can_see_player()
		if sees:
			suspicion = minf(1,suspicion+(.32 if world.player.crouching else .5))
			last_seen = world.player.global_position
			memory = 5
		else: suspicion = maxf(0,suspicion-.06)
		if sees and suspicion >= 1:
			destination = last_seen
			state = State.ATTACK if global_position.distance_to(last_seen) < 9 else State.CHASE
		elif state == State.ATTACK or state == State.CHASE:
			state = State.SEARCH
			destination = last_seen
		elif memory <= 0 and (state == State.INVESTIGATE or state == State.SEARCH): state = State.PATROL
		if state == State.PATROL:
			if global_position.distance_to(destination) < .8: patrol_index = (patrol_index+1)%patrol.size()
			destination = patrol[patrol_index]
		nav.target_position = destination
	indicator.text = "!" if state == State.ATTACK or state == State.CHASE else ("?" if suspicion > .1 or state == State.INVESTIGATE else "")
	var direction := Vector3.ZERO
	if state == State.ATTACK:
		var to: Vector3 = world.player.global_position-global_position
		model.rotation.y = lerp_angle(model.rotation.y,atan2(-to.x,-to.z),delta*7)
		if shoot_timer <= 0 and can_see_player():
			shoot_timer = 1.2 if not boss else .85
			Game.damage(damage)
			world.tracer(global_position+Vector3.UP*1.3,world.player.global_position+Vector3.UP*1.1,true)
			Audio.play("shot",-13)
	else:
		if NavigationServer3D.map_get_iteration_id(nav.get_navigation_map()) > 0 and not nav.is_navigation_finished():
			direction = nav.get_next_path_position()-global_position
			direction.y = 0
			direction = direction.normalized()
		if direction.length() > .1: model.rotation.y = lerp_angle(model.rotation.y,atan2(-direction.x,-direction.z),delta*7)
	velocity.x = direction.x*speed*(.55 if state == State.PATROL else 1.0)
	velocity.z = direction.z*speed*(.55 if state == State.PATROL else 1.0)
	velocity.y -= delta*22
	move_and_slide()
	model.animate(anim_time,Vector2(velocity.x,velocity.z).length(),state == State.ATTACK,false)

func take_damage(amount: float) -> void:
	if hp <= 0: return
	if shielded:
		Game.toast.emit("Victor khóa quyền an ninh. Hãy đến terminal khôi phục!")
		return
	hp = maxf(0,hp-amount)
	suspicion = 1
	last_seen = world.player.global_position
	destination = last_seen
	memory = 7
	state = State.CHASE
	if boss and hp <= 200 and phase == 1:
		phase = 2
		shielded = true
		world.security.revoked = true
		world.security.close()
		Game.set_objective("override","Khôi phục quyền Keeper tại terminal màu cam")
		Game.main.hud.dialogue("VICTOR HALE", "Cháu nghĩ mình điều khiển được North Point? Marcus cũng đã nghĩ vậy.")
	if boss and hp <= 100 and phase == 2:
		phase = 3
		damage = 20
		Game.main.hud.dialogue("HỆ THỐNG", "Quyền Keeper khôi phục. Dùng ĐÈN / EMP trên Tablet để vô hiệu hóa Victor.")
	if hp <= 0:
		collision_layer = 0
		collision_mask = 0
		indicator.text = ""
		model.animation.stop()
		model.rotation.z = PI/2
		model.position.y = .3
		set_physics_process(false)
		Game.kills += 1
		if boss: world.boss_defeated()
		else: world.drop_supply(global_position)
