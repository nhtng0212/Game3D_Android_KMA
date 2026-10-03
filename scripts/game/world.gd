class_name NorthPointWorld
extends Node3D
var player: PlayerController
var security: SecuritySystem
var environment: WorldEnvironment
var enemies: Array[EnemyAI] = []
var interactables: Array[InteractionPoint] = []
var control_lights: Array[OmniLight3D] = []
var obstacles: Array[Rect2] = []
var boss: EnemyAI
var upload_time := -1.0
var upload_started := false
var stage := 0
var effects: Array[Node3D] = []
var effect_cursor := 0
var surface_materials: Dictionary = {}
const WALL := Color("35434b")
const STEEL := Color("26333b")
const CYAN := Color("8ce2d0")

func _ready() -> void:
	stage = Game.stage
	setup_environment()
	if stage <= 1: build_shop()
	else: build_bunker()
	dress_environment()
	player = PlayerController.new()
	player.world = self
	add_child(player)
	player.position = Vector3(0,.05,10 if stage <= 1 else 1)
	security = SecuritySystem.new()
	security.world = self
	add_child(security)
	security.setup()
	build_navigation()
	for i in 12:
		var fx := Visuals.box(self,Vector3.ZERO,Vector3(.025,.025,1),Color("f1ce8e"),false,2)
		fx.visible = false
		effects.append(fx)
	setup_story()

func setup_environment() -> void:
	environment = WorldEnvironment.new()
	var env := Environment.new()
	env.background_mode = Environment.BG_COLOR
	env.background_color = Color("111e2b")
	env.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	env.ambient_light_color = Color("a5c1d0")
	env.ambient_light_energy = .48
	env.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	environment.environment = env
	add_child(environment)
	var moon := DirectionalLight3D.new()
	moon.rotation_degrees = Vector3(-55,-30,0)
	moon.light_color = Color("a3bbc8")
	moon.light_energy = .22
	moon.shadow_enabled = true
	moon.directional_shadow_max_distance = 40
	add_child(moon)

func block(pos: Vector3, size: Vector3, color: Color = WALL, nav_block: bool = true) -> Node3D:
	var result := Visuals.box(self,pos,size,color,true)
	if size.x>5 or size.z>5:
		var kind := "floor" if size.y<.5 else "concrete"
		var key := kind+str(color)
		if not surface_materials.has(key):
			var m := StandardMaterial3D.new()
			m.albedo_color = color.lightened(.13)
			m.albedo_texture = load("res://assets/textures/"+kind+".png")
			m.uv1_triplanar = true
			m.uv1_world_triplanar = true
			m.uv1_scale = Vector3.ONE*.3
			m.roughness = .86
			surface_materials[key] = m
		result.get_child(0).material_override = surface_materials[key]
	if nav_block: obstacles.append(Rect2(Vector2(pos.x-size.x*.5-.6,pos.z-size.z*.5-.6),Vector2(size.x+1.2,size.z+1.2)))
	return result

func fixture(pos: Vector3, color: Color = CYAN) -> void:
	Visuals.box(self,pos,Vector3(1.8,.06,.18),color,false,1.5)
	var light := OmniLight3D.new()
	light.position = pos-Vector3(0,.3,0)
	light.light_color = color
	light.light_energy = 2.5
	light.omni_range = 7
	add_child(light)
	control_lights.append(light)

func prop(id: String, pos: Vector3, size: float = 1, yaw: float = 0) -> void:
	var path := "res://assets/models/"+id+".glb"
	if not ResourceLoader.exists(path): return
	var node = load(path).instantiate()
	add_child(node)
	node.position = pos
	node.scale = Vector3.ONE*size
	node.rotation.y = yaw

func crate(pos: Vector3, tall: bool = false) -> void:
	var height := 1.6 if tall else 1.05
	block(pos+Vector3(0,height/2,0),Vector3(1.3,height,1.1),Color("675e4e"))
	for x in [-.5,.5]: Visuals.box(self,pos+Vector3(x,height/2,.56),Vector3(.08,height,.04),Color("242d31"))
	Visuals.label(self,"NP / 071",pos+Vector3(0,height*.65,.58),18,Color("c5bea4"))

