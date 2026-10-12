namespace BlackMarket {
 public static class DemoCheckpoints {
  public static readonly string[] Labels={"01 · Cửa hàng / tìm USB","02 · Đọc Order 71","03 · Nhóm áo đen tới","04 · Kho vũ khí","05 · Chiến đấu 30 tên","06 · Nghỉ nhặt chiến lợi phẩm","07 · Victor tiếp viện","08 · Trung tâm điều phối","09 · Trận cuối / kính đêm","10 · Giải mã và kết thúc"};
  public static void Jump(Campaign g,int point){
   if(point<0||point>=Labels.Length)return;
   g.demoMode=true;g.player?.ResetTouch();
   int stage=point<3?0:point<7?1:2;
   g.state=new CampaignSave{stage=stage,introCompleted=true,keycard=point>0,hp=100,armed=point>=4,hasAK=point>=4,hasSniper=point>=4,hasNightVision=point>=4,selectedWeapon=1,rifleAmmo=30,rifleReserve=300,sniperAmmo=5,sniperReserve=40,grenades=8,checkpoint=point==4?1:point==5?6:point==6?2:point==8?3:point==9?4:0,lightsDestroyed=point==8||point==9,codeRecovered=point==9};
   g.LoadWorld(stage);g.Save();
   if(point==1)g.ui.OpenComputerDesktop();
   if(point==2){g.flags.Add("computer");g.opening.Begin();}
   if(point==8||point==9)g.player.nightVision.Toggle();
   g.ui.Toast("DEMO: "+Labels[point]);
  }
 }
}
