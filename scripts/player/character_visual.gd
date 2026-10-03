class_name CharacterVisual
extends Node3D
var animation: AnimationPlayer
var skeleton: Skeleton3D
var gun: Node3D
var rig: Node3D
var armed := false
var is_enemy := false

func setup(enemy: bool) -> void:
	is_enemy = enemy
	rig = preload("res://assets/characters/character.fbx").instantiate()
	add_child(rig)
	rig.scale = Vector3.ONE*.49
	# The source model faces +Z; actor forward is -Z.
	rig.rotation.y = PI
	skeleton = rig.find_child("Skeleton3D",true,false)
	var mesh: MeshInstance3D = rig.find_child("characterMedium",true,false)
	var skin := StandardMaterial3D.new()
	skin.albedo_texture = preload("res://assets/characters/operator.png") if enemy else preload("res://assets/characters/alex.png")
	skin.roughness = .9
	mesh.material_override = skin
	animation = AnimationPlayer.new()
	rig.add_child(animation)
	var library := AnimationLibrary.new()
	for id in ["idle","run"]:
		var source = load("res://assets/characters/"+id+".fbx").instantiate()
		var source_player: AnimationPlayer = source.find_child("AnimationPlayer",true,false)
		var clip: Animation = source_player.get_animation("Root|Idle" if id=="idle" else "Root|Run").duplicate()
		clip.loop_mode = Animation.LOOP_LINEAR
		library.add_animation(id,clip)
		source.free()
	animation.add_animation_library("",library)
	animation.play("idle")
	gun = Node3D.new()
	add_child(gun)
	gun.position = Vector3(.32,1.12,-.38)
	Visuals.box(gun,Vector3.ZERO,Vector3(.1,.13,.36),Color("15232b"))
	Visuals.box(gun,Vector3(0,-.09,.09),Vector3(.09,.18,.09),Color("26383d"))
	if enemy:
		Visuals.box(self,Vector3(0,1.08,-.14),Vector3(.38,.36,.11),Color("34434a"))
		Visuals.box(self,Vector3(0,1.18,-.2),Vector3(.24,.025,.02),Color("e59566"),false,.6)
	else:
		Visuals.box(self,Vector3(0,1.08,.22),Vector3(.33,.36,.16),Color("2d464e"))
		Visuals.box(self,Vector3(0,1.18,.305),Vector3(.23,.025,.01),Color("b2c6b0"))

func animate(time: float, speed: float, aiming: bool, crouch: bool) -> void:
	var desired := "run" if speed>.15 else "idle"
	if animation.current_animation != desired: animation.play(desired,.18)
	animation.speed_scale = clampf(speed/4,.5,1.5) if desired=="run" else 1.0
	position.y = -.3 if crouch else 0.0
	rotation.x = .14 if crouch else 0.0
	gun.visible = armed or is_enemy
	gun.position = Vector3(.3,1.35,-.5) if aiming else Vector3(.31,.86,-.08)
	gun.rotation.x = 0 if aiming else .8
	# Additive upper-body pose keeps imported walk/run animation in the legs.
	for side in ["Left","Right"]:
		var bone := skeleton.find_bone(side+"Arm")
		if aiming:
			var pose := skeleton.get_bone_pose_rotation(bone)
			var offset := Quaternion(Vector3.FORWARD, -.9 if side=="Right" else .75)
			skeleton.set_bone_pose_rotation(bone,pose*offset)
