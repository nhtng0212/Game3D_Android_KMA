class_name GameHUD
extends Control
const MINT := Color("a4e6d2")
const MUTED := Color("90a7b4")
const INK := Color("0d1923")
const ORANGE := Color("e4a070")
const FONT = preload("res://assets/fonts/Regular.ttf")
const BOLD = preload("res://assets/fonts/Bold.ttf")
const MONO = preload("res://assets/fonts/Mono.ttf")
var overlay: Control
var mobile: Control
var tablet_panel: Control
var subtitle := ""
var speaker := ""
var dialogue_time := 0.0
var toast_text := ""
var toast_time := 0.0
var hurt_time := 0.0
var hit_marker := 0.0
var joy_index := -1
var look_index := -1
var touch_buttons: Dictionary = {}
var joy_origin := Vector2(145,560)
var joy_position := Vector2.ZERO
var button_nodes: Dictionary = {}
var main: Node3D
var time := 0.0
var tablet_status: Label
var menu_camera := false

func _ready() -> void:
	set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	mouse_filter = Control.MOUSE_FILTER_IGNORE
	Game.toast.connect(show_toast)
	build_mobile()

func label_at(parent: Control, value: String, pos: Vector2, font_size: int = 18, color: Color = Color.WHITE, font: Font = FONT, width: float = 0) -> Label:
	var label := Label.new()
	label.add_theme_font_override("font",font)
	label.add_theme_font_size_override("font_size",font_size)
	label.add_theme_color_override("font_color",color)
	if width > 0:
		label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
		label.size.x = width
	parent.add_child(label)
	label.position = pos
	label.text = value
	return label

func style(color: Color, border: Color = Color("40555f")) -> StyleBoxFlat:
	var s := StyleBoxFlat.new()
	s.bg_color = color
	s.border_color = border
	s.set_border_width_all(1)
	s.set_corner_radius_all(4)
	s.content_margin_left = 18
	s.content_margin_right = 18
	return s

func button(parent: Control, title: String, rect: Rect2, callback: Callable, primary: bool = false) -> Button:
	var b := Button.new()
	parent.add_child(b)
	b.position = rect.position
	b.size = rect.size
	b.text = title
	b.add_theme_font_override("font",MONO)
	b.add_theme_font_size_override("font_size",17)
	b.add_theme_color_override("font_color",INK if primary else Color("d5e2e7"))
	b.add_theme_stylebox_override("normal",style(MINT if primary else Color("142732")))
	b.add_theme_stylebox_override("hover",style(Color("c1efdf") if primary else Color("26424f"),MINT))
	b.add_theme_stylebox_override("pressed",style(Color("6ab99f") if primary else Color("365c67"),MINT))
	b.pressed.connect(func(): Audio.play("beep",-15); callback.call())
	b.focus_mode = Control.FOCUS_NONE
	return b

func clear_overlay() -> void:
	if is_instance_valid(overlay):
		remove_child(overlay)
		overlay.queue_free()
	overlay = null

func modal() -> Control:
	clear_overlay()
	Game.blocked = true
	reset_touches()
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
	overlay = Control.new()
	add_child(overlay)
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	var bg := ColorRect.new()
	overlay.add_child(bg)
	bg.color = Color(.025,.055,.08,.95)
	bg.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	return overlay

func resume() -> void:
	clear_overlay()
	Game.blocked = false
	if not main.is_touch(): Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func main_menu() -> void:
	Game.active = false
	var p := modal()
	p.get_child(0).color = Color(.02,.045,.06,.65)
	label_at(p,"NORTH POINT / CLASSIFIED",Vector2(70,65),18,MINT,MONO)
	label_at(p,"BLACK\nMARKET",Vector2(64,125),88,Color("eef5ed"),BOLD)
	label_at(p,"THE BUILDING IS YOUR WEAPON.",Vector2(70,353),18,ORANGE,MONO)
	label_at(p,"Một cuộc gọi. Một di sản. Một nơi không có lối ra.",Vector2(70,395),18,Color("becbd0"))
	button(p,"BẮT ĐẦU CHIẾN DỊCH   →",Rect2(70,450,400,54),func(): main.new_campaign(),true)
	var b := button(p,"TIẾP TỤC CHECKPOINT",Rect2(70,518,400,48),func(): main.continue_campaign())
	b.disabled = Game.read_save().is_empty()
	button(p,"CÀI ĐẶT",Rect2(70,579,190,46),func(): settings(false))
	button(p,"ĐIỀU KHIỂN",Rect2(278,579,192,46),func(): help_screen(false))
	label_at(p,"01 — 07   /   SINGLE PLAYER   /   OFFLINE",Vector2(70,666),13,MUTED,MONO)
	label_at(p,"NP\n071",Vector2(size.x-240,420),70,Color("537275"),MONO)
	label_at(p,"KEEPER STATUS\n[ UNKNOWN ]",Vector2(size.x-280,610),15,MINT,MONO)

