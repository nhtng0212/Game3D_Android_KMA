# Màn 2 — hướng dẫn chơi và chỉnh sửa

## Mở và thử

- Mở Unity, chờ nhập tài nguyên và biên dịch xong.
- Chọn **BLACK MARKET → Map 2 → Play from weapon room chapter** để chơi ngay đầu màn 2, không cần đi lại màn 1. Tùy chọn này bắt đầu một lượt chơi ở màn 2; các checkpoint sau đó vẫn sử dụng hệ thống lưu bình thường của game.
- Chọn **BLACK MARKET → Map 2 → Open editable prefab** để sửa bố cục bằng kéo thả. Prefab đang chạy thực tế là `Assets/BlackMarket/Resources/Worlds/UndergroundArmory.prefab`.
- `Scenes/UndergroundArmory_Environment.unity` là cảnh bố trí để xem toàn map. Sửa cảnh này riêng sẽ không tự cập nhật prefab mà trò chơi tải.
- Lệnh **Rebuild Map 2 and desktop computers** dựng lại bố cục từ mã và ghi đè prefab màn 2. Chỉ dùng nếu muốn dựng lại, không dùng sau khi đã tự kéo thả chỉnh sửa mà muốn giữ các thay đổi đó.

## Bố cục

Từ cầu thang 006 nhìn xuống hành lang:

| Bên trái | Bên phải |
| --- | --- |
| Kho vũ khí riêng | Văn phòng vận hành, máy tính nhiệm vụ |
| Kho hồ sơ | Phòng họp |
| Phòng máy móc | Văn phòng kỹ thuật |

Có 16 cột trên hành lang, cửa mở rộng 3 m, cây cảnh và các tủ tiếp tế y tế trong các phòng. Các vách kính là vật cản đạn trong phiên bản này: ra cửa hoặc chọn góc trống khi bắn. Có 12 kệ súng với 108 khẩu trưng bày; bàn chọn trang bị có AK47, M700 và bộ lựu đạn/đạn dự trữ. Súng trưng bày không phải từng điểm nhặt độc lập.

Mật độ đồ đạc đã tăng gấp ba: 18 kệ hồ sơ, 36 bàn làm việc, 39 máy tính để bàn, 3 bàn họp, 18 chậu cây, 12 máy phát và 12 tủ máy chủ. Các lối đi và đường tới điểm nhiệm vụ đã được kiểm tra bằng NavMesh.

## Chơi

