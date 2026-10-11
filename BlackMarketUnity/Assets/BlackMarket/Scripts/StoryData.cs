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
   "Alex tới North Point để tìm hiểu cái chết của Marcus. Kiểm tra máy tính trong phòng làm việc của chú.",
   "Cửa 006 đóng sau lưng Alex. Kho vũ khí và các trạm máy tính cho thấy đây là căn cứ ngầm. Trang bị trước khi tìm hiểu hồ sơ vận hành.",
   "Trung tâm điều phối có nguồn điện dự phòng riêng cho máy chủ. Dùng kính nhìn đêm đã lấy ở kho quân giới và bắn hỏng tủ điện chiếu sáng để giành lợi thế trước Victor."
  };
  public static string Purpose(string id,int stage){switch(id){
   case "drawers":return "Máy tính yêu cầu USB xác thực. Kiểm tra các ngăn kéo trong văn phòng.";
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
   case "final_terminal":return "USB của Marcus và máy chủ điều phối cùng xác thực quyền xuất hồ sơ.";
   case "escape":return "Bằng chứng đã an toàn. Quy trình tiêu hủy của Victor sắp phá sập căn cứ.";
   default:return "Tìm sự thật về Marcus và mang bằng chứng rời North Point.";
  }}
  public const string UnlockCode="7106201723";
  public const string Terminal="NORTH POINT / HỆ THỐNG NỘI BỘ\nHỒ SƠ ĐƯỢC KHÔI PHỤC — ORDER #071\nTrạng thái: ĐANG CHỜ\nNgười phụ trách: Marcus Carter\nThời gian nhận: 23:00\nMã người nhận: 47\nNội dung đơn hàng: KHÔNG HIỂN THỊ\nVị trí lưu trữ: KHU VỰC SAU CỬA 006\n\nHỒ SƠ BỊ GIỚI HẠN TRUY CẬP — ĐÃ KHÓA\nNội dung chi tiết đã được mã hóa. Cần mã xác thực gồm 10 chữ số để mở.\nThiết bị hiện tại chỉ có quyền xem thông tin đơn hàng.";
  public const string BaseFile="NORTH POINT / HỒ SƠ VẬN HÀNH\nCửa hàng điện tử là điểm tiếp nhận của một căn cứ ngầm. Khu quân giới cung cấp vũ khí và thiết bị cho các đội tác chiến của Chợ Đen.\n\nMarcus Carter phụ trách hệ thống xác thực và hồ sơ vận hành. Phụ lục Order 71 chỉ được giải mã bằng USB của người quản lý cùng mã xác thực 10 chữ số tại máy chủ điều phối ở tầng sâu nhất. Lối xuống nằm cuối kho quân giới.\n\nCẢNH BÁO: Tài khoản MARCUS CARTER đã được đánh dấu ĐÃ CHẾT. Phiên đăng nhập mới bằng USB của tài khoản này vừa được ghi nhận. Đang gửi cảnh báo tới bộ phận an ninh…";
  public const string FinalFile="ORDER #071 — NỘI DUNG ĐÃ GIẢI MÃ\nLoại hồ sơ: Lệnh xử lý nội bộ\nNgười phê duyệt: Victor Hale\nĐơn vị tiếp nhận: Đội 47\nĐối tượng: Marcus Carter\nChấm dứt quyền truy cập của Marcus Carter. Thu hồi thiết bị xác thực cá nhân và toàn bộ bản sao dữ liệu do đối tượng lưu giữ.\nLoại bỏ đối tượng. Nguyên nhân tử vong phải được ghi nhận là tai nạn.\n\nQUY ƯỚC CHE GIẤU HỒ SƠ\nCác lệnh xử lý nội bộ được hiển thị dưới dạng đơn hàng trên hệ thống cửa hàng.\n“Người phụ trách” là danh tính được dùng để ngụy trang đối tượng cần xử lý.\n“Người nhận 47” là Đội 47, đơn vị thực hiện thu hồi và thủ tiêu.\n“Thời gian nhận 23:00” là hạn bàn giao USB cùng dữ liệu thu hồi về cơ sở.\n\nTRẠNG THÁI THU HỒI\nTrạng thái “Đang chờ” vẫn được giữ cho đến khi thiết bị xác thực và các bản sao được kiểm soát đầy đủ. Nó không phản ánh tình trạng sống chết của đối tượng.\n\nHỒ SƠ CƠ SỞ\nCửa hàng điện tử North Point là vỏ bọc cho một trung tâm lưu trữ và điều phối hoạt động buôn bán vũ khí, dữ liệu quân sự.\nVictor Hale điều hành mạng lưới. Marcus Carter phụ trách hệ thống xác thực, bảo trì máy chủ và hồ sơ giao dịch.\nMarcus không phải người ngoài cuộc. Ông đã xây dựng những hệ thống giúp tổ chức che giấu hoạt động trong nhiều năm.\n\nBÁO CÁO VI PHẠM NỘI BỘ\nTài khoản Marcus Carter đã truy cập trái phép danh sách các nhân chứng bị loại bỏ và sao chép hồ sơ liên quan.\nĐối tượng có dấu hiệu chuẩn bị đưa dữ liệu ra ngoài. Một số bản sao đã được phát hiện nhưng chưa thể xác định đầy đủ nơi lưu trữ.\n\nPHÊ DUYỆT XỬ LÝ\nVictor Hale phê duyệt Order #071 ngay sau khi phát hiện hành vi này.\nƯu tiên thu hồi USB cá nhân: thiết bị chứa khóa cần thiết để xác thực và xuất các hồ sơ gốc.\n\nBẢN GHI CÁ NHÂN / MARCUS CARTER\nChú đã tự thuyết phục mình rằng chú chỉ làm công việc kỹ thuật. Nhưng mỗi lần chú xóa một dấu vết, lại có ai đó biến mất mà không còn người chịu trách nhiệm.\nKhi chú quyết định giữ lại bằng chứng, mọi chuyện đã quá muộn.\n\nBẢN GHI CÁ NHÂN / TIẾP\nAlex, chú không để những thứ này lại để kéo cháu vào đây. Chú đã định tự đưa chúng ra ngoài. Cuộc gọi đó chỉ để cảnh báo cháu.\nChú từng đăng ký khuôn mặt cháu vào hệ thống khẩn cấp, phòng khi cháu gặp nguy hiểm tại cửa hàng. Chú chưa bao giờ muốn cháu phải dùng đến nó.\n\nBẢN GHI CÁ NHÂN / KẾT\nNếu cháu đã tới được đây, chú xin lỗi. Chú không mong cháu tha thứ. Hãy mang bằng chứng ra ngoài, rồi rời khỏi nơi này.\n\nGÓI BẰNG CHỨNG ĐÃ SẴN SÀNG\nBao gồm: lệnh Order #071 bản gốc, chữ ký phê duyệt của Victor Hale, hồ sơ giao dịch và nhật ký chỉnh sửa dữ liệu.\nUSB của Marcus và máy chủ điều phối đã xác thực thành công. Có thể sao chép hồ sơ và gửi tới các địa chỉ tiếp nhận bên ngoài do Marcus chuẩn bị.";
  public const string DestroyEnding="Alex rời North Point với USB của Marcus. Bằng chứng đã được gửi tới các cơ quan điều tra và nhiều nơi lưu trữ độc lập.\n\nCửa hàng sụp đổ phía sau. Cái chết của Marcus không còn là một vụ tai nạn không lời giải.\n\nKẾT THÚC / SỰ THẬT ĐƯỢC GIỮ LẠI";
  // Kept for old editor utilities; the playable campaign has one canonical ending.
  public const string AcceptEnding=DestroyEnding,Recording=FinalFile,Prologue="Cuộc gọi cuối cùng của Marcus dẫn Alex tới North Point.";
 }
}