func story(title: String, body: String, next: Callable) -> void:
	var p := modal()
	var x := maxf(50,(size.x-940)/2)
	label_at(p,"NORTH POINT  /  SECURE ARCHIVE",Vector2(x,65),16,MINT,MONO)
	label_at(p,title,Vector2(x,115),34,Color("eef4ed"),BOLD,940)
	label_at(p,body,Vector2(x,195),22,Color("c2d0d7"),FONT,900)
	button(p,"TIẾP TỤC   →",Rect2(x,size.y-108,320,54),func(): resume(); next.call(),true)

func dialogue(who: String, text: String) -> void:
	speaker = who
	subtitle = text
	dialogue_time = maxf(7,text.length()*.065)

func show_toast(text: String) -> void:
	toast_text = text
	toast_time = 4

func hurt() -> void:
	hurt_time = .45

func pause() -> void:
	if not Game.active: return
	if main.world.security.opened: main.world.security.close()
	var p := modal()
	var x := (size.x-540)/2
	label_at(p,"PAUSED / NORTH POINT",Vector2(x,115),30,MINT,BOLD)
	label_at(p,"Checkpoint được lưu tự động khi chuyển chương.",Vector2(x,175),17,MUTED)
	button(p,"TIẾP TỤC",Rect2(x,230,540,52),resume,true)
	button(p,"ĐIỀU KHIỂN",Rect2(x,297,540,48),func(): help_screen(true))
	button(p,"CÀI ĐẶT",Rect2(x,360,540,48),func(): settings(true))
	button(p,"CHƠI LẠI CHECKPOINT",Rect2(x,423,540,48),func(): main.continue_campaign())
	button(p,"VỀ MENU",Rect2(x,486,540,48),func(): main.return_menu())

func game_over() -> void:
	if main.world.security.opened: main.world.security.close()
	var p := modal()
	var x := (size.x-620)/2
	label_at(p,"SIGNAL LOST",Vector2(x,180),58,ORANGE,BOLD)
	label_at(p,"Alex đã bị phát hiện. Thay đổi cách tiếp cận:\ndùng góc khuất, báo động và cửa an ninh để chia cắt đối thủ.",Vector2(x,275),20,MUTED,FONT,650)
	button(p,"THỬ LẠI TỪ CHECKPOINT",Rect2(x,385,620,58),func(): main.continue_campaign(),true)
	button(p,"MENU",Rect2(x,461,620,48),func(): main.return_menu())

func ending_choice() -> void:
	var p := modal()
	var x := maxf(50,(size.x-960)/2)
	label_at(p,"SUCCESSOR VERIFIED / ALEX CARTER",Vector2(x,85),18,MINT,MONO)
	label_at(p,"THE NEXT KEEPER",Vector2(x,145),48,Color.WHITE,BOLD)
	label_at(p,"Victor đã im lặng. Bằng chứng đã ra khỏi North Point.\nNhưng hệ thống vẫn đang chờ một mệnh lệnh từ bạn.",Vector2(x,235),22,MUTED,FONT,930)
	button(p,"DESTROY / PHÁ HỦY",Rect2(x,365,450,65),func(): ending("destroy"),true)
	button(p,"ACCEPT / TIẾP QUẢN",Rect2(x+480,365,450,65),func(): ending("accept"))
	label_at(p,"Xóa cơ sở. Phơi bày The Market.\nTừ bỏ quyền lực Marcus để lại.",Vector2(x,457),19,MUTED,FONT,440)
	label_at(p,"Trở thành Keeper mới.\nGiữ North Point hoạt động từ bên trong.",Vector2(x+480,457),19,MUTED,FONT,440)

