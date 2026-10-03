# BLACK MARKET — Unity rebuild

Mở `Assets/BlackMarket/Scenes/NorthPoint.unity`, bấm Play. Không mở scene Environment để chơi: các scene này chỉ phục vụ chỉnh môi trường.

Nếu menu BLACK MARKET chưa xuất hiện: Assets → Refresh, chờ Unity biên dịch. Công cụ editor tạo nội dung bằng menu `BLACK MARKET → 1 - Prepare North Point project`. Scene SampleScene của bạn không bị ghi đè.

## Hai dự án độc lập

- Dự án Godot vẫn ở thư mục cha: `project.godot`, `scripts`, `scenes`, `assets` giữ nguyên.
- Dự án Unity chỉ ở `BlackMarketUnity/`. `.gdignore` ngăn Godot nhập Library và asset của Unity.
- `godot-baseline-sha256.json` là dấu vân tay các file Godot trước lượt chuyển đổi này.
- Save Unity dùng thư mục `Application.persistentDataPath` riêng; không đọc/ghi checkpoint Godot.
- Mã Unity được viết bằng C#, không phải bản tự chuyển đổi GDScript.

## Nội dung

- Luồng 7 chương: cửa hàng điện tử → đột nhập và radio → Door 06 → Armory/ORDER 071/FOR_ALEX → Security → Storage/Medical → Uplink → Victor ba phase → DESTROY/ACCEPT.
- Bám thời gian trong báo cáo Word: cuộc gọi 02:17, tin Marcus chết sáng hôm sau, Alex trở lại ba ngày sau; vào cửa hàng lúc 21:30, SUV đến 22:58, sự kiện 23:00.
- North Point là cửa hàng điện tử/sửa chữa, không phải siêu thị thực phẩm.
- Điều khiển góc nhìn thứ ba, crouch, hitscan, reload; AI NavMesh có nghe/nhìn; Tablet hoạt động thời gian thực; checkpoint đầu chương.
- Môi trường có prefab, vật liệu URP Lit, normal/metallic/smoothness, đạo cụ Poly Haven; mốc gameplay chỉnh bằng WorldMarker và Interaction trong prefab.
- Bloom, ACES, vignette vừa phải; ánh sáng lạnh/ấm trong nội thất, âm thanh không gian và foley bước chân thu âm.

## Chỉnh sửa

Prefab môi trường nằm trong `Assets/BlackMarket/Resources/Worlds/`. Mở prefab để dời vật thể và mốc tương tác. Sau khi đổi tường/kệ, bake lại NavMeshSurface và lưu dữ liệu navmesh. Không xóa các mốc spawn, camera, shutter, alarm, enemy, boss đang được code sử dụng.

Các scene `*_Environment.unity` là bản dựng để xem và chỉnh tham khảo. Game thực tế tải prefab trong Resources/Worlds. Muốn thay đổi xuất hiện trong game, chỉnh prefab tương ứng.

`Prepare North Point project` là công cụ tái tạo: nó ghi lại nội dung sinh tự động trong Resources/Worlds, Resources/Materials và các scene BlackMarket. Nếu đã chỉnh prefab bằng tay, hãy sao lưu/đổi tên trước khi chạy lại.

## Mức hoàn thiện cần hiểu đúng

Lượt nâng cấp này dùng nhân vật Rocketbox có quần áo/texture và idle/walk/run, súng FBX có texture, 14 asset Poly Haven (đạo cụ/vật liệu), tiếng mưa và nạp đạn thu thật, foley bước chân biến thiên. Đã sửa vị trí model nhập, UV theo kích thước thật, biển chữ có depth test, thêm chỉ dẫn khoảng cách tới mục tiêu và mưa ngoài cửa hàng.

Các giới hạn còn lại: cử động ngắm là tư thế tay điều chỉnh bằng code, chưa có mocap chiến đấu/nạp đạn chuyên dụng; Victor dùng chung model an ninh; cutscene là chữ và chưa có lồng tiếng. Tiếng tín hiệu/radio/nền bunker còn một phần audio tổng hợp. Các khu ngầm vẫn dùng chung cấu trúc module. Đây chưa phải đồ họa AAA hay bản nghiệm thu mỹ thuật cuối cùng.

Không tuyên bố đã đạt 20–30 phút hoặc FPS Android trước khi chơi thử trên máy thật. Bản Linux và kiểm thử tự động không thay thế việc đó.

## Kiểm thử và build

- `BLACK MARKET → 5 - Validate and render environment previews`: kiểm tra vật liệu, mốc scene, đường NavMesh tới điểm tương tác và chụp các góc môi trường.
- `BLACK MARKET → 3 - Build Linux preview`: Development build vào Builds/Linux.
- `BLACK MARKET → 4 - Build Android preview`: APK thử nghiệm, package riêng `vn.kma.blackmarket.unity`.
- Linux Development build nhận `--self-test` để chạy kiểm tra campaign có save tách biệt trong temporaryCachePath; `--capture` chụp các màn hình. Chạy từ thư mục gốc dự án Unity để log nằm ở Documentation.

Nguồn và license: `ASSET_CREDITS.md`, metadata/hashes file tải: `media-manifest.json`.

## Chạy bản Linux

Từ thư mục `BlackMarketUnity`, chạy `./play-linux.sh`. Script mở bản trong `Builds/Linux/BlackMarket.x86_64` và ghi log `Documentation/player.log`.

WASD di chuyển, chuột xoay camera, Shift chạy, C khom, E tương tác; chuột phải ngắm, chuột trái bắn, R nạp; Tab Tablet khi đã mở khóa, Esc tạm dừng. Menu có nút Thoát game.

Biểu tượng ◇ và số mét chỉ vị trí mục tiêu hiện tại. Checkpoint được lưu ở đầu mỗi chương; thử lại từ menu khi chết.
