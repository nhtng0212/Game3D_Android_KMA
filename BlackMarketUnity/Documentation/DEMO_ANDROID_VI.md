# Demo và điều khiển Android

## Camera

Menu ☰ → CÀI ĐẶT → Tốc độ xoay: 0,4×–5× → LƯU & QUAY LẠI. Máy chưa lưu tùy chọn dùng mặc định 1,8×; máy đã chỉnh giữ nguyên lựa chọn cũ.

Mỗi ngón có vùng điều khiển độc lập: joystick, nhìn, bắn, chạy, ngắm. Nếu ngón nhìn đang hoạt động, ngón bắn chỉ bắn để không cộng hai chuyển động camera. Nếu không có ngón nhìn riêng, có thể kéo ngay trên nút bắn để nhìn. Việc tìm vật tương tác dùng danh sách được làm mới định kỳ, tránh quét và cấp phát toàn bộ danh sách hai lần mỗi khung hình.

## Demo

Ở màn hình chính hoặc menu tạm dừng chọn DEMO / CHỌN PHÂN ĐOẠN:

1. Cửa hàng / tìm USB.
2. Máy tính / đọc Order 71.
3. Nhóm áo đen tới.
4. Kho vũ khí.
5. Chiến đấu 30 tên.
6. Nghỉ nhặt chiến lợi phẩm.
7. Victor tiếp viện.
8. Trung tâm điều phối.
9. Trận cuối / kính đêm.
10. Giải mã và kết thúc.

Demo nạp trạng thái và trang bị theo mốc, dùng north-point-demo.json và lịch sử đọc riêng. Về màn hình chính hoặc bắt đầu chiến dịch mới sẽ thoát chế độ demo; không ghi đè điểm lưu chiến dịch chính.

## Build và kiểm tra

Menu build Android hiện xuất APK không bật Development Build; runtime debug UI đã tắt. Log lỗi khởi động vẫn được ghi để chẩn đoán, không che màn hình chơi.

14 kiểm tra Unity đạt: nạp đủ 10 mốc, đồng thời 3 ngón di chuyển/nhìn/bắn, ngắm giữ trạng thái sau khi thả, không kẹt điều khiển khi thả tay, thoát demo. Đây là kiểm tra xử lý input bằng code; chưa phải kiểm tra ba ngón vật lý trên thiết bị.

Nguyên nhân có thể gây menu debug khi chơi đa chạm: URP DebugManager.GetActionToggleDebugMenuWithTouch() có cử chỉ ba ngón chạm đúp. Đã đặt DebugManager.instance.enableRuntimeUI=false và build không Development để không kích hoạt menu này trong game.

Bản release đã build thành công và cài đè lên Xiaomi/Redmi Android 16 qua ADB. Ảnh AndroidDebug/demo-release-menu.png xác nhận menu có nút demo, không có watermark Development Build; log khởi động không có exception/Render Graph error. Kiểm tra thao tác nhiều ngón vật lý vẫn cần người chơi thực hiện trên máy.
