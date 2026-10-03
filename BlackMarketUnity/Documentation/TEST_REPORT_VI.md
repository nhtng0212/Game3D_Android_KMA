# Kiểm tra cửa hàng và stealth — 03/10/2026

Unity 6000.3.25f1, URP, Linux x86_64/OpenGL Core. Build: Succeeded / 0 errors.

- **70 kiểm tra PASS, 0 FAIL**: runtime-test.txt.
- Tuần tra không cầm súng; vùng đèn nhìn thấy người chơi; tường chặn phát hiện; đứng lộ đầu/khom được che; tư thế khom thực sự hạ đầu xuống sau vật che (đo sau LateUpdate); nghi ngờ trước khi xác nhận; thời gian rút súng trước phát đầu; bắn gây sát thương; mất dấu/tìm kiếm; nhớ vị trí cuối; không bắn xuyên tường; hết thời gian tìm kiếm quay về tuần tra; nghe tiếng động đi điều tra.
- Kiểm tra lại đường đi bằng CharacterController, tiến trình truyện, súng/reload, Tablet/cửa/EMP, Victor, hai kết thúc và checkpoint.
- Asset/NavMesh: cả ba môi trường, các điểm nhiệm vụ và toàn bộ điểm tuần tra đều đạt; kích thước model nhập khớp collider. Xem validation.txt.
- Không có exception runtime hoặc lỗi thiếu animation trong runtime-test.log.
- **115 tệp Godot giữ nguyên**, đối chiếu SHA256.

## Phạm vi

Kiểm thử tự động chạy trong bản Linux, dùng save riêng và gọi một số tương tác truyện trực tiếp. Các kiểm tra AI sử dụng NPC chạy thật theo thời gian, vật che thử nghiệm và NavMeshObstacle. Đây không phải một lượt chơi tay hoàn chỉnh để đánh giá cân bằng. Chưa đo FPS/cảm ứng trên điện thoại Android thật.

Cửa hàng có 13 bộ kệ thép mở, 202 carton, 10 bàn sửa chữa có chi tiết, thêm TV/radio/tủ dụng cụ/thùng gỗ. Đồ vật nhỏ được tạo trong dự án kết hợp model tải từ Poly Haven; không phải tất cả là model quét 3D.

Ảnh bố cục: Previews/shop-floorplan.png. Ảnh đèn pin và núp: Playtest/flashlight-patrol.png, Playtest/crouch-cover.png. Hướng dẫn cơ chế/giới hạn: SHOP_STEALTH_VI.md.
