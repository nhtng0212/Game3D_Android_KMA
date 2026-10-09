# Asset credits

## Cửa hàng điện tử mới — 07/10/2026

10 model mới từ [Poly Haven](https://polyhaven.com/models), [CC0](https://polyhaven.com/license): classic_laptop, CashRegister_01, boombox, Camera_01, vintage_electric_kettle, vintage_microwave, security_camera_01, plastic_monobloc_chair_01, steel_frame_shelves_02, steel_frame_shelves_03. Tác giả từng model, URL tải và SHA256 trong [shop-assets-manifest.json](shop-assets-manifest.json).

Ba model `Sink_A`, `Toilet_Elongated_A`, `Urinal_A` từ [Toilets — loafbrr_1](https://opengameart.org/node/165996), CC0. Giữ README và license ở `Assets/BlackMarket/Art/ShopSources/Sanitary`. Texture 1K; normal/metallic/roughness được đưa vào material URP. Các mask metallic/smoothness được chuyển từ texture nguồn.

Kiến trúc, kệ trưng bày, quầy, TV phẳng, nhãn North Point, texture sàn/gỗ đơn giản là nội dung do dự án tạo. Xem [SHOP_REBUILD_VI.md](SHOP_REBUILD_VI.md).

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
- Nhân viên an ninh ban đầu: Security_Male_01. Victor hiện dùng Police_Male_02, xem mục đội NPC bên dưới.
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

## Bảy tầng / nhạc căng thẳng

Bố cục các tầng, cầu thang, WC, giường y tế, tủ điện, mesh chùm đèn pin và shader do dự án tạo. Đồ đạc nhập tiếp tục dùng các nguồn được ghi phía trên; không thêm asset tải ngoài trong lượt này.

`TensionScore.cs` tạo nhạc nền drone/pulse bằng tổng hợp sóng, nội dung gốc của dự án. Không sử dụng bản thu nhạc bên thứ ba.

## Đội NPC và Victor — 04/10/2026

Bổ sung từ [Microsoft Rocketbox](https://github.com/microsoft/Microsoft-Rocketbox), MIT: Military_Male_01, Military_Male_05, Police_Male_04, Police_Male_06, Male_Adult_04 và Police_Male_02 (Victor). Bản cố định `0943055db6ec570bcef9f2c8b41c9e5467c808f9`; URL, kích thước và SHA256 từng file trong `roster-manifest.json`. Giữ nguyên thông báo MIT tại Resources/Rocketbox/LICENSE.md.

Model AK, kính nhìn đêm, bàn họp, ghế có lưng, tủ cá nhân và phù hiệu Victor được tạo bằng hình học/code trong dự án, không thêm giấy phép asset bên thứ ba. Nhạc tiếp tục dùng nội dung đã ghi ở trên. Âm thanh AK/pistol đã được thay bằng bộ bản thu riêng ở mục bên dưới.

## Âm thanh súng mới — 04/10/2026

[The Free Firearm Sound Library](https://opengameart.org/node/21826), CC0, bản thu của Ben Jaszczak, Brian Nelson, Kevin Heras và Matthew Nanney. Pistol dùng ba phát riêng từ Walther PPQ `X_39P.wav`; AK dùng ba phát riêng từ AK-47 `C_28P.wav`. Mono 44,1 kHz PCM, lọc tiếng ù tần số thấp, thêm phản xạ phòng nhẹ và fade đuôi; không tăng cao độ tiếng pistol để giả làm AK. URL/SHA256 nguồn và từng clip ở `combat-audio-manifest.json`; script tái tạo: `Tools/prepare_combat_audio.py`.

Tiếng cửa trượt kết hợp texture từ Kenney `impactMetal_light_000.ogg` đã có trong dự án (CC0) với tiếng motor tổng hợp gốc. Tiếng chốt và va chạm tiếp tục dùng Foley Kenney. Không thêm nhạc có bản quyền.

## Đoạn mở đầu và nhóm truy bắt

- Nhân vật dân sự mặc vest: `Male_Adult_03`, [Microsoft Rocketbox](https://github.com/microsoft/Microsoft-Rocketbox/tree/master/Assets/Avatars/Adults/Male_Adult_03), giấy phép MIT. Dùng bộ chuyển động idle/walk/run Rocketbox đã có; vật liệu áo được làm tối, texture nguồn giảm về 1K. License và manifest SHA256 nguồn giữ trong `Assets/BlackMarket/Art/Opening/Rocketbox`.
- Xe sedan: [Kenney Car Kit](https://kenney.nl/assets/car-kit), CC0. Chỉ nhập sedan và bảng màu; chỉnh màu sơn tối. License kèm `Assets/BlackMarket/Art/Opening/Cars`.
- Âm thanh động cơ tiếp cận `arrival_engine.wav`: tổng hợp mới bằng sóng điều hòa và nhiễu, không có bản thu lời thoại tải ngoài. Hội thoại nhóm truy bắt hiển thị bằng phụ đề.

## Cập nhật cửa hàng: Alex trẻ và sedan

- **Alex: Male_Adult_08 / m014**, Microsoft Rocketbox, MIT. Nguồn: https://github.com/microsoft/Microsoft-Rocketbox/tree/master/Assets/Avatars/Adults/Male_Adult_08 . Bản giấy phép nằm ở `Assets/BlackMarket/Art/Revision/Rocketbox/LICENSE.md`; URL và SHA-256 từng tệp tại `Art/Revision/young-man-manifest.json`. Model dùng rig và hoạt ảnh Rocketbox sẵn trong dự án; vật liệu được chuyển sang URP.
- **Large Sedan (Black, LOD0)**, MrJaneLAB, CC0. Nguồn và giấy phép do tác giả công bố: https://mrjanelab.itch.io/free-low-poly-large-sedan-lod-included . Dùng thân xe và bánh riêng, chuyển vật liệu sang URP, thêm biển số và đèn pha. SHA-256 tệp ZIP tại `Assets/BlackMarket/Art/Revision/Sedan/source.json`.
- Tủ mở được, giá sách, cầu thang, khối nhà phố, vỉa hè và biển chỉ dẫn bổ sung được dựng trong dự án. Kệ kho tiếp tục dùng các model miễn phí đã ghi nguồn ở trên.

## Xe máy Alex — mở đầu màn 1

- **Yet Another PSX Style Low Poly Bike**, tác giả **Duhgless**.
- Nguồn: https://duhgless.itch.io/yet-another-psx-style-low-poly-bike
- Giấy phép: **CC0**, ghi trên trang phát hành; tải ngày 2026-10-08.
- Đã dùng: Fonk-Bike.obj, Fonk-Bike.mtl, Fonk-Frame.png, Forks.png, Front-Wheeel.png, Rear-Tire.png.
- Đã chuẩn hóa kích thước 2,25 m, hướng di chuyển, chuyển material sang URP và thêm biển số trong prefab. Đường dẫn texture tuyệt đối trong MTL được đổi thành đường dẫn tương đối; thêm khai báo nhóm trong OBJ để Unity tách bánh xe và khung đúng theo các bộ phận gốc.
- Phòng ngủ, đồ nội thất hình học và biển số được tạo riêng trong dự án; không cần mua asset.
- `Resources/Audio/alex_motorcycle.wav`: âm động cơ tổng hợp riêng từ các sóng hài và nhiễu, không phải bản ghi âm tải từ bên thứ ba.