func ending(choice: String) -> void:
	Game.active = false
	var title := "ENDING A / ORDER #072" if choice == "destroy" else "ENDING B / THE KEEPER"
	var text := "Alex phát tán ORDER 071 và kích hoạt thanh tẩy North Point. Bình minh đến khi cửa hàng chỉ còn là tro bụi.\n\nMột điện thoại vô danh sáng lên:\nORDER #072 — TARGET: ALEX CARTER.\n\nSự thật đã được công bố. The Market vẫn còn tồn tại." if choice == "destroy" else "North Point trở lại trực tuyến. Alex Carter được ghi vào hệ thống với tư cách Keeper.\n\nĐiện thoại trên bàn Marcus đổ chuông.\n“Chúng tôi có một contract mới.”\n\nAlex nhìn Door 06. Lần này, anh biết phía sau nó là gì."
	text += "\n\nCHIẾN DỊCH HOÀN TẤT  •  %d phút  •  %d đối thủ bị hạ" % [int(Game.elapsed/60),Game.kills]
	story(title,text,func(): main.return_menu())

func settings(from_pause: bool) -> void:
	var p := modal()
	var x := (size.x-720)/2
	label_at(p,"CÀI ĐẶT",Vector2(x,90),36,MINT,BOLD)
	label_at(p,"Âm lượng",Vector2(x,176),20)
	var volume := HSlider.new()
	p.add_child(volume)
	volume.position = Vector2(x+290,184)
	volume.size = Vector2(400,32)
	volume.min_value = 0
	volume.max_value = 1
	volume.step = .05
	volume.value = Game.settings.volume
	volume.value_changed.connect(func(v): Game.settings.volume=v; Game.save_settings())
	label_at(p,"Độ nhạy camera",Vector2(x,256),20)
	var sensitivity := HSlider.new()
	p.add_child(sensitivity)
	sensitivity.position = Vector2(x+290,264)
	sensitivity.size = Vector2(400,32)
	sensitivity.min_value = .4
	sensitivity.max_value = 2
	sensitivity.step = .1
	sensitivity.value = Game.settings.sensitivity
	sensitivity.value_changed.connect(func(v): Game.settings.sensitivity=v; Game.save_settings())
	var assist := CheckButton.new()
	p.add_child(assist)
	assist.position = Vector2(x,342)
	assist.size = Vector2(690,50)
	assist.text = "Hỗ trợ ngắm và giảm sát thương nhận vào"
	assist.button_pressed = Game.settings.assisted
	assist.toggled.connect(func(v): Game.settings.assisted=v; Game.save_settings())
	var touch := CheckButton.new()
	p.add_child(touch)
	touch.position = Vector2(x,408)
	touch.size = Vector2(690,50)
	touch.text = "Hiện nút cảm ứng khi chơi trên máy tính"
	touch.button_pressed = Game.settings.touch
	touch.toggled.connect(func(v): Game.settings.touch=v; Game.save_settings(); build_mobile())
	button(p,"LƯU & QUAY LẠI",Rect2(x,540,690,54),func(): if from_pause: pause()
	else: main_menu(),true)

func help_screen(from_pause: bool) -> void:
	var p := modal()
	var x := maxf(50,(size.x-940)/2)
	label_at(p,"FIELD MANUAL / 071",Vector2(x,70),32,MINT,BOLD)
	label_at(p,"DI CHUYỂN & SINH TỒN",Vector2(x,140),19,ORANGE,MONO)
	label_at(p,"Android: kéo joystick bên trái; vuốt vùng trống bên phải để xoay camera.\nNút CHẠY và KHOM bật/tắt. Giữ NGẮM / BẮN. Chạm E để tương tác.\nMáy tính: WASD, chuột; Shift chạy, C đi khom, E tương tác, R nạp đạn.",Vector2(x,182),20,Color("d0dde2"),FONT,940)
	label_at(p,"THE BUILDING IS YOUR WEAPON",Vector2(x,307),19,ORANGE,MONO)
	label_at(p,"TAB mở Tablet: chọn camera, kéo địch bằng báo động, đóng cửa B, tắt đèn.\nTablet hoạt động theo thời gian thực: hãy nấp trước khi mở.\nĐịch nghe tiếng chạy và súng; tường chắn tầm nhìn. Đi khom trong bóng tối.\nCửa B có thể mở lại bất kỳ lúc nào. Thùng cam hồi máu và bổ sung đạn.\nBoss khóa Tablet ở phase 2: tìm terminal khôi phục tại khu B.",Vector2(x,350),20,Color("d0dde2"),FONT,960)
	button(p,"ĐÃ HIỂU",Rect2(x,size.y-100,340,50),func(): if from_pause: pause()
	else: main_menu(),true)

