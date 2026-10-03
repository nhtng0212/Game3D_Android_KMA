extends Node
var clips: Dictionary = {}
var silent := "--test-silent" in OS.get_cmdline_user_args()
var voices: Array[AudioStreamPlayer] = []
var cursor := 0
var ambience: AudioStreamPlayer

func _ready() -> void:
	for name in ["shot", "reload", "door", "beep", "alarm", "hit", "step", "ring", "ambient"]:
		clips[name] = load("res://assets/audio/" + name + ".wav")
	for i in 10:
		var voice := AudioStreamPlayer.new()
		add_child(voice)
		voices.append(voice)
	ambience = AudioStreamPlayer.new()
	ambience.stream = clips.ambient
	ambience.volume_db = -18
	add_child(ambience)
	if not silent: ambience.play()
	AudioServer.set_bus_volume_db(0, linear_to_db(float(Game.settings.volume)))

func play(id: String, volume: float = 0.0) -> void:
	if silent or not clips.has(id): return
	var voice := voices[cursor]
	cursor = (cursor + 1) % voices.size()
	voice.stop()
	voice.stream = clips[id]
	voice.volume_db = volume
	voice.pitch_scale = randf_range(0.96, 1.04)
	voice.play()

func stop_all() -> void:
	for voice in voices:
		voice.stop()
		voice.stream = null
	ambience.stop()
	ambience.stream = null
	clips.clear()

func _exit_tree() -> void:
	stop_all()
