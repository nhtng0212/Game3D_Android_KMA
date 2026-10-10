using UnityEngine;
namespace BlackMarket {
    public static class StealthGuidance {
        public static string Current(Campaign g){
            if(g.state.stage==0){
                string use=g.ui.TouchControls?"chạm DÙNG":"nhấn [E]";
                switch(g.objectiveId){
                    case "door06":return "Chạy vào kho, vòng qua các kệ tới Cửa 006 và "+use+" để dùng USB đỏ. Sau đó vào khoang quét mặt.";
                    case "face_scan":return "Đi qua cửa ngoài, đứng trên dấu chân rồi "+use+" tại máy quét mặt của lớp cửa trong.";
                    case "basement_exit":return "Alex đang tự chạy xuống cầu thang. Chờ tới B1 để tiếp tục điều khiển.";
                    case "keycard":return "Đến sát ngăn kéo đang mở, "+use+" để nhặt USB đỏ.";
                    case "computer":return "Đến máy tính trên bàn Marcus, "+use+". Bấm tệp Order 71 trên màn hình rồi đọc hết từng trang.";
                    default:return "Vào văn phòng MARCUS cuối cửa hàng bên trái. Đến tủ cá nhân, "+use+" để mở lần lượt các ngăn tủ sát tường tìm USB đỏ.";
                }
            }
            if(g.underground && g.underground.State==UndergroundCampaign.Encounter.Resupply)return "Nhặt túi chiến lợi phẩm cạnh xác địch để lấy máu, đạn rồi nạp súng. Quân tiếp viện sẽ tới khi hết thời gian nghỉ.";
            if(g.objectiveId=="breaker")return "Bắn tủ điện bên trái màn hình trung tâm. "+(g.ui.TouchControls?"Chạm KÍNH ĐÊM":"Nhấn [N]")+" để nhìn trong bóng tối; máy chủ có nguồn riêng.";
            if(g.objectiveId=="retreat")return "Theo dấu mục tiêu đến cửa cuối phòng. Không cần hạ hết quân tiếp viện để rút lui.";
            if(g.objectiveId=="escape")return "Quay lại cầu thang nơi vừa đi xuống. Đến gần chân cầu thang để thoát. Đồng hồ chỉ chạy sau khi đọc hết hồ sơ.";
            if(g.ui.TouchControls)return "Nấp sau bàn, cột; chạy khỏi vòng cảnh báo lựu đạn. Dùng NGẮM, BẮN, NẠP và ĐỔI SÚNG; chạm DÙNG khi tới mục tiêu.";
            return "Nấp sau bàn, cột; chạy khỏi vòng cảnh báo lựu đạn. Chuột phải ngắm, chuột trái bắn; R nạp, Q đổi súng, G ném lựu đạn.";
        }
    }
}