func monitor(pos: Vector3, text: String) -> void:
	Visuals.box(self,pos,Vector3(1.05,.65,.12),Color("0b161d"))
	Visuals.box(self,pos+Vector3(0,0,.07),Vector3(.93,.53,.02),Color("19474d"),false,.3)
	Visuals.label(self,text,pos+Vector3(0,0,.095),18,CYAN)

func room_wall(z: float) -> void:
	block(Vector3(-5,1.6,z),Vector3(7.6,3.2,.25))
	block(Vector3(5,1.6,z),Vector3(7.6,3.2,.25))
	Visuals.box(self,Vector3(0,2.9,z),Vector3(2.4,.15,.35),CYAN,false,.6)

func build_shop() -> void:
	block(Vector3(0,-.18,-8),Vector3(24,.35,48),Color("25343d"),false)
	# Parking marks and street edge.
	for x in [-8,-4,4,8]: Visuals.box(self,Vector3(x,.008,11),Vector3(.06,.015,5),Color("92937f"))
	block(Vector3(-10,1.6,-10),Vector3(.3,3.2,30))
	block(Vector3(10,1.6,-10),Vector3(.3,3.2,30))
	block(Vector3(0,1.6,-25),Vector3(20,3.2,.3))
	block(Vector3(-6,1.6,5),Vector3(8,3.2,.3))
	block(Vector3(6,1.6,5),Vector3(8,3.2,.3))
	Visuals.box(self,Vector3(0,3.1,5),Vector3(20,.6,.4),Color("172c36"))
	Visuals.label(self,"NORTH POINT   /   SUPPLY",Vector3(0,3.1,5.23),56,CYAN)
	Visuals.label(self,"ELECTRONICS   •   REPAIR   •   EST. 1998",Vector3(0,2.65,5.23),20)
	fixture(Vector3(0,3,3))
	for z in [0,-8,-18]: fixture(Vector3(0,3.05,z))
	room_wall(-6)
	room_wall(-14)
	Visuals.label(self,"01 / SHOP",Vector3(-4,2.5,-5.8),32)
	Visuals.label(self,"02 / MARCUS CARTER",Vector3(4,2.5,-13.8),30)
	Visuals.label(self,"WAREHOUSE / RESTRICTED",Vector3(-4,2.5,-24.8),26)
	block(Vector3(-4,.5,0),Vector3(3,1,1),Color("3d5055"))
	for x in [-7,7]:
		for z in [0,-3]:
			prop("bookcaseOpen",Vector3(x,0,z),1.8,PI if x<0 else 0)
			block(Vector3(x,.8,z),Vector3(1.5,1.6,.5),STEEL)
	prop("desk",Vector3(4,0,-10),2)
	block(Vector3(4,.43,-10),Vector3(2.2,.85,1),Color("45545a"))
	monitor(Vector3(4,1.2,-10),"M. CARTER\nORDER #071")
	prop("chairDesk",Vector3(4,0,-11.4),1.8)
	prop("plantSmall1",Vector3(-8,0,3),2.5)
	prop("table",Vector3(-5,0,-10),1.8)
	prop("speaker",Vector3(-5,.95,-10),1.8)
	for p in [Vector3(-5,0,-18),Vector3(-5,0,-21),Vector3(5,0,-19),Vector3(6.4,0,-19),Vector3(5,0,-22)]: crate(p)
	for i in 5:
		Visuals.box(self,Vector3(-9.8,1.1,-15-i*1.8),Vector3(.12,2,1.25),STEEL)
		var sign := Visuals.label(self,"0"+str(i+1),Vector3(-9.65,2.3,-15-i*1.8),24)
		sign.rotation.y = PI/2
	block(Vector3(0,1.2,-24.5),Vector3(2.2,2.4,.25),STEEL)
	Visuals.label(self,"06",Vector3(0,1.65,-24.3),110,CYAN)
	Visuals.label(self,"BIOMETRIC ACCESS",Vector3(0,.95,-24.3),18)
	item("keycard","THẺ MARCUS",Vector3(3,0,-9))
	item("computer","TERMINAL",Vector3(4.5,0,-9))
	item("radio","RADIO",Vector3(-5,0,-9))
	item("door06","DOOR 06",Vector3(0,0,-23))
	item("note","GHI CHÚ CỦA MARCUS",Vector3(-4,0,1.3))
	if stage == 1:
		environment.environment.ambient_light_energy = .35
		spawn_enemy(Vector3(-4,0,-17),[Vector3(-4,0,-17),Vector3(0,0,-11)],"scout")
		spawn_enemy(Vector3(4,0,-20),[Vector3(4,0,-20),Vector3(3,0,-15)],"scout")

