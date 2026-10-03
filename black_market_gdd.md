# BLACK MARKET
**BÁO CÁO THIẾT KẾ VÀ KẾ HOẠCH PHÁT TRIỂN GAME ANDROID**
*3D Third-Person Action / Survival / Stealth / Thriller*

**Nền tảng:** Android
**Game Engine dự kiến:** Godot 4
**Chế độ:** Single Player
**Thời lượng vertical slice:** 20–30 phút

---

## MỤC LỤC NỘI DUNG
1. Tổng quan đề tài và định hướng sản phẩm
2. Thế giới, cốt truyện và nhân vật
3. Cấu trúc tổng thể của game và game flow
4. Khung cảnh, level và asset cần chuẩn bị
5. Thiết kế từng chapter/màn chơi
6. Hệ thống điều khiển Android và HUD
7. Cấu trúc project Godot và tổ chức source code
8. Player System
9. Interaction và Inventory System
10. Enemy AI và Stealth System
11. Combat System
12. Security Tablet và hệ thống điều khiển môi trường
13. Objective, Trigger, Checkpoint, Dialogue và Ending
14. Công cụ phát triển và pipeline asset
15. Thứ tự triển khai code và prototype
16. Tối ưu hóa và kiểm thử trên Android
17. Phạm vi vertical slice cuối kỳ
18. Kết luận
19. PHỤ LỤC BỔ SUNG: Thông số kỹ thuật cho AI

---

## 1. TỔNG QUAN ĐỀ TÀI VÀ ĐỊNH HƯỚNG SẢN PHẨM

| Hạng mục | Thông tin |
| :--- | :--- |
| **Tên game** | BLACK MARKET |
| **Thể loại** | 3D Third-Person Action / Survival / Stealth / Thriller |
| **Nền tảng** | Android |
| **Chế độ** | Single Player |
| **Góc nhìn** | Third-person |
| **Engine dự kiến** | Godot 4 |
| **Thời lượng bản cuối kỳ**| 20–30 phút |
| **Định hướng** | Vertical slice có thể chơi từ đầu đến cuối |

BLACK MARKET là game hành động sinh tồn góc nhìn thứ ba, kết hợp stealth, combat và điều khiển môi trường. Người chơi vào vai Alex Carter - một sinh viên 21 tuổi vô tình bị cuốn vào mạng lưới bí mật mang tên The Market sau cái chết của người chú Marcus Carter.

Điểm nhận diện cốt lõi của game không nằm ở việc bắn càng nhiều kẻ địch càng tốt, mà ở khả năng dùng chính cơ sở North Point như một vũ khí. Người chơi quan sát vị trí địch, lập kế hoạch, điều khiển camera, cửa, ánh sáng và báo động để tạo lợi thế trước khi hành động.

**EXPLORE → OBSERVE → PLAN → MANIPULATE → FIGHT / STEALTH → PROGRESS**

**Thông điệp gameplay cốt lõi:**
> THE BUILDING IS YOUR WEAPON.
> SEE WHAT THEY CAN'T SEE.
> CONTROL WHAT THEY CAN'T CONTROL.
> SURVIVE NORTH POINT.

---

## 2. THẾ GIỚI, CỐT TRUYỆN VÀ NHÂN VẬT

### 2.1. The Market và hệ thống Point
Trong thế giới của BLACK MARKET tồn tại một mạng lưới bí mật có tên The Market. Tổ chức này không trực tiếp thực hiện các vụ ám sát mà cung cấp cơ sở hạ tầng cho các Operator hoạt động trên toàn thế giới:
* Thông tin và dữ liệu tình báo.
* Thiết bị, vũ khí và phương tiện.
* Danh tính giả và nơi trú ẩn.
* Contract và dịch vụ vận chuyển bí mật.

Các cơ sở bí mật của The Market được gọi là Point. Mỗi Point ngụy trang dưới dạng một doanh nghiệp bình thường và do một Keeper quản lý. North Point là một trong số đó, có vỏ bọc là cửa hàng cung cấp và sửa chữa thiết bị điện tử.

