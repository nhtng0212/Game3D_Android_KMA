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
            "Alex đã vào cửa hàng và khóa cửa trước. Marcus gọi cầu cứu rồi chết trong một 'tai nạn'. Tìm thẻ của chú trong văn phòng để mở terminal và biết chuyện gì đã xảy ra.",
            "SUV chặn lối ra. Alex trốn qua Door 06 xuống tầng kỹ thuật. Bốn lính đang lục soát; bạn chưa có súng. Radio trong xưởng có thể kéo chúng khỏi cầu thang xuống kho hồ sơ.",
            "Máy quét nhận diện Alex là người thừa kế quyền của Marcus. Tự trang bị tại Armory, lấy bằng chứng trong Locker 071, rồi đọc bản ghi FOR_ALEX để hiểu vì sao chú bị giết.",
            "Marcus trao cho Alex Tablet của Keeper. Victor đang đưa đội thanh trừng xuống cơ sở. Dùng camera tìm lối đi, báo động kéo lính đi và cửa B mở tuyến cầu thang xuống dưới.",
            "Tuyến lên mặt đất đã bị phong tỏa. Máy phát Uplink ở B5 là cách duy nhất đưa bằng chứng ra ngoài. Vượt qua kho hàng và khu y tế bằng cách điều khiển đèn và cửa.",
            "ORDER 071 chứng minh Victor đã sửa hợp đồng và ra lệnh giết Marcus. Gửi dữ liệu ra ngoài trước khi Victor xóa máy chủ. Tín hiệu truyền sẽ gọi đội thanh trừng đến đây.",
            "Bằng chứng đã được gửi. Victor vẫn giữ quyền điều khiển North Point và khóa đường thoát. Phòng quân nhu bên trái chiếu nghỉ có AK; phòng quang học bên phải có kính nhìn đêm. Nhặt trang bị nếu cần, dùng N bật kính rồi tắt đèn bằng Tablet để chiếm lợi thế. Hạ Victor, giành quyền Keeper rồi quyết định phá hủy hay tiếp quản cơ sở."
        };
        public static string Purpose(string id, int stage) {
            switch(id) {
                case "keycard": return "Thẻ của Marcus mở terminal — manh mối về cái chết của chú.";
                case "computer": return "Đọc hồ sơ cuối cùng Marcus để lại để tìm nơi cất bằng chứng.";
                case "door06": return stage==0 ? "Door 06 dẫn tới kho hồ sơ bí mật mà terminal vừa nhắc đến." : "Thoát đội lục soát và xuống B2 tìm hồ sơ Marcus đã giấu.";
                case "radio": return "Tiếng radio kéo lính khỏi lối xuống. Bạn chưa có súng: tránh vùng đèn pin.";
                case "pistol": return "Trang bị để tự vệ trước khi đi sâu vào cơ sở đang bị thanh trừng.";
                case "order": return "ORDER 071 là hồ sơ Marcus đã giấu trước khi chết; hãy tìm bản gốc.";
                case "recording": return "Bản ghi của Marcus giải thích bằng chứng và trao quyền điều khiển tòa nhà.";
                case "security": return stage==3 ? "Camera tìm đường; báo động dụ lính; cửa B chia cắt lính để bạn tới cầu thang xuống B4." : "Tắt đèn để quan sát chùm đèn pin, dùng cửa chia cắt lính trên đường tới Uplink.";
                case "upload": return "Đưa bằng chứng ra ngoài để Victor không thể xóa sạch sự thật.";
                case "survive": return "Giữ mạng tới khi truyền xong; không cần tiêu diệt toàn bộ đội thanh trừng.";
                case "override": return "Victor khóa quyền Keeper và bật bảo vệ. Terminal cục bộ sẽ gỡ khóa đó.";
                case "boss": return "Phòng trái có AK, phòng phải có kính đêm. Tắt đèn + bật kính [N] để đánh Victor từ chỗ núp.";
                case "final": return "Sự thật đã ra ngoài. Bạn sẽ phá hủy North Point hay trở thành Keeper?";
                case "exit": return stage==2 ? "Mang Tablet xuống B3 để mở tuyến đi tới máy phát dữ liệu." : stage==5 ? "Dữ liệu đã an toàn; xuống B6 giành quyền điều khiển từ Victor." : "Tiếp tục xuống tầng dưới để đưa ORDER 071 tới máy phát Uplink.";
                default: return "Khám phá North Point và tìm đường tới mục tiêu tiếp theo.";
            }
        }
        public static readonly string[] Chapters = {"NORTH POINT SUPPLY", "VISITORS ARRIVE", "EMERGENCY SUCCESSION", "EYES IN THE DARK", "DIVIDE & SURVIVE", "THE EVIDENCE", "VICTOR HALE"};
        public const string Prologue = "02:17 AM\n\nMARCUS CARTER\nAlex… nghe chú nói. Đừng tin bất kỳ ai biết tên North Point. Nếu chú không gọi lại, hãy đến cửa hàng.\n\nVà Alex… đừng mở Door 06.\n\nSáng hôm sau, Alex nhận tin Marcus chết trong một tai nạn. Ba ngày sau, lúc 21:30, anh vào cửa hàng North Point Supply và khóa cửa trước. Anh cần biết vì sao chú chết. Thẻ trên bàn Marcus sẽ mở terminal trong văn phòng.";
        public const string Recording = "North Point là một cơ sở của The Market. Chúng cung cấp vũ khí, danh tính và contract cho các Operator. Chú là Keeper của nơi này.\n\nVictor Hale đã sửa những contract bảo vệ thành lệnh ám sát. Các Keeper biết sự thật đều bị thủ tiêu. ORDER 071 là bằng chứng.\n\nChú đã để lại quyền Emergency Keeper Access cho cháu. Đừng cố thắng chúng bằng súng. Hãy dùng chính tòa nhà.\n\nNETWORK BREACH DETECTED / PURGE AUTHORIZED";
        public const string DestroyEnding = "Alex phát tán ORDER 071 và kích hoạt thanh tẩy North Point. Bình minh đến khi cửa hàng chỉ còn là tro bụi.\n\nMột điện thoại vô danh sáng lên:\nORDER #072 — TARGET: ALEX CARTER.\n\nSự thật đã được công bố. The Market vẫn còn tồn tại.";
        public const string AcceptEnding = "North Point trở lại trực tuyến. Alex Carter được ghi vào hệ thống với tư cách Keeper.\n\nĐiện thoại trên bàn Marcus đổ chuông.\n“Chúng tôi có một contract mới.”\n\nAlex nhìn Door 06. Lần này, anh biết phía sau nó là gì.";
    }
}
