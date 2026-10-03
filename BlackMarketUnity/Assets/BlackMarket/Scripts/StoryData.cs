using System;
namespace BlackMarket {
    [Serializable] public class CampaignSave {
        public int version = 1, stage, ammo = 8, reserve = 32, kills;
        public float hp = 100, elapsed;
        public bool armed, drive, tablet, keycard;
    }
    public static class StoryData {
        public static readonly string[] Chapters = {"NORTH POINT SUPPLY", "VISITORS ARRIVE", "EMERGENCY SUCCESSION", "EYES IN THE DARK", "DIVIDE & SURVIVE", "THE EVIDENCE", "VICTOR HALE"};
        public const string Prologue = "02:17 AM\n\nMARCUS CARTER\nAlex… nghe chú nói. Đừng tin bất kỳ ai biết tên North Point. Nếu chú không gọi lại, hãy đến cửa hàng.\n\nVà Alex… đừng mở Door 06.\n\nSáng hôm sau, Alex nhận tin Marcus chết trong một tai nạn. Ba ngày sau, anh trở lại North Point Supply.";
        public const string Recording = "North Point là một cơ sở của The Market. Chúng cung cấp vũ khí, danh tính và contract cho các Operator. Chú là Keeper của nơi này.\n\nVictor Hale đã sửa những contract bảo vệ thành lệnh ám sát. Các Keeper biết sự thật đều bị thủ tiêu. ORDER 071 là bằng chứng.\n\nChú đã để lại quyền Emergency Keeper Access cho cháu. Đừng cố thắng chúng bằng súng. Hãy dùng chính tòa nhà.\n\nNETWORK BREACH DETECTED / PURGE AUTHORIZED";
        public const string DestroyEnding = "Alex phát tán ORDER 071 và kích hoạt thanh tẩy North Point. Bình minh đến khi cửa hàng chỉ còn là tro bụi.\n\nMột điện thoại vô danh sáng lên:\nORDER #072 — TARGET: ALEX CARTER.\n\nSự thật đã được công bố. The Market vẫn còn tồn tại.";
        public const string AcceptEnding = "North Point trở lại trực tuyến. Alex Carter được ghi vào hệ thống với tư cách Keeper.\n\nĐiện thoại trên bàn Marcus đổ chuông.\n“Chúng tôi có một contract mới.”\n\nAlex nhìn Door 06. Lần này, anh biết phía sau nó là gì.";
    }
}
