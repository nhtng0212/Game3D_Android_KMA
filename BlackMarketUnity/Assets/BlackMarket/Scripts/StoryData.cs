using System;
namespace BlackMarket {
    [Serializable] public class CampaignSave {
        public int version = 1, stage, ammo = 8, reserve = 32, kills;
        public float hp = 100, elapsed;
        public bool armed, drive, tablet, keycard, hasAK, hasNightVision;
        public int selectedWeapon, rifleAmmo, rifleReserve;
    }
    public static class StoryData {
        public static readonly string[] Worlds = {"NorthPointShop", "UtilityBasement", "ArmoryArchive", "SecurityHub", "MedicalStorage", "ServerUplink", "ControlRoom"};
        public static readonly string[] Locations = {"TẦNG 1 / CỬA HÀNG", "B1 / KỸ THUẬT", "B2 / VŨ KHÍ & HỒ SƠ", "B3 / AN NINH", "B4 / Y TẾ & KHO", "B5 / MÁY CHỦ", "B6 / ĐIỀU HÀNH"};
        public static readonly string[] Briefings = {
            "Alex đã vào cửa hàng và khóa cửa trước. Marcus gọi cầu cứu rồi chết trong một 'tai nạn'. Tìm thẻ của chú trong văn phòng để mở máy tính và biết chuyện gì đã xảy ra.",
            "Nhóm người mặc vest chặn lối ra. Alex trốn qua Cửa 006 xuống tầng kỹ thuật. Bốn lính đang lục soát; bạn chưa có súng. Đài phát thanh trong xưởng có thể kéo chúng khỏi cầu thang xuống kho hồ sơ.",
            "Máy quét nhận diện Alex là người thừa kế quyền của Marcus. Tự trang bị tại kho vũ khí, lấy bằng chứng trong Tủ hồ sơ 071, rồi đọc bản ghi GỬI ALEX để hiểu vì sao chú bị giết.",
            "Marcus trao cho Alex máy tính bảng của người quản lý. Victor đang đưa đội thanh trừng xuống cơ sở. Dùng máy quay tìm lối đi, báo động kéo lính đi và cửa B mở tuyến cầu thang xuống dưới.",
            "Tuyến lên mặt đất đã bị phong tỏa. Trạm truyền dữ liệu ở B5 là cách duy nhất đưa bằng chứng ra ngoài. Vượt qua kho hàng và khu y tế bằng cách điều khiển đèn và cửa.",
            "HỒ SƠ 071 chứng minh Victor đã sửa hợp đồng và ra lệnh giết Marcus. Gửi dữ liệu ra ngoài trước khi Victor xóa máy chủ. Tín hiệu truyền sẽ gọi đội thanh trừng đến đây.",
            "Bằng chứng đã được gửi. Victor vẫn giữ quyền điều khiển North Point và khóa đường thoát. Phòng quân nhu bên trái chiếu nghỉ có AK; phòng quang học bên phải có kính nhìn đêm. Nhặt trang bị nếu cần, dùng N bật kính rồi tắt đèn bằng máy tính bảng để chiếm lợi thế. Hạ Victor, giành quyền quản lý rồi quyết định phá hủy hay tiếp quản cơ sở."
        };
        public static string Purpose(string id, int stage) {
            switch(id) {
                case "drawers": return "Marcus giấu thẻ đỏ trong một trong các ngăn tủ sát tường của tủ cá nhân.";
                case "face_scan": return "Thẻ mở cửa ngoài; lớp cửa trong chỉ chấp nhận khuôn mặt Alex Carter.";
                case "basement_exit": return "Sau xác thực, Alex tự chạy xuống cầu thang tới B1.";
                case "keycard": return "Thẻ xác thực quyền mở máy tính của chú. Tài liệu cần đọc nằm trong máy tính.";
                case "computer": return "Đọc hồ sơ cuối cùng Marcus để lại để tìm nơi cất bằng chứng.";
                case "door06": return stage==0 ? "Cửa trước đã bị chặn. Cửa 006 là lối xuống tầng kỹ thuật để thoát nhóm truy bắt." : "Thoát đội lục soát và xuống B2 tìm hồ sơ Marcus đã giấu.";
                case "radio": return "Tiếng đài phát thanh kéo lính khỏi lối xuống. Bạn chưa có súng: tránh vùng đèn pin.";
                case "pistol": return "Trang bị để tự vệ trước khi đi sâu vào cơ sở đang bị thanh trừng.";
                case "order": return "HỒ SƠ 071 là hồ sơ Marcus đã giấu trước khi chết; hãy tìm bản gốc.";
                case "recording": return "Bản ghi của Marcus giải thích bằng chứng và trao quyền điều khiển tòa nhà.";
                case "security": return stage==3 ? "Máy quay tìm đường; báo động dụ lính; cửa B chia cắt lính để bạn tới cầu thang xuống B4." : "Tắt đèn để quan sát chùm đèn pin, dùng cửa chia cắt lính trên đường tới trạm truyền dữ liệu.";
                case "upload": return "Đưa bằng chứng ra ngoài để Victor không thể xóa sạch sự thật.";
                case "survive": return "Giữ mạng tới khi truyền xong; không cần tiêu diệt toàn bộ đội thanh trừng.";
                case "override": return "Victor khóa quyền quản lý và bật bảo vệ. Máy tính cục bộ sẽ gỡ khóa đó.";
                case "boss": return "Phòng trái có AK, phòng phải có kính đêm. Tắt đèn + bật kính [N] để đánh Victor từ chỗ núp.";
                case "final": return "Sự thật đã ra ngoài. Bạn sẽ phá hủy North Point hay trở thành người quản lý?";
                case "exit": return stage==2 ? "Mang máy tính bảng xuống B3 để mở tuyến đi tới máy phát dữ liệu." : stage==5 ? "Dữ liệu đã an toàn; xuống B6 giành quyền điều khiển từ Victor." : "Tiếp tục xuống tầng dưới để đưa HỒ SƠ 071 tới trạm truyền dữ liệu.";
                default: return "Khám phá North Point và tìm đường tới mục tiêu tiếp theo.";
            }
        }
        public static readonly string[] Chapters = {"CỬA HÀNG NORTH POINT", "NHỮNG VỊ KHÁCH KHÔNG MỜI", "KẾ THỪA KHẨN CẤP", "ĐÔI MẮT TRONG BÓNG TỐI", "CHIA CẮT ĐỂ SỐNG SÓT", "BẰNG CHỨNG", "VICTOR HALE"};
        public const string Terminal = "GIỜ THU HỒI: 23:00\nNGƯỜI QUẢN LÝ: MARCUS CARTER\nTÌNH TRẠNG: ĐÃ CHẾT\nNGƯỜI RA LỆNH: VICTOR HALE\n\nAlex, nếu cháu đọc được những dòng này thì cái chết của chú đã bị dựng thành một vụ tai nạn. Victor sẽ cử người tới thu hồi hồ sơ và xóa mọi dấu vết, kể cả cháu.\n\nBản gốc nằm trong TỦ HỒ SƠ 071 ở tầng hầm B2. Thẻ đỏ của chú mở CỬA 006 phía sau các kệ trong kho hàng. Qua cửa ngoài, đứng trước máy quét mặt của lớp cửa trong. Hệ thống đã đăng ký khuôn mặt cháu; chỉ khi xác thực xong cháu mới xuống được tầng hầm. Chú từng dặn cháu đừng mở nó vì muốn giữ cháu tránh xa nơi này. Nhưng nếu cửa trước đã bị chặn, đó là đường thoát duy nhất.\n\nHãy tìm bản ghi GỬI ALEX trong kho hồ sơ. Đừng để chúng lấy được bằng chứng.\n";
        public const string Prologue = "02:17 SÁNG\n\nMARCUS CARTER\nAlex… nghe chú nói. Đừng tin bất kỳ ai biết tên North Point. Nếu chú không gọi lại, hãy đến cửa hàng.\n\nVà Alex… đừng mở Cửa 006.\n\nSáng hôm sau, Alex nhận tin Marcus chết trong một tai nạn. Ba ngày sau, lúc 21:30, anh vào cửa hàng North Point và khóa cửa trước. Anh cần biết vì sao chú chết. Marcus cất thẻ đỏ trong một trong các ngăn tủ sát tường của tủ cá nhân tại văn phòng. Tìm thẻ để mở máy tính, rồi mở tệp Order 71.";
        public const string Recording = "North Point là một cơ sở của Chợ Đen. Chúng cung cấp vũ khí, danh tính và hợp đồng cho các đặc vụ. Chú là người quản lý của nơi này.\n\nVictor Hale đã sửa những hợp đồng bảo vệ thành lệnh ám sát. Những người quản lý biết sự thật đều bị thủ tiêu. HỒ SƠ 071 là bằng chứng.\n\nChú đã để lại quyền quản lý khẩn cấp cho cháu. Đừng cố thắng chúng bằng súng. Hãy dùng chính tòa nhà.\n\nPHÁT HIỆN XÂM NHẬP / ĐÃ CHO PHÉP THANH TRỪNG";
        public const string DestroyEnding = "Alex phát tán HỒ SƠ 071 và kích hoạt thanh tẩy North Point. Bình minh đến khi cửa hàng chỉ còn là tro bụi.\n\nMột điện thoại vô danh sáng lên:\nLỆNH 072 — MỤC TIÊU: ALEX CARTER.\n\nSự thật đã được công bố. Chợ Đen vẫn còn tồn tại.";
        public const string AcceptEnding = "North Point trở lại trực tuyến. Alex Carter được ghi vào hệ thống với tư cách người quản lý.\n\nĐiện thoại trên bàn Marcus đổ chuông.\n“Chúng tôi có một hợp đồng mới.”\n\nAlex nhìn Cửa 006. Lần này, anh biết phía sau nó là gì.";
    }
}
