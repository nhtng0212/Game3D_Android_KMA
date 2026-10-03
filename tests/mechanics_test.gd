extends SceneTree
var main
var game
var failures: Array[String] = []
func _initialize() -> void: call_deferred("run")
func check(value: bool, description: String) -> void:
	print("PASS: " if value else "FAIL: ",description)
	if not value: failures.append(description)
func frames(n: int = 4) -> void:
	for i in n: await physics_frame
func walk_to(goal: Vector3) -> bool:
	var world = main.world
	var path = NavigationServer3D.map_get_path(world.get_world_3d().navigation_map,world.player.position,goal,true)
	if path.is_empty():
		print("NO PATH ",world.player.position," to ",goal)
		return false
	world.player.yaw = 0
	for corner in path:
		var count := 0
		while Vector2(world.player.position.x,world.player.position.z).distance_to(Vector2(corner.x,corner.z))>.35 and count<500:
			var diff = corner-world.player.position
			world.player.touch_move = Vector2(diff.x,diff.z).normalized()
			await physics_frame
			count += 1
		if count>=500:
			print("STUCK AT ",world.player.position," trying ",corner)
			world.player.touch_move = Vector2.ZERO
			return false
	world.player.touch_move = Vector2.ZERO
	print("END ",world.player.position," goal ",goal)
	return world.player.position.distance_to(goal)<1
func touch(index: int, pos: Vector2, pressed: bool) -> void:
	var e := InputEventScreenTouch.new()
	e.index = index
	e.position = pos
	e.pressed = pressed
	main.hud._input(e)
func run() -> void:
	main = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	game = root.get_node("Game")
	game.new_game()
	main.load_stage()
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	await frames(8)
	check(await walk_to(Vector3(3,0,-8.8)),"walk from parking through shop into office")
	check(await walk_to(Vector3(-5,0,-9)),"walk between desk and workshop")
	check(await walk_to(Vector3(0,0,-23)),"walk through warehouse to Door 06")
	for stage in range(2,7):
		game.stage = stage
		game.armed = true
		game.tablet = true
		game.hp = 100
		main.load_stage()
		Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
		for enemy in main.world.enemies: enemy.set_physics_process(false)
		await frames(6)
		check(await walk_to(Vector3(3,0,-12)),"traverse room B in chapter "+str(stage))
		check(await walk_to(Vector3(-7,0,-24)),"reach medical supply in chapter "+str(stage))
		check(await walk_to(Vector3(0,0,-31)),"reach chapter exit "+str(stage))
	game.stage = 3
	game.armed = true
	game.ammo = 8
	game.tablet = true
	main.load_stage()
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	await frames(6)
	var target = main.world.enemies[0]
	target.position = Vector3(0,0,-4)
	target.set_physics_process(false)
	main.world.player.position = Vector3(0,0,1)
	main.world.player.yaw = 0
	main.world.player.pitch = -.05
	await frames(8)
	var before = target.hp
	main.world.player.fire()
	check(game.ammo == 7,"firing consumes one round")
	check(target.hp == before-25,"hitscan from camera and muzzle damages visible enemy")
	main.world.player.fire()
	check(game.ammo == 7,"fire rate prevents duplicate shot")
	# Vision obstruction test uses the actual shutter collider.
	target.position = Vector3(0,0,-18)
	target.model.rotation.y = PI
	main.world.player.position = Vector3(0,0,-14)
	main.world.security.toggle_door()
	await frames(4)
	check(not target.can_see_player(),"closed security shutter blocks line of sight")
	main.world.security.toggle_door()
	await frames(4)
	check(target.can_see_player(),"opening security shutter restores line of sight")
	main.world.player.position = Vector3(0,0,-16)
	await frames()
	main.world.security.toggle_door()
	check(not main.world.security.locked,"door anti-crush protects player")
	game.settings.touch = true
	main.hud.build_mobile()
	await frames()
	var origin: Vector2 = main.hud.joy_origin
	touch(0,origin+Vector2(40,0),true)
	var fire_button: Button = main.hud.button_nodes.fire
	touch(1,fire_button.get_global_rect().get_center(),true)
	var aim_button: Button = main.hud.button_nodes.aim
	touch(2,aim_button.get_global_rect().get_center(),true)
	check(main.world.player.touch_move.x>.4 and main.world.player.touch_fire and main.world.player.touch_aim,"three-finger move / fire / aim")
	touch(0,origin,false)
	touch(1,fire_button.position,false)
	touch(2,aim_button.position,false)
	check(main.world.player.touch_move == Vector2.ZERO and not main.world.player.touch_fire and not main.world.player.touch_aim,"touch release clears all held controls")
	main.hud.pause()
	check(game.blocked,"pause freezes gameplay")
	main.hud.resume()
	check(not game.blocked,"resume returns to gameplay")
	main.world.player.position = Vector3(0,0,1)
	main.world.player.pitch = -.18
	main.hud.dialogue_time = 0
	main.hud.toast_time = 0
	if "--capture" in OS.get_cmdline_user_args():
		await frames(4)
		await RenderingServer.frame_post_draw
		root.get_texture().get_image().save_png("res://builds/mobile.png")
	game.settings.touch = false
	print("MECHANICS TEST: ","PASS" if failures.is_empty() else "FAIL"," / ",failures.size()," failures")
	root.get_node("Audio").stop_all()
	main.queue_free()
	await create_timer(.2).timeout
	quit(0 if failures.is_empty() else 1)
