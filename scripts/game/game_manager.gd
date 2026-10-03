extends Node
## Campaign authority. Saves only safe chapter-entry snapshots, never a half-finished encounter.
signal changed
signal toast(text: String)
const SAVE_PATH := "user://north_point_v1.json"
const SETTINGS_PATH := "user://settings.cfg"
const CHAPTERS := ["01 / NORTH POINT SUPPLY", "02 / VISITORS ARRIVE", "03 / EMERGENCY SUCCESSION", "04 / EYES IN THE DARK", "05 / DIVIDE & SURVIVE", "06 / THE EVIDENCE", "07 / VICTOR HALE"]
var stage := 0
var hp := 100.0
var ammo := 8
var reserve := 32
var armed := false
var keycard := false
var drive := false
var tablet := false
var flags: Dictionary = {}
var objective := ""
var objective_id := ""
var active := false
var blocked := true
var main: Node
var kills := 0
var elapsed := 0.0
var settings := {"volume": 0.65, "sensitivity": 1.0, "assisted": true, "touch": false}
var checkpoint: Dictionary = {}

func _ready() -> void:
	var cfg := ConfigFile.new()
	if cfg.load(SETTINGS_PATH) == OK:
		for k in settings: settings[k] = cfg.get_value("settings", k, settings[k])
	for action in ["forward", "back", "left", "right", "run", "crouch", "interact", "reload", "tablet", "pause", "fire", "aim"]:
		InputMap.add_action(action)
	var keys := {"forward": KEY_W, "back": KEY_S, "left": KEY_A, "right": KEY_D, "run": KEY_SHIFT, "crouch": KEY_C, "interact": KEY_E, "reload": KEY_R, "tablet": KEY_TAB, "pause": KEY_ESCAPE}
	for action in keys:
		var event := InputEventKey.new()
		event.physical_keycode = keys[action]
		InputMap.action_add_event(action, event)
	for action in ["fire", "aim"]:
		var event := InputEventMouseButton.new()
		event.button_index = MOUSE_BUTTON_LEFT if action == "fire" else MOUSE_BUTTON_RIGHT
		InputMap.action_add_event(action, event)

func _process(delta: float) -> void:
	if active and not blocked: elapsed += delta

func new_game() -> void:
	stage = 0
	hp = 100
	ammo = 8
	reserve = 32
	armed = false
	keycard = false
	drive = false
	tablet = false
	flags.clear()
	kills = 0
	elapsed = 0
	save_checkpoint()

func set_objective(id: String, value: String) -> void:
	objective_id = id
	objective = value
	changed.emit()

func snapshot() -> Dictionary:
	return {"version": 1, "stage": stage, "hp": hp, "ammo": ammo, "reserve": reserve, "armed": armed, "keycard": keycard, "drive": drive, "tablet": tablet, "kills": kills, "elapsed": elapsed}

func save_checkpoint() -> void:
	checkpoint = snapshot()
	var file := FileAccess.open(SAVE_PATH + ".tmp", FileAccess.WRITE)
	if file == null:
		toast.emit("Không thể ghi checkpoint trên thiết bị này.")
		return
	file.store_string(JSON.stringify(checkpoint))
	file.close()
	DirAccess.rename_absolute(SAVE_PATH + ".tmp", SAVE_PATH)

func read_save() -> Dictionary:
	if not FileAccess.file_exists(SAVE_PATH): return {}
	var parsed = JSON.parse_string(FileAccess.get_file_as_string(SAVE_PATH))
	if not parsed is Dictionary: return {}
	if parsed.get("version", 0) != 1 or not parsed.has_all(["stage", "hp", "ammo", "reserve", "armed", "keycard", "drive", "tablet", "kills", "elapsed"]): return {}
	if not parsed.stage is float and not parsed.stage is int: return {}
	if int(parsed.stage) < 0 or int(parsed.stage) > 6: return {}
	for k in ["hp", "ammo", "reserve", "elapsed", "kills"]:
		if not parsed[k] is float and not parsed[k] is int: return {}
	for k in ["armed", "keycard", "drive", "tablet"]:
		if not parsed[k] is bool: return {}
	return parsed

func restore() -> bool:
	var data := read_save()
	if data.is_empty(): return false
	stage = int(data.stage)
	hp = clampf(data.hp, 1, 100)
	ammo = clampi(int(data.ammo), 0, 8)
	reserve = clampi(int(data.reserve), 0, 32)
	armed = data.armed
	keycard = data.keycard
	drive = data.drive
	tablet = data.tablet
	kills = int(data.kills)
	elapsed = float(data.elapsed)
	flags.clear()
	checkpoint = data
	return true

func save_settings() -> void:
	var cfg := ConfigFile.new()
	for k in settings: cfg.set_value("settings", k, settings[k])
	cfg.save(SETTINGS_PATH)
	AudioServer.set_bus_volume_db(0, linear_to_db(float(settings.volume)))

func damage(amount: float) -> void:
	if blocked or not active or hp <= 0: return
	hp = maxf(0, hp - amount * (0.65 if settings.assisted else 1.0))
	changed.emit()
	main.hud.hurt()
	Audio.play("hit")
	if hp <= 0:
		blocked = true
		main.hud.game_over()

func advance() -> void:
	stage += 1
	hp = maxf(hp, 70)
	if armed: reserve = maxi(reserve, 24)
	flags.clear()
	save_checkpoint()
	main.load_stage.call_deferred()
