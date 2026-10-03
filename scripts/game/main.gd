extends Node3D
var world: NorthPointWorld
var hud: GameHUD
var menu_cam: Camera3D
var menu_time := 0.0

func _ready() -> void:
	Game.main = self
	world = NorthPointWorld.new()
	add_child(world)
	var layer := CanvasLayer.new()
	add_child(layer)
	hud = GameHUD.new()
	hud.main = self
	layer.add_child(hud)
	menu_cam = Camera3D.new()
	add_child(menu_cam)
	menu_cam.position = Vector3(12,7,17)
	menu_cam.look_at(Vector3(0,1,0))
	menu_cam.current = true
	hud.main_menu()
	get_tree().auto_accept_quit = false

func is_touch() -> bool:
	return OS.has_feature("android") or OS.has_feature("ios") or Game.settings.touch

func _process(delta: float) -> void:
	if not Game.active:
		menu_time += delta
		menu_cam.position = Vector3(11+sin(menu_time*.08)*2,6.5,16)
		menu_cam.look_at(Vector3(0,1,-2))

func new_campaign() -> void:
	Game.new_game()
	Audio.play("ring",-4)
	hud.story("02:17 AM / THE LAST CALL", "MARCUS CARTER\n\nAlex... nghe chú nói. Đừng tin bất kỳ ai biết tên North Point. Nếu chú không gọi lại, hãy đến cửa hàng.\n\nVà Alex... đừng mở Door 06.\n\nCuộc gọi kết thúc. Hai ngày sau, chú Marcus qua đời.",func(): load_stage())

func continue_campaign() -> void:
	if Game.restore(): load_stage()
	else: hud.show_toast("Không tìm thấy checkpoint hợp lệ.")

func load_stage() -> void:
	hud.hide_tablet()
	hud.clear_overlay()
	if is_instance_valid(world):
		remove_child(world)
		world.queue_free()
	Game.active = true
	Game.blocked = false
	world = NorthPointWorld.new()
	add_child(world)
	hud.reset_touches()
	hud.dialogue_time = 0
	if not is_touch(): Input.mouse_mode = Input.MOUSE_MODE_CAPTURED
	else: Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	hud.show_toast("CHECKPOINT ĐÃ LƯU  •  " + Game.CHAPTERS[Game.stage])
	if Game.stage == 0: hud.dialogue("21:30 / ALEX CARTER","Cửa hàng vẫn sáng đèn. Tìm thẻ trên bàn trong văn phòng phía sau Shop. Di chuyển WASD hoặc joystick; E để tương tác.")
	if Game.stage == 1: hud.dialogue("23:00 / INTERCOM","Collection seventy-one. Không để nhân chứng rời khỏi North Point.")

func return_menu() -> void:
	Game.active = false
	Game.blocked = true
	world.security.close()
	menu_cam.current = true
	hud.main_menu()

func _unhandled_input(event: InputEvent) -> void:
	if event.is_action_pressed("pause") and Game.active:
		if is_instance_valid(hud.overlay) and Game.hp>0:
			# Story and ending overlays can only be dismissed by their explicit action.
			return
		elif world.security.opened: world.security.close()
		else: hud.pause()
	if event.is_action_pressed("tablet") and Game.active and not Game.blocked: world.security.toggle()

func _notification(what: int) -> void:
	if not is_instance_valid(hud): return
	if what == NOTIFICATION_APPLICATION_PAUSED or what == NOTIFICATION_APPLICATION_FOCUS_OUT:
		if Game.active and not Game.blocked: hud.pause()
	if what == NOTIFICATION_WM_GO_BACK_REQUEST:
		if Game.active and not Game.blocked: hud.pause()
	if what == NOTIFICATION_WM_CLOSE_REQUEST:
		Audio.stop_all()
		await get_tree().create_timer(.15).timeout
		get_tree().quit()