func build_mobile() -> void:
	if is_instance_valid(mobile):
		remove_child(mobile)
		mobile.queue_free()
	mobile = Control.new()
	add_child(mobile)
	mobile.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	mobile.mouse_filter = Control.MOUSE_FILTER_IGNORE
	button_nodes.clear()
	var w := get_viewport_rect().size.x
	var h := get_viewport_rect().size.y
	joy_origin = Vector2(145,h-135)
	var actions := {
		"interact":["E / DÙNG",Rect2(w-310,h-186,115,60)],
		"fire":["BẮN",Rect2(w-148,h-202,100,100)],
		"aim":["NGẮM",Rect2(w-148,h-300,100,65)],
		"reload":["NẠP",Rect2(w-282,h-100,90,56)],
		"run":["CHẠY",Rect2(66,h-310,95,55)],
		"crouch":["KHOM",Rect2(178,h-310,95,55)],
		"tablet":["TABLET",Rect2(w-440,h-100,140,56)]}
	for id in actions:
		var b := button(mobile,actions[id][0],actions[id][1],func(): touch_action(id))
		b.add_theme_font_size_override("font_size",15)
		button_nodes[id] = b
		if id in ["fire","aim"]:
			b.button_down.connect(func(): set_hold(id,true))
			b.button_up.connect(func(): set_hold(id,false))
	button_nodes["pause"] = button(mobile,"Ⅱ",Rect2(w-72,25,46,42),pause)
	if OS.has_feature("android") or OS.has_feature("ios"):
		for b in button_nodes.values(): b.mouse_filter = Control.MOUSE_FILTER_IGNORE

func set_hold(id: String, pressed: bool) -> void:
	if not is_instance_valid(main.world): return
	if id == "fire": main.world.player.touch_fire = pressed
	if id == "aim": main.world.player.touch_aim = pressed

func touch_action(id: String) -> void:
	if Game.blocked: return
	var player = main.world.player
	match id:
		"pause": pause()
		"interact": player.interact()
		"reload": player.reload()
		"crouch": player.crouching = not player.crouching
		"run": player.touch_run = not player.touch_run
		"tablet": main.world.security.toggle()

func reset_touches() -> void:
	joy_index = -1
	look_index = -1
	touch_buttons.clear()
	joy_position = Vector2.ZERO
	if is_instance_valid(main.world) and is_instance_valid(main.world.player):
		main.world.player.touch_move = Vector2.ZERO
		main.world.player.touch_fire = false
		main.world.player.touch_aim = false

func _input(event: InputEvent) -> void:
	if Game.blocked or not Game.active or not main.is_touch() or main.world.security.opened: return
	if event is InputEventScreenTouch:
		if event.pressed:
			for id in button_nodes:
				var b: Button = button_nodes[id]
				if b.visible and b.get_global_rect().has_point(event.position):
					touch_buttons[event.index] = id
					if id in ["fire","aim"]: set_hold(id,true)
					else: touch_action(id)
					get_viewport().set_input_as_handled()
					return
			if event.position.distance_to(joy_origin)<110 and joy_index<0:
				joy_index = event.index
				joy_position = (event.position-joy_origin).limit_length(62)
				main.world.player.touch_move = joy_position/62
			elif event.position.x>size.x*.4 and look_index<0:
				var over_button := false
				for b in button_nodes.values():
					if b.get_global_rect().has_point(event.position): over_button = true
				if not over_button: look_index = event.index
		else:
			if touch_buttons.has(event.index):
				set_hold(touch_buttons[event.index],false)
				touch_buttons.erase(event.index)
				get_viewport().set_input_as_handled()
				return
			if event.index == joy_index:
				joy_index = -1
				joy_position = Vector2.ZERO
				main.world.player.touch_move = Vector2.ZERO
			if event.index == look_index: look_index = -1
	elif event is InputEventScreenDrag:
		if event.index == joy_index:
			joy_position = (event.position-joy_origin).limit_length(62)
			main.world.player.touch_move = joy_position/62
		elif event.index == look_index: main.world.player.look(event.relative)

