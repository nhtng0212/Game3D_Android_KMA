# Phòng, cửa và khoảng hở tuần tra — 04/10/2026

## Sửa rơi khỏi tầng cửa hàng

`ShopUpgrade` dọn đồ cũ ở góc phải phía sau để xây WC/phòng điện. Bộ lọc cũ vô tình chọn cả `Floor tile` vì chỉ kiểm tra vị trí và độ cao. Bộ lọc mới giữ lại sàn. Kiểm tra editor quét 3.905 điểm trên toàn mặt sàn cửa hàng; runtime kiểm tra CharacterController đứng thật tại năm vị trí, gồm góc phải sau. Cơ chế hồi về spawn nếu rơi vẫn là phương án dự phòng.

## Bố cục và cửa

- Cửa hàng: chia khu bán hàng bằng các vách, giữ hành lang giữa; các gian audio/máy tính, văn phòng Marcus, WC và phòng điện có cửa. Thêm kệ đầy hàng, bàn làm việc, tủ và thùng trong kho/hành lang.
- B1: phòng nghỉ, lối SERVICE sau khu WC, cửa phụ Workshop và vật che ở đường rút.
- B2: chia khu quân nhu và hàng trả, thêm tủ/kho; cửa phòng có thể tương tác.
- B3: chia khu kiểm tra thiết bị và phòng thẩm vấn; thêm tủ, thùng và cửa phòng.
- B4: thêm vách chia khu dược/y tế và cửa cách ly.
- B5: chia phòng backup, mạng, WC và bổ sung vật che gần Uplink.
- B6: cửa phòng AK, kính đêm và hồ sơ Keeper; giữ tám phòng chức năng.

Tổng cộng 15 cửa phòng tương tác: cửa hàng 6; B1 1; B2 2; B3 1; B4 1; B5 1; B6 3.

Đứng gần cửa và nhấn **E**, hoặc nút tương tác trên điện thoại. Cửa trượt vào vách; đóng lại sẽ chặn người, đạn và tầm nhìn. Cửa không đóng khi người chơi/NPC nằm trong vùng quét của cánh. NavMesh cập nhật theo cánh cửa. Lính có thể mở cửa khi tới gần, nên đóng cửa không phải cách khóa AI vĩnh viễn. Trạng thái cửa được đặt lại khi tải checkpoint.

## Cách tiếp cận B1

Bốn lính vẫn hoạt động; hai lính phía trái tuần tra kỹ thuật/máy phát, một lính kiểm tra WC, một lính đi giữa Workshop và cuối hành lang. Một số điểm kiểm tra có khoảng dừng rõ ràng: Điểm chờ chính B1 là 9 giây; các tầng sau là 5 giây. Lính WC dừng thêm 3 giây và quay về phía tủ trước khi đổi chiều, tránh quét đèn ngang lối SERVICE ngay lúc quay đầu. Khi nghỉ tuần tra, lính vẫn quan sát và phát hiện Alex bình thường.

1. Xuống cầu thang, vòng qua đầu trái dãy kệ rồi đi khom men tường bên phải tới lối SERVICE sau khu WC.
2. Chờ sau vách ở cửa phụ Workshop. Lính Workshop đi ra phía cuối hành lang, cách radio khoảng 6 m, và dừng kiểm tra với đèn quay ra xa.
3. Khi đèn quay đi, đi khom qua cửa phụ tới bàn radio và nhấn E.
4. Rút lại cửa phụ, vòng sau thùng hàng bên phải rồi đi tới máy quét cuối tầng. Không chạy thẳng ngược hướng lính đang bị radio thu hút.

HUD và nhật ký J hiển thị cách tiếp cận theo tầng/mục tiêu. Ở B1, chỉ dẫn đổi theo việc người chơi đã tới khu chờ, lính Workshop đã dừng ở cuối hành lang, và radio đã bật. Gợi ý tiến tới radio chỉ xuất hiện khi khoảng dừng còn ít nhất 5 giây, kèm số giây còn lại; nếu tới muộn thì chờ lượt sau. Đây là chỉ dẫn quan sát; nếu đã gây tiếng động hoặc bị phát hiện thì phải nấp và chờ lính trở về tuần tra.

Tường/cửa làm giảm phạm vi NPC nghe tiếng động. Lính trong phòng khác không tự nghe một radio nhỏ rõ như khi không có vật chắn.

B3/B4 dùng phòng, giường và vách để đi từ chỗ núp này sang chỗ núp khác; quan sát camera trước khi cắt ngang hành lang. B5 là encounter giữ Uplink sau khi chủ động kích hoạt, còn Victor là trận chiến, không áp dụng lời hứa có thể đi qua tất cả NPC mà không giao tranh.
