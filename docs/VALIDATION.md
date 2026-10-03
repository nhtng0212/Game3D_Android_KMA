# Kiểm chứng bản 0.1.0

Thực hiện ngày 02/10/2026, múi giờ Asia/Bangkok.

## Kết quả đã có

- `tests/campaign_test.gd`: 39 kiểm tra đạt. Bao gồm thứ tự chapter, khóa Door 06, Keycard/ORDER 071, reload, Tablet, camera, alarm, cửa, bóng tối, upload, ba phase Victor, cả hai ending, chết và retry.
- `tests/mechanics_test.gd`: 28 kiểm tra đạt. Điều khiển player qua navigation path và va chạm thực để đi từ bãi xe đến văn phòng/Workshop/Door 06; đi xuyên ba khu của mỗi chapter ngầm; bắn hitscan vào địch, giới hạn tốc độ bắn, cửa chắn line-of-sight, cảm biến chống kẹp cửa, input ba ngón, pause/resume.
- `tests/ui_layout_test.gd`: các menu, cài đặt, hướng dẫn, prologue, FOR_ALEX và hai ending nằm trong biên viewport. Chạy cấu hình 1280×720 và 1600×720. Hình ảnh menu, gameplay, Tablet, boss, lựa chọn và ending đã chụp bằng renderer OpenGL để kiểm tra trực quan; touch HUD đã chụp riêng.
- Đã sửa clearance của navigation mesh quanh rack/thùng hàng và đồng bộ map trước khi AI bắt đầu tìm đường.
- Đã sửa giới hạn chiều rộng Label trước khi gán text để tránh tràn và chiều cao tính sai.
- Đã kiểm tra bản Linux export khởi động bằng executable đã đóng gói, không chạy từ source.
- APK xuất thành công, xác minh chữ ký v2/v3 và `zipalign -c -P 16 4` đạt.
- Đã sửa manifest do bộ export gán trùng authority cho FileProvider và AndroidX Startup. `tools/build.sh` tự sửa, align và ký lại; `aapt dump xmltree` xác nhận hai authority riêng. Kiểm tra CRC toàn bộ ZIP và chạy lại thao tác sửa trên manifest đã đúng đều đạt.
- Metadata APK: `vn.kma.blackmarket`, version `0.1.0`, min SDK 24, target SDK 36, ARMv7 và ARM64. Không yêu cầu Internet.

## Cách tái lập

```bash
./tools/test.sh
./tools/build.sh
```

Để chụp màn hình bằng renderer thật, trong phiên desktop có DISPLAY:

```bash
XDG_DATA_HOME="$PWD/.tools/visual-tests" ./tools/run.sh --script tests/campaign_test.gd -- --capture
XDG_DATA_HOME="$PWD/.tools/visual-tests" ./tools/run.sh --fixed-fps 60 --script tests/mechanics_test.gd -- --capture
```

Log kiểm thử/build và PNG nằm trong `builds/`. Kiểm thử headless dùng `--test-silent` để không kiểm tra luồng audio bằng dummy audio driver; lượt render đồ họa chạy âm thanh thật.

## Giới hạn của bằng chứng kiểm thử

Campaign test kích hoạt tương tác trực tiếp để kiểm tra state machine; nó không chứng minh người mới sẽ tự tìm được mọi mục tiêu. Traversal test dùng đường navigation và tắt AI đang tấn công để tách lỗi va chạm khỏi độ khó. Test cảm ứng gửi sự kiện ba ngón vào handler, chưa đo độ trễ hoặc cảm giác thao tác trên màn hình điện thoại thật. Test uplink rút ngắn countdown trong test, còn bản game chạy 35 giây.

Chưa có thiết bị Android kết nối qua ADB trong phiên làm việc. Chưa chứng minh FPS, mức RAM, pin/nhiệt, notch/safe area, tương thích GPU thực tế hoặc thời lượng chơi 20–30 phút. APK là bản debug ký nội bộ. Cần một lượt chơi từ đầu đến cuối trên điện thoại mục tiêu để nghiệm thu sản phẩm.

## Các bước kiểm tra trên điện thoại

1. Cài APK, mở mới từ launcher và bắt đầu chiến dịch.
2. Di chuyển + xoay camera, rồi sau Armory thử di chuyển + giữ ngắm + giữ bắn cùng lúc.
3. Chuyển ứng dụng ra nền và quay lại: phải hiện Pause; tiếp tục không được kẹt joystick/bắn.
4. Thử tắt đèn, báo động, đóng/mở cửa, dùng Tablet trong khi địch đang hoạt động.
5. Thử chết, retry, đóng hẳn app và Continue; checkpoint phải trở về đầu chương.
6. Hoàn thành Victor bằng terminal override; xem lần lượt DESTROY và ACCEPT bằng retry checkpoint.
7. Ghi model điện thoại, phiên bản Android, thời gian chơi, cảm nhận touch và tình trạng nóng/FPS để cân bằng lượt sau.