### 2.2. Alex Carter
* **Tuổi:** 21.
* **Nghề nghiệp:** Sinh viên.
* **Vai trò:** Nhân vật duy nhất người chơi điều khiển.
* Không biết gì về The Market, Operator, Keeper hay bản chất thực của North Point ở đầu game.
* Việc Alex không biết sự thật giúp người chơi khám phá thế giới BLACK MARKET đồng thời với nhân vật chính. Đây là nền tảng cho cách kể chuyện thông qua môi trường, terminal, vật thể tương tác và các sự kiện trong game.

### 2.3. Marcus Carter
Marcus là chú của Alex, bề ngoài là chủ cửa hàng North Point Supply nhưng thực tế là Keeper của North Point trong gần 20 năm. Ông phát hiện một nhóm bên trong The Market thao túng contract, biến nhiệm vụ bảo vệ thành ám sát và thủ tiêu những Keeper hoặc Operator phát hiện sự thật.

Marcus bí mật thu thập bằng chứng và lưu trong **ORDER #071**. Trước khi chết, ông để lại một cuộc gọi cảnh báo Alex không được tin bất kỳ ai biết tên North Point và đặc biệt không được mở Door 06.

### 2.4. Victor Hale
Victor là người quen cũ của Marcus và là nhân vật đối đầu cuối game. Victor đại diện cho mối đe dọa từ bên trong The Market và là người buộc Alex phải sử dụng toàn bộ kiến thức đã học về combat, stealth và hệ thống bảo mật của North Point.

---

## 3. CẤU TRÚC TỔNG THỂ CỦA GAME VÀ GAME FLOW

Main Menu
↓
Prologue - The Last Call
↓
Outside North Point
↓
Shop / Office / Workshop / Warehouse
↓
23:00 - Visitors Arrive (Power Cut / Break-in / Stealth)
↓
Door 06
↓
Underground North Point (Armory -> Order Room -> Server Room)
↓
Security Tablet Unlocked
↓
Purge Team (Security → Storage → Server → Control Room)
↓
Victor Boss Fight
↓
Final Decision:
├── DESTROY → Ending A
└── ACCEPT  → Ending B

Game được thiết kế theo progression tuyến tính nhưng mỗi combat encounter nên có từ hai cách giải quyết trở lên: chiến đấu trực tiếp, stealth, hoặc thao túng môi trường để chia cắt đối phương.

---

## 4. KHUNG CẢNH, LEVEL VÀ ASSET CẦN CHUẨN BỊ

### 4.1. Asset nhân vật
| Nhóm | Asset / Animation cần có |
| :--- | :--- |
| **Alex Carter** | Model 3D, rig/skeleton, Idle, Walk, Run, Aim, Shoot, Reload, Interact, Hit, Death. |
| **Enemy** | Model cơ bản; có thể tái sử dụng cho Scout/Assault/Heavy bằng material, helmet, weapon, scale và stats khác nhau. |
| **Victor** | Có thể dùng base enemy nâng cấp hoặc model riêng; cần animation cover, aim, shoot, move, hit, defeat. |

### 4.2. Asset môi trường tầng trên
* Exterior North Point Supply, biển hiệu, đường, bãi đỗ xe, hàng rào và đèn ngoài trời.
* Shop counter, kệ hàng, thùng hàng, thiết bị điện tử, poster, bàn ghế.
* Marcus Office: desk, PC, chair, filing cabinet, keycard, ảnh gia đình, document.
* Workshop: bàn sửa chữa, tool rack, radio, linh kiện điện tử.
* Warehouse: kệ hàng, pallet, boxes, security room, electrical panel, generator, Door 01–06.
* CCTV camera, đèn hành lang, bảng điều khiển, khóa điện tử và biometric scanner.

### 4.3. Asset khu North Point ngầm
* Elevator và hành lang bê tông.
* Main Hall, Armory, Security Room, Medical, Storage, Order Room, Server Room, Control Room.
* Server rack, locker, terminal, cable, security monitor, weapon rack, cover object.
* Pistol, rifle, ammo, medkit, data drive Order 071.
* Camera an ninh, alarm speaker, light fixture, door controller, security terminal.

