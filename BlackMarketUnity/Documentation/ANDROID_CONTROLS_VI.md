# Điều khiển Android và chỉnh sửa gameplay — 06/10/2026

## Cảm ứng

- Joystick trái bắt đầu ngay dưới ngón tay; có vùng chết, giới hạn độ dài vector và tốc độ theo mức kéo.
- Vuốt vùng trống bên phải để xoay camera. Một ngón tay giữ nguyên vai trò đến khi nhấc lên, nên vuốt qua nút BẮN không tự nổ súng.
- Giữ NGẮM, BẮN hoặc CHẠY. Chạm KHOM để chuyển khom/đứng. Khi đang khom, giữ CHẠY không tự phá trạng thái lén lút.
- Có thể di chuyển, nhìn, ngắm và bắn đồng thời bằng nhiều ngón tay. Chỉ một ngón điều khiển joystick, một ngón điều khiển góc nhìn.
- DÙNG, NẠP, ĐỔI SÚNG, KÍNH ĐÊM, TABLET, NHẬT KÝ và tạm dừng sử dụng chung logic gameplay với bàn phím.
- Các nút trang bị chỉ xuất hiện sau khi nhận được vật phẩm. Tạm dừng, mở Tablet, đọc truyện, chết hoặc mất tiêu điểm đều xóa các thao tác giữ. Khi quay lại phải chạm mới để tránh bắn/chạy ngoài ý muốn.
- HUD cảm ứng dùng cùng phép biến đổi với vùng nhận chạm, nằm trong `Screen.safeArea`, giữ tỷ lệ trên điện thoại dài và máy tính bảng.

## Menu và Tablet

Dự án giữ giao diện IMGUI để phù hợp cấu trúc hiện có. Input System mới không tự cấp sự kiện runtime cho `GUI.Button`/`GUI.HorizontalSlider`, vì vậy `GameInterface` đọc chuột và Enhanced Touch rồi xử lý nút, kéo thanh trượt và cuộn nội dung trực tiếp. Không cần bật cả hai hệ thống input.

Nguồn kỹ thuật: [Unity Input System — IMGUI](https://github.com/Unity-Technologies/InputSystem/blob/develop/Packages/com.unity.inputsystem/Documentation~/use-imgui-alongside-input-system.md).

Tablet vẫn chạy thời gian thực: lính có thể phát hiện người chơi. Nhân vật không thể bắn hay di chuyển khi đang điều khiển Tablet.

## Dáng khom

Thay kiểu hạ hông 0,5 m bằng hạ vừa phải 0,20 m, nghiêng thân ra trước qua nhiều đốt sống, giữ đầu nhìn về trước và dùng IK cho chân. Bước chân ngắn hơn, giữ chu kỳ nâng/hạ chân của animation gốc. Tốc độ animation và tiếng bước chân dựa vào vận tốc thực, nên đẩy vào tường không tiếp tục đi bộ tại chỗ. Pose được khôi phục trước mỗi lần lấy animation để tránh cộng dồn khi tạm dừng. Điểm phát đạn và camera cũng hạ theo trạng thái khom.

Đây là chỉnh sửa chuyển động bằng code trên rig Rocketbox hiện có; chưa thay bằng bộ mocap đi khom chuyên dụng.

## Âm thanh

`TensionScore` tổng hợp nhạc gốc gồm giai điệu nền, bass/pad, nhịp căng thẳng và chuông báo động nhanh. Nhạc nền chạy cả trong menu; nhịp tăng theo nghi ngờ. Chuông chỉ bật khi địch đã xác nhận và đang đuổi, tấn công hoặc tìm kiếm sau phát hiện. Sau khi thoát truy đuổi có khoảng giữ ngắn rồi giảm âm dần; đổi tầng/chơi lại xóa trạng thái truy đuổi. Tạm dừng giảm nhạc nền và tắt dần chuông. Cài đặt âm lượng áp dụng chung qua AudioListener.

## Kiểm tra không xuất bản chạy

Có thể chạy trong Unity Editor, không cần tạo `.exe`/`.apk`:

```bash
/path/to/Unity -batchmode -projectPath "$PWD" \
  -executeMethod BlackMarket.Editor.PlayVerification.Run \
  --self-test --controls-check --touch-test --capture -logFile Logs/editor-controls.log
```

Bỏ `--controls-check --touch-test` để chạy toàn bộ chiến dịch. `--touch-test` bật đường input cảm ứng trong Editor để kiểm tra cả sự kiện Touchscreen giả lập. Không thêm `-quit` vì bài kiểm tra tự đóng Editor khi hoàn thành. Không thêm `-nographics` khi cần ảnh. Trong batch mode, ảnh lấy trực tiếp từ camera nên không chứa HUD IMGUI.

Bài kiểm tra dùng checkpoint riêng trong temporaryCachePath, không sửa save chơi thật. Kết quả toàn chiến dịch ghi vào `Documentation/runtime-test.txt`, kiểm tra điều khiển riêng ghi vào `Documentation/controls-test.txt`; ảnh pose Editor ở `Documentation/EditorPlaytest/crouch-*.png`.

Cần kiểm tra thêm trên Android thật: cảm giác nút/vuốt, tai thỏ, Back/Home, Bluetooth, tốc độ khung hình và nhiệt. Không có thiết bị Android kết nối trong phiên này.
