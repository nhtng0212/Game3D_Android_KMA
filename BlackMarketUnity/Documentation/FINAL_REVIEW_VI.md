# Rà soát chiến dịch Order 71

## Mạch truyện thống nhất

1. Marcus gọi Alex, căn dặn tìm USB đỏ; cuộc gọi bị ngắt. Sau tin Marcus chết, Alex tới North Point.
2. Alex tìm USB trong ngăn tủ, dùng nó mở máy tính Marcus. Order 71 hé lộ vụ thanh trừng, phần phụ lục cần mã 10 chữ số.
3. Nhóm truy bắt nhận ra xe Alex. Alex thoát qua cửa 006: USB xác thực cửa ngoài, khuôn mặt xác thực quyền kế thừa ở cửa trong.
4. Ở kho quân giới, Alex lấy AK, M700 và bộ lựu đạn/đạn/kính nhìn đêm. Đăng nhập tài khoản Marcus đã bị đánh dấu chết gây báo động.
5. Nhóm đầu phá cửa, ném lựu đạn rồi tiến theo từng toán. Sau 15 đối thủ là 45 giây tiếp tế; tiếp đó Victor và 10 thuộc hạ áp chế, buộc Alex rút xuống.
6. Alex băng bó tạm khi xuống trung tâm, tới màn hình chính. Victor đuổi kịp. Phá tủ điện chỉ ngắt chiếu sáng, máy chủ vẫn dùng nguồn riêng. Victor cũng có kính đêm.
7. Hạ đối thủ, dùng USB tại máy chủ lấy mã và giải mã phụ lục. Khóa an toàn của Marcus giữ lệnh tự hủy trong phiên giải mã; gửi bằng chứng xong mới bắt đầu 90 giây thoát hiểm.
8. Alex ra lối bảo trì, lên xe máy rời đi; cơ sở nổ. Bằng chứng đã được gửi ra ngoài.

## Chỉnh trong lượt rà soát này

- Bỏ tiết lộ vị trí mã khỏi lời giới thiệu đầu game.
- Viết lại lời Marcus về ngăn tủ cho tự nhiên hơn; bổ sung kính nhìn đêm trong hướng dẫn nhận trang bị.
- Giải thích thời điểm bắt đầu tự hủy, thống nhất với bộ đếm thực tế.
- Sửa câu Victor để rõ chỉ hắn có kính đêm; thuộc hạ không được ngầm hiểu là cũng có kính.
- Thông báo tới tầng cuối xác nhận băng bó và bổ sung đạn; không gọi nhầm tầng đó là B3.
- Kéo dài đoạn thoại Alex/Victor tại trung tâm; phụ đề tự tính chiều cao, thông báo dài có thêm thời gian đọc. Tạm dừng không làm mất thời gian phụ đề.
- Hướng dẫn cảm ứng dùng tên nút thay cho phím chuột/bàn phím; hướng dẫn nghỉ tiếp tế và tránh vòng lựu đạn rõ hơn.
- Mở rộng kiểm tra màn 3 qua giải mã, đọc hết hồ sơ, đếm ngược, hoạt cảnh xe máy/vụ nổ và lưu hoàn thành.
- Kiểm tra các trang hồ sơ/câu thoại điện thoại bằng kích thước font và khung chữ thực tế.

## Lồng tiếng

Chưa thêm bộ lồng tiếng mới. Giọng tổng hợp tạm `hunt_shout` vẫn tắt theo phản hồi trước đó. Dự án chưa có bản thu diễn xuất tiếng Việt để đánh giá chất lượng. Phụ đề luôn là nội dung chính, không phụ thuộc âm thanh. Không phát sinh phí dịch vụ.

## Giới hạn kiểm chứng

Kiểm tra chạy trong Unity Editor ở chế độ batch có đồ họa. Ảnh camera dùng để xem model, bố trí và tư thế; chúng không chứa lớp IMGUI. Kiểm tra kích thước chữ không thay thế trải nghiệm đọc và thao tác trên điện thoại thật. Chưa đo FPS, nhiệt độ, độ rõ âm thanh trên loa điện thoại hay xác nhận mọi góc camera/va chạm trong mọi tình huống chơi.

## Kết quả chạy cuối

- Mở đầu: 48/48 kiểm tra đạt (`intro-runtime-test.txt`).
- Màn 2 và chuyển tầng: 74/74 kiểm tra đạt (`armory-revision-test.txt`).
- Màn 3 tới kết truyện: 26/26 kiểm tra đạt (`control-revision-test.txt`).
- Tổng cộng 148 kiểm tra runtime đạt, không có kiểm tra thất bại trong lượt chạy cuối.
- Quét 26 prefab trong Worlds/Actors/Opening: 20.319 renderer và 19.507 mesh, không phát hiện tham chiếu script, mesh, vật liệu hay shader bị thiếu (`campaign-asset-review.txt`). Bao gồm cả prefab cũ còn được giữ trong Resources.
- Kiểm tra M700 đồng bộ transform vật lý của mục tiêu được tạo bằng mã trước khi bắn. Kiểm tra dọn cảnh đợi Unity hoàn tất Destroy ở cuối khung hình. Kiểm tra mở đầu đếm đúng 10 người truy bắt theo hiện trạng chiến dịch.
- Các lượt này dùng dữ liệu lưu thử nghiệm riêng; không dùng tệp tiến trình của người chơi.
