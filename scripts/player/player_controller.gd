class_name PlayerController
extends CharacterBody3D
var yaw := 0.0
var pitch := -0.18
var model: Node3D
var pivot: Node3D
var arm: SpringArm3D
var camera: Camera3D
var world: Node3D
var touch_move := Vector2.ZERO
var touch_run := false
var touch_aim := false
var touch_fire := false
var crouching := false
var cooldown := 0.0
var reload_time := 0.0
var clock := 0.0
var step_time := 0.0
var aiming := false
var target: Node3D

func _ready() -> void:
	collision_layer = 2
	collision_mask = 1
	var c := CollisionShape3D.new()
	var capsule := CapsuleShape3D.new()
	capsule.radius = .3
	capsule.height = 1.8
	c.shape = capsule
	c.position.y = .9
	add_child(c)
	model = CharacterVisual.new()
	add_child(model)
	model.setup(false)
	pivot = Node3D.new()
	add_child(pivot)
	pivot.position.y = 1.55
	arm = SpringArm3D.new()
	pivot.add_child(arm)
	arm.spring_length = 3.3
	arm.margin = .2
	arm.collision_mask = 1
	camera = Camera3D.new()
	arm.add_child(camera)
	camera.fov = 75
	camera.near = .1
	camera.current = true

func look(delta: Vector2) -> void:
	yaw -= delta.x * .003 * float(Game.settings.sensitivity)
	pitch = clampf(pitch - delta.y*.003*float(Game.settings.sensitivity), -.85,.55)

func _unhandled_input(event: InputEvent) -> void:
	if Game.blocked or not Game.active: return
	if event is InputEventMouseMotion and Input.mouse_mode == Input.MOUSE_MODE_CAPTURED: look(event.relative)
	if event.is_action_pressed("interact"): interact()
	if event.is_action_pressed("reload"): reload()
	if event.is_action_pressed("crouch"): crouching = not crouching

func _physics_process(delta: float) -> void:
	if Game.blocked or not Game.active:
		velocity = Vector3.ZERO
		return
	clock += delta
	cooldown = maxf(0,cooldown-delta)
	if reload_time > 0:
		reload_time -= delta
		if reload_time <= 0:
			var amount := mini(8-Game.ammo,Game.reserve)
			Game.ammo += amount
			Game.reserve -= amount
			Game.changed.emit()
	aiming = Game.armed and ((Input.is_action_pressed("aim") and not Game.main.is_touch()) or touch_aim)
	var movement := Input.get_vector("left","right","forward","back") + touch_move
	movement = movement.limit_length()
	var sprint := (Input.is_action_pressed("run") or touch_run) and not aiming and not crouching
	var speed := 6.5 if sprint else (2.0 if crouching else 4.0)
	if aiming: speed = 2.5
	var direction := Vector3(movement.x,0,movement.y).rotated(Vector3.UP,yaw)
	velocity.x = move_toward(velocity.x,direction.x*speed,delta*22)
	velocity.z = move_toward(velocity.z,direction.z*speed,delta*22)
	velocity.y -= 22*delta
	move_and_slide()
	if direction.length() > .1:
		model.rotation.y = lerp_angle(model.rotation.y,atan2(-direction.x,-direction.z),delta*14)
	if aiming: model.rotation.y = yaw
	pivot.rotation = Vector3(pitch,yaw,0)
	pivot.position.y = lerpf(pivot.position.y,1.3 if crouching else 1.55,delta*10)
	arm.spring_length = lerpf(arm.spring_length,1.7 if aiming else 3.3,delta*10)
	arm.position.x = lerpf(arm.position.x,.55 if aiming else .35,delta*10)
	camera.fov = lerpf(camera.fov,60 if aiming else 75,delta*8)
	model.armed = Game.armed
	model.animate(clock,Vector2(velocity.x,velocity.z).length(),aiming,crouching)
	step_time -= delta
	if movement.length() > .2 and step_time <= 0:
		step_time = .32 if sprint else .5
		Audio.play("step",-20 if crouching else -12)
		if sprint: world.noise(global_position,5)
	if Game.armed and ((Input.is_action_pressed("fire") and not Game.main.is_touch()) or touch_fire): fire()
	find_target()

func find_target() -> void:
	target = null
	var best := 2.5
	for item in world.interactables:
		if not is_instance_valid(item) or not item.available(): continue
		var distance: float = global_position.distance_to(item.global_position)
		if distance > best: continue
		var query := PhysicsRayQueryParameters3D.create(global_position+Vector3.UP*1.4,item.global_position+Vector3.UP*.5,1)
		var hit := get_world_3d().direct_space_state.intersect_ray(query)
		if not hit.is_empty() and hit.position.distance_to(item.global_position+Vector3.UP*.5) > .7: continue
		best = distance
		target = item

func interact() -> void:
	if Game.blocked: return
	find_target()
	if is_instance_valid(target): target.interact()

func reload() -> void:
	if Game.blocked or not Game.armed or reload_time > 0 or Game.ammo == 8: return
	if Game.reserve == 0:
		Game.toast.emit("Hết đạn dự trữ. Tìm thùng tiếp tế màu cam.")
		return
	reload_time = 1.5
	Audio.play("reload",-5)

func fire() -> void:
	if cooldown > 0 or reload_time > 0: return
	if Game.ammo <= 0:
		reload()
		cooldown = .4
		return
	Game.ammo -= 1
	Game.changed.emit()
	cooldown = .4
	Audio.play("shot",-5)
	world.noise(global_position,25)
	var origin := camera.global_position
	var direction := -camera.global_basis.z
	# Soft assist is visibility-constrained and only snaps within a small center cone.
	if Game.settings.assisted:
		var best := .975
		for enemy in world.enemies:
			if not is_instance_valid(enemy) or enemy.hp <= 0: continue
			var to: Vector3 = enemy.global_position+Vector3.UP*1.2-origin
			var dot := direction.dot(to.normalized())
			if dot > best:
				var check := PhysicsRayQueryParameters3D.create(origin,origin+to,5)
				var result := get_world_3d().direct_space_state.intersect_ray(check)
				if not result.is_empty() and result.collider == enemy:
					best = dot
					direction = to.normalized()
	var hit := get_world_3d().direct_space_state.intersect_ray(PhysicsRayQueryParameters3D.create(origin,origin+direction*70,5))
	var endpoint: Vector3 = hit.position if not hit.is_empty() else origin+direction*40
	# Resolve muzzle obstruction too: no shooting around a corner through the camera.
	var muzzle := global_position+Vector3.UP*1.35+Vector3(.3,0,-.4).rotated(Vector3.UP,yaw)
	var close_hit := get_world_3d().direct_space_state.intersect_ray(PhysicsRayQueryParameters3D.create(muzzle,endpoint,5))
	if not close_hit.is_empty():
		hit = close_hit
		endpoint = hit.position
	if not hit.is_empty() and hit.collider.has_method("take_damage"):
		hit.collider.take_damage(25)
		Game.main.hud.hit_marker = .18
	world.tracer(muzzle,endpoint)
	pitch = clampf(pitch+.012,-.85,.55)
