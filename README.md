# BLACK MARKET — North Point

Bản playable 0.1.0 cho Android, xây dựng bằng Godot 4.7.2 từ `black_market_gdd.md` và tài liệu Word trong thư mục gốc. Có thể chơi xuyên tuyến truyện từ cuộc gọi của Marcus đến lựa chọn DESTROY hoặc ACCEPT. Đây là bản vertical slice đầu tiên để cài và đánh giá gameplay.

## Chạy ngay

- Android: chép `builds/BLACK_MARKET.apk` sang điện thoại, mở file và cho phép cài ứng dụng từ nguồn đang sử dụng khi Android yêu cầu. APK được ký debug để dùng nội bộ, chạy offline, có ARM64 và ARMv7.
- Linux: chạy `./builds/BLACK_MARKET.x86_64`.
- Chạy từ source với engine có sẵn: `./tools/run.sh`.
- Chỉnh sửa: mở `project.godot` bằng Godot 4.7.2, hoặc chạy `./tools/run.sh --editor`.

APK khai báo Android 7.0 trở lên (min SDK 24), dùng renderer Compatibility/OpenGL ES 3.0. Chưa có kết quả đo trên điện thoại thật; khả năng duy trì FPS, nhiệt và cảm giác touch cần được đánh giá trên thiết bị mục tiêu.

## Điều khiển

| Hành động | Android | Máy tính |
| --- | --- | --- |
| Di chuyển | Joystick trái | WASD |
| Camera | Vuốt vùng trống bên phải | Chuột |
| Chạy | Nút CHẠY bật/tắt | Giữ Shift |
| Đi khom | Nút KHOM bật/tắt | C |
| Tương tác | E / DÙNG | E |
| Ngắm / bắn | Giữ NGẮM / BẮN | Chuột phải / trái |
| Nạp đạn | NẠP | R |
| An ninh | TABLET | Tab |
| Tạm dừng | Nút Ⅱ hoặc Back Android | Esc |

Nút vũ khí chỉ xuất hiện sau Armory; Tablet mở sau bản ghi FOR_ALEX. Có thể dùng đồng thời joystick, ngắm và bắn. Trong Cài đặt có âm lượng, độ nhạy camera, hỗ trợ ngắm và tùy chọn hiện touch UI trên desktop.

## Tuyến chơi

1. **North Point Supply:** lấy thẻ Marcus, đọc terminal, kiểm tra Door 06.
2. **Visitors Arrive:** chưa có súng; bật radio, đi khom và lẻn đến máy quét sinh trắc.
3. **Emergency Succession:** nhận Pistol, lấy ORDER 071, đọc FOR_ALEX.
4. **Eyes in the Dark:** mở Camera, kích hoạt báo động C và điều khiển cửa B. Đến cửa chuyển khu.
5. **Divide & Survive:** dùng ánh sáng và cửa để chia cắt địch, qua Storage/Medical.
6. **The Evidence:** khởi động Uplink, sống sót trong 35 giây, đến Control Room.
7. **Victor Hale:** phase 1 chiến đấu; phase 2 khôi phục quyền tại terminal; phase 3 dùng EMP/đèn và chiến đấu. Sau đó quyết định DESTROY/ACCEPT.

Tablet chạy theo thời gian thực: tìm chỗ nấp trước khi mở. Báo động thu hút địch chưa giao chiến; tường và cửa đóng chắn tầm nhìn. Bóng tối và đi khom giảm tầm phát hiện. Cửa B đóng/mở được từ xa và có cảm biến tránh kẹp người. Thùng cam và vật phẩm rơi bổ sung đạn, hồi máu.

Checkpoint lưu tự động tại **đầu mỗi chương**, gồm máu, đạn, vật phẩm và tiến độ. Thử lại khôi phục toàn bộ encounter của chương đó. Menu không tự ghi đè checkpoint bằng trạng thái đang giao chiến. Bắt đầu chiến dịch mới thay checkpoint hiện tại. Save và cài đặt nằm trong `user://` của Godot, không nằm trong thư mục source.

## Build và kiểm thử

```bash
./tools/test.sh
./tools/build.sh
```

Script build dùng Godot, template export, Android SDK và debug keystore đã có trong `.tools/`. Nếu chuyển sang máy khác, cần cài lại các công cụ này hoặc cấu hình đường dẫn tương ứng trong Godot Editor Settings; `.tools/` không nằm trong gói source.

`builds/` chứa APK, bản Linux, checksum SHA-256 và log. Script build còn sửa authority của AndroidX Startup trong manifest do bộ export hiện có gán trùng, rồi align và ký lại APK. Script test dùng thư mục save riêng `.tools/automated-tests` để không chạm vào save người chơi. Các bài test headless tắt âm thanh và chạy fixed-step; kiểm tra hình ảnh/âm thanh thực hiện riêng bằng renderer OpenGL.

Đọc `docs/VALIDATION.md` để biết phạm vi kiểm thử và `assets/CREDITS.md` để biết nguồn asset.

## Tổ chức source

- `scripts/game/`: campaign, save, chapter, dựng môi trường, tương tác, âm thanh.
- `scripts/player/`: di chuyển/camera, súng hitscan, rig và animation nhân vật.
- `scripts/ai/`: patrol, investigate, chase, attack, search; raycast và navigation.
- `scripts/security/`: camera, cửa vật lý, ánh sáng, báo động, EMP.
- `scripts/ui/`: menu, HUD tiếng Việt, multi-touch, Tablet, các ending.
- `assets/`: model, animation, font, texture và âm thanh được đóng gói offline.
- `tests/`: campaign, mechanics/traversal và layout.

## Giới hạn của bản đầu

Tuyến truyện và các hệ thống cốt lõi đã triển khai, nhưng chưa phải bản đã nghiệm thu trên Android thật. Thời lượng 20–30 phút trong GDD là mục tiêu, chưa được đo bằng playtest. Các chương ngầm dùng chung bộ phòng module với bố trí đạo cụ khác nhau. Cảnh kể chuyện dùng chữ và âm thanh tổng hợp, chưa có lồng tiếng hay cinematic diễn hoạt. Nhân vật dùng animation idle/run có sẵn kết hợp pose ngắm; chưa có bộ animation riêng cho mọi hành động. Các mục này là phần cần tiếp tục polish sau phản hồi chơi thử.
