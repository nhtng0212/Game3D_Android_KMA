# NPC, trang bị và độ khó — 04/10/2026

## Quân số

Tăng gấp đôi các encounter hiện có; giữ tầng khám phá không có lính theo tuyến truyện.

| Tầng | Trước | Hiện tại |
| --- | ---: | ---: |
| Tầng 1 / cửa hàng | 0 | 0 |
| B1 / kỹ thuật | 2 | 4 |
| B2 / nhận súng và hồ sơ | 0 | 0 |
| B3 / an ninh | 2 | 4 |
| B4 / kho và y tế | 3 | 6 |
| B5 / Uplink, sau khi bắt đầu truyền | 3 | 6 |
| B6 / trận cuối | 1 Victor | 1 Victor + 1 lính hỗ trợ |

Mỗi lính mới có điểm spawn và tuyến tuần tra riêng. Bố cục phòng/cửa và nhịp dừng tuần tra cập nhật trong [ROOMS_AND_STEALTH_VI.md](ROOMS_AND_STEALTH_VI.md). Sáu prefab lính dùng sáu model nguồn khác nhau; trong cùng encounter không lặp model. Tầng B5 dùng đủ sáu mẫu. Không tạo hai bản sao Victor.

## Nhân vật

Nguồn [Microsoft Rocketbox](https://github.com/microsoft/Microsoft-Rocketbox), MIT. Giữ license trong Resources/Rocketbox; URL phiên bản cố định và SHA256 trong `roster-manifest.json`.

| Prefab | Model nguồn | Vai trò hình ảnh |
| --- | --- | --- |
| Operator | Security_Male_01 | Bảo vệ mặc đồng phục |
| MilitaryHelmet | Military_Male_01 | Quân phục và mũ bảo hộ |
| MilitaryCap | Military_Male_05 | Quân phục và mũ mềm |
| RedBeret | Police_Male_04 | Áo tối màu và mũ nồi |
| Tactical | Police_Male_06 | Áo giáp trên áo ngắn tay |
| Plainclothes | Male_Adult_04 | Nhân viên thường phục |
| Victor | Police_Male_02 | Trang phục chiến thuật đen, che mặt, cao 1,92 m và phù hiệu đỏ riêng |

Các model nguồn có trang phục nghề nghiệp sẵn; trong cốt truyện chúng đóng vai đội thanh trừng của The Market. Animation idle/walk/run dùng chung bộ rig tương thích, còn hình học/khuôn mặt/trang phục là các model khác nhau. Ảnh kiểm tra từng prefab: `Previews/actor-*.png`.

## Hai phát trúng ở các tầng trước Victor

Trước B6, mỗi phát đạn NPC trúng gây **50 HP**. Từ 100 HP, phát đầu còn 50 và phát thứ hai làm Alex gục. Hỗ trợ ngắm không giảm sát thương nữa. Không tự hồi máu; vật phẩm hồi máu vẫn hoạt động, vì vậy có thể sống lâu hơn nếu kịp hồi giữa các lần trúng.

B6 giữ sát thương theo từng đối thủ/phase để trang bị mới có ý nghĩa và trận boss không kết thúc quá nhanh. Các cảnh báo nghi ngờ, độ trễ rút súng, vật che, radio và Tablet vẫn hoạt động.

## Tầng Victor

Tám phòng chức năng: quân nhu, quang học, chỉ huy, override, WC/nhân viên, hồ sơ, phòng điện, successor. So với bốn khu chức năng của bố cục trước, số phòng tăng gấp đôi. Có **51 vật chắn chính**, so với 24 trước đó; không tính tường/sàn, chi tiết trang trí và ghế nhỏ. Baseline trong `encounter-baseline.json`; bộ kiểm tra editor đếm tối thiểu 48 vật chắn và đúng 8 mốc phòng.

Thêm bàn họp, ghế có lưng tựa, tủ cá nhân, tủ điện, thiết bị vệ sinh và các cụm chắn. Giữ các lối đi nối phòng và tới mục tiêu; không lấp kín đường thoát bằng đạo cụ.

## Nhặt AK và kính nhìn đêm

Ngay sau cầu thang B6:

1. Phòng **QUÂN NHU bên trái**: đến bàn, nhấn **E** để nhặt AK.
2. Phòng **QUANG HỌC bên phải**: đến bàn, nhấn **E** để nhặt kính đêm.
3. **Q** đổi Pistol/AK; **N** bật/tắt kính. Android có nút ĐỔI SÚNG và KÍNH ĐÊM sau khi nhặt.
4. Có thể tắt đèn bằng Tablet rồi bật kính để nhìn rõ hơn. Kính không nhìn xuyên tường, không vô hiệu hóa đèn pin hoặc khả năng phát hiện của NPC.

AK: băng 30 viên, dự trữ tối đa 90, 22 sát thương/phát, khoảng cách giữa hai phát 0,11 giây, nạp 2,2 giây. Giữ nút bắn để bắn tự động. Đạn AK và Pistol tách biệt; đổi súng hủy thao tác nạp đang dở. Hộp tiếp tế bổ sung đạn AK sau khi có súng.

Kính dùng hậu kỳ tăng sáng và sắc xanh chỉ cho camera người chơi; không chiếu sáng thế giới, không ảnh hưởng camera Tablet. Bật/tắt được, không có thời lượng pin trong phiên bản này. Hai trang bị đều tùy chọn; lá chắn Victor vẫn cần terminal override.

Trang bị thuộc trạng thái lưu, nhưng game tiếp tục chỉ tự lưu **đầu chương**. Thử lại checkpoint B6 trước khi nhặt sẽ phải vào phòng lấy lại trang bị.

AK và kính là model tạo bằng hình học trong dự án, không phải scan/asset tải ngoài; pose hai tay điều chỉnh bằng code trên rig. Bàn/ghế/WC/tủ mới cũng kết hợp hình học dự án với các model đã có.
