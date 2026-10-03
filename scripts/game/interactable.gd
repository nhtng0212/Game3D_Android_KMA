class_name InteractionPoint
extends Node3D
var id := ""
var title := ""
var world: Node3D
var consumed := false
var beacon: Node3D
var time := 0.0

func _ready() -> void:
	beacon = Visuals.box(self,Vector3(0,.8,0),Vector3(.16,.16,.16),Color("95dfc8"),false,1.2)
	var text := Visuals.label(self,title,Vector3(0,1.1,0),22,Color("b3e9d8"))
	text.billboard = BaseMaterial3D.BILLBOARD_ENABLED
	text.visibility_range_end = 8

func _process(delta: float) -> void:
	time += delta
	beacon.rotation.y += delta
	beacon.position.y = .8+sin(time*2)*.08
	visible = available()

func available() -> bool:
	return not consumed and world.item_available(id)

func interact() -> void:
	if not available(): return
	Audio.play("beep",-8)
	world.on_interact(id,self)
