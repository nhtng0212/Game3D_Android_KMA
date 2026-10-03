# Asset credits

Tất cả asset cần thiết nằm trong project; game không tải dữ liệu lúc chạy.

| Thành phần | Nguồn | Giấy phép / ghi chú |
| --- | --- | --- |
| Furniture Kit | https://kenney.nl/assets/furniture-kit | CC0; model chọn lọc trong `models/`, bản license đi kèm |
| Animated Characters Protagonists | https://kenney.nl/assets/animated-characters-protagonists | CC0; mesh, rig, Idle/Run và skin trong `characters/`; skin Alex được đổi màu cho bối cảnh |
| DejaVu Sans | Gói font hệ thống DejaVu | Xem `fonts/DejaVu-COPYRIGHT.txt` |
| JetBrains Mono | Gói font hệ thống JetBrains Mono | Xem `fonts/JetBrainsMono-COPYRIGHT.txt` |
| Kiến trúc, cửa, CCTV, rack server, xe, icon | Tạo riêng cho project bằng geometry và SVG | Source nằm trong project |
| Vật liệu concrete/floor | Tạo riêng bằng `tools/generate_textures.py` | Không lấy texture bên ngoài |
| Súng, bước chân, cửa, alarm, ring, ambient | Tổng hợp riêng bằng `tools/generate_audio.py` | Không lấy bản ghi thương mại |

Tài liệu kỹ thuật export Android: https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_android.html

Tham số nền tảng đã được kiểm tra bằng engine thực tế trong `.tools/godot/`, không phụ thuộc phiên bản trang tài liệu trực tuyến.
