# Sửa màn hình đen Android — 12/10/2026

Thiết bị Xiaomi/Redmi 25053RT47C, Android 16.

Bản APK cũ lặp lỗi Render Graph Execution error / NullReferenceException tại PostProcessPassRenderGraph.RenderPostProcessingRenderGraph ngay khi tải cửa hàng.

Bản sửa giữ các type URP dùng qua reflection trong link.xml và tắt Strip Unused Post Processing Variants vì game tạo camera/volume lúc chạy. Vẫn dùng Render Graph; không chuyển Compatibility Mode. Giữ hậu kỳ và kính nhìn đêm.

Build thành công; cài đè bằng adb install -r không xóa dữ liệu. Trực tiếp chụp màn hình đã thấy menu cửa hàng và hoạt cảnh phòng ngủ/mất liên lạc. Log tiến trình mới không có lỗi Render Graph hoặc exception ở thời gian kiểm tra. Chưa kiểm tra hết chiến dịch hay kính đêm trên điện thoại.

APK: Builds/Android/BlackMarket.apk. Bản này cũng có MasterAudioGain 5x kèm giới hạn đỉnh. Chưa đo âm lượng đầu ra bằng thiết bị đo.
