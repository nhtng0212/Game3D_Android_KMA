extends SceneTree
var main
var failures: Array[String] = []
func _initialize() -> void: call_deferred("run")
func check(value: bool, description: String) -> void:
	if not value:
		failures.append(description)
		print("FAIL: ",description)
func inspect_overlay() -> void:
	for i in 4: await process_frame
	var bottom: float = main.hud.size.y
	var right: float = main.hud.size.x
	for child in main.hud.overlay.get_children():
		if child is Label or child is Button:
			var r: Rect2 = child.get_global_rect()
			check(r.end.x<=right-10,"text/control fits width: "+child.text.left(25))
			check(r.end.y<=bottom-10,"text/control fits height: "+child.text.left(25)+" "+str(r)+" viewport="+str(bottom))
func run() -> void:
	main = load("res://scenes/main.tscn").instantiate()
	root.add_child(main)
	for i in 4: await process_frame
	await inspect_overlay()
	main.hud.settings(false)
	await inspect_overlay()
	main.hud.help_screen(false)
	await inspect_overlay()
	main.new_campaign()
	await inspect_overlay()
	main.hud.story("FOR_ALEX / MARCUS CARTER", "North Point thuộc The Market. Chúng cung cấp vũ khí, danh tính và contract cho các Operator.\n\nVictor đã biến lệnh bảo vệ thành lệnh ám sát. ORDER 071 là bằng chứng. Chú đã trao quyền Keeper cho cháu.\n\nĐừng cố thắng chúng bằng súng. Hãy dùng chính tòa nhà.\n\n[ NETWORK BREACH • PURGE AUTHORIZED ]",func(): pass)
	await inspect_overlay()
	main.hud.ending_choice()
	await inspect_overlay()
	main.hud.ending("destroy")
	await inspect_overlay()
	main.hud.ending("accept")
	await inspect_overlay()
	print("UI LAYOUT TEST: ","PASS" if failures.is_empty() else "FAIL"," / ",failures.size()," failures")
	root.get_node("Audio").stop_all()
	main.queue_free()
	await create_timer(.2).timeout
	quit(0 if failures.is_empty() else 1)
