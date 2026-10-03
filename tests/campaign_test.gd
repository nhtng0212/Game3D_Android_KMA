extends SceneTree
var main
var failures: Array[String] = []
var captures := false
func check(condition: bool, message: String) -> void:
	if not condition:
		failures.append(message)
		push_error("TEST: " + message)
	else: print("PASS: " + message)

func _initialize() -> void:
	call_deferred("run")

func wait_frames(count: int = 4) -> void:
	for i in count: await physics_frame

func interact(id: String) -> void:
	for point in main.world.interactables:
		if point.id == id:
			point.interact()
			return
	check(false,"interaction exists: "+id)

func dismiss() -> void:
	if not is_instance_valid(main.hud.overlay): return
	for child in main.hud.overlay.get_children():
		if child is Button and child.text.begins_with("TIẾP TỤC"):
			child.pressed.emit()
			return

func capture(id: String) -> void:
	if not captures: return
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	if is_instance_valid(main.world.player):
		main.world.player.yaw = 0
		main.world.player.pitch = -.18
	await wait_frames(4)
	await process_frame
	await RenderingServer.frame_post_draw
	root.get_texture().get_image().save_png("res://builds/"+id+".png")

func run() -> void:
	captures = "--capture" in OS.get_cmdline_user_args()
	main = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	await wait_frames()
	await capture("menu")
	var game = root.get_node("Game")
	main.new_campaign()
	check(is_instance_valid(main.hud.overlay),"prologue modal")
	dismiss()
	await wait_frames()
	check(game.stage == 0 and game.active,"new game starts outside")
	main.hud.toast_time = 0
	main.hud.dialogue_time = 0
	await capture("outside")
	# Real collision / navigation reachability and interaction range checks.
	main.world.player.position = Vector3(0,0,0)
	main.world.player.find_target()
	check(main.world.player.target == null,"cannot interact at long range")
	main.world.player.position = Vector3(3,0,-8.8)
	await wait_frames()
	main.world.player.find_target()
	check(is_instance_valid(main.world.player.target),"nearby visible object can be interacted with")
	interact("door06")
	check(game.stage == 0,"Door 06 gated before terminal")
	interact("keycard")
	check(game.keycard,"keycard inventory")
	interact("computer")
	dismiss()
	interact("door06")
	dismiss()
	await wait_frames()
	check(game.stage == 1,"visitors chapter")
	interact("door06")
	check(game.stage == 1,"radio gate before escape")
	interact("radio")
	check(main.world.enemies[0].state == 1,"radio drives enemy investigation")
	interact("door06")
	dismiss()
	await wait_frames()
	check(game.stage == 2,"biometric descent")
	interact("pistol")
	check(game.armed,"weapon unlocked")
	game.ammo = 0
	game.reserve = 12
	main.world.player.reload()
	await create_timer(1.7).timeout
	check(game.ammo == 8 and game.reserve == 4,"reload transfers ammo after delay")
	main.world.player.position = Vector3(0,0,-1)
	main.hud.dialogue_time = 0
	main.hud.toast_time = 0
	await capture("bunker")
	interact("order")
	check(game.drive,"ORDER 071 collected")
	interact("recording")
	dismiss()
	await wait_frames()
	check(game.stage == 3 and game.tablet,"recording unlocks tablet and purge")
	main.world.security.toggle()
	check(main.world.security.opened,"tablet opens live camera")
	main.world.security.alarm()
	check(main.world.security.alarm_cooldown>0,"alarm has cooldown")
	main.world.security.toggle_door()
	check(main.world.security.locked and main.world.security.shutter.position.y == 1.5,"security door closes physical collider")
	check(game.flags.get("security_done",false),"security tutorial completes")
	await capture("tablet")
	main.world.security.toggle_door()
	main.world.security.close()
	var enemy = main.world.enemies[0]
	enemy.position = Vector3(0,0,-4)
	enemy.model.rotation.y = 0
	main.world.player.position = Vector3(0,0,-9)
	await wait_frames()
	check(enemy.can_see_player(),"enemy sees player through open doorway")
	main.world.player.position = Vector3(-5,0,-9)
	await wait_frames()
	check(not enemy.can_see_player(),"wall occludes enemy vision")
	# Nav path actually crosses the shared corridor rather than ignoring room walls.
	var map = main.world.get_world_3d().navigation_map
	var path = NavigationServer3D.map_get_path(map,Vector3(-4,0,-3),Vector3(-4,0,-11),true)
	check(path.size()>2,"navigation routes around solid room wall")
	main.world.player.position = Vector3(0,0,1)
	interact("exit")
	await wait_frames()
	check(game.stage == 4,"storage transition")
	main.world.security.toggle_light()
	main.world.security.toggle_door()
	check(main.world.security.dark and game.flags.get("security_done",false),"darkness and door tutorial")
	interact("exit")
	await wait_frames()
	check(game.stage == 5,"server transition")
	interact("upload")
	check(main.world.enemies.size() == 3,"purge spawns bounded team")
	interact("exit")
	check(game.stage == 5,"cannot leave before upload")
	main.world.upload_time = .02
	await wait_frames(6)
	check(game.flags.get("uploaded",false),"timed uplink completes")
	interact("exit")
	await wait_frames()
	check(game.stage == 6,"boss chapter")
	var boss = main.world.boss
	boss.take_damage(100)
	check(boss.phase == 2 and boss.shielded,"Victor phase 2 revokes security")
	boss.take_damage(25)
	check(boss.hp == 200,"boss gate blocks damage before override")
	interact("override")
	check(not boss.shielded and not main.world.security.revoked,"local override restores security")
	boss.take_damage(100)
	check(boss.phase == 3,"Victor phase 3")
	main.world.security.toggle_light()
	check(boss.stun>0,"security EMP stuns boss")
	main.hud.dialogue_time = 0
	main.hud.toast_time = 0
	main.world.player.position = Vector3(0,0,-9)
	await capture("boss")
	boss.take_damage(100)
	check(game.flags.get("boss_dead",false),"boss defeated")
	interact("final")
	check(game.blocked,"final choice modal")
	await capture("choice")
	main.hud.ending("destroy")
	check(not game.active,"destroy ending finishes campaign")
	await capture("ending-a")
	main.hud.ending("accept")
	await capture("ending-b")
	main.continue_campaign()
	await wait_frames()
	check(game.stage == 6 and game.hp>0 and main.world.boss.hp == 300,"checkpoint restores complete boss encounter")
	game.damage(1000)
	check(game.hp == 0 and game.blocked,"death opens game over")
	main.continue_campaign()
	await wait_frames()
	check(game.hp>0 and not game.blocked,"retry clears death state")
	print("CAMPAIGN TEST: ","PASS" if failures.is_empty() else "FAIL", " / ",failures.size()," failures")
	root.get_node("Audio").stop_all()
	main.queue_free()
	await create_timer(.2).timeout
	await process_frame
	quit(0 if failures.is_empty() else 1)
