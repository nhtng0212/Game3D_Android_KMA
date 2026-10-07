using UnityEngine;
namespace BlackMarket {
    public static class VietnameseText {
        // Also used by the editor migration so visible map labels remain Vietnamese after saving.
        static readonly string[,] Terms={
            {"STAIRS  /  DOWN","CẦU THANG / ĐI XUỐNG"},{"ENCRYPTED","ĐÃ MÃ HÓA"},{"UPLINK READY","SẴN SÀNG TRUYỀN DỮ LIỆU"},{"TRẠM TRUYỀN DỮ LIỆU READY","SẴN SÀNG TRUYỀN DỮ LIỆU"},{"HOME / TECH","ĐIỆN TỬ GIA DỤNG"},{"CAM / 071","MÁY ẢNH / 071"},{"NORTH / PC","NORTH / MÁY TÍNH"},{"THU NGAN / CHECKOUT","QUẦY THU NGÂN"},{"CHECKOUT","THU NGÂN"},{"IN STOCK","CÒN HÀNG"},{"DEMO / NORTH","HÀNG TRƯNG BÀY"},{"STOCK / 071","HÀNG LƯU KHO / 071"},{"LAPTOP","MÁY TÍNH XÁCH TAY"},{"NORTH\nVISION","NORTH\nHÌNH ẢNH"},{"WC NAM / MEN","NHÀ VỆ SINH NAM"},{"WC NU / WOMEN","NHÀ VỆ SINH NỮ"},
            {"NORTH POINT / ELECTRONICS","NORTH POINT / ĐIỆN TỬ"},{"CLOSED / 21:30","ĐÃ ĐÓNG CỬA / 21:30"},{"M. CARTER / OFFICE","VĂN PHÒNG / M. CARTER"},{"KHO HANG / STOCKROOM","KHO HÀNG"},{"06 / STAFF STAIRS","06 / LỐI XUỐNG HẦM"},
            {"ARMORY / PERSONAL DEFENCE","KHO VŨ KHÍ / TỰ VỆ"},{"FOR_ALEX / PRIVATE ARCHIVE","GỬI ALEX / HỒ SƠ RIÊNG"},{"STAFF BREAK ROOM","PHÒNG NGHỈ NHÂN VIÊN"},{"NORTH POINT OPERATIONS","ĐIỀU HÀNH NORTH POINT"},
            {"SUCCESSOR TERMINAL","MÁY TÍNH KẾ THỪA"},{"LOCAL OVERRIDE","KHÔI PHỤC TẠI CHỖ"},{"CARTER SCANNER","MÁY QUÉT CARTER"},{"CONTROL ROOM","PHÒNG ĐIỀU HÀNH"},{"ACCESS CONTROL","KIỂM SOÁT RA VÀO"},{"EQUIPMENT CHECK","KIỂM TRA THIẾT BỊ"},
            {"EXTERNAL UPLINK","TRUYỀN DỮ LIỆU RA NGOÀI"},{"KEEPER ARCHIVE","HỒ SƠ NGƯỜI QUẢN LÝ"},{"DATA HALL","PHÒNG DỮ LIỆU"},{"DATA DRIVE","Ổ DỮ LIỆU"},{"ORDER ROOM","PHÒNG HỒ SƠ"},{"ORDER #071","HỒ SƠ 071"},{"ORDER 071","HỒ SƠ 071"},{"DOOR 06","CỬA 06"},{"FOR_ALEX","GỬI ALEX"},{"THE MARKET","CHỢ ĐEN"},
            {"PERSONAL DEFENCE","TỰ VỆ"},{"SURVEILLANCE","GIÁM SÁT"},{"KEEPERS","NGƯỜI QUẢN LÝ"},{"KEEPER","NGƯỜI QUẢN LÝ"},{"TERMINAL","MÁY TÍNH"},{"SUCCESSOR","NGƯỜI KẾ THỪA"},
            {"MAINTENANCE","BẢO TRÌ"},{"ELECTRICAL","PHÒNG ĐIỆN"},{"GENERATOR","MÁY PHÁT ĐIỆN"},{"WORKSHOP","XƯỞNG KỸ THUẬT"},{"SERVICE","LỐI KỸ THUẬT"},{"INTERVIEW","THẨM VẤN"},{"SECURITY","AN NINH"},{"ISOLATION","CÁCH LY"},{"PHARMACY","DƯỢC PHẨM"},{"MEDICAL","Y TẾ"},{"TRIAGE","SƠ CỨU"},{"WARD","BUỒNG BỆNH"},
            {"LOCKERS","TỦ ĐỒ"},{"LOCKER","TỦ HỒ SƠ"},{"ARMORY","KHO VŨ KHÍ"},{"PISTOL","SÚNG NGẮN"},{"STORAGE","KHO HÀNG"},{"FREIGHT","HÀNG VẬN CHUYỂN"},{"RETURNS","HÀNG TRẢ LẠI"},{"INVENTORY","KIỂM KÊ"},{"BACKUP","DỰ PHÒNG"},
            {"CCTV / LIVE","MÁY QUAY / TRỰC TIẾP"},{"NETWORK","MẠNG"},{"ROUTING","ĐỊNH TUYẾN"},{"SERVER","MÁY CHỦ"},{"UPLINK","TRẠM TRUYỀN DỮ LIỆU"},{"UPS","BỘ LƯU ĐIỆN"},{"COOLING","LÀM MÁT"},{"COMMAND","CHỈ HUY"},{"STAFF","NHÂN VIÊN"},{"EVIDENCE","BẰNG CHỨNG"},
            {"COMPUTING","MÁY TÍNH"},{"APPLIANCES","ĐỒ GIA DỤNG"},{"CAMERAS","MÁY ẢNH"},{"AUDIO","ÂM THANH"},{"RADIO","ĐÀI PHÁT THANH"},{"WC","NHÀ VỆ SINH"}
        };
        public static string Translate(string value){if(string.IsNullOrEmpty(value))return value;for(int i=0;i<Terms.GetLength(0);i++)value=value.Replace(Terms[i,0],Terms[i,1]);return value;}
        public static void TranslateLabel(TextMesh text){string old=text.text;string value=Translate(old);if(value==old)return;text.text=value;int before=0,after=0;foreach(var line in old.Split('\n'))before=Mathf.Max(before,line.Length);foreach(var line in value.Split('\n'))after=Mathf.Max(after,line.Length);if(after>before && before>0)text.characterSize*=Mathf.Max(.58f,(float)before/after);}
        public static void Apply(GameObject root){
            foreach(var t in root.GetComponentsInChildren<TextMesh>(true))TranslateLabel(t);
            foreach(var i in root.GetComponentsInChildren<Interaction>(true))i.title=Translate(i.title);
            foreach(var d in root.GetComponentsInChildren<RoomDoor>(true))d.roomName=Translate(d.roomName);
        }
    }
}
