# Cửa hàng tầng 1 — 07/10/2026

Đã bổ sung cảnh xe đến, truy bắt, kho hẹp và Việt hóa. Hướng dẫn mới nhất: [Mở đầu cửa hàng](OPENING_VI.md).

Map đã được thay trực tiếp theo yêu cầu; không tạo bản sao lưu. Các tầng B1–B6 không được tái tạo. Giữ nguyên tuyến lấy thẻ Marcus → terminal ORDER 071 → Door 06 → B1.

## Mở và chỉnh

- `BLACK MARKET → Shop → 2 - Open editable shop prefab`: mở map thực sự được game tải.
- `BLACK MARKET → Shop → 3 - Show drag-and-drop shop furniture`: thư viện prefab dùng lại ở `Assets/BlackMarket/Prefabs/Shop`.
- Trong Hierarchy, chọn object cha của quầy/kệ để di chuyển cả cụm. Các cụm là prefab lồng trong map; không phải hàng nghìn chi tiết cùng nằm ở gốc.
- Nhóm `09 - CEILING` chứa trần và đèn; dùng biểu tượng con mắt trong Hierarchy để ẩn tạm khi chỉnh mặt bằng. Không tắt GameObject bằng checkbox nếu muốn trần vẫn xuất hiện khi chơi.
- Sau khi sửa, Ctrl+S. Nếu thay tường/vật cản, lưu và thoát Prefab Mode rồi dùng `Shop → 4 - Bake navigation from saved shop (keeps layout)` để bake lại NavMesh mà không dựng lại đồ đạc.
- `Shop → 1` là lệnh tái tạo từ code, **ghi đè chỉnh sửa thủ công tầng 1**. Menu Prepare toàn dự án cũng tái tạo tầng 1 bằng bố cục mới này và các tầng khác như trước.
- Chơi bằng scene `Assets/BlackMarket/Scenes/NorthPoint.unity`, chọn **BẮT ĐẦU CHIẾN DỊCH**. Bản Linux cũ không có map mới.

## Bố cục

Mặt bằng 28 × 36 m, cửa vào ở giữa cạnh trước. Từ cửa nhìn vào:

- Quầy thu ngân bên phải: laptop POS, máy tính tiền, ghế, mặt quầy gỗ và ghi chú Marcus.
- Bàn trải nghiệm laptop bên trái; các kệ TV dọc tường trái, máy ảnh dọc tường phải.
- 12 cụm kệ hai mặt chia thành Audio, Computing, Appliances và Cameras; có model trưng bày, hộp hàng và nhãn. Lối giữa dẫn thẳng tới kho.
- Phòng Marcus nằm cuối bên trái khu bán hàng: bàn, laptop, đèn, ghế, kệ hồ sơ và tủ dụng cụ; thẻ và terminal ở bàn.
- WC Nam và WC Nữ riêng nằm cuối bên phải, có cửa tương tác, lavabo/gương và vách buồng; Nam có thêm bồn tiểu.
- Kho nằm phía sau, có kệ kim loại nhập, thùng hàng, bàn đóng gói. Door 06 ở giữa tường sau.

## Model miễn phí đã sử dụng

13 model/biến thể tải mới, tất cả CC0:

- Poly Haven: `classic_laptop`, `CashRegister_01`, `boombox`, `Camera_01`, `vintage_electric_kettle`, `vintage_microwave`, `security_camera_01`, `plastic_monobloc_chair_01`, `steel_frame_shelves_02`, `steel_frame_shelves_03`.
- OpenGameArt, tác giả loafbrr_1: `Sink_A`, `Toilet_Elongated_A`, `Urinal_A`. Chỉ nhập ba mẫu và texture liên quan, không đưa archive 99 MB hay mọi biến thể vào Assets.
- Dùng lại đèn bàn, thùng gỗ/carton, tủ dụng cụ và cửa cuốn đã có. Quầy, kệ trưng bày, TV phẳng, tường, biển hiệu và hộp sản phẩm là hình học do dự án tạo; không quảng bá là model tải ngoài.

Nguồn nằm ngoài Resources tại `Assets/BlackMarket/Art/ShopSources`; material URP ở `Art/ShopMaterials`. Texture giới hạn 1K. Manifest ghi tác giả, URL tải và SHA256: [shop-assets-manifest.json](shop-assets-manifest.json). License giữ kèm nguồn. Model miễn phí có phong cách thiết bị cũ; đây là cửa hàng North Point theo phong cách đó, không phải showroom điện thoại hiện đại.

## Kiểm tra

- [shop-validation.txt](shop-validation.txt): đường NavMesh tới nhiệm vụ và các phòng, 3.905 điểm phủ sàn, tham chiếu material.
- [shop-runtime-test.txt](shop-runtime-test.txt): CharacterController đi qua map, cửa đóng/mở, tương tác ghi chú/thẻ/terminal/Door 06 và chuyển sang B1 có bốn NPC.
- [ShopPreview](ShopPreview): ảnh showroom, quầy, hàng hóa, phòng Marcus, kho, WC và mặt bằng. Ảnh môi trường không có HUD, ánh sáng preview có thể khác camera gameplay.
- Kiểm tra đường đi dùng cửa đã mở để tách lỗi đường đi khỏi logic cửa; kiểm tra cửa riêng bằng linecast. Chưa đo hiệu năng trên Android thật và chưa xuất APK/EXE.

Chạy lại test trong Unity Editor (không tạo player build):

```bash
/path/to/Unity -batchmode -force-glcore -projectPath "$PWD" \
  -executeMethod BlackMarket.Editor.PlayVerification.Run --shop-check \
  -logFile /tmp/blackmarket-shop-runtime.log
```

Không thêm `-quit`; test tự thoát Editor. Save test nằm trong temporaryCachePath.
