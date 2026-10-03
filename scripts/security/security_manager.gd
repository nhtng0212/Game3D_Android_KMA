class_name SecuritySystem
extends Node
var world: Node3D
var opened := false
var dark := false
var locked := false
var revoked := false
var selected := 0
var alarm_cooldown := 0.0
var emp_cooldown := 0.0
var cams: Array[Camera3D] = []
var shutter: Node3D
var zones := [Vector3(-6,3,-4),Vector3(6,3,-14),Vector3(-6,3,-25)]

func _process(delta: float) -> void:
	if Game.blocked: return
	alarm_cooldown = maxf(0,alarm_cooldown-delta)
	emp_cooldown = maxf(0,emp_cooldown-delta)

func setup() -> void:
	for p in zones:
		var c := Camera3D.new()
		world.add_child(c)
		c.position = p
		c.look_at(Vector3(0,1,p.z-3))
		c.fov = 85
		cams.append(c)
	shutter = Visuals.box(world,Vector3(0,4.6,-16),Vector3(2.4,3,.22),Color("33454a"),true)
	Visuals.label(shutter,"B / SECURITY",Vector3(0,0,.14),23)

func toggle() -> void:
	if opened:
		close()
		return
	if not Game.tablet:
		Game.toast.emit("Security Tablet chưa mở khóa. Tìm bản ghi FOR_ALEX.")
		return
	if revoked:
		Game.toast.emit("ACCESS REVOKED • Khôi phục quyền tại terminal.")
		return
	if Game.blocked: return
	opened = true
	Game.main.hud.reset_touches()
	world.player.touch_move = Vector2.ZERO
	world.player.touch_fire = false
	world.player.set_physics_process(false)
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	view_camera(0)
	Game.main.hud.show_tablet()

func close() -> void:
	if not opened: return
	opened = false
	Game.main.hud.reset_touches()
	world.player.set_physics_process(true)
	world.player.camera.current = true
	Game.main.hud.hide_tablet()
	if not Game.main.is_touch(): Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func view_camera(index: int) -> void:
	selected = index
	cams[index].current = true
	Game.flags["camera_used"] = true
	if Game.stage == 3: world.update_security_objective()

func toggle_light() -> void:
	dark = not dark
	for light in world.control_lights: light.visible = not dark
	world.environment.environment.ambient_light_energy = .15 if dark else .48
	Audio.play("door",-10)
	Game.flags["lights_used"] = true
	if Game.stage == 6 and emp_cooldown <= 0 and not revoked:
		emp_cooldown = 12
		for enemy in world.enemies:
			if is_instance_valid(enemy) and enemy.hp > 0:
				enemy.stun = 5
		Game.toast.emit("Xung EMP: địch bị vô hiệu hóa trong 5 giây.")
	if Game.stage == 4: world.update_security_objective()

func toggle_door() -> void:
	# Never close directly on a character; keeps both the player and AI out of colliders.
	if not locked:
		var occupants: Array = []
		occupants.append_array(world.enemies)
		occupants.append(world.player)
		for actor in occupants:
			if is_instance_valid(actor) and absf(actor.position.z+16)<.7 and absf(actor.position.x)<1.6:
				Game.toast.emit("Cảm biến cửa: có người trong ngưỡng cửa.")
				return
	locked = not locked
	shutter.position.y = 1.5 if locked else 4.6
	Audio.play("door",-4)
	Game.flags["door_used"] = true
	if Game.stage in [3,4]: world.update_security_objective()

func alarm() -> void:
	if alarm_cooldown > 0: return
	alarm_cooldown = 8
	Audio.play("alarm",-5)
	world.noise(Vector3(-5,0,-21),30)
	Game.flags["alarm_used"] = true
	if Game.stage == 3: world.update_security_objective()
