# Âm thanh và ánh sáng — 04/10/2026

- **Pistol / AK:** hai bộ bản thu súng thật khác nhau, mỗi bộ ba phát; chọn biến thể không lặp ngay phát trước. AK giữ nhịp 0,11 giây, pistol 0,4 giây. Âm súng gồm tiếng nổ đầu và đuôi vang, không dùng chung một tiếng cho cả hai súng.
- Nguồn CC0 [The Free Firearm Sound Library](https://opengameart.org/node/21826): Walther PPQ và AK-47. Sáu file mono PCM 44,1 kHz; nhập giải nén sẵn để giữ tiếng nổ đầu và giảm trễ. Xem manifest và ASSET_CREDITS.
- Pool 32 nguồn âm thanh, ưu tiên tiếng súng hơn bước chân. Tìm nguồn rảnh trước để không cắt đuôi tiếng súng liên tục. Âm vị trí giảm theo khoảng cách; tường/cửa làm âm giảm và lọc bớt tần số cao. Không dùng collider của chính NPC để tự làm nghẹt tiếng súng của NPC.
- Có tiếng cửa di chuyển/chốt, nạp đạn, lên đạn sau khi nạp xong, bước chân, va chạm đạn với kim loại/gỗ/tường, tín hiệu Tablet, báo động, radio, mưa ngoài cửa hàng và nhạc căng thẳng theo nguy hiểm. Đổi súng ngắt tiếng nạp đang dở.
- Chớp lửa súng sáng ngắn 70 ms, giảm nhanh rồi tắt. Cửa có đèn chỉ vị trí; đèn khẩn cấp và đèn pin NPC vẫn sáng khi tắt điện bằng Tablet. Kính đêm tăng sáng ở camera Alex.

File nghe thử: `Audio/pistol-and-ak.wav` gồm ba phát pistol, sau đó một loạt AK 12 viên ở đúng nhịp và mức âm lượng trong game. Script kiểm tra mức peak của file này dưới 1 để tránh clipping số. Đây không thay thế việc nghe cân bằng âm lượng trên loa/tai nghe và thiết bị Android thực tế.

Tái tạo audio: tải archive theo URL trong `combat-audio-manifest.json`, cài numpy/soundfile/py7zr trong môi trường Python riêng rồi chạy `python Tools/prepare_combat_audio.py <archive.7z>`. Archive đầy đủ không được đưa vào Assets; chỉ nhập các đoạn được dùng.
