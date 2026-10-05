# Kiểm tra map, cửa, stealth, âm thanh và ánh sáng — 05/10/2026

Unity 6000.3.25f1, URP, Linux x86_64/OpenGL Core. Bản Linux: **Succeeded / 0 errors**.

## Kiểm tra

- **217 kiểm tra runtime PASS, 0 FAIL**; tiến trình thoát mã 0, không có exception trong log. Kết quả ở `runtime-test.txt`, log chi tiết ở `runtime-test.log`.
- 7/7 prefab môi trường đạt kiểm tra vật liệu, mesh/collider, spawn, NavMesh tới các tương tác và tuyến tuần tra. Tám prefab nhân vật có texture và idle/walk/run.
- **3.905 mẫu phủ sàn cửa hàng** đều có sàn, gồm góc sau phải từng bị xóa nhầm. CharacterController đứng thật ở năm vị trí góc, không rơi.
- **15 cửa phòng**, trong đó sáu cửa ở cửa hàng. Kiểm tra tương tác E, mở, đóng, chặn đường nhìn khi đóng, thông đường nhìn khi mở và chống đóng đè lên người chơi.
- B1: chạy tuyến từ chân cầu thang vòng qua kệ → SERVICE → radio → máy quét với **cả bốn NPC hoạt động**, kiểm tra tương tác radio thực sự tới được và không mất máu. Chờ điểm tuần tra còn ít nhất 5 giây trước khi đi.
- B3/B4: đo liên tục 38 giây với AI hoạt động tại các điểm cắt ngang hành lang; cần có ít nhất một khoảng liên tục 4 giây mà tất cả các điểm kiểm tra đều ngoài tầm nhìn NPC. Thời gian đo thực tế: live NPC corridor opportunity floor 3 / longest=8.5s (minimum 4s); live NPC corridor opportunity floor 4 / longest=5.8s (minimum 4s). Xem `runtime-test.txt`.
- Các bài traversal khác dùng CharacterController đi thật xuống sáu cầu thang và tới mục tiêu, tạm dừng AI để tách lỗi va chạm khỏi giao tranh.
- AI thử nghiệm riêng: đèn pin, vật che, khom, tích lũy nghi ngờ, độ trễ rút súng, bắn, mất dấu/tìm kiếm, trở lại tuần tra, nghe tiếng động.
- HP trước B6: hai phát trúng từ 100 HP làm gục, kể cả bật hỗ trợ ngắm.
- Pistol/AK hitscan và nạp đạn; hạ/nâng súng; đổi súng; nhặt kính đêm, bật/tắt; lưu/tải trang bị; pickup đã lấy không xuất hiện lại khi tải bản lưu có trang bị.
- Kiểm tra hai bộ audio pistol/AK, mỗi bộ ba bản thu. Sáu file mono PCM 44,1 kHz, đầu nổ trong khoảng 7–14 ms, không có sample bị clipping. `Audio/validation.json` chứa số đo; `Audio/pistol-and-ak.wav` là file nghe thử ở nhịp bắn thực tế.
- Đèn khẩn cấp và đèn pin NPC vẫn hoạt động khi tắt điện, tại cả sáu tầng hầm. Kính đêm đã có ảnh bật/tắt trong cùng phòng.
- Tablet, cửa B, báo động, Uplink, ba phase Victor, cả hai ending, checkpoint/chết/thử lại và pause/resume.

## Chạy lại

```bash
./play-linux.sh --self-test --capture
```

Kiểm tra riêng tuyến B1: thêm `--stealth-check`. Test dùng checkpoint riêng trong temporaryCachePath, không ghi vào save người chơi.

Ảnh `Previews` gồm các sơ đồ tầng, phòng, cầu thang và nhân vật; `Playtest` gồm gameplay, nhật ký, AK, kính đêm và điểm chờ tuần tra. `build-final.log` là log build/validation cuối.

## Phạm vi

Tuyến B1 được kiểm tra với AI thật; các khoảng hở B3/B4 là phép đo tại những điểm hành lang cụ thể, không chứng minh mọi cách đi đều an toàn. B5 là phòng thủ Uplink; Victor là trận chiến. Chưa có playtest tay dài để chốt cân bằng toàn bộ chiến dịch.

Ảnh được kiểm tra trên Linux. File nghe thử và thông số audio được kiểm tra, nhưng chưa đánh giá cân bằng nghe trên loa/tai nghe điện thoại. Chưa build lại APK hoặc đo FPS, nhiệt, cảm ứng trên Android trong lượt này.
