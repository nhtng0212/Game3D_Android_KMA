# Mở đầu màn 1 — cuộc gọi cuối cùng

Mở `Assets/BlackMarket/Scenes/NorthPoint.unity`, bấm Play rồi **BẮT ĐẦU CHIẾN DỊCH**. Điểm lưu đã hoàn tất phần mở đầu sẽ đi thẳng vào cửa hàng khi chọn **TIẾP TỤC ĐIỂM LƯU**.

## Luồng hoạt cảnh

1. 02:17, Alex ngồi ở bàn máy tính trong phòng ngủ. Phòng có giường, giá sách, bàn, ghế, cửa sổ và cửa phòng.
2. Điện thoại reo, Alex đưa điện thoại lên tai. Marcus chỉ nói hai câu, không có lựa chọn trả lời:
   - “Alex, nghe chú… đừng tin bất kỳ ai biết đến cái tên North Point.”
   - “Và dù có chuyện gì xảy ra, cũng đừng mở Cửa 006!”
3. Mỗi câu chạy chữ với tốc độ 42 ký tự/giây. Sau ký tự cuối cùng 3 giây, hoạt cảnh tự tiếp tục. Tạm dừng sẽ dừng cả thời gian đọc.
4. Có tiếng va đập rồi cuộc gọi mất liên lạc. Bảng chuyển cảnh giải thích: sáng hôm sau Alex nhận tin Marcus chết trong một vụ tai nạn; ba ngày sau cuộc gọi, Alex tới cửa hàng vì không tin lời giải thích ấy.
5. Alex đi xe máy tới, xuống xe và tự đi tới gần cửa chính. Camera cắt vào bên trong cửa hàng, sau đó trả quyền điều khiển.
6. Nhiệm vụ tiếp tục: kiểm tra máy tính bị khóa → tìm USB đỏ → máy tính → Order 71 → nhóm người mặc vest → Cửa 006 → quét mặt → xuống hầm.

## Dữ liệu và chơi lại

Hai câu nằm trong `Assets/BlackMarket/Scripts/PhoneDialogue.cs`; luồng tự chuyển nằm trong `PrologueSequence.LastCall()`. Nhật ký ghi lời cảnh báo về North Point và Cửa 006. Phần này hiện là hội thoại tuyến tính, không còn cây hội thoại cho bài tập.

Hoàn tất phần mở đầu sẽ mở quyền bỏ qua khi chơi lại. Bảng tin ba ngày sau vẫn dùng nút tiếp tục sau khi chữ chạy xong và chờ 3 giây. Hiện có phụ đề và hiệu ứng âm thanh, chưa có lồng tiếng Marcus.

## Chỉnh sửa trong Unity

- Phòng ngủ: mở prefab `Assets/BlackMarket/Resources/Opening/AlexBedroom.prefab`. Kéo thả các đồ vật ở đây; vị trí gốc của prefab là tâm phòng. Bàn và ghế có liên hệ với điểm ngồi trong `PrologueSequence.cs`.
- Xe máy: `Assets/BlackMarket/Resources/Opening/AlexMotorcycle.prefab`; các điểm Seat, Left grip và Right grip được đặt sẵn để tham khảo khi chỉnh model.
- Luồng/camera/thời gian: `PrologueSequence.cs`. `ParkPosition` là vị trí xe đỗ để nhóm truy bắt nhận ra xe Alex.
- Tư thế ngồi, nghe điện thoại, lái xe: `IntroActorPose.cs`; tay đi khom: `CrouchPose.cs`.
- Chỉ dựng lại hai prefab mới bằng **BLACK MARKET / Build bedroom and motorcycle only** nếu cần khôi phục bố cục mặc định. Lệnh này ghi lại hai prefab, không dựng lại map cửa hàng.

Model xe: Duhgless, *Yet Another PSX Style Low Poly Bike*, CC0. Đây là model low-poly có texture, không phải model quang thực. Chuyển động nghe điện thoại/ngồi xe/xuống xe dùng điều chỉnh xương và nội suy; chưa có lồng tiếng.

## Kiểm tra bản hiện tại

- `short-call-test.txt`: 35 kiểm tra đạt, 0 lỗi, gồm hai câu tự chuyển, khóa điều khiển, tạm dừng, âm thanh va đập, mất liên lạc, chuyển ba ngày, cảnh xe máy, nhiệm vụ USB, nhật ký và bỏ qua khi chơi lại.
- Chạy Unity với `--short-call-check` qua `BlackMarket.Editor.PlayVerification.Run`. Tham số cũ `--intro-check` cũng chạy bộ kiểm tra này.
- Các báo cáo `intro-ui-test.txt`, `intro-runtime-test.txt` và ảnh `IntroPreview` cũ thuộc phiên bản có cây hội thoại, không phải bằng chứng cho giao diện hiện tại.
- Chưa kiểm tra trên thiết bị Android thật cho thay đổi này.

Bản cập nhật ba màn: [UNDERGROUND_CAMPAIGN_VI.md](UNDERGROUND_CAMPAIGN_VI.md).
