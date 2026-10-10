# Điều chỉnh 30 địch, cận vệ Victor và cảnh kết

- Màn 2: 30 đối thủ thường, 6 nhóm × 5. Nhóm kế tiếp chỉ bắt đầu xuống khi cả nhóm trước bị hạ; không bù từng người vô hạn.
- Sau địch thứ 30: 45 giây nhặt chiến lợi phẩm cạnh xác địch. Mỗi điểm cho 18 máu, 30 đạn AK, 4 đạn súng ngắm và 1 lựu đạn, có giới hạn kho. Danh sách vị trí đồ rơi lưu cùng checkpoint; đồ đã nhặt được loại khỏi danh sách. Các điểm tiếp tế cố định không dùng trong lượt nghỉ này.
- Victor vào màn 2 sau 20 cận vệ. Cận vệ có 140 HP thay vì 80, độ chính xác cao hơn. Trận cuối tiếp nối tối đa 20 cận vệ trừ những người đã bị hạ ở màn 2.
- Xác suất trúng khi đứng yên tăng; chạy giảm khoảng 55% xác suất trước các hệ số khoảng cách/tư thế. Vật che vẫn chặn đạn. Giới hạn sát thương đạn đồng thời: tối đa 6 HP mỗi 0,35 giây.
- Trong pha Victor áp chế, dưới 50 HP kích hoạt rút lui tự động. Hạ 10 cận vệ làm tăng thêm độ chính xác ×2,5 (chặn ở 100%). Sau 10 giây áp chế tăng cường, nếu còn ít nhất 50 HP, một vụ nổ theo kịch bản đưa Alex về 49 HP và bắt đầu rút lui. Đây là cơ chế bảo đảm tiến trình, không phải đạn bắn xuyên vật che.
- Lựu đạn chiến thuật: khoảng cách tối thiểu giữa các lượt ném toàn đội giảm từ 8 xuống 2,6 giây; Victor từ 6 xuống 1,8 giây. Hồi chiêu riêng từng người 5 giây, Victor 2,2 giây. Tối đa 4 quả chiến thuật đang hoạt động. Vẫn có vòng cảnh báo và ngòi 3,8 giây.
- Máu Victor màn cuối: 1.350; nếu đã bị hạ gục/tổn thương ở màn 2: 990. Đều bằng 3 lần mức cũ tương ứng.
- Tủ điện áp sát tường sau, bên trái màn hình chính. Vị trí tương tác và vùng đi bộ được cập nhật.
- Đọc và gửi hồ sơ xong: người chơi tự điều khiển quay về cầu thang đầu màn. Vào vùng cách điểm thoát dưới 3 m tự bắt đầu hoạt cảnh, màn hình tối dần trong 2 giây rồi hiện cảnh bên ngoài.
- Cảnh kết bổ sung 28 nhà phố có cửa/cửa sổ/mái hiên, đường kéo dài, vỉa hè, vạch đường, 14 đèn đường và 6 xe đỗ. Không sử dụng asset trả phí.

Kiểm tra tự động cập nhật trong `ArmoryRevisionSelfTest` và `ControlRevisionSelfTest`; kết quả ở `armory-revision-test.txt` và `control-revision-test.txt`. Chưa đo FPS trên điện thoại thật với lượng cận vệ tăng thêm.

Bổ sung kiểm tra lưu: checkpoint nghỉ số 6 được bộ đọc save chấp nhận. Tải lại phục hồi các vị trí chiến lợi phẩm chưa nhặt cùng thi thể; không hồi lại đồ đã lấy. Save cũ chưa ghi vị trí chiến lợi phẩm được chuyển đổi sang một số điểm đồ rơi trong khu giao tranh. Giới hạn lưu đạn được đồng bộ với kho mới (450 AK, 80 súng ngắm, 12 lựu đạn).

Kết quả lượt cuối: màn 2 **84/84**, màn 3 **30/30**, kiểm tra riêng cảnh kết sau chỉnh góc máy **5/5**. Tổng **119 kiểm tra đạt**. Ảnh kiểm tra cảnh kết: `ending-explosion-neighborhood.png`. Scene được để lại là `Assets/BlackMarket/Scenes/NorthPoint.unity`.
