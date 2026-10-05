# Bảy chương — bảy bản đồ riêng

Mỗi chương tải một prefab riêng, không dùng lại NorthPointBunker. Một tầng tương ứng một chương. Tầng trên mặt đất là cửa hàng; B1–B6 là sáu tầng hầm của North Point.

| Chương / tầng | Prefab trong Resources/Worlds | Bố cục và mục đích |
| --- | --- | --- |
| 1 / Tầng 1 | NorthPointShop | Cửa hàng kính, quầy dịch vụ, văn phòng Marcus, phòng hồ sơ/nhân viên, xưởng, kho, WC và phòng điện. Tìm thẻ và đọc terminal để biết ORDER 071 được giấu ở đâu. |
| 2 / B1 | UtilityBasement | Máy phát, phòng điện, tủ đồ/WC, xưởng radio, hành lang có đường vòng. Chưa có súng: dùng radio kéo đội lục soát khỏi cầu thang. |
| 3 / B2 | ArmoryArchive | Kho vũ khí phía tây, hồ sơ phía đông, phòng FOR_ALEX ở giữa. Nhận súng, lấy bằng chứng, đọc bản ghi; tự đi tới cầu thang sau khi mở khóa Tablet. |
| 4 / B3 | SecurityHub | Phòng giám sát ở trung tâm, tuyến vòng hai bên, phòng thẩm vấn và kiểm soát truy cập. Học camera → báo động → cửa để vượt đội thanh trừng. |
| 5 / B4 | MedicalStorage | Dãy phòng bệnh/cách ly phía tây, kho kệ dày phía đông. Các dãy hàng tạo lối hẹp và khoảng trống để đổi tuyến; dùng đèn và cửa để tới Uplink. |
| 6 / B5 | ServerUplink | Dãy máy chủ, nguồn UPS, thiết bị làm mát xen kẽ và phòng truyền phía sau. Giữ mạng 35 giây để gửi bằng chứng ra ngoài. |
| 7 / B6 | ControlRoom | Tám phòng, 51 vật chắn chính; AK trong phòng quân nhu, kính đêm trong phòng quang học, override ở phòng phụ. Hạ Victor và quyết định số phận cơ sở. |

## Cầu thang và vị trí bắt đầu

Alex bắt đầu bên trong cửa hàng, sau mặt tiền kính có va chạm. Cửa trước khóa. Door 06 vẫn giữ vai trò lối vào bí mật trong cốt truyện, nay dẫn tới cầu thang B1 thay vì thang máy.

Ở B1–B6, Alex xuất hiện trên chiếu nghỉ cao 3 m. Người chơi tự đi xuống 15 bậc, mỗi bậc cao 0,2 m. Có tường bao, lan can và cửa phía sau. Mở cửa cầu thang cuối tầng hiển thị lý do đi tiếp rồi tải tầng kế tiếp. Đây là chuyển bản đồ giữa các tầng, không phải một tòa nhà được tải liền mạch.

Nếu nhân vật rơi xuống dưới y = −4, game đưa Alex về vị trí bắt đầu an toàn của tầng hiện tại mà không xóa tiến độ trong tầng.

## Cốt truyện và hướng dẫn

- HUD luôn có hành động cần làm và dòng **VÌ SAO** giải thích mục đích.
- **J / NHẬT KÝ** mở bối cảnh các tầng đã tới, mục tiêu hiện tại và tiến trình tầng đang chơi. Nhật ký tạm dừng game; Tablet vẫn chạy thời gian thực.
- Các đoạn chuyển tầng giải thích Alex đi đâu, mang theo gì và nguy hiểm tiếp theo.
- Tutorial Tablet cập nhật từng bước còn thiếu thay vì chỉ đưa một chuỗi lệnh chung.
- Thẻ Marcus → terminal → Door 06 → radio → máy quét Carter → súng → ORDER 071 → FOR_ALEX → Tablet → Uplink → Victor → DESTROY/ACCEPT vẫn là tuyến truyện chính.

## Hình ảnh và âm thanh

Xem [ENCOUNTERS_VI.md](ENCOUNTERS_VI.md) về quân số mới, model nhân vật, hai phát trúng và trang bị B6.

Nhân vật hạ súng khi không ngắm/bắn, nâng súng khi ngắm hoặc vừa bắn và hạ khi nạp đạn. Tư thế chuyển mượt bằng điều chỉnh rig; chưa có clip mocap hạ súng/nạp đạn riêng.

Đèn pin NPC sáng hơn, có chùm sáng hiển thị trong không khí. Mesh chùm sáng được cắt theo raycast vật cản; góc và tầm lấy trực tiếp từ đèn mà AI dùng để phát hiện. Đây là hiệu ứng mesh trong suốt, không phải volumetric fog vật lý. Vẫn có nhận biết tiếp xúc rất gần, và lính đã rút súng có vùng nhìn rộng hơn chùm đèn tuần tra.

Nhạc nền tổng hợp gốc gồm lớp drone và nhịp căng thẳng tăng theo nghi ngờ, Uplink hoặc trận Victor. Nhạc giảm khi mở màn hình nội dung/tạm dừng; âm lượng chung điều khiển cả nhạc. Không tải nhạc từ mạng và không thêm bản nhạc bên thứ ba.

## Mã nguồn và chỉnh sửa

- `Assets/BlackMarket/Editor/FloorLayouts.cs`: sáu tầng hầm, cầu thang, vật dụng mới và chỉnh mặt tiền/WC/phòng điện cửa hàng.
- `ShopLayout.cs`: phần nền của cửa hàng.
- `StoryData.cs`, `Campaign.cs`, `GameInterface.cs`: tuyến truyện, mục đích và nhật ký.
- `WeaponPose.cs`, `FlashlightBeam.cs`, `TensionScore.cs`: tư thế súng, chùm sáng và nhạc.

`Prepare North Point project` tái tạo prefab và scene môi trường, có thể ghi đè chỉnh sửa thủ công. Chỉnh prefab trong `Resources/Worlds` nếu cần sửa trực tiếp; bake lại NavMesh khi thay vật cản. Các scene `_Environment` chỉ là bản xem/chỉnh tham khảo.

Ảnh từng tầng và sơ đồ nhìn từ trên nằm trong `Documentation/Previews`; ảnh gameplay, nhật ký và tư thế súng trong `Documentation/Playtest`. Giới hạn hiệu năng, cân bằng và cảm ứng vẫn cần chơi thử trên điện thoại thật.