func build_bunker() -> void:
	block(Vector3(0,-.18,-14),Vector3(18,.35,38),Color("35434a"),false)
	block(Vector3(-9,1.6,-14),Vector3(.3,3.2,38))
	block(Vector3(9,1.6,-14),Vector3(.3,3.2,38))
	block(Vector3(0,1.6,5),Vector3(18,3.2,.3))
	block(Vector3(0,1.6,-33),Vector3(18,3.2,.3))
	for z in [-7,-16,-25]: room_wall(z)
	for z in [1,-10,-20,-29]:
		fixture(Vector3(0,3.1,z))
		for x in [-8.8,8.8]:
			Visuals.box(self,Vector3(x,1.1,z-2),Vector3(.08,.05,7),CYAN,false,.7)
		for x in [-4,4]:
			Visuals.box(self,Vector3(x,.012,z-2),Vector3(.045,.015,6),Color("af9866"))
		for x in [-8,8]: block(Vector3(x,1.6,z),Vector3(.5,3.2,.5),STEEL)
	var titles: Array = ["ARMORY", "ORDER ROOM / 071", "SERVER / FOR_ALEX", "NORTH POINT"]
	if stage == 3: titles = ["SECURITY / A", "OBSERVATION / B", "ALARM / C", "TRANSFER"]
	if stage == 4: titles = ["STORAGE / A", "CONTAINMENT / B", "MEDICAL / C", "SERVER ACCESS"]
	if stage == 5: titles = ["SERVER / A", "EVIDENCE / B", "UPLINK / C", "CONTROL ACCESS"]
	if stage == 6: titles = ["CONTROL ROOM", "KEEPER ACCESS", "VICTOR HALE", "SUCCESSION"]
	for i in 4:
		Visuals.label(self,titles[i],Vector3(-4,2.45,-6.8-i*8.6),29)
		Visuals.label(self,"−03   /   0"+str(i+1),Vector3(5,2.4,-6.8-i*8.6),24,Color("c69c74"))
	for z in [-4,-12,-22,-30]:
		if stage in [2,5]:
			for x in [-7,-5,6]: server_rack(Vector3(x,0,z))
		else:
			crate(Vector3(-5,0,z))
			crate(Vector3(5,0,z+1))
	monitor(Vector3(4,1.35,-30),"KEEPER SYSTEM\nNORTH POINT")
	block(Vector3(4,.5,-30),Vector3(2,1,1),STEEL)
	item("supply_a","TIẾP TẾ",Vector3(6.8,0,-6))
	item("supply_b","TIẾP TẾ",Vector3(-7,0,-24))
	for p in [Vector3(6.8,.3,-6),Vector3(-7,.3,-24)]: Visuals.box(self,p,Vector3(.65,.5,.5),Color("b58154"))
	if stage == 2:
		item("pistol","PISTOL / 8 ROUND",Vector3(3,0,-3))
		Visuals.box(self,Vector3(3,.8,-3),Vector3(2,.15,1),STEEL)
		Visuals.box(self,Vector3(3,1,-3),Vector3(.2,.15,.6),Color("a8b5b5"))
		item("order","LOCKER 071",Vector3(3,0,-13))
		item("recording","FOR_ALEX",Vector3(3,0,-23))
		monitor(Vector3(3,1.5,-23.6),"FOR_ALEX\nENCRYPTED")
	elif stage in [3,4]:
		item("exit","CỬA CHUYỂN KHU",Vector3(0,0,-31))
		spawn_enemy(Vector3(-4,0,-11),[Vector3(-4,0,-11),Vector3(0,0,-14)],"assault")
		spawn_enemy(Vector3(4,0,-22),[Vector3(4,0,-22),Vector3(0,0,-19)],"scout")
		if stage == 4: spawn_enemy(Vector3(-4,0,-28),[Vector3(-4,0,-28),Vector3(0,0,-26)],"heavy")
	elif stage == 5:
		item("upload","UPLINK / ORDER 071",Vector3(3,0,-12))
		monitor(Vector3(3,1.5,-12.6),"ORDER #071\nUPLOAD READY")
		item("exit","CONTROL ROOM",Vector3(0,0,-31))
	else:
		item("override","KHÔI PHỤC KEEPER",Vector3(5,0,-12))
		monitor(Vector3(5,1.5,-12.6),"LOCAL OVERRIDE")
		item("final","SUCCESSOR TERMINAL",Vector3(4,0,-29))
		boss = spawn_enemy(Vector3(0,0,-20),[Vector3(0,0,-20),Vector3(0,0,-11)],"boss")

