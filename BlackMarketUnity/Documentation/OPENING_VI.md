# Mở đầu cửa hàng và hướng dẫn chơi

Chiến dịch hiện bắt đầu ở phòng ngủ Alex với cuộc gọi phân nhánh, sau đó là cảnh đi xe máy tới cửa hàng. Xem [cây hội thoại và cách chỉnh hoạt cảnh](PROLOGUE_DIALOGUE_VI.md). Các bước dưới đây áp dụng khi Alex đã tự bước vào cửa hàng và trả quyền điều khiển.

## Thẻ và máy tính là hai bước khác nhau

Thẻ đỏ được giấu trong một ngăn tủ tại phòng Marcus. Đây là chìa khóa xác thực; tài liệu cần đọc nằm trong máy tính. Alex dùng thẻ để mở máy tính của chú. Tệp **Order 71** trên máy tính giải thích cái chết bị dàn dựng của Marcus, nhóm truy bắt của Victor và đường thoát qua Cửa 006. Bản gốc bằng chứng cùng bản ghi GỬI ALEX nằm ở B2.

1. Đi tới cuối cửa hàng, vào văn phòng Marcus bên trái.
2. Đến gần cửa, nhấn **E** để mở. Đến **TỦ CÁ NHÂN / MARCUS**, dùng **E** kiểm tra các ngăn của sáu cụm tủ sát tường cho tới khi thấy thẻ. Khi thấy thẻ đỏ, nhấn **E** để nhặt riêng.
3. Đến gần **MÁY TÍNH MARCUS / HỒ SƠ 071**, nhấn **E**.
4. Màn hình máy tính hiện lớn với biểu tượng tệp **Order 71**. Bấm tệp để mở; mở máy tính thôi chưa kích hoạt cảnh xe đến. Chữ hiện dần; đọc xong một trang rồi bấm **ĐỌC TRANG TIẾP**. Trang cuối có nút **ĐÓNG HỒ SƠ / RỜI MÁY TÍNH**.
5. Hai xe đến, nhóm người mặc vest bước ra và tiến vào cửa hàng. Trong cảnh này không điều khiển Alex.
6. Khi camera về lại Alex, giữ **Shift** chạy ra hành lang giữa rồi vào **KHO HÀNG** phía sau. Nhấn **E** mở cửa kho.
7. Đi vòng các kệ đặt lệch nhau; **CỬA 006** nằm cuối kho. Dùng thẻ đỏ mở cửa ngoài bằng **E**, đi vào khoang, đứng trên dấu chân và **E** quét mặt tại cửa trong. Cửa ngoài khép lại, máy quét xác nhận Alex và mở cửa trong. Sau khi cửa trong mở, Alex tự chạy xuống cầu thang; người chơi không điều khiển trong đoạn này. Chạy hết cầu thang mới tải B1.

Trên điện thoại: dùng cần trái để di chuyển, vuốt vùng trống bên phải để nhìn, chạm **DÙNG** thay phím E và giữ **CHẠY** thay Shift.

## Đọc lần đầu và chơi lại

Lần đầu, chữ hiện dần. Đúng 3 giây sau ký tự cuối, nút tiếp tục sáng lên; không có thêm bộ đếm thời gian đọc theo độ dài nội dung. Quy tắc này áp dụng cả cốt truyện thông thường lẫn từng trang Order 71. Không cần chạy đua với chữ: trang không tự chuyển. Trò chơi không thể xác nhận bạn đã đọc thật, chỉ kiểm tra thời gian hiển thị và việc đi hết nội dung.

Chỉ khi hoàn tất toàn bộ tài liệu/cảnh mới ghi nhận đã xem. Khi chết hoặc bắt đầu lại, nút **BỎ QUA HỒ SƠ ĐÃ ĐỌC** và **BỎ QUA CẢNH ĐÃ XEM** được mở cho nội dung đã hoàn tất. Bỏ qua cảnh vẫn tạo đủ nhóm truy bắt và nhiệm vụ chạy trốn. Dữ liệu đã đọc nằm riêng trong tệp `.reading` cạnh tệp lưu chiến dịch; không bị xóa khi bắt đầu lại.

## Khi bị phát hiện

Nhóm người mặc vest nhìn theo hướng quay mặt; tường và vật che kín cản tầm nhìn. Khi xác định được Alex, chúng đuổi theo. Nếu bị áp sát, Alex bị bắt và lượt chơi kết thúc. Hãy chạy, rẽ sau vật che và cắt đường nhìn; đừng đứng ở cửa chờ chúng tới. Những kệ thấp/hở không che được toàn bộ người.