### 4.4. Asset UI và âm thanh
* Main Menu, Pause, Game Over.
* HUD: HP, Ammo, Weapon, Objective.
* Mobile control: joystick, Run, Interact, Aim, Fire, Reload, Security Tablet.
* Security Tablet UI: Map, Camera, Door, Light, Alarm.
* Terminal UI, Order #071 UI, dialogue/subtitle UI, ending selection.
* Audio: footsteps, gunshots, reload, alarm, radio, door, scanner, elevator, ambient hum, power cut, UI click.

*(Đối với Android, asset 3D cần được tối ưu polygon, texture, material và số lượng light realtime. Có thể dùng asset prototype đơn giản ở giai đoạn đầu rồi thay thế sau khi gameplay ổn định.)*

---

## 5. THIẾT KẾ TỪNG CHAPTER / MÀN CHƠI

**5.1. Prologue - The Last Call**
* Khung cảnh: Màn hình tối, điện thoại reo lúc 02:17 AM.
* Nội dung: Marcus cảnh báo Alex không tin bất kỳ ai biết tên North Point và không mở Door 06.
* Gameplay: Cutscene/voice-over ngắn.

**5.2. Chapter 1 - Outside North Point**
* Khung cảnh: 21:30, đường vắng, bãi xe và cửa hàng.
* Gameplay: Tutorial điều khiển (joystick, camera, chạy, interact). Objective: ENTER NORTH POINT -> SEARCH MARCUS'S OFFICE.

**5.3. Shop và Marcus Office**
* Gameplay: Tại Office, player lấy MARCUS'S KEYCARD và truy cập terminal để phát hiện ORDER #071.

**5.4. Warehouse và Door 06**
* Khung cảnh: Warehouse có hành lang Door 01–06. Door 06 khóa điện tử, không nhãn mác.
* Gameplay: Player thử keycard bị từ chối. Tạo mystery.

**5.5. Chapter 2 - Visitors Arrive**
* Khung cảnh: 22:58 SUV đến. Điện cắt, external communication bị jam.
* Gameplay: Chuyển sang stealth. Objective: FIND A WAY OUT.

**5.6. Stealth Encounter**
* Gameplay: Player chưa có súng, phải ẩn nấp, dụ địch bằng radio.

**5.7. Chapter 3 - Door 06**
* Gameplay: Bị dồn về Door 06. Biometric scanner quét DNA Carter, mở cửa thang máy xuống tầng -3.

**5.8. Chapter 4 - Underground North Point**
* Gameplay: Objective: DISCOVER WHAT MARCUS WAS HIDING. Khám phá cơ sở ngầm.

**5.9. Armory**
* Gameplay: Nhận súng Pistol. Mở khóa cơ chế Aim, Shoot, Reload, Combat.

**5.10. Order Room - ORDER 071**
* Gameplay: Mở locker bằng Keycard + DNA. Nhận DATA DRIVE.

**5.11. Server Room và Marcus Recording**
* Gameplay: Nghe file FOR_ALEX. Mở khóa cơ chế SECURITY TABLET.

**5.12. Purge**
* Khung cảnh: Hệ thống báo NETWORK BREACH, PURGE AUTHORIZED. Kẻ địch tràn vào.

**5.13. Survive North Point**
* Gameplay: Đi qua các khu vực, mỗi khu dạy một mechanic Tablet (Camera -> Alarm -> Door -> Kết hợp).

**5.14. Chapter 8 - Victor**
* Gameplay: Boss fight 3 phase (Phase 1 Combat -> Phase 2 Terminal kích hoạt -> Phase 3 Dùng hệ thống an ninh áp đảo).

**5.15. Final Decision**
* Gameplay: Chọn DESTROY (kết thúc A) hoặc ACCEPT (kết thúc B).

---

## 6. ĐIỀU KHIỂN ANDROID VÀ HUD

| Khu vực màn hình | Điều khiển |
| :--- | :--- |
| **Bên trái** | Virtual Joystick - di chuyển Alex. |
| **Bên phải** | Swipe / drag để xoay camera. |
| **Nút hành động** | INTERACT, RUN, AIM, FIRE, RELOAD, SECURITY TABLET. |
| **HUD** | HP, Ammo, Weapon hiện tại, Objective, thông báo tương tác. |