1. Vào kho bên trái; **E** lấy AK47, M700 và bộ lựu đạn, đạn dự trữ, kính nhìn đêm [N].
2. **Q** đổi AK/M700. **Chuột phải** giữ ngắm, **chuột trái** bắn, **R** nạp đạn. Khi ngắm M700, camera phóng gần và tốc độ xoay giảm để dễ ngắm xa.
3. **G** ném lựu đạn. Tay lấy đà rồi mới nhả; không phóng ngay khi vừa nhấn. Tránh bán kính nổ.
4. Sang máy tính văn phòng vận hành để đọc hồ sơ.
5. Cửa 006 bị đặt thuốc nổ. Ba tên đầu giữ ở đầu cầu thang và ném nối tiếp **6 lựu đạn** vào hành lang. Vòng cam báo vùng nguy hiểm, mỗi quả nổ sau 3,8 giây kể từ lúc ném; Alex được điều khiển để né. Sau loạt nổ, địch mới chạy xuống theo nhóm. Tầm phát hiện ở màn 2 là 85 m với góc nhìn giới hạn và kiểm tra vật che. Địch tìm kiếm/áp sát vị trí đã thấy hoặc nghe thấy Alex; tối đa bốn tên khai hỏa đồng thời ở đợt đầu. Đứng xa ngoài hành lang không còn an toàn. Đạn có tỷ lệ bắn trượt và đường đạn sượt quanh người. Chạy thực sự càng nhanh thì nguy cơ trúng càng giảm; chạy nhanh giảm khoảng 88% xác suất so với đứng yên cùng khoảng cách, khom giảm thêm 28%. **Shift** để chạy nhanh. Va vào tường mà không di chuyển không được tính là chạy.
6. Nấp sau cột/bàn, ngắm từ xa qua lối cửa trống. Đổi chỗ sau khi bắn. Tủ y tế bổ sung máu, đạn và lựu đạn, mỗi tủ một lần trong lượt chơi.
7. Hạ đủ **30 tên, chia thành 6 nhóm × 5** sẽ kích hoạt lời tự hỏi của Alex. Có **45 giây** không có quân mới để nhặt tiếp tế, hồi máu và nạp đạn; chiến lợi phẩm nằm cạnh xác địch, không dùng tủ tiếp tế cố định trong lượt nghỉ. Hết khoảng nghỉ, **20 cận vệ và Victor đi sau cùng** xuống qua cửa đã phá; không có vụ nổ cửa thứ hai. Tối đa tám tên bắn áp chế theo loạt dài, nhiệm vụ rút xuống tầng cuối xuất hiện ngay. Dùng vật che để rút lui; không cần hạ hết đội này. Nếu cố ở lại và máu xuống **dưới 50**, camera chuyển lên cao, Alex tự chạy theo đường đi tới tầng cuối; đội truy đuổi chạy theo. Trong hoạt cảnh này Alex không nhận sát thương và người chơi không điều khiển. Tạm dừng vẫn dừng cả cuộc truy đuổi.
8. Khi hồi sinh tại checkpoint giao tranh, Alex ở kho vũ khí, được hồi đầy máu và bổ sung đạn, có khoảng chuẩn bị trước khi nhóm đầu xuất hiện.

Các địch dùng chung khoảng nghỉ 0,5 giây giữa những phát gây sát thương (tối đa 6 máu/phát), để nhiều nòng súng không dồn sát thương trong cùng một khung hình. Lựu đạn địch gây tối đa 18 máu/quả, vật che chặn sức nổ; các vụ nổ sát nhau cũng không cộng dồn tức thì.

Trên cảm ứng dùng các nút **NGẮM**, **BẮN**, **NẠP**, **ĐỔI SÚNG**, **LỰU ĐẠN** tương ứng.

## Máy tính và chuyển cảnh

Máy tính Marcus và các máy tính đặt mới dùng prefab `Prefabs/Shop/Desktop computer.prefab`. Phía **+Z** là phía người sử dụng: màn hình và bàn phím phải hướng về ghế. Điểm tương tác máy Marcus đã chuyển sang phía ghế.

Đoạn xuống từ màn 1 dùng một lớp phủ đen toàn màn hình, chuyển cảnh khi đã tối và giữ lớp phủ qua lúc tải màn 2 rồi mới sáng lại. Không dùng tấm mặt phẳng đen cắt ngang cầu thang nữa.

Màn 3 đã được dựng lại thành phòng điều phối với bàn máy tính vòng cung. Xem `MAP3_REVISION_VI.md`.

## Nguồn và kiểm tra

Nguồn model, giấy phép và ghi chú giọng tổng hợp nằm trong `ASSET_CREDITS.md`. Không mua asset. Ảnh kiểm tra nằm trong `ArmoryRevisionPreview/`; kết quả kiểm tra Unity nằm trong `armory-revision-test.txt`. Chưa đo tốc độ khung hình trên thiết bị Android thật.

Giọng AI hô truy bắt đã tắt; thông báo chữ vẫn giữ. Mức âm thanh đã tăng ba lần trong các nguồn hiệu ứng, nhạc nền và xe máy (giới hạn mức tối đa của Unity). Thanh âm lượng trong cài đặt vẫn hoạt động.

Chi tiết cân bằng mới, cơ chế áp chế sau 10 cận vệ và cảnh kết: [bản điều chỉnh 30 địch](COMBAT_30_REVISION_VI.md).