Điểm lưu ở đầu tầng. Chọn **CHƠI LẠI ĐIỂM LƯU** sau khi chết; phần đã đọc/xem vẫn được bỏ qua.

## Điều khiển còn lại

- **W A S D**: di chuyển; chuột: xoay góc nhìn; **C**: đi khom.
- **J**: nhật ký và mục tiêu; **Esc**: tạm dừng. Màn hình chính có **ĐIỀU KHIỂN**.
- Khi có súng: chuột phải ngắm, chuột trái bắn, **R** nạp đạn, **Q** đổi súng.
- Sau khi nhận máy tính bảng ở B2: **Tab** mở bảng an ninh. Bảng này **không tạm dừng** trò chơi; nấp trước khi mở.
- Khi đã nhặt kính nhìn đêm: **N** bật/tắt kính.

## Chỉnh sửa trong Unity

Mở scene `Assets/BlackMarket/Scenes/NorthPoint.unity` rồi Play để thử cốt truyện. Chọn **BẮT ĐẦU CHIẾN DỊCH** để xem từ phòng ngủ Alex; chọn **TIẾP TỤC ĐIỂM LƯU** để dùng điểm lưu đã có.

Map được cập nhật trên prefab đã lưu, không dựng lại toàn bộ bố cục. Nhóm `10 - Kho hẹp và sân ngoài` chứa các kệ mới và phần mặt đường. Các tầng hầm chỉ đổi nhãn hiển thị; không dựng lại hình học của chúng. Sau khi di chuyển vật cản trong tầng 1, lưu prefab và dùng `BLACK MARKET → Shop → 4` để cập nhật đường đi.

Các thuật ngữ giao diện, nhiệm vụ, tài liệu và biển chỉ dẫn được Việt hóa. Tên riêng Alex, Marcus, Victor, North Point và mã số/phím điều khiển được giữ. Chữ trang trí gắn trong texture model tải ngoài không phải nội dung hướng dẫn.

## Chỉnh các vật mới

Nhóm `11 - Thẻ đỏ và Cửa 006` chứa tủ cá nhân và khoang xác thực. Các ngăn có `OfficeDrawer`; thẻ đỏ là vật tương tác riêng nằm trong một ngăn. `BasementAirlock` điều khiển hai cánh cửa, vệt quét và chuyển tầng. Giữ các tham chiếu này khi chỉnh map.

## Kiểm tra bản cập nhật

- [Kiểm tra luồng chơi](revision-runtime-test.txt): nhịp chữ + 3 giây, 24 ngăn tủ, thẻ, máy tính, kho 36 giá, đường vòng khoảng 146 m, quét mặt, tự xuống cầu thang và chơi lại.
- [Kiểm tra giao diện thực tế](revision-ui-test.txt), đã xem ảnh: [văn phòng](RevisionPreview/01-office.png), [xe tới](RevisionPreview/04-car-arrival.png), [quét mặt](RevisionPreview/07-face-scan.png), [tự chạy xuống cầu thang](RevisionPreview/08-auto-stairs.png).
- Chưa xuất APK/EXE và chưa đo hiệu năng trên điện thoại Android thật. Các báo cáo `opening-*` và `mission-*` trước đây ghi nhận phiên bản cũ.

## Bố trí mở rộng

Nhóm `12 - Mở rộng cửa hàng` chứa sáu cụm tủ (24 ngăn có thể mở), sáu giá sách sát tường, kho 28 × 18 m với 36 giá hàng và cảnh phố. Kho cũ 28 × 6 m, có 9 giá; diện tích tăng đúng 3 lần, số giá tăng 4 lần. Lối chạy uốn quanh sáu dãy, không đi thẳng xuyên kệ. Bàn máy tính, cửa văn phòng và các khu bán hàng/nhà vệ sinh được giữ.

Alex dùng model nam trẻ mới. Đoàn xe dùng sedan đen mới của MrJaneLAB. Biển và phụ đề gọi nhóm truy bắt là **Nhóm người mặc vest**. Các vật cảnh phố nằm ngoài đường đi chính.

Kiểm tra cập nhật: [chạy thử luồng mới](revision-runtime-test.txt). Các báo cáo trước đó ghi nhận phiên bản trước khi mở rộng kho và thêm đoạn tự xuống cầu thang.