func server_rack(pos: Vector3) -> void:
	block(pos+Vector3(0,1.1,0),Vector3(1.2,2.2,.8),Color("17272e"))
	for i in 7:
		Visuals.box(self,pos+Vector3(0,.3+i*.26,.415),Vector3(1,.12,.035),Color("3a505a"))
		Visuals.box(self,pos+Vector3(.36,.3+i*.26,.44),Vector3(.08,.035,.02),CYAN,false,1)

func build_navigation() -> void:
	var mesh := NavigationMesh.new()
	var vertices := PackedVector3Array()
	var polygons: Array[PackedInt32Array] = []
	var ids: Dictionary = {}
	for x in range(-19,19):
		for z in range(-65,31):
			var center := Vector2(x*.5+.25,z*.5+.25)
			if center.x < -8.55 or center.x > 8.55 or center.y < (-32.5 if stage>1 else -24.0) or center.y > (4.5 if stage>1 else 14.5): continue
			var blocked := false
			for rect in obstacles:
				if rect.has_point(center):
					blocked = true
					break
			if blocked: continue
			var polygon := PackedInt32Array()
			for corner in [Vector2i(x,z),Vector2i(x,z+1),Vector2i(x+1,z+1),Vector2i(x+1,z)]:
				if not ids.has(corner):
					ids[corner] = vertices.size()
					vertices.append(Vector3(corner.x*.5,0,corner.y*.5))
				polygon.append(ids[corner])
			polygons.append(polygon)
	mesh.vertices = vertices
	for polygon in polygons: mesh.add_polygon(polygon)
	var region := NavigationRegion3D.new()
	region.navigation_mesh = mesh
	add_child(region)
	NavigationServer3D.map_force_update(get_world_3d().navigation_map)

func item(id: String, title: String, pos: Vector3) -> InteractionPoint:
	var point := InteractionPoint.new()
	point.id = id
	point.title = title
	point.world = self
	point.position = pos
	add_child(point)
	interactables.append(point)
	return point

func spawn_enemy(pos: Vector3, patrol: Array[Vector3], kind: String) -> EnemyAI:
	var enemy := EnemyAI.new()
	enemy.position = pos
	enemy.world = self
	enemy.patrol = patrol
	if kind == "scout":
		enemy.hp = 50
		enemy.speed = 4.5
		enemy.vision_range = 20
		enemy.vision_angle = 90
		enemy.damage = 10
	elif kind == "heavy":
		enemy.hp = 150
		enemy.speed = 2
		enemy.damage = 20
	elif kind == "boss":
		enemy.boss = true
		enemy.hp = 300
		enemy.speed = 3.5
	enemy.max_hp = enemy.hp
	add_child(enemy)
	enemies.append(enemy)
	return enemy

func setup_story() -> void:
	match stage:
		0: Game.set_objective("keycard","Vào North Point • Tìm thẻ của Marcus trong văn phòng")
		1: Game.set_objective("radio","Không có súng • Bật radio để dụ lính khỏi Door 06")
		2: Game.set_objective("pistol","Khám phá tầng −03 • Nhận Pistol tại Armory")
		3: Game.set_objective("security","Mở Tablet [TAB] • Camera → Báo động → Cửa B")
		4: Game.set_objective("security","Dùng Tablet tắt đèn và chia cắt địch bằng cửa B")
		5: Game.set_objective("upload","Đến terminal Uplink • Phát tán bằng chứng ORDER 071")
		6: Game.set_objective("victor","Đối mặt Victor • Dùng vật che chắn và Security Tablet")

func item_available(id: String) -> bool:
	match id:
		"keycard": return stage == 0 and not Game.keycard
		"computer": return stage == 0 and Game.keycard and not Game.flags.get("computer",false)
		"radio": return stage == 1
		"pistol": return not Game.armed
		"order": return Game.armed and not Game.drive
		"recording": return Game.drive and not Game.tablet
		"override": return is_instance_valid(boss) and boss.shielded
		"final": return Game.flags.get("boss_dead",false)
		"upload": return not upload_started
	return true