*Cấu trúc HUD mô phỏng:*
```text
┌──────────────────────────────────────────┐
│ HP ███████████       OBJECTIVE           │
│                                          │
│          THIRD-PERSON VIEW               │
│                                          │
│                                  [AIM]   │
│                           [INTERACT] [●] │
│                                          │
│   ◯                              [FIRE]  │
│ JOYSTICK                        [TABLET] │
└──────────────────────────────────────────┘
```

---

## 7. CẤU TRÚC PROJECT GODOT VÀ TỔ CHỨC SOURCE CODE

**Cây thư mục đề xuất:**
```text
res://
├── scenes/
│   ├── menu/
│   ├── prologue/
│   ├── player/
│   ├── enemies/
│   ├── weapons/
│   ├── shop/
│   ├── underground/
│   └── ui/
├── scripts/
│   ├── player/
│   ├── ai/
│   ├── interaction/
│   ├── combat/
│   ├── security/
│   └── game/
├── assets/
│   ├── models/
│   ├── textures/
│   ├── animations/
│   └── audio/
└── main.tscn
```

---

## 8. PLAYER SYSTEM

**Node đề xuất:**
```text
Player (CharacterBody3D)
├── CollisionShape3D
├── Model
├── AnimationPlayer / AnimationTree
├── CameraPivot
│   └── Camera3D
├── InteractionRay
├── WeaponSocket
└── PlayerController
```

**Mã nguồn cơ bản Movement (GDScript):**
```gdscript
extends CharacterBody3D

@export var move_speed := 4.0

func _physics_process(delta):
    var input_dir = Input.get_vector(
        "move_left", "move_right",
        "move_forward", "move_backward"
    )
    var direction = Vector3(input_dir.x, 0, input_dir.y)
    velocity.x = direction.x * move_speed
    velocity.z = direction.z * move_speed
    move_and_slide()
```

---

## 9. INTERACTION VÀ INVENTORY SYSTEM

InteractionRay từ camera hoặc player dùng để phát hiện object thuộc group `interactable`.

**Base Class Interactable:**
```gdscript
func interact(player):
    # object-specific action
    pass
```

---

## 10. ENEMY AI VÀ STEALTH SYSTEM

**State Machine:**
```gdscript
enum State {
    PATROL,
    INVESTIGATE,
    CHASE,
    ATTACK,
    SEARCH
}
```

**Logic Tầm Nhìn (Vision):**
```gdscript
func can_see_player():
    if player == null:
        return false
    vision_ray.target_position = player.global_position - global_position
    vision_ray.force_raycast_update()
    return vision_ray.get_collider() == player
```

**Enemy Types:**
* **Scout:** Nhanh, HP thấp, vision tốt.
* **Assault:** HP trung bình, súng phổ thông.
* **Heavy:** Chậm, HP cao, xuất hiện cuối game.

---

## 11. COMBAT SYSTEM

Sử dụng Hitscan (RayCast3D) để tối ưu cho Mobile.
```gdscript
func fire():
    if ammo <= 0:
        return
    ammo -= 1
    shoot_ray.force_raycast_update()
    if shoot_ray.is_colliding():
        var target = shoot_ray.get_collider()
        if target.has_method("take_damage"):
            target.take_damage(damage)
```

---

## 12. SECURITY TABLET VÀ HỆ THỐNG ĐIỀU KHIỂN MÔI TRƯỜNG

Sau khi mở khóa Tablet, player điều khiển: `Cameras`, `Doors`, `Lights`, `Alarms`.

```gdscript
func set_door_locked(id, value):
    doors[id].locked = value

func set_light(id, value):
    lights[id].visible = value

func trigger_alarm(id):
    alarms[id].activate()
```

---

## 13. OBJECTIVE, TRIGGER, CHECKPOINT, VÀ ENDING

* **Trigger:** Dùng `Area3D` kích hoạt cutscene, spawn AI, cập nhật objective.
* **Game Over:** HP <= 0 -> Hiện màn hình Retry từ Checkpoint.

---

## 14 & 15. THỨ TỰ TRIỂN KHAI VÀ PIPELINE

Nên phát triển theo hệ thống để kiểm thử độc lập:
1. Player (Movement, Camera, Touch Control).
2. Environment (Door, Keycard, Interact).
3. Enemy AI (Nav, Patrol, Vision).
4. Combat (Shoot, HP, Death).
5. Security (Tablet UI, Camera, Door remote).
6. Game Flow (Objective, Trigger).

