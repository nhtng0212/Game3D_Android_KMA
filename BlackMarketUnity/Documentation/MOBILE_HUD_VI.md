# Giao diện cảm ứng

Bật BLACK MARKET → Preview Android - Nút cảm ứng rồi Play trong Simulator hoặc Game View.

- Joystick tròn bên trái: giữ và kéo để di chuyển.
- Vuốt vùng trống bên phải để xoay camera.
- Các nút tròn bên phải: DÙNG, CHẠY, KHOM. Khi có vũ khí sẽ hiện BẮN, NGẮM, NẠP và LỰU ĐẠN. Kính đêm chỉ hiện sau khi nhặt.
- Hai ô AK và SÚNG NGẮM ở giữa mép dưới chọn trực tiếp vũ khí đã nhặt, hiển thị đạn trong băng/đạn dự trữ. Vũ khí đang chọn có viền vàng.
- Nút ba gạch góc trên phải mở menu tạm dừng: tiếp tục, nhật ký/nhiệm vụ, hướng dẫn, cài đặt, chơi lại, màn hình chính.

Nút hành động có vùng bấm hình tròn. Không dùng các góc ngoài hình tròn làm vùng bấm. Preview bằng chuột hỗ trợ một ngón; kiểm tra đồng thời di chuyển/ngắm/bắn trên điện thoại thật.

Kiểm tra tĩnh: 13 vùng điều khiển nằm trong khung 1280×720, không chồng lấn. Chưa xác nhận hình ảnh và thao tác trong phiên Unity đang mở của người dùng.

## Asset và cách preview cập nhật

Đã nhập 10 biểu tượng PNG từ Game-icons.net (Lorc, Delapouite, Sbed, Skoll; CC BY 3.0) vào Resources/UI/Mobile. Bản SVG gốc, danh sách nguồn và giấy phép ở Documentation/AssetSources/MobileHUD. Trong game: menu tạm dừng → NGUỒN BIỂU TƯỢNG.

Tham khảo bố cục: https://blog.activision.com/es/call-of-duty/2019-10/Getting-a-Grip-on-the-Call-of-Duty-Mobile-Controls (joystick trái, vùng nhìn và nút bắn phải, đổi vũ khí).

CHẠY là nút bật/tắt tốc độ chạy, không tự di chuyển. Chạm rồi kéo joystick; chạm lại để tắt. Vào menu, mất tiêu điểm hoặc khom sẽ hủy trạng thái chạy. Khi bắn có thể kéo ngón bắn để chỉnh hướng camera.

Simulator: giữ chuột trái và kéo vùng trống bên phải để xoay góc nhìn. Game View: có thể dùng WASD kết hợp giữ chuột phải kéo để nhìn; con trỏ vẫn mở để bấm các nút. Chuột đơn không thay thế kiểm tra đa chạm trên điện thoại.

Đã kiểm tra hình ảnh bộ icon và biên dịch C# bằng compiler/reference của Unity hiện tại: không lỗi (còn cảnh báo PlayerMotor.camera che thuộc tính kế thừa). Chưa chạy lại trên điện thoại hoặc xác nhận toàn bộ HUD trong phiên Unity đang mở.

## Điều chỉnh bố cục tiếp theo

- Bỏ dòng “01—03 / MỘT NGƯỜI CHƠI / KHÔNG CẦN MẠNG”.
- Đường kính nút bắn tăng 15% (112 → 128,8).
- DÙNG chỉ hiện khi mục tiêu tương tác còn hợp lệ; nằm trên ô AK. Kính đêm nằm trên ô súng ngắm.
- CHẠY ở phía trên joystick. Kéo ngón đang điều khiển lên cao qua đầu joystick để bật chạy; giữ ngón để di chuyển, chạm CHẠY để tắt. Thả joystick dừng di chuyển.
- NGẮM ở phía trên nút bắn; chạm một lần bật, lần nữa tắt. Chạy nhanh hủy ngắm; bật ngắm hủy chạy. Vào menu/mất tiêu điểm xóa trạng thái giữ.
- C# biên dịch thành công, kiểm tra tĩnh kích thước và các vùng bấm không chồng lấn. Chưa xác nhận thao tác trên điện thoại thật.

## Giữ chạy và tương tác gần

Cập nhật mới thay cơ chế bật/tắt chạy: nút CHẠY được nâng thêm 50 đơn vị, chỉ chạy nhanh trong lúc giữ nút hoặc giữ joystick kéo lên vùng CHẠY. Thả hoặc kéo ngón ra khỏi vùng chạy sẽ hết chạy nhanh. NGẮM vẫn chạm bật/tắt.

DÙNG và dòng gợi ý cùng kiểm tra mục tiêu hợp lệ, khoảng cách tương tác trên giao diện cảm ứng tối đa 2 m và đường nhìn không bị che. Làm mới mục tiêu trước khi xử lý input để không dùng tham chiếu của khung hình trước. C# đã biên dịch; chưa xác nhận trực tiếp trong phiên Unity/Android.
