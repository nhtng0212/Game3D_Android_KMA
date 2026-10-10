# Chiến dịch Order 71 — ba màn

## Mở và chỉnh map trong Unity

Mở `Assets/BlackMarket/Scenes/NorthPoint.unity`, nhấn Play để chạy chiến dịch.

Các scene môi trường dành cho việc kéo thả, chỉnh vị trí:
- `NorthPointShop_Environment.unity`: cửa hàng, văn phòng, nhà kho đã có.
- `UndergroundArmory_Environment.unity`: kho quân giới, khoảng 52 × 64 m.
- `UndergroundControl_Environment.unity`: trung tâm điều phối mới, khoảng 44 × 52 m.

Các scene môi trường chứa prefab tương ứng trong `Assets/BlackMarket/Resources/Worlds/`. Sau khi sửa instance, chọn Overrides → Apply All để thay đổi xuất hiện trong trò chơi. Nếu đổi tường hoặc vật cản, cập nhật NavMeshSurface của map. Không chạy lại builder sau khi đã tự chỉnh map vì builder dựng lại hai map từ bố cục gốc.

## Luồng nhiệm vụ

1. Giữ phần mở đầu điện thoại và xe máy. Alex tìm USB đỏ trong văn phòng Marcus, mở máy tính, đọc phần đầu Order 71. Phần phụ lục báo khóa và cần mã 10 số. Alex nói “Mã ư?” trước cảnh đoàn xe tới. Mười người mặc vest truy bắt Alex; USB mở cửa ngoài 006, xác thực mặt mở cửa trong.
2. Xuống kho quân giới, lấy AK, M700, lựu đạn và kính nhìn đêm. Đọc máy tính vận hành, cửa 006 bị phá. Đợt đầu ném sáu lựu đạn rồi tràn xuống; hạ đủ 30 tên theo 6 nhóm × 5.
3. Có 45 giây để nhặt máu, đạn, nạp súng. Sau đó 20 cận vệ và Victor xuống qua cửa đã phá. Địch tiến theo tổ giữa vật che, bắn ghìm, nhô ra bắn và tiếp tục ném lựu đạn để ép đổi vị trí.
4. Mục tiêu rút lui mở ngay khi đội Victor tới. Nếu máu dưới 50, camera lên cao và Alex tự chạy xuống tầng cuối; không nhận sát thương trong hoạt cảnh. Địch bị hạ tại đây không hồi sinh ở tầng cuối; làm Victor bị thương giảm máu trận cuối.
5. Vào thẳng trung tâm điều phối, không còn phòng chờ. Alex băng bó tạm và tự chạy tới màn hình lớn, nhận ra đây là đầu não Order 71. Victor xuất hiện, mục tiêu phá tủ điện mở ngay. Bật kính [N] đã lấy ở kho; Victor vẫn nhìn được trong bóng tối, thuộc hạ mất khả năng quan sát. Bố cục có 21 bàn vòng cung, bàn máy chủ trung tâm và các kệ thùng hồ sơ.
6. Hạ Victor và những thuộc hạ còn lại, cắm USB vào máy chủ trung tâm. Máy chủ cung cấp mã 10 số; nút GIẢI KHÓA ORDER 71 mở hồ sơ đầy đủ. Chữ chạy theo từng trang; nút tiếp tục bật sau khi chữ chạy xong 3 giây. Đã đọc thì được bỏ qua khi chơi lại.
7. Đọc xong mới sao chép/gửi bằng chứng và bắt đầu đồng hồ thoát hiểm 90 giây. Tự chạy về cầu thang đã đi xuống; tới gần sẽ chuyển cảnh chậm để xem Alex lên xe máy, rời đi, North Point phát nổ. Chỉ có một kết thúc chính thức.

## Điều khiển

WASD di chuyển, Shift chạy, C khom; E tương tác. Chuột phải ngắm, chuột trái bắn; R nạp; Q đổi súng; G ném lựu đạn (nổ sau 2 giây, có sát thương lên Alex); N bật/tắt kính đêm. J nhật ký, Esc tạm dừng. Thiết bị cảm ứng có các nút tương ứng.

Dùng bàn và cột làm vật che. Tường cản đạn và vụ nổ. Máy tính/hồ sơ và hoạt cảnh khóa điều khiển; mở nhật ký hoặc tạm dừng sẽ dừng nhịp chiến đấu.

## Điểm lưu

Lưu đầu tầng và đầu các chặng chiến đấu. Chết sẽ quay lại đầu chặng; số địch đã hạ trong chặng chưa hoàn thành không phải một bản lưu tức thời. Danh sách thuộc hạ bị hạ ở kho quân giới được mang theo khi rút xuống tầng cuối. Bản lưu cũ ở tầng hầm được chuyển về đầu màn kho quân giới mới. Bản lưu đã hoàn thành mở lại phần kết; chọn chiến dịch mới để chơi từ đầu.

Chi tiết bố cục và cân bằng hiện tại: [màn 2](MAP2_REVISION_VI.md), [màn 3](MAP3_REVISION_VI.md).

## Tệp chính

`UndergroundCampaign.cs`: nhiệm vụ, đợt địch, chuyển cảnh, điểm lưu.
`CombatBrain.cs`: tầm nhìn, di chuyển, bắn loạt và mất tầm nhìn trong bóng tối.
`BlastEffects.cs`: hiệu ứng nổ, lựu đạn, tủ điện.
`StoryData.cs`: toàn bộ nội dung hồ sơ và mô tả nhiệm vụ.
`UndergroundBuilder.cs`: bố cục map và vật liệu, chỉ dùng để dựng lại map gốc.
`UndergroundSelfTest.cs`: kiểm tra có chủ đích khi chạy với `--campaign-v2-check`; dùng bản lưu thử riêng.

## Asset miễn phí

Model mới: hộp đạn, xe đẩy công nghiệp và đèn bảo vệ Poly Haven (CC0). Vật liệu sử dụng texture 1K, normal map và bản đồ metallic/smoothness đóng gói cho URP. Màn hình điều phối là texture giao diện tạo riêng trong dự án. Nguồn, giấy phép và checksum được lưu cùng asset; xem `ASSET_CREDITS.md`.

Tái tạo nguồn khi cần: `Tools/fetch_underground_assets.py`, `Tools/prepare_underground_textures.py`, `Tools/generate_underground_audio.py`. Hai script texture cần Pillow; tải nguồn cần requests. Không cần chạy lại chúng để mở dự án.

## Kết quả kiểm tra

- `underground-flow-test.txt`: các chặng từ quân giới tới máy chủ trung tâm đã qua kiểm tra; phần kết được kiểm tra riêng.
- `underground-final-test.txt`: không có mục kiểm tra thất bại; gồm chuyển bản lưu cũ, rút lui theo thời gian, chuyển tầng, điểm lưu, đường đi, kính đêm, lựu đạn, giải mã, đếm giờ và lưu hoàn thành.
- `underground-ending-test.txt`: kiểm tra riêng chuyển bằng chứng → xe máy → nổ → kết thúc; không có mục thất bại.
- Ảnh kiểm tra nằm trong `Documentation/UndergroundPreview/`.

Đã chạy trong Unity Editor 6000.3.25f1. Chưa xuất APK và chưa đo FPS trên điện thoại Android thật. Báo cáo kiểm tra bảy tầng cũ không áp dụng cho chiến dịch ba màn này.

Chi tiết cân bằng mới, cơ chế áp chế sau 10 cận vệ và cảnh kết: [bản điều chỉnh 30 địch](COMBAT_30_REVISION_VI.md).
