# BLACK MARKET — Unity

Unity 6000.3.25f1 / URP. Mở `Assets/BlackMarket/Scenes/NorthPoint.unity` rồi Play, hoặc chạy `./play-linux.sh` để mở bản Linux đã build.

## Bản đồ và tuyến chơi

**Bảy chương dùng bảy bản đồ riêng:** cửa hàng tầng 1 → kỹ thuật B1 → vũ khí/hồ sơ B2 → an ninh B3 → y tế/kho B4 → máy chủ B5 → điều hành B6.

Alex bắt đầu ở trong cửa hàng có mặt tiền kính và cửa khóa. Door 06 dẫn vào cầu thang bí mật. Các tầng hầm bắt đầu trên chiếu nghỉ, người chơi tự đi cầu thang xuống. Mỗi tầng có bố cục, đồ đạc, màu sắc và mục đích riêng. WC, phòng điện, khu y tế, kệ kho dày và các phòng phụ bổ sung đường vòng/chỗ núp.

Chi tiết: [MAPS_VI.md](MAPS_VI.md). Cơ chế tuần tra: [SHOP_STEALTH_VI.md](SHOP_STEALTH_VI.md).

Âm thanh và ánh sáng: [AUDIO_LIGHTING_VI.md](AUDIO_LIGHTING_VI.md).

Sửa sàn, phòng/cửa mới và hướng dẫn lẻn qua lính: [ROOMS_AND_STEALTH_VI.md](ROOMS_AND_STEALTH_VI.md).

NPC, trang bị và cân bằng mới: [ENCOUNTERS_VI.md](ENCOUNTERS_VI.md). Các encounter tăng gấp đôi; B6 có 8 phòng, 51 vật chắn chính, AK và kính đêm có thể nhặt. Trước B6, hai phát trúng từ đủ máu sẽ làm Alex gục.

## Điều khiển

WASD di chuyển; chuột xoay camera; Shift chạy; C khom; E tương tác; chuột phải ngắm; chuột trái bắn; R nạp; Q đổi súng; N kính đêm; Tab Tablet; **J nhật ký/mục đích**; Esc tạm dừng. Android có các nút cảm ứng tương ứng và vuốt bên phải để nhìn.

HUD giải thích cả hành động và lý do. Nhật ký giúp đọc lại bối cảnh. Tablet không tạm dừng thời gian: nấp trước khi sử dụng. Súng hạ khi không ngắm/bắn; đèn pin NPC có chùm sáng rõ hơn. Nhạc nền tăng độ căng thẳng theo nguy hiểm.

Checkpoint lưu đầu mỗi chương, riêng trong `Application.persistentDataPath`. Chọn **BẮT ĐẦU CHIẾN DỊCH** để xem bản đồ mới từ đầu; tiếp tục checkpoint sẽ tới tầng tương ứng.

## Cấu trúc và chỉnh sửa

- `Scripts`: campaign, gameplay, UI, AI, hiệu ứng và tự kiểm thử.
- `Editor/FloorLayouts.cs`, `Editor/ShopLayout.cs`: bố trí môi trường.
- `Resources/Worlds`: bảy prefab game thực sự tải, tên cụ thể trong MAPS_VI.
- `Scenes/*_Environment.unity`: bản tham khảo môi trường, không phải scene để chơi.

Menu `BLACK MARKET → 1 - Prepare North Point project` tái tạo nội dung tự động, ghi đè prefab/scene/vật liệu sinh ra. Sao lưu các chỉnh sửa thủ công trước khi dùng. Sau khi chỉnh vật cản, bake lại NavMeshSurface.

## Build và kiểm thử

- `BLACK MARKET → 5`: kiểm tra asset, spawn, đường NavMesh tới tương tác/tuần tra và render ảnh môi trường.
- `BLACK MARKET → 3`: build Linux Development vào `Builds/Linux`.
- `BLACK MARKET → 4`: build Android Development vào `Builds/Android`, package `vn.kma.blackmarket.unity`.
- `./play-linux.sh --self-test --capture`: kiểm tra runtime với save riêng trong temporaryCachePath; kết quả `Documentation/runtime-test.txt`, ảnh `Documentation/Playtest`.

Game Unity và Godot ở thư mục cha độc lập; không dùng chung source runtime hay save. `.gdignore` ngăn Godot nhập dữ liệu Unity.

## Giới hạn

Pose súng/crouch điều chỉnh bằng code trên animation idle/walk/run; chưa có mocap chiến đấu chuyên dụng. Victor dùng model chiến thuật đen riêng. Kể chuyện bằng chữ, chưa có lồng tiếng. Nhạc căng thẳng là nhạc tổng hợp gốc; chùm đèn pin là mesh trong suốt có kiểm tra vật cản. Chưa đo FPS, nhiệt, cảm ứng hay thời lượng 20–30 phút trên Android thật.

Nguồn asset: [ASSET_CREDITS.md](ASSET_CREDITS.md). Kết quả kiểm thử: [TEST_REPORT_VI.md](TEST_REPORT_VI.md).
