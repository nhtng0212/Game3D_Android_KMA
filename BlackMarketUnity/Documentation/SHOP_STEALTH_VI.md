# Cửa hàng, tầng kỹ thuật và stealth

Cửa hàng tầng 1 có mặt tiền kính, văn phòng Marcus, quầy/kệ điện tử, xưởng sửa chữa, kho, phòng hồ sơ, WC và phòng điện. Alex xuất hiện bên trong cửa hàng; kính và cửa trước chặn việc đi ra ngoài.

Chương Visitors Arrive nay dùng **bản đồ B1 / UtilityBasement riêng**, không tải lại cửa hàng. SUV chặn cửa trước; Alex qua Door 06 xuống khu kỹ thuật. Bốn lính tuần tra quanh máy phát, phòng điện và xưởng radio. Bật radio để kéo chúng khỏi cầu thang xuống B2.

Các tầng sau cũng có bố cục riêng: xem [MAPS_VI.md](MAPS_VI.md).

## Vật che và tuần tra

- Kệ mở có va chạm theo khung, tầng và hàng hóa; tầm nhìn có thể xuyên khe trống.
- Vật che thấp có thể che Alex khi khom. Collider khom cao 1,25 m, rig hạ xương chậu và giải chân bằng IK; kiểm tra chỗ trống trước khi đứng lên.
- Đèn pin tuần tra: góc 48°, tầm 14 m, cường độ 24, có bóng và chùm sáng trong không khí. Chùm sáng cắt theo vật cản; AI kiểm tra thân/đầu từ nguồn đèn. Ở rất gần vẫn có thể nhận biết ngoài góc đèn.
- Thấy thoáng qua tăng nghi ngờ, khom làm chậm xác nhận. Xác nhận xong lính rút súng trong khoảng 0,7 giây trước khi bắn.
- Khi cầm súng, tầm/góc nhìn chuyển sang 18 m / nửa góc 48°; chùm đèn tuần tra không còn hiển thị.
- Mất dấu, lính đi tới vị trí nhìn thấy cuối rồi tìm kiếm. Khoảng 12 giây không thấy lại thì cất súng và trở về tuần tra. Tiếng động thu hút lính chưa báo động đi điều tra.
- Đèn phòng không điều khiển đèn pin NPC. Tường và đồ vật có collider chặn đạn; chưa có xuyên đạn/phá hủy vật che.

## Âm thanh và súng

Bước chân, bắn, nạp/rút súng có âm thanh; vật cản làm âm thanh nhỏ và bớt âm cao. Nhạc drone/pulse tổng hợp tăng theo nghi ngờ và các tình huống Uplink/Victor. Alex giữ súng hạ thấp khi bình thường, nâng khi ngắm/bắn rồi hạ lại. Chuyển tư thế bằng code, chưa phải clip mocap riêng.

## Kiểm chứng

Kiểm thử runtime có bộ cảnh thử riêng cho tầm nhìn, vật che, rút súng, bắn và tìm kiếm; kiểm thử traversal dùng CharacterController qua cầu thang và tới mục tiêu của từng tầng. Xem `runtime-test.txt`, `validation.txt` và `TEST_REPORT_VI.md`. Các kiểm tra này không thay thế chơi tay để đánh giá độ khó và hiệu năng Android.