func on_interact(id: String, point: InteractionPoint) -> void:
	match id:
		"note": Game.main.hud.dialogue("GHI CHÚ CỦA MARCUS","Alex, chú đã để chìa khóa trên bàn. Đừng ở lại sau 23 giờ. Và dù chuyện gì xảy ra... đừng mở Door 06.")
		"keycard":
			Game.keycard = true
			point.consumed = true
			Game.set_objective("computer","Đọc terminal trên bàn Marcus • ORDER #071")
			Game.main.hud.dialogue("ALEX CARTER","Thẻ nhân viên của chú... Nhưng tại sao lại có quyền truy cập tầng −03?")
		"computer":
			Game.flags["computer"] = true
			Game.set_objective("door06","Tìm Door 06 ở cuối kho • Kiểm tra lệnh nhận hàng")
			Game.main.hud.story("ORDER #071", "COLLECTION: 23:00\nKEEPER: MARCUS CARTER\nSTATUS: DECEASED\n\nKhông phải một đơn hàng. Đây là hồ sơ bằng chứng. Tên của chú đã nằm trong danh sách bị xóa.",func(): pass)
		"door06":
			if stage == 0:
				if not Game.flags.get("computer",false):
					Game.toast.emit("ACCESS DENIED • Kiểm tra terminal của Marcus trước.")
					return
				Game.main.hud.story("22:58 / VISITORS ARRIVE", "ACCESS DENIED.\n\nMột chiếc SUV dừng trước cửa.\n“Collection seventy-one.”\n\nĐèn tắt. Sóng điện thoại biến mất. Có tiếng giày trong nhà kho. Alex chưa có vũ khí.",func(): Game.advance())
			else:
				if not Game.flags.get("radio",false):
					Game.toast.emit("Bật radio trong Workshop để kéo lính khỏi lối đi.")
					return
				Game.main.hud.story("EMERGENCY SUCCESSION", "KEYCARD REJECTED.\nCARTER DNA: MATCH.\n\nDoor 06 mở ra. Một thang máy dẫn xuống tầng −03. Chú Marcus đã chuẩn bị cho ngày này.",func(): Game.advance())
		"radio":
			noise(point.global_position,12)
			Audio.play("ring",-8)
			Game.flags["radio"] = true
			Game.set_objective("door06","Đi khom [C] • Lẻn đến máy quét Door 06 ở cuối kho")
		"pistol":
			Game.armed = true
			point.consumed = true
			Game.set_objective("order","Tìm Locker 071 • Dùng thẻ Marcus và dấu vân tay")
			Game.main.hud.dialogue("ARMORY","PISTOL: 8 viên / băng. Giữ ngắm, bắn vào mục tiêu. Nạp đạn [R]. Thùng màu cam chứa đạn và túi cứu thương.")
		"order":
			Game.drive = true
			point.consumed = true
			Game.set_objective("recording","Đưa ORDER 071 đến terminal FOR_ALEX trong Server Room")
			Game.main.hud.dialogue("ORDER #071","Contract bị sửa. Các Keeper bị thủ tiêu. Chữ ký cuối cùng: VICTOR HALE.")
		"recording":
			Game.tablet = true
			Game.main.hud.story("FOR_ALEX / MARCUS CARTER", "North Point thuộc The Market. Chúng cung cấp vũ khí, danh tính và contract cho các Operator.\n\nVictor đã biến lệnh bảo vệ thành lệnh ám sát. ORDER 071 là bằng chứng. Chú đã trao quyền Keeper cho cháu.\n\nĐừng cố thắng chúng bằng súng. Hãy dùng chính tòa nhà.\n\n[ NETWORK BREACH • PURGE AUTHORIZED ]",func(): Game.advance())
		"exit":
			if stage in [3,4] and not Game.flags.get("security_done",false):
				Game.toast.emit("Hoàn thành thao tác an ninh trong mục tiêu trước khi chuyển khu.")
				return
			if stage == 5 and not Game.flags.get("uploaded",false):
				Game.toast.emit("Chưa hoàn tất truyền bằng chứng ORDER 071.")
				return
			Game.advance()
		"upload":
			upload_started = true
			upload_time = 35
			Game.set_objective("survive","Uplink đang chạy • Sống sót 35 giây hoặc dùng cửa chia cắt địch")
			spawn_enemy(Vector3(0,0,-28),[Vector3(0,0,-28),Vector3(0,0,-12)],"assault")
			spawn_enemy(Vector3(6,0,-22),[Vector3(6,0,-22),Vector3(0,0,-12)],"assault")
			spawn_enemy(Vector3(-5,0,-4),[Vector3(-5,0,-4),Vector3(0,0,-12)],"scout")
			noise(player.global_position,40)
			Audio.play("alarm",-7)
		"override":
			boss.shielded = false
			security.revoked = false
			boss.stun = 4
			Game.set_objective("victor","Quyền Keeper đã khôi phục • Đánh bại Victor")
			Game.main.hud.dialogue("HỆ THỐNG","LOCAL OVERRIDE ACCEPTED. Camera, đèn, cửa và báo động đã hoạt động trở lại.")
		"final": Game.main.hud.ending_choice()
		_:
			if id.begins_with("supply"):
				if Game.hp >= 100 and Game.reserve >= 32:
					Game.toast.emit("Máu và đạn dự trữ đã đầy.")
					return
				Game.hp = minf(100,Game.hp+45)
				Game.reserve = mini(32,Game.reserve+16)
				point.consumed = true
				Game.toast.emit("+45 HP  /  +16 viên đạn")
	Game.changed.emit()

