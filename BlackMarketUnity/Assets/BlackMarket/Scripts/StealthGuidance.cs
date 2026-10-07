using UnityEngine;
namespace BlackMarket {
    public static class StealthGuidance {
        public static string Current(Campaign g){
            if(g.state.stage==0){
                string use=g.ui.TouchControls?"chạm DÙNG":"nhấn [E]";
                switch(g.objectiveId){
                    case "door06":return "Chạy vào kho, vòng qua các kệ tới Cửa 006 và "+use+" để dùng thẻ đỏ. Sau đó vào khoang quét mặt.";
                    case "face_scan":return "Đi qua cửa ngoài, đứng trên dấu chân rồi "+use+" tại máy quét mặt của lớp cửa trong.";
                    case "basement_exit":return "Alex đang tự chạy xuống cầu thang. Chờ tới B1 để tiếp tục điều khiển.";
                    case "keycard":return "Đến sát ngăn kéo đang mở, "+use+" để nhặt thẻ đỏ.";
                    case "computer":return "Đến máy tính trên bàn Marcus, "+use+". Bấm tệp Order 71 trên màn hình rồi đọc hết từng trang.";
                    default:return "Vào văn phòng MARCUS cuối cửa hàng bên trái. Đến tủ cá nhân, "+use+" để mở lần lượt các ngăn tủ sát tường tìm thẻ đỏ.";
                }
            }
            foreach(var enemy in g.enemies)if(enemy.Armed && !enemy.boss)return "Lính đã rút súng: lùi sau tường hoặc vật chắn, cắt đường nhìn và chờ chúng tìm kiếm xong; chưa băng qua hành lang lúc này.";
            if(g.state.stage==1){
                if(g.flags.Contains("radio"))return "Đài phát thanh đã bật: rời bàn theo cửa phụ bên phải, vòng sau thùng hàng → máy quét cuối tầng. Tránh hướng lính đang tới đài phát thanh.";
                if(g.player.transform.position.z<14)return "Đi khom [C], vòng đầu trái dãy kệ trước cầu thang, rồi men tường phải qua LỐI KỸ THUẬT sau tủ WC. Nấp trước cửa phụ xưởng kỹ thuật; quan sát đèn pin.";
                EnemyController guard=null;foreach(var e in g.enemies)if(e.routeId=="enemy_b")guard=e;
                if(guard && guard.PatrolRestRemaining>=5 && guard.transform.position.z>26 && !guard.CanSee())return "Lính xưởng kỹ thuật còn dừng khoảng "+Mathf.FloorToInt(guard.PatrolRestRemaining)+" giây. Đi khom tới đài phát thanh bên trái cửa phụ, nhấn [E], rồi rút ra LỐI KỸ THUẬT.";
                return "Chờ sau vách LỐI KỸ THUẬT tới khi lính xưởng kỹ thuật đi qua cửa phía sau và dừng ở cuối hành lang (cách đài phát thanh khoảng 6 m). Đợi đèn quay đi rồi tới đài phát thanh.";
            }
            if(g.state.stage==2)return "Tầng hồ sơ chưa có lính. Mở cửa quân nhu bên trái để lấy súng → Tủ hồ sơ 071 bên phải → máy tính GỬI ALEX ở giữa.";
            if(g.state.stage==3)return "Nấp sau vách trước phòng máy quay. Đợi lính đi về cuối phòng rồi mở máy tính bảng; xem máy quay, bật báo động và đóng cửa B để tách lính.";
            if(g.state.stage==4)return "Qua cánh y tế bên trái, núp sau giường. Chờ lính đi qua cửa cách ly về cuối tầng, rồi đi khom tới vật che kế tiếp; dùng máy quay kiểm tra lối ra.";
            if(g.state.stage==5)return g.upload>0?"Đóng cửa B bằng máy tính bảng, rời vị trí vừa gây tiếng động và nấp sau tủ gần trạm truyền dữ liệu. Chờ truyền xong; kiểm tra máy quay trước khi ra cầu thang.":"Chưa bật trạm truyền dữ liệu thì chưa có đội truy đuổi. Nhận tiếp tế, xem đường tới cửa B và tìm chỗ nấp trước khi nhấn [E] bắt đầu truyền.";
            return g.security.revoked?"Đi qua phòng bên trái để tới KHÔI PHỤC TẠI CHỖ. Dùng bàn và cột chắn đạn; khôi phục quyền trước khi bắn Victor tiếp.":"Mở phòng trái lấy AK, phòng phải lấy kính đêm. Chờ lính hỗ trợ quay đi, nấp sau bàn; tắt đèn bằng máy tính bảng rồi bật kính [N].";
        }
    }
}
