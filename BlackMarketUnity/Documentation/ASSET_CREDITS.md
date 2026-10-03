# Asset credits

## Poly Haven — CC0 1.0

https://polyhaven.com/license

Đã tải qua API công khai, kiểm tra MD5 theo metadata. Chi tiết tác giả từng asset và URL từng file nằm trong `media-manifest.json`. Chọn mức 1K cho lượt thử mobile đầu tiên. Map kiến trúc do dự án tự bố trí; không phải một map thương mại tải trọn gói.

- https://polyhaven.com/a/metal_office_desk
- https://polyhaven.com/a/vintage_radio_transceiver
- https://polyhaven.com/a/metal_tool_chest
- https://polyhaven.com/a/rollershutter_door
- https://polyhaven.com/a/wooden_crate_01
- https://polyhaven.com/a/metal_stool_01
- https://polyhaven.com/a/Shelf_01
- https://polyhaven.com/a/painted_plaster_wall
- https://polyhaven.com/a/concrete_floor_02
- https://polyhaven.com/a/asphalt_02
- https://polyhaven.com/a/brick_wall_001
- https://polyhaven.com/a/metal_plate

Unity metallic/smoothness masks được chuyển từ bản đồ metallic/roughness của nguồn; normal dùng OpenGL. Kiến trúc, biển hiệu North Point và bố trí phòng là nội dung riêng của dự án.

## Kenney — CC0

- Foley: https://kenney.nl/assets/impact-sounds — file license đi kèm trong Resources/Foley. Bước chân concrete được sử dụng trong game.
- Bản sao lưu nhân vật cũ (không dùng ở prefab Alex/Operator mới): https://kenney.nl/assets/animated-characters-protagonists — sao chép từ assets/characters của Godot, giữ license trong Resources/LegacyCharacters. Không quảng bá đây là nhân vật chân thực mới.

## Nội dung kế thừa

- Font DejaVu Sans và JetBrains Mono: license đi kèm trong Resources/Fonts.
- Audio tổng hợp door/beep/alarm/hit/ring/ambient: còn dùng cho tín hiệu gameplay và nền cơ sở ngầm. Shot/reload đã thay bằng bản thu, foley bước chân và mưa lấy từ nguồn bên dưới.
- Cốt truyện BLACK MARKET: theo báo cáo thiết kế Word và black_market_gdd.md của dự án.

## Microsoft Rocketbox — MIT

https://github.com/microsoft/Microsoft-Rocketbox

- Alex: Male_Adult_07 (áo khoác thường phục).
- Nhân viên an ninh/Victor: Security_Male_01.
- Animation gốc: m_idle_neutral_01, m_walk_neutral_01, m_run_neutral_01.
- Giữ thông báo bản quyền đầy đủ trong `Resources/Rocketbox/LICENSE.md`.
- Shader chuyển sang URP, texture giới hạn 1K, chỉ bật mesh hipoly của mỗi nhân vật, bỏ displacement ở root animation để CharacterController/NavMesh quản lý di chuyển. Tư thế tay cầm súng bổ sung bằng code, chưa phải mocap chiến đấu.
- URL đường dẫn nguồn và SHA256: `rocketbox-manifest.json`.

## Súng và âm thanh tải bổ sung — CC0

- Pistol — loafbrr_1: https://opengameart.org/content/pistol-5 . FBX + texture Pistol_1; license đi kèm Weapons/README.txt.
- Reload — SpringySpringo: https://opengameart.org/content/gun-reload-sounds . Bản thu airsoft.
- Rain — Ylmir: https://opengameart.org/content/rain-loopable . Loop 1, giảm âm lượng khi vào nhà.
- Gunshots — kurt: https://opengameart.org/content/gunshots . Tách một phát từ 22 Pistol, chuẩn hóa và fade phần đuôi.
- Poly Haven bổ sung: https://polyhaven.com/a/desk_lamp_arm_01 và https://polyhaven.com/a/street_lamp_01.

`additional-media.json` ghi URL tải và SHA256. Character3D của NephthysGameDev đã tải để đánh giá nhưng không được chọn sử dụng; không phải nhân vật trong game.

## Bổ sung cửa hàng / stealth

Poly Haven, CC0: https://polyhaven.com/a/Television_01 , https://polyhaven.com/a/television_02 , https://polyhaven.com/a/cardboard_box_01 . File tải/MD5/tác giả nằm trong media-manifest.json. Kệ mở, bố cục phòng, chi tiết dụng cụ nhỏ và đèn pin được tạo trong dự án.