func update_security_objective() -> void:
	var done := false
	if stage == 3: done = Game.flags.get("camera_used",false) and Game.flags.get("alarm_used",false) and Game.flags.get("door_used",false)
	if stage == 4: done = Game.flags.get("lights_used",false) and Game.flags.get("door_used",false)
	if done:
		Game.flags["security_done"] = true
		Game.set_objective("exit","Đến cửa chuyển khu ở cuối hành lang • Có thể lẻn qua địch")

func _process(delta: float) -> void:
	if Game.blocked or not Game.active: return
	if upload_time > 0:
		upload_time -= delta
		if upload_time <= 0:
			Game.flags["uploaded"] = true
			Game.set_objective("exit","Bằng chứng đã truyền • Đến Control Room đối mặt Victor")
			Game.main.hud.dialogue("UPLINK","ORDER 071: TRANSMITTED. Victor không thể xóa sự thật nữa.")

func noise(pos: Vector3, radius: float) -> void:
	for enemy in enemies:
		if is_instance_valid(enemy): enemy.hear(pos,radius)

func tracer(start: Vector3, end: Vector3, hostile: bool = false) -> void:
	var effect := effects[effect_cursor]
	effect_cursor = (effect_cursor+1)%effects.size()
	effect.position = (start+end)*.5
	if start.distance_to(end) > .01: effect.look_at(end)
	effect.scale = Vector3(1,1,start.distance_to(end))
	effect.visible = true
	var mesh: MeshInstance3D = effect.get_child(0)
	mesh.material_override = Visuals.material(Color("ef8e60") if hostile else Color("e8dcb5"),2)
	get_tree().create_timer(.065).timeout.connect(func(): if is_instance_valid(effect): effect.visible = false)

func drop_supply(pos: Vector3) -> void:
	item("supply_drop","ĐẠN / CỨU THƯƠNG",pos)

func boss_defeated() -> void:
	Game.flags["boss_dead"] = true
	security.revoked = false
	Game.set_objective("final","Victor đã gục • Đến Successor Terminal và quyết định số phận North Point")
	Game.main.hud.dialogue("VICTOR HALE","Cháu có thể phá nơi này... nhưng The Market sẽ tìm một Keeper khác.")


