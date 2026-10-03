class_name Visuals
extends RefCounted
static var materials: Dictionary = {}

static func material(color: Color, emission: float = 0.0) -> StandardMaterial3D:
	var key := str(color) + str(emission)
	if materials.has(key): return materials[key]
	var m := StandardMaterial3D.new()
	m.albedo_color = color
	m.roughness = 0.8
	if emission > 0:
		m.emission_enabled = true
		m.emission = color
		m.emission_energy_multiplier = emission
	materials[key] = m
	return m

static func box(parent: Node3D, pos: Vector3, size: Vector3, color: Color, solid: bool = false, emission: float = 0) -> Node3D:
	var root: Node3D = StaticBody3D.new() if solid else Node3D.new()
	parent.add_child(root)
	root.position = pos
	var mesh := MeshInstance3D.new()
	var shape := BoxMesh.new()
	shape.size = size
	mesh.mesh = shape
	mesh.material_override = material(color, emission)
	root.add_child(mesh)
	if solid:
		var collision := CollisionShape3D.new()
		var geometry := BoxShape3D.new()
		geometry.size = size
		collision.shape = geometry
		root.add_child(collision)
	return root

static func cylinder(parent: Node3D, pos: Vector3, radius: float, height: float, color: Color) -> MeshInstance3D:
	var mesh := MeshInstance3D.new()
	var geo := CylinderMesh.new()
	geo.top_radius = radius
	geo.bottom_radius = radius
	geo.height = height
	geo.radial_segments = 10
	mesh.mesh = geo
	mesh.material_override = material(color)
	parent.add_child(mesh)
	mesh.position = pos
	return mesh

static func label(parent: Node3D, text: String, pos: Vector3, size: int = 44, color: Color = Color("b5d2d4")) -> Label3D:
	var l := Label3D.new()
	l.text = text
	l.font = preload("res://assets/fonts/Mono.ttf")
	l.font_size = size
	l.pixel_size = 0.007
	l.modulate = color
	l.outline_size = 0
	parent.add_child(l)
	l.position = pos
	return l

static func person(parent: Node3D, color: Color, enemy: bool = false) -> Node3D:
	var body := Node3D.new()
	parent.add_child(body)
	box(body, Vector3(0,1.22,0), Vector3(.55,.64,.32), color)
	box(body, Vector3(0,1.22,-.18), Vector3(.43,.44,.08), Color("15262c"))
	box(body, Vector3(0,1.66,0), Vector3(.3,.33,.28), Color("b59179") if not enemy else Color("263238"))
	box(body, Vector3(0,1.83,.015), Vector3(.33,.1,.3), Color("1b2328"))
	if enemy:
		box(body,Vector3(0,1.68,-.15),Vector3(.27,.07,.03),Color("ec784f"),false,1.2)
	else:
		box(body,Vector3(0,1.25,.23),Vector3(.4,.48,.22),Color("293b42"))
		box(body,Vector3(0,1.37,.35),Vector3(.27,.025,.01),Color("a4c6bc"))
	for side in [-1,1]:
		var leg := Node3D.new()
		leg.name = "LegL" if side == -1 else "LegR"
		body.add_child(leg)
		leg.position = Vector3(side*.16,.94,0)
		box(leg, Vector3(0,-.4,0), Vector3(.23,.78,.26), Color("202b32"))
		box(leg, Vector3(0,-.87,-.065), Vector3(.25,.15,.38), Color("111920"))
		var arm := Node3D.new()
		arm.name = "ArmL" if side == -1 else "ArmR"
		body.add_child(arm)
		arm.position = Vector3(side*.36,1.47,0)
		box(arm,Vector3(0,-.23,0),Vector3(.18,.48,.22),color)
		box(arm,Vector3(0,-.5,0),Vector3(.16,.12,.19),Color("b59179") if not enemy else Color("151e25"))
	var gun := box(body.get_node("ArmR"), Vector3(0,-.48,-.2),Vector3(.1,.14,.4),Color("10191f"))
	gun.name = "Gun"
	return body

static func animate_person(body: Node3D, time: float, speed: float, aiming: bool, crouch: bool) -> void:
	var stride := minf(speed / 4, 1)
	body.get_node("LegL").rotation.x = sin(time * 9) * .65 * stride
	body.get_node("LegR").rotation.x = -sin(time * 9) * .65 * stride
	body.get_node("ArmL").rotation.x = -1.05 if aiming else -sin(time * 9)*.4*stride
	body.get_node("ArmR").rotation.x = -1.4 if aiming else sin(time * 9)*.4*stride
	body.position.y = (-.35 if crouch else 0.0) + absf(sin(time * 9)) * .035 * stride
	body.rotation.x = .15 if crouch else 0.0
