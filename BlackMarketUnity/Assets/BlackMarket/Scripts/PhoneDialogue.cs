using System.Collections.Generic;
namespace BlackMarket {
    // Stable node IDs also make the dialogue graph inspectable and testable.
    public static class PhoneDialogue {
        public class Choice { public string text,next,effect; public Choice(string text,string next,string effect=""){this.text=text;this.next=next;this.effect=effect;} }
        public class Node {public string speaker,text;public Choice[] choices;public Node(string speaker,string text,params Choice[] choices){this.speaker=speaker;this.text=text;this.choices=choices;} }
        public static readonly Dictionary<string,Node> Nodes=new Dictionary<string,Node>{
            {"start",new Node("MARCUS","Alex… nghe chú. Nếu chú không gọi lại, hãy đến cửa hàng North Point. Đừng đi ngay đêm nay. Chờ khi mọi thứ yên xuống.",new Choice("Chú đang gặp nguy hiểm sao?","danger"),new Choice("Cháu cần làm gì ở cửa hàng?","card"),new Choice("Sao chú từng dặn cháu tránh Cửa 006?","door"))},
            {"danger",new Node("MARCUS","Có người muốn chú im lặng. Chú đã giữ lại thứ mà chúng muốn xóa sạch.",new Choice("Để cháu gọi cảnh sát!","police"),new Choice("Ai đang truy đuổi chú?","hunters","hunters"))},
            {"police",new Node("MARCUS","Đừng nhắc tên North Point qua điện thoại nữa. Chú không biết ai đang nghe. Hãy tìm bằng chứng trước.",new Choice("Bằng chứng ở đâu?","card"),new Choice("Còn Cửa 006 thì sao?","door"))},
            {"hunters",new Node("MARCUS","Những người mặc vest đen. Đừng tưởng họ là khách. Nếu thấy họ tới cửa hàng, đừng để họ nhìn thấy cháu.",new Choice("Cháu phải tìm thứ gì?","card"),new Choice("Cháu sẽ cẩn thận. Cửa 006 là gì?","door"))},
            {"card",new Node("MARCUS","Thẻ đỏ nằm trong một ngăn kéo ở văn phòng chú. Chú để nhiều tủ sát tường; cháu sẽ phải mở từng ngăn để tìm.",new Choice("Thẻ đó dùng để làm gì?","computer","card"),new Choice("Cháu hiểu. Nhưng Cửa 006 là gì?","door","card"))},
            {"computer",new Node("MARCUS","Dùng thẻ mở máy tính của chú. Trên màn hình có tệp Order 71. Đọc hết nó rồi cháu sẽ hiểu vì sao chú gọi.",new Choice("Cháu sẽ tìm thẻ và đọc tệp đó.","door","card"))},
            {"door",new Node("MARCUS","Cửa 006 ở cuối nhà kho. Thẻ đỏ mở được lớp cửa đầu tiên, nhưng đằng sau nó… Hứa với chú, đừng xuống đó.",new Choice("Cháu hứa, chú Marcus.","promise","promise"),new Choice("Chú đang giấu cháu chuyện gì?","secret"))},
            {"promise",new Node("MARCUS","Cảm ơn cháu. Chú chỉ mong cháu được sống một cuộc đời bình thường.",new Choice("Chú đang ở đâu? Để cháu tới đón.","cut"))},
            {"secret",new Node("MARCUS","Chú đã sai khi nghĩ mình có thể kiểm soát mọi thứ. Tệp trong máy tính sẽ nói thay chú. Xin lỗi, Alex.",new Choice("Chú đừng cúp máy!","cut"))},
            {"cut",new Node("MARCUS","Khoan… có tiếng bước chân. Alex, nhớ lời chú. Đừng tin—\n\n[Tiếng va đập. Cuộc gọi bị ngắt.]",new Choice("Chú Marcus? Chú nghe cháu không?","end"))}
        };
        public static void Apply(CampaignSave state,string effect){if(effect=="card")state.askedAboutCard=true;if(effect=="hunters")state.askedAboutHunters=true;if(effect=="promise")state.promisedMarcus=true;}
    }
}