func dress_environment() -> void:
	var width := 20.0 if stage<=1 else 18.0
	var start := 5.0
	var end := -25.0 if stage<=1 else -33.0
	# Closed interior with structural ceiling beams and utility pipes.
	block(Vector3(0,3.55,(start+end)/2),Vector3(width,.2,start-end),Color("202c33"),false)
	for z in range(int(end)+1,4,4):
		Visuals.box(self,Vector3(0,3.32,z),Vector3(width,.2,.16),Color("172831"))
		for x in [-1,1]:
			Visuals.box(self,Vector3(x*(width/2-.19),.28,z),Vector3(.1,.55,3.95),Color("243741"))
			Visuals.box(self,Vector3(x*(width/2-.25),2.8,z),Vector3(.1,.1,4.1),Color("846b54"))
	for z in [-5,-14,-24]:
		for x in [-1.3,1.3]:
			Visuals.box(self,Vector3(x,1.35,z),Vector3(.12,2.7,.25),Color("192f38"))
		# Visible CCTV hardware and emergency indicators.
		var cam := Visuals.box(self,Vector3(2,2.75,z+.4),Vector3(.35,.23,.5),Color("c1c5ba"))
		cam.rotation.y = -.4
		Visuals.box(cam,Vector3(0,0,.26),Vector3(.16,.12,.025),Color("11222b"))
		Visuals.box(self,Vector3(2.3,2.75,z+.42),Vector3(.04,.04,.04),Color("ec8c60"),false,1)
	if stage<=1:
		# Shop windows sit in front of the facade, with display merchandise behind a blue pane.
		for x in [-6,6]:
			Visuals.box(self,Vector3(x,1.6,5.19),Vector3(5.5,1.65,.06),Color("152f3d"))
			for frame_x in [-2.7,0,2.7]: Visuals.box(self,Vector3(x+frame_x,1.6,5.26),Vector3(.07,1.75,.07),Color("617079"))
			for frame_y in [.75,2.45]: Visuals.box(self,Vector3(x,frame_y,5.26),Vector3(5.5,.07,.07),Color("617079"))
			Visuals.label(self,"SERVICE  /  PARTS" if x<0 else "OPEN UNTIL 23:00",Vector3(x,1.6,5.3),24,Color("7b9caa"))
		block(Vector3(0,-.083,6.1),Vector3(22,.15,1.8),Color("687078"),false)
		for x in [-10.4,10.4]:
			Visuals.cylinder(self,Vector3(x,2.5,10),.06,5,Color("273841"))
			fixture(Vector3(x,5,10),Color("edbe8b"))
		# Neighbouring silhouettes make the shop part of a deserted industrial street.
		for i in 6:
			var x := -36.0+i*13
			var height := 7.0+float(i%3)*2
			Visuals.box(self,Vector3(x,height/2,-40),Vector3(10,height,10),Color("1b2b39"))
			for j in 3: Visuals.box(self,Vector3(x-3+j*3,4,-34.9),Vector3(1,.75,.04),Color("43616c"),false,.2)
		# Maintenance details in Marcus's office and workshop.
		prop("coatRack",Vector3(8,0,-12),1.8)
		prop("cabinetBedDrawer",Vector3(7.5,0,-10),2)
		prop("computerKeyboard",Vector3(4,.9,-9.6),1.7)
		for x in [-7,-6,-5]:
			Visuals.box(self,Vector3(x,1.8,-13.7),Vector3(.15,.7,.1),Color("997a59"))
		# Parked collection vehicle; decorative, no driving system.
		var car := Node3D.new()
		add_child(car)
		car.position = Vector3(7,0,11)
		Visuals.box(car,Vector3(0,.8,0),Vector3(2,1,4.3),Color("18252c"))
		Visuals.box(car,Vector3(0,1.5,-.3),Vector3(1.85,.65,2.5),Color("283b46"))
		Visuals.box(car,Vector3(0,1.55,1),Vector3(1.65,.42,.06),Color("617b84"))
		for x in [-1.0,1.0]:
			for z in [-1.35,1.35]:
				var wheel := Visuals.cylinder(car,Vector3(x,.45,z),.43,.22,Color("101b21"))
				wheel.rotation.z = PI/2
		for x in [-.65,.65]: Visuals.box(car,Vector3(x,.85,-2.17),Vector3(.4,.16,.03),Color("d4dabb"),false,.6)
	else:
		for z in [-5,-14,-23,-31]:
			monitor(Vector3(-8.8,1.6,z),"NP / ONLINE")
			for x in [-1,1]:
				Visuals.box(self,Vector3(x*1.5,.013,z),Vector3(.25,.015,1.3),Color("b79252"))
		if stage == 6:
			for x in [-4,-2,0,2]: monitor(Vector3(x,2,-32.7),"KEEPER ACCESS
[ VERIFIED ]")
			Visuals.label(self,"THE MARKET",Vector3(0,2.8,-32.7),46,Color("d9af81"))