func show_tablet() -> void:
	if is_instance_valid(tablet_panel): tablet_panel.queue_free()
	tablet_panel = Control.new()
	add_child(tablet_panel)
	tablet_panel.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	tablet_panel.mouse_filter = Control.MOUSE_FILTER_IGNORE
	var bg := Panel.new()
	tablet_panel.add_child(bg)
	bg.position = Vector2(size.x-345,0)
	bg.size = Vector2(345,size.y)
	bg.add_theme_stylebox_override("panel",style(Color(.03,.08,.11,.97)))
	label_at(tablet_panel,"KEEPER OS / 4.1",Vector2(size.x-318,34),22,MINT,MONO)
	label_at(tablet_panel,"LIVE • KHÔNG TẠM DỪNG",Vector2(35,115),16,ORANGE,MONO)
	label_at(tablet_panel,"NORTH POINT / −03",Vector2(size.x-318,78),16,MUTED,MONO)
	for i in 3:
		button(tablet_panel,"CAM 0%d  /  KHU %s" % [i+1,["A","B","C"][i]],Rect2(size.x-318,125+i*58,290,46),func(): main.world.security.view_camera(i))
	button(tablet_panel,"BÁO ĐỘNG KHU C",Rect2(size.x-318,325,290,50),func(): main.world.security.alarm())
	button(tablet_panel,"ĐÓNG / MỞ CỬA B",Rect2(size.x-318,387,290,50),func(): main.world.security.toggle_door())
	button(tablet_panel,"ĐÈN / XUNG EMP",Rect2(size.x-318,449,290,50),func(): main.world.security.toggle_light())
	tablet_status = label_at(tablet_panel,"",Vector2(size.x-318,515),14,MINT,MONO,290)
	button(tablet_panel,"ĐÓNG TABLET  [TAB]",Rect2(size.x-318,size.y-92,290,54),func(): main.world.security.close(),true)

func hide_tablet() -> void:
	if is_instance_valid(tablet_panel):
		remove_child(tablet_panel)
		tablet_panel.queue_free()
	tablet_panel = null

func _process(delta: float) -> void:
	time += delta
	dialogue_time = maxf(0,dialogue_time-delta)
	toast_time = maxf(0,toast_time-delta)
	hurt_time = maxf(0,hurt_time-delta)
	hit_marker = maxf(0,hit_marker-delta)
	mobile.visible = Game.active and not Game.blocked and not main.world.security.opened
	for id in button_nodes:
		button_nodes[id].visible = main.is_touch() or id == "pause"
		if id in ["aim","fire","reload"]: button_nodes[id].visible = main.is_touch() or id == "pause" and Game.armed
		if id == "tablet": button_nodes[id].visible = main.is_touch() or id == "pause" and Game.tablet
	if is_instance_valid(tablet_panel):
		var sec = main.world.security
		tablet_status.text = "CAM: 0%d / LIVE\nCỬA B: %s\nÁNH SÁNG: %s\nBÁO ĐỘNG: %s" % [sec.selected+1,"LOCKED" if sec.locked else "OPEN","OFF" if sec.dark else "ON","READY" if sec.alarm_cooldown<=0 else "%ds"%int(ceil(sec.alarm_cooldown))]
	queue_redraw()

func text(value: String, pos: Vector2, font_size: int, color: Color, font: Font = MONO) -> void:
	draw_string(font,pos,value,HORIZONTAL_ALIGNMENT_LEFT,-1,font_size,color)