**Prototype tối thiểu cần đạt trước khi làm Full Game:**
1 Hành lang + 3 Phòng + 1 Player + 2 Enemies + 1 Camera + 1 Alarm + 2 Doors + 1 Light.

---

## 16. TỐI ƯU HÓA MOBILE (ANDROID)
* Giới hạn AI: 4-6 enemy mỗi encounter.
* Dùng Object Pooling cho Effect.
* Ưu tiên Baked Lighting.
* Tối ưu UI/Nút bấm cảm ứng cho nhiều tỉ lệ màn hình.

---

## 17 & 18. KẾT LUẬN
BLACK MARKET có mechanic độc đáo: "Điều khiển căn nhà thay vì chỉ điều khiển nhân vật". Cần ưu tiên Core System trước khi đổ vào Asset và Polish.

***

## 19. PHỤ LỤC BỔ SUNG: THÔNG SỐ KỸ THUẬT CHO AI (AI GUIDELINES)
*Phần này cung cấp các Hard Numbers để AI thiết lập biến và cấu trúc logic.*

### A. Thông số cân bằng (Game Balancing)
**1. Nhân vật chính (Player):**
* `max_hp`: 100
* `walk_speed`: 4.0 m/s
* `run_speed`: 6.5 m/s
* `interact_range`: 2.5 m (Khoảng cách raycast)
* `camera_fov`: 75 (Aiming FOV: 60)

**2. Kẻ địch (Enemy):**
* **Scout:** `max_hp`: 50 | `speed`: 4.5 m/s | `vision_range`: 20 m | `vision_angle`: 90 độ | `damage`: 10.
* **Assault:** `max_hp`: 100 | `speed`: 3.0 m/s | `vision_range`: 15 m | `vision_angle`: 60 độ | `damage`: 15.
* **Victor (Boss):** `max_hp`: 300 | `speed`: 3.5 m/s. Phase 2 < 200 HP, Phase 3 < 100 HP.

**3. Vũ khí & Âm thanh:**
* **Pistol:** `damage`: 25 | `mag_size`: 8 | `max_reserve_ammo`: 32 | `fire_rate`: 0.4s | `reload_time`: 1.5s.
* **Bán kính phát âm thanh (Noise Radius):** Chạy = 5m | Bắn súng = 25m | Radio = 12m | Báo động = 30m.

### B. Kích thước Môi trường (Level Metrics)
*1 unit trong Godot = 1 mét.*
* Chiều cao tường: 3.0m
* Bề rộng hành lang: 2.5m
* Kích thước cửa (Door): 1.2m (rộng) x 2.2m (cao).
* Vật che chắn (Cover): Cao 1.0m - 1.2m.
* Marcus Office: 5m x 5m.
* Warehouse: 15m x 20m.

### C. Trạng thái Game (Game Manager Dictionary)
Yêu cầu Autoload script `game_manager.gd`:
```gdscript
var game_state = {
    "player": {
        "current_hp": 100,
        "has_marcus_keycard": false,
        "unlocked_tablet": false,
        "current_ammo": 8,
        "reserve_ammo": 16
    },
    "story_flags": {
        "door_06_opened": false,
        "order_071_collected": false,
        "purge_initiated": false,
        "boss_defeated": false
    },
    "current_objective": "ENTER NORTH POINT"
}
```

### D. AI Prompt Pipeline (Hướng dẫn Prompt cho AI Editor)
1. **Khởi tạo:** Xây dựng `player_controller.gd` (CharacterBody3D) với mobile touch input, camera xoay, walk (4.0), run (6.5).
2. **Tương tác:** Viết class `interactable.gd` và `interaction_ray.gd` (raycast dài 2.5m).
3. **Kẻ địch:** Viết `enemy_ai.gd` dùng `NavigationAgent3D` với State Machine và Line of Sight (góc 60 độ).
4. **Hệ thống An ninh:** Viết `security_manager.gd` quản lý cửa, đèn, camera, báo động.
5. **UI & Game Loop:** Ghép nối hệ thống, tạo HUD, viết `game_manager.gd`.