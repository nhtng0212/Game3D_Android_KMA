using System;
namespace BlackMarket {
 [Serializable] public class CampaignSave {
  public bool corpseLootRecorded;
  public System.Collections.Generic.List<UnityEngine.Vector3> corpseLoot=new System.Collections.Generic.List<UnityEngine.Vector3>();
  public int version=2,stage,ammo=8,reserve=32,kills,selectedWeapon,rifleAmmo,rifleReserve,grenades,sniperAmmo=5,sniperReserve=25;
  public float hp=100,elapsed;
  public bool armed,drive,tablet,keycard,hasAK,hasSniper,hasNightVision,introCompleted,askedAboutCard,askedAboutHunters,promisedMarcus;
  // Checkpoints are encounter boundaries, not mid-animation snapshots.
  public int checkpoint,reinforcementDeaths;
  public bool victorWounded,lightsDestroyed,codeRecovered,evidenceCopied,campaignComplete;
 }
 public static class StoryData {
  public static readonly string[] Worlds={"NorthPointShop","UndergroundArmory","UndergroundControl"};
  public static readonly string[] Locations={"MÀN 1 / NORTH POINT","MÀN 2 / KHO QUÂN GIỚI","MÀN 3 / TRUNG TÂM ĐIỀU PHỐI"};
  public static readonly string[] Chapters={"CUỘC GỌI CUỐI CÙNG","CĂN CỨ DƯỚI LÒNG ĐẤT","SỰ THẬT VỀ ORDER 71"};
  public static readonly string[] Briefings={
   "Marcus để lại USB bảo mật màu đỏ trong ngăn kéo văn phòng. Dùng USB mở máy tính và đọc tệp Order 71 để tìm hiểu điều chú muốn nhắn lại.",
   "Cửa 006 đóng sau lưng Alex. Kho vũ khí và các trạm máy tính cho thấy đây là căn cứ ngầm. Trang bị trước khi tìm hiểu hồ sơ vận hành.",
   "Trung tâm điều phối có nguồn điện dự phòng riêng cho máy chủ. Dùng kính nhìn đêm đã lấy ở kho quân giới và bắn hỏng tủ điện chiếu sáng để giành lợi thế trước Victor."
  };
  public static string Purpose(string id,int stage){switch(id){
   case "drawers":return "Marcus giấu USB bảo mật màu đỏ trong một ngăn kéo ở văn phòng.";
   case "keycard":return "USB chứa khóa xác thực của Marcus; mang nó tới máy tính.";
   case "computer":return "Đọc Order 71 để tìm manh mối về cái chết của Marcus.";
   case "door06":return "Nhóm truy bắt chặn lối ra. Cửa 006 là đường thoát duy nhất.";
   case "face_scan":return "USB mở lớp cửa ngoài; lớp cửa trong xác thực khuôn mặt Alex.";
   case "basement_exit":return "Cửa an ninh đóng phía sau, tạm thời ngăn nhóm truy bắt.";
   case "sniper":case "pistol":case "ak":case "grenades":return "Lấy AK, súng ngắm và bộ trang bị gồm lựu đạn, đạn dự trữ, kính nhìn đêm.";
   case "base_computer":return "Máy tính vận hành có thể chỉ ra nơi lưu mã mở phần còn lại của Order 71.";
   case "first_wave":return "Đội truy bắt đã phá cửa 006. Tiêu diệt cả 30 tên để giữ đường xuống.";
   case "assault":return "Victor có quân tiếp viện. Giữ vật che và tìm thời cơ rút lui.";
   case "retreat":return "Rút xuống trung tâm điều phối. Alex sẽ băng bó tạm trên đường; kính nhìn đêm đã có trong bộ trang bị ở kho.";
   case "medical":return "Nhặt hộp tiếp tế trong phòng điều phối để bổ sung máu và đạn.";
   case "nightvision":return "Kính khuếch đại ánh sáng yếu; cần thiết khi điện chiếu sáng bị ngắt.";
   case "control_entry":return "Máy chủ nằm cuối phòng điều phối. Victor đang đuổi theo.";
   case "breaker":return "Tắt đèn để vô hiệu hóa tầm nhìn của thuộc hạ; máy chủ vẫn dùng nguồn dự phòng.";
   case "boss":return "Victor cũng có kính đêm. Di chuyển giữa các cột và đừng đứng yên khi bắn.";
   case "final_terminal":return "USB và máy chủ điều phối cùng xác thực quyền kế thừa của Alex.";
   case "escape":return "Bằng chứng đã an toàn. Quy trình tiêu hủy của Victor sắp phá sập căn cứ.";
   default:return "Tìm sự thật về Marcus và mang bằng chứng rời North Point.";
  }}
  public const string UnlockCode="7106201723";
  public const string Terminal="GIỜ THU HỒI: 23:00\nNGƯỜI QUẢN LÝ: MARCUS CARTER\nTÌNH TRẠNG: ĐÃ CHẾT\nNGƯỜI RA LỆNH: VICTOR HALE\n\nAlex, nếu cháu đọc được những dòng này thì cái chết của chú đã bị dựng thành một vụ tai nạn. Victor sẽ cử người tới thu hồi hồ sơ và xóa mọi dấu vết, kể cả cháu.\n\nUSB đỏ của chú mở CỬA 006 phía sau các kệ trong kho hàng. Qua cửa ngoài, đứng trước máy quét mặt của lớp cửa trong. Hệ thống đã đăng ký khuôn mặt cháu. Chú từng dặn cháu đừng mở nó vì muốn giữ cháu tránh xa nơi này. Nhưng nếu cửa trước đã bị chặn, đó là đường thoát duy nhất.\n\nĐừng giao USB cho bất kỳ ai. Phần bằng chứng đầy đủ được bảo vệ riêng bên dưới.\n\nPHỤ LỤC MẬT — ĐÃ KHÓA\nYêu cầu USB của người quản lý và mã xác thực gồm 10 chữ số.\nMã được lưu tại máy chủ điều phối trung tâm. Chưa đủ quyền để nhập mã tại thiết bị này.";
  public const string BaseFile="NORTH POINT / HỒ SƠ VẬN HÀNH\nCửa hàng điện tử là điểm tiếp nhận của một căn cứ ngầm. Khu quân giới cung cấp vũ khí và thiết bị cho các đội tác chiến của Chợ Đen.\n\nMarcus Carter phụ trách hệ thống xác thực và hồ sơ vận hành. Phụ lục Order 71 chỉ được giải mã bằng USB của người quản lý cùng mã xác thực 10 chữ số tại máy chủ điều phối ở tầng sâu nhất. Lối xuống nằm cuối kho quân giới.\n\nCẢNH BÁO: Tài khoản MARCUS CARTER đã được đánh dấu ĐÃ CHẾT. Phiên đăng nhập mới bằng USB của tài khoản này vừa được ghi nhận. Đang gửi cảnh báo tới bộ phận an ninh…";
  public const string FinalFile="ORDER 71 / PHỤ LỤC ĐÃ GIẢI MÃ\nNorth Point là vỏ bọc cho mạng lưới buôn bán vũ khí và dữ liệu quân sự do Victor Hale điều hành. Marcus Carter từng quản lý kỹ thuật, xác thực và hồ sơ của căn cứ.\n\nMarcus đã tiếp tay cho tổ chức. Khi phát hiện các hợp đồng bị thay thành lệnh thủ tiêu nhân chứng, ông bắt đầu sao lưu bằng chứng và tìm cách rời khỏi mạng lưới.\n\nOrder 71 là lệnh thanh trừng nội bộ do Victor ký: thủ tiêu Marcus, dựng hiện trường tai nạn, xóa hồ sơ và thu hồi USB. Alex bị đưa vào danh sách mục tiêu khi quay lại cửa hàng.\n\nMarcus chia quyền giải mã thành USB cá nhân và mã ở máy chủ điều phối, đồng thời tạo bản sao kiểm chứng trên hệ thống độc lập. Chỉ thu hồi USB không đủ để Victor xóa toàn bộ dấu vết.\n\nAlex được đăng ký làm người thừa kế quyền truy cập. Marcus vẫn dặn cháu tránh Cửa 006 vì ông hiểu căn cứ này nguy hiểm đến mức nào.\n\nBẢN GHI CUỐI / MARCUS\nAlex, chú không mong cháu tha thứ. Chú chỉ mong những gì chú để lại đủ để chấm dứt chuyện này. Đừng mang sự thật xuống mồ cùng chú.\n\nNHẬT KÝ AN NINH\nVictor đã ra lệnh tiêu hủy cơ sở trước cuộc giao tranh. Khóa an toàn của Marcus đang tạm giữ lệnh để bảo vệ phiên giải mã. Khi sao chép và gửi bằng chứng hoàn tất, phiên kết thúc và bộ đếm tiêu hủy bắt đầu: 90 giây. Máy chủ dùng nguồn dự phòng; hãy rời đi ngay sau khi gửi dữ liệu.";
  public const string DestroyEnding="Alex rời North Point với USB của Marcus. Bằng chứng đã được gửi tới các cơ quan điều tra và nhiều nơi lưu trữ độc lập.\n\nCửa hàng sụp đổ phía sau. Cái chết của Marcus không còn là một vụ tai nạn không lời giải.\n\nKẾT THÚC / SỰ THẬT ĐƯỢC GIỮ LẠI";
  // Kept for old editor utilities; the playable campaign has one canonical ending.
  public const string AcceptEnding=DestroyEnding,Recording=FinalFile,Prologue="Cuộc gọi cuối cùng của Marcus dẫn Alex tới North Point.";
 }
}
