using UnityEngine;
namespace BlackMarket {
    public static class StealthGuidance {
        public static string Current(Campaign g){
            if(g.state.stage==0){
                if(g.objectiveId=="door06")return "Rời văn phòng, theo hành lang giữa qua kho tới biển 06 ở cuối cửa hàng. Dùng thẻ Marcus tại máy quét [E].";
                if(g.objectiveId=="computer")return "Thẻ đã lấy. Đứng cạnh bàn Marcus và nhấn [E] tại TERMINAL để đọc ORDER 071.";
                return "Đi theo lối giữa → văn phòng MARCUS bên trái. [E] mở cửa; lấy thẻ rồi đọc máy tính. [J] xem lại hướng dẫn.";
            }
            foreach(var enemy in g.enemies)if(enemy.Armed && !enemy.boss)return "Lính đã rút súng: lùi sau tường hoặc vật chắn, cắt đường nhìn và chờ chúng tìm kiếm xong; chưa băng qua hành lang lúc này.";
            if(g.state.stage==1){
                if(g.flags.Contains("radio"))return "Radio đã bật: rời bàn theo cửa phụ bên phải, vòng sau thùng hàng → máy quét cuối tầng. Tránh hướng lính đang tới radio.";
                if(g.player.transform.position.z<14)return "Đi khom [C], vòng đầu trái dãy kệ trước cầu thang, rồi men tường phải qua SERVICE sau tủ WC. Nấp trước cửa phụ Workshop; quan sát đèn pin.";
                EnemyController guard=null;foreach(var e in g.enemies)if(e.routeId=="enemy_b")guard=e;
                if(guard && guard.PatrolRestRemaining>=5 && guard.transform.position.z>26 && !guard.CanSee())return "Lính Workshop còn dừng khoảng "+Mathf.FloorToInt(guard.PatrolRestRemaining)+" giây. Đi khom tới radio bên trái cửa phụ, nhấn [E], rồi rút ra lối SERVICE.";
                return "Chờ sau vách SERVICE tới khi lính Workshop đi qua cửa phía sau và dừng ở cuối hành lang (cách radio khoảng 6 m). Đợi đèn quay đi rồi tới radio.";
            }
            if(g.state.stage==2)return "Tầng hồ sơ chưa có lính. Mở cửa quân nhu bên trái để lấy súng → Locker 071 bên phải → terminal FOR_ALEX ở giữa.";
            if(g.state.stage==3)return "Nấp sau vách trước phòng camera. Đợi lính đi về cuối phòng rồi mở Tablet; xem camera, bật báo động và đóng cửa B để tách lính.";
            if(g.state.stage==4)return "Qua cánh y tế bên trái, núp sau giường. Chờ lính đi qua cửa cách ly về cuối tầng, rồi đi khom tới vật che kế tiếp; dùng camera kiểm tra lối ra.";
            if(g.state.stage==5)return g.upload>0?"Đóng cửa B bằng Tablet, rời vị trí vừa gây tiếng động và nấp sau tủ gần Uplink. Chờ truyền xong; kiểm tra camera trước khi ra cầu thang.":"Chưa bật Uplink thì chưa có đội truy đuổi. Nhận tiếp tế, xem đường tới cửa B và tìm chỗ nấp trước khi nhấn [E] bắt đầu truyền.";
            return g.security.revoked?"Đi qua phòng bên trái để tới LOCAL OVERRIDE. Dùng bàn và cột chắn đạn; khôi phục quyền trước khi bắn Victor tiếp.":"Mở phòng trái lấy AK, phòng phải lấy kính đêm. Chờ lính hỗ trợ quay đi, nấp sau bàn; tắt đèn bằng Tablet rồi bật kính [N].";
        }
    }
}