func _draw() -> void:
	if not Game.active: return
	var w := size.x
	var h := size.y
	draw_rect(Rect2(0,0,w,98),Color(.025,.06,.085,.87))
	text("ALEX CARTER",Vector2(28,30),14,MUTED)
	draw_rect(Rect2(28,43,190,5),Color("354851"))
	draw_rect(Rect2(28,43,190*Game.hp/100,5),MINT if Game.hp>30 else ORANGE)
	text("%03d HP"%int(Game.hp),Vector2(28,74),17,MINT)
	text(Game.CHAPTERS[Game.stage],Vector2(265,30),15,ORANGE)
	text(Game.objective,Vector2(265,64),17,Color("e1eaeb"),FONT)
	var world = main.world
	if is_instance_valid(world.boss) and world.boss.hp>0:
		var bw := 350.0
		draw_rect(Rect2(w/2-bw/2,109,bw,5),Color("354851"))
		draw_rect(Rect2(w/2-bw/2,109,bw*world.boss.hp/300,5),ORANGE)
		text("VICTOR / PHASE %d%s" % [world.boss.phase," / LOCKED" if world.boss.shielded else ""],Vector2(w/2-bw/2,138),14,ORANGE)
	if world.upload_time>0: text("UPLINK  %02d s"%int(ceil(world.upload_time)),Vector2(30,136),20,MINT)
	if not world.security.opened:
		var center := size/2
		var cc := MINT if hit_marker>0 else Color(1,1,1,.6)
		draw_line(center+Vector2(-9,0),center+Vector2(-3,0),cc,1.5)
		draw_line(center+Vector2(3,0),center+Vector2(9,0),cc,1.5)
		draw_line(center+Vector2(0,-9),center+Vector2(0,-3),cc,1.5)
		draw_line(center+Vector2(0,3),center+Vector2(0,9),cc,1.5)
		if hit_marker>0:
			draw_circle(center,15,Color(MINT,.45),false,2)
		if is_instance_valid(world.player.target):
			var title: String = "[ E ]  " + world.player.target.title
			var tw := MONO.get_string_size(title,HORIZONTAL_ALIGNMENT_LEFT,-1,18).x
			draw_rect(Rect2(w/2-tw/2-18,h*.61-27,tw+36,44),Color(.04,.1,.13,.9))
			text(title,Vector2(w/2-tw/2,h*.61),18,MINT)
		if Game.armed:
			var ammo_text := "%02d / %02d" % [Game.ammo,Game.reserve]
			if world.player.reload_time>0: ammo_text = "NẠP ĐẠN..."
			text(ammo_text,Vector2(w-213,h-28),22,MINT)
		else: text("UNARMED",Vector2(w-178,h-28),18,MUTED)
		if main.is_touch():
			draw_circle(joy_origin,78,Color(.06,.14,.18,.4))
			draw_arc(joy_origin,78,0,TAU,64,Color(.7,.9,.85,.45),2,true)
			draw_circle(joy_origin+joy_position,30,Color(.6,.85,.78,.3))
		else:
			text("WASD  DI CHUYỂN    E  DÙNG    C  KHOM    TAB  TABLET    ESC  MENU",Vector2(28,h-28),12,MUTED)
		var danger := 0.0
		for enemy in world.enemies:
			if is_instance_valid(enemy) and enemy.hp>0: danger = maxf(danger,enemy.suspicion)
		text("PHÁT HIỆN" if danger>=1 else ("NGHI NGỜ" if danger>.1 else "ẨN MÌNH"),Vector2(28,126),13,ORANGE if danger>.1 else MINT)
		if Game.objective_id != "" and not Game.blocked:
			for point in world.interactables:
				if point.id == Game.objective_id and point.available():
					var dist: float = world.player.position.distance_to(point.position)
					if dist>3 and not world.player.camera.is_position_behind(point.global_position+Vector3.UP*1.8):
						var p: Vector2 = world.player.camera.unproject_position(point.global_position+Vector3.UP*1.8)
						p.x = clampf(p.x,30,w-60)
						p.y = clampf(p.y,160,h-160)
						draw_arc(p,11,0,TAU,4,MINT,1.5)
						text("%dm"%int(dist),p+Vector2(16,5),13,MINT)
	if dialogue_time>0 and not Game.blocked:
		var lines: PackedStringArray = wrap_text(subtitle,760,18)
		var height := 49+lines.size()*25
		var origin := Vector2(w/2-400,h-height-130)
		draw_rect(Rect2(origin,Vector2(800,height)),Color(.025,.06,.08,.92))
		text(speaker,origin+Vector2(18,25),14,ORANGE)
		for i in lines.size(): text(lines[i],origin+Vector2(18,52+i*25),18,Color("dbe5e6"),FONT)
	if toast_time>0:
		var lines := wrap_text(toast_text,800,17)
		draw_rect(Rect2(w/2-425,151,850,25+lines.size()*24),Color(.07,.14,.18,.94))
		for i in lines.size(): text(lines[i],Vector2(w/2-405,178+i*24),17,MINT,FONT)
	if hurt_time>0:
		draw_rect(Rect2(0,0,w,h),Color(.65,.12,.055,hurt_time*.3))
		draw_rect(Rect2(3,3,w-6,h-6),Color(.9,.22,.1,hurt_time),false,6)

func wrap_text(value: String, width: float, font_size: int) -> PackedStringArray:
	var lines := PackedStringArray()
	var line := ""
	for word in value.split(" "):
		if FONT.get_string_size(line+word,HORIZONTAL_ALIGNMENT_LEFT,-1,font_size).x>width:
			lines.append(line)
			line = ""
		line += word+" "
	lines.append(line)
	return lines
