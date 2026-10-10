using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
namespace BlackMarket {
 [DefaultExecutionOrder(50)]
 public class UndergroundCampaign:MonoBehaviour {
  public enum Encounter {Equip,FirstWave,Resupply,Assault,Retreat,ControlPrep,FinalBattle,Decrypt,Escape,Complete}
  public Encounter State {get;private set;}
  public bool Playing {get;private set;}
  public bool Suppressing=>game&&game.state.stage==1&&(State==Encounter.Assault||State==Encounter.Retreat);
  public bool VolleyActive {get;private set;}
  public bool AutoRetreat {get;private set;}
  public int OpeningGrenadesThrown {get;private set;}
  float overwhelmingSince=-1;public bool Overwhelming=>Suppressing && EscortDeaths>=10;public int EscortDeaths=>Enumerable.Range(0,20).Count(i=>(game.state.reinforcementDeaths&(1<<i))!=0);
  int restShown=-1;bool volleyPending;float nextCombatDamage,nextGrenade;public float RestRemaining {get;private set;}
  public bool Dark=>game.state.lightsDestroyed;
  public string Caption {get;private set;}
  public float EscapeRemaining {get;private set;}
  public int Scheduled=>scheduled;
  public int Living=>game.enemies.Count(e=>e&&e.hp>0&&e.combat);
  Campaign game;int scheduled,total,firstDeaths;float waveClock,spawnClock,shake;bool healed,gearReady,endingStarted;
  Vector3 shotEye,shotTarget;float shotFov=58;bool cinematicCamera;
  public void Setup(Campaign owner){
   game=owner;game.state.tablet=false;if(game.state.corpseLoot==null)game.state.corpseLoot=new System.Collections.Generic.List<Vector3>();
   if(game.state.stage==1){
    State=game.state.checkpoint==1?Encounter.FirstWave:game.state.checkpoint==2?Encounter.Assault:game.state.checkpoint==6?Encounter.Resupply:Encounter.Equip;
    if(game.state.checkpoint>0){game.state.hasNightVision=true;game.state.hp=100;game.state.armed=game.state.hasAK=game.state.hasSniper=true;game.state.rifleAmmo=30;game.state.rifleReserve=Mathf.Max(150,game.state.rifleReserve);game.state.sniperAmmo=5;game.state.sniperReserve=Mathf.Max(25,game.state.sniperReserve);game.state.grenades=Mathf.Max(3,game.state.grenades);game.player.SwitchWeapon(1);}
    if(State==Encounter.FirstWave)StartFirstWave(false);else if(State==Encounter.Assault)StartAssault(false);else if(State==Encounter.Resupply)StartResupply();else EquipmentObjective();
   }else{
    State=game.state.checkpoint>=4?Encounter.Decrypt:game.state.checkpoint==3?Encounter.FinalBattle:Encounter.ControlPrep;
    if(State==Encounter.Decrypt){game.state.lightsDestroyed=true;game.security.SetDark(true);game.Objective("final_terminal","Cắm USB vào máy tính trung tâm để lấy mã mở Order 71.");}
    else if(State==Encounter.FinalBattle){healed=gearReady=true;game.security.SetDark(Dark);BeginFinal(false);}
    else{game.state.lightsDestroyed=false;game.state.hasNightVision=true;game.state.hp=Mathf.Max(50,game.state.hp);game.state.rifleReserve=Mathf.Max(120,game.state.rifleReserve);StartCoroutine(ControlArrival());}
   }
  }
  void EquipmentObjective(){
   if(!game.state.hasAK)game.Objective("ak","Lấy AK tại giá vũ khí bên cạnh.");
   else if(!game.state.hasSniper)game.Objective("sniper","Lấy súng ngắm M700 ở kho vũ khí. Q đổi súng; giữ chuột phải để ngắm xa.");
   else if(!game.flags.Contains("grenade_kit"))game.Objective("grenades","Nhận bộ lựu đạn, đạn dự trữ và kính nhìn đêm trước khi kiểm tra máy tính.");
   else game.Objective("base_computer","Cắm USB vào máy tính vận hành để tìm nơi lưu mã Order 71.");
  }
  public bool Available(string id){switch(id){
   case "pistol":return State==Encounter.Equip&&!game.state.armed;
   case "sniper":return State==Encounter.Equip&&!game.state.hasSniper;
   case "ak":return State==Encounter.Equip&&!game.state.hasAK;
   case "grenades":return State==Encounter.Equip&&!game.flags.Contains("grenade_kit");
   case "base_computer":return State==Encounter.Equip;
   case "retreat":return State==Encounter.Retreat;
   case "medical":return State==Encounter.ControlPrep&&!healed;
   case "nightvision":return State==Encounter.ControlPrep&&!gearReady;
   case "control_entry":return State==Encounter.ControlPrep;
   case "breaker":return State==Encounter.FinalBattle&&!Dark;
   case "final_terminal":return State==Encounter.Decrypt;
   case "escape":return State==Encounter.Escape;
   default:return id.StartsWith("corpse_loot_")?State==Encounter.Resupply:id.StartsWith("supply")&&State!=Encounter.Resupply;
  }}
  public void Interact(Interaction item){switch(item.id){
   case "pistol":game.state.armed=true;game.state.ammo=8;game.state.reserve=64;Consume(item);EquipmentObjective();game.ui.Toast("SÚNG LỤC / Chuột phải ngắm, chuột trái bắn. R nạp đạn.");break;
   case "sniper":game.state.hasSniper=true;game.state.sniperAmmo=5;game.state.sniperReserve=35;game.player.SwitchWeapon(2);Consume(item);EquipmentObjective();game.ui.Toast("M700 / Giữ chuột phải để ngắm xa. Q đổi AK và súng ngắm. Bắn rồi đổi vị trí sau cột.");break;
   case "ak":game.state.armed=true;game.state.hasAK=true;game.state.rifleAmmo=30;game.state.rifleReserve=210;game.player.SwitchWeapon(1);Consume(item);EquipmentObjective();game.ui.Toast("AK / Giữ BẮN để bắn tự động. Q đổi súng.");break;
   case "grenades":game.state.hasNightVision=true;game.state.grenades=Mathf.Max(4,game.state.grenades);game.flags.Add("grenade_kit");Consume(item);EquipmentObjective();game.ui.Toast("ĐÃ NHẬN KÍNH NHÌN ĐÊM [N], LỰU ĐẠN [G] VÀ ĐẠN DỰ TRỮ.");break;
   case "base_computer":if(!game.state.hasSniper||!game.state.hasAK||!game.flags.Contains("grenade_kit")){game.ui.Toast("Lấy đủ AK, súng ngắm và lựu đạn trước khi truy cập máy tính.");return;}game.ui.Document("HỒ SƠ VẬN HÀNH",StoryData.BaseFile,()=>StartFirstWave(true),"base-file-v2");break;
   case "retreat":StartCoroutine(Descend());break;
   case "medical":healed=true;game.state.hp=100;game.state.reserve=96;game.state.rifleReserve=300;game.state.grenades=Mathf.Max(4,game.state.grenades);Consume(item);game.Objective("nightvision","Nhận kính nhìn đêm ở tủ thiết bị của phòng trực.");game.ui.Toast("ĐÃ HỒI PHỤC / Máu, đạn và lựu đạn được bổ sung.");break;
   case "nightvision":gearReady=true;game.state.hasNightVision=true;Consume(item);if(!game.player.nightVision.Active)game.player.nightVision.Toggle();game.Objective(healed?"control_entry":"medical",healed?"Mở cửa trung tâm điều phối. N bật/tắt kính nhìn đêm.":"Dùng bộ sơ cứu trước khi vào phòng điều phối.");break;
   case "control_entry":game.state.checkpoint=3;game.Save();BeginFinal(true);break;
   case "breaker":game.ui.Toast("Tủ điện chiếu sáng: ngắm và bắn để phá. Máy chủ có nguồn dự phòng riêng.");break;
   case "final_terminal":game.state.codeRecovered=true;game.ui.OpenDecoder();break;
   case "escape":StartCoroutine(Ending());break;
   default:if(item.id.StartsWith("corpse_loot_")){game.state.hp=Mathf.Min(100,game.state.hp+18);game.state.rifleReserve=Mathf.Min(450,game.state.rifleReserve+30);game.state.sniperReserve=Mathf.Min(80,game.state.sniperReserve+4);game.state.grenades=Mathf.Min(12,game.state.grenades+1);game.state.corpseLoot.Remove(item.transform.position);Consume(item);game.Save();game.ui.Toast("ĐÃ THU CHIẾN LỢI PHẨM / Băng cứu thương, đạn và lựu đạn.");}else if(item.id.StartsWith("supply")){game.state.hp=Mathf.Min(100,game.state.hp+35);game.state.reserve=Mathf.Min(96,game.state.reserve+24);game.state.rifleReserve=Mathf.Min(450,game.state.rifleReserve+75);game.state.sniperReserve=Mathf.Min(80,game.state.sniperReserve+10);game.state.grenades=Mathf.Min(12,game.state.grenades+1);Consume(item);game.ui.Toast("TIẾP TẾ / Máu, đạn và một lựu đạn.");}break;
  }}
  void Consume(Interaction i){i.consumed=true;i.gameObject.SetActive(false);}
  void Checkpoint(int index){game.state.checkpoint=index;game.Save();}
  void StartFirstWave(bool film){State=Encounter.FirstWave;scheduled=firstDeaths=0;total=30;game.state.corpseLootRecorded=true;game.state.corpseLoot.Clear();waveClock=0;spawnClock=8;volleyPending=true;Checkpoint(1);game.Objective("first_wave","Hạ 30 tên truy bắt theo 6 nhóm, mỗi nhóm 5 người. Dùng M700 ngắm từ xa; nấp sau cột, tránh vòng lựu đạn và đổi vị trí sau khi bắn.");if(film){game.sound.Play("alarm",.6f);StartCoroutine(Breach(false));}else OpenBreach();}
  void StartAssault(bool film){State=Encounter.Assault;scheduled=0;total=20;waveClock=0;spawnClock=8;game.state.reinforcementDeaths=0;overwhelmingSince=-1;Checkpoint(2);game.Objective("assault","Victor mang quân tiếp viện xuống! Nấp sau vật che và chống trả.");if(film)StartCoroutine(ReinforcementArrival());else{OpenBreach();BeginRetreat();}}
  void BeginFinal(bool film){if(!film)OpenBreach();State=Encounter.FinalBattle;scheduled=0;total=20;waveClock=spawnClock=0;OpenObject("Control gate");game.Objective(Dark?"boss":"breaker",Dark?"Hạ Victor và những kẻ truy đuổi còn lại.":"Bắn hỏng tủ điện chiếu sáng ở cánh trái. Bật kính nhìn đêm [N].");if(film)StartCoroutine(FinalArrival());else SpawnBoss();}
  void OpenBreach(){OpenObject("Breach intact");var wreck=transform.Find("Breach wreck");if(wreck)wreck.gameObject.SetActive(true);}
  void OpenObject(string name){var t=transform.Find(name);if(t)t.gameObject.SetActive(false);}
  IEnumerator Breach(bool victor){
   if(game.state.stage==1){yield return BreachArmory(victor);yield break;}
   Playing=true;game.player.visual.gameObject.SetActive(false);game.player.ResetTouch();game.LockCursor(false);var cam=game.view;var oldPos=cam.transform.position;var oldRot=cam.transform.rotation;float fov=cam.fieldOfView;
   Caption=victor?"Một tiếng nổ khác… Victor đích thân xuống đây!":"CỬA 006 / ĐỘI TRUY BẮT ĐANG ĐẶT THUỐC NỔ";
   var entry=game.Marker("breach").position;Shot(entry+new Vector3(6,3.2f,9),entry+Vector3.up,58);yield return Wait(1.3f);
   BlastEffects.Explode(entry+Vector3.up*1.4f,4);game.sound.Play("explosion",.8f);Shake(1.2f);OpenBreach();if(game.state.stage==2)OpenObject("Control gate");
   Caption=victor?"VICTOR: Nó giữ USB của Marcus. Khóa lối ra! Đừng để nó chạm tới máy chủ!":"NHÓM NGƯỜI MẶC VEST: Xuống dưới! Thu hồi USB!";
   var crowd=new GameObject("Đội hình hoạt cảnh");crowd.transform.SetParent(transform);int n=victor?(game.state.stage==2?1+Enumerable.Range(0,15).Count(i=>(game.state.reinforcementDeaths&(1<<i))==0):16):10;
   for(int i=0;i<n;i++){var a=Instantiate(Resources.Load<GameObject>("Actors/"+(victor&&i==0?"Victor":"BlackSuit")),crowd.transform);var weapon=Instantiate(Resources.Load<GameObject>("Actors/AK"),a.transform);var pose=a.AddComponent<WeaponPose>();pose.weapon=weapon.transform;pose.weight=1;a.transform.position=entry+new Vector3((i%4-1.5f)*1.05f,0,-1-i/4*1.2f);var animation=a.GetComponentInChildren<Animation>();if(animation){animation.cullingType=AnimationCullingType.AlwaysAnimate;animation.Play("run");}}
   float t=0;while(t<4.3f){if(!game.paused&&game.active){t+=Time.deltaTime;foreach(Transform a in crowd.transform)a.position+=Vector3.forward*Time.deltaTime*2.1f;}yield return null;}
   Destroy(crowd);cinematicCamera=false;cam.transform.SetPositionAndRotation(oldPos,oldRot);cam.fieldOfView=fov;game.player.visual.gameObject.SetActive(true);Playing=false;game.LockCursor(true);if(victor)SpawnBoss();
  }
  IEnumerator BreachArmory(bool victor){
   Playing=true;game.player.ResetTouch();game.LockCursor(false);var entry=game.Marker("breach").position;
   Caption=victor?"VICTOR: Chặn các lối ra. Nó vẫn còn ở dưới!":"CỬA 006 / THUỐC NỔ ĐÃ ĐƯỢC GẮN VÀO KHÓA";
   var charge=GameObject.CreatePrimitive(PrimitiveType.Cube);charge.name="Thuốc nổ / khóa 006";charge.transform.SetParent(transform);charge.transform.position=entry+new Vector3(0,1.25f,-.18f);charge.transform.localScale=new Vector3(.24f,.3f,.08f);charge.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("Materials/Black");Destroy(charge.GetComponent<Collider>());
   var saboteur=Instantiate(Resources.Load<GameObject>("Actors/BlackSuit"),transform);saboteur.transform.SetPositionAndRotation(entry+new Vector3(-.25f,0,-.7f),Quaternion.identity);var animation=saboteur.GetComponentInChildren<Animation>();if(animation)animation.Play("idle");saboteur.AddComponent<ChargePlacementPose>().target=charge.transform.position;
   Shot(entry+new Vector3(1.25f,1.65f,-1.8f),entry+Vector3.up*1.25f,58);yield return Wait(2);Destroy(saboteur);Shot(entry+new Vector3(1.3f,1.6f,6),entry+Vector3.up*1.3f,55);yield return Wait(.5f);Destroy(charge);
   BlastEffects.Explode(entry+Vector3.up*1.2f,4);game.sound.Play("explosion",.8f);Shake(1);OpenBreach();yield return Wait(.8f);
   Caption="NHÓM NGƯỜI MẶC VEST: Lùng bắn hắn! Từng nhóm một, kiểm tra các phòng!";game.sound.Play("hunt_shout",1);
   VolleyActive=true;volleyPending=false;for(int i=0;i<5;i++)Spawn(scheduled++);
   cinematicCamera=false;Playing=false;game.LockCursor(true);yield return OpeningVolley();
  }
  IEnumerator OpeningVolley(){
   VolleyActive=true;volleyPending=false;
   game.Objective("first_wave","LỰU ĐẠN! Rời vòng cam, chạy lùi và nấp sau cột! Giữ Shift để chạy nhanh.");
   game.ui.Subtitle("ALEX","Chúng ném lựu đạn! Phải lùi lại, tránh chân cầu thang!");
   yield return Wait(1);
   for(int i=0;i<6;i++){
    var thrower=game.enemies.Where(e=>e&&e.hp>0&&e.combat).ElementAtOrDefault(i%3);
    if(thrower){var point=new Vector3((i%3-1)*2.5f,0,5+(i/3)*8);if(NavMesh.SamplePosition(point,out var snap,2,NavMesh.AllAreas))point=snap.position;StartCoroutine(ThrowOpeningGrenade(thrower,point));}
    yield return Wait(.65f);
   }
   yield return Wait(HostileGrenade.Fuse+1.2f);VolleyActive=false;spawnClock=5;
   game.Objective("first_wave","Địch đang tràn xuống! Chạy đổi vị trí để giảm nguy cơ trúng đạn; nấp sau cột, dùng M700 ngắm xa. Hạ đủ 30 tên.");
  }
  IEnumerator ThrowOpeningGrenade(EnemyController enemy,Vector3 target){
   var pose=enemy.GetComponentsInChildren<WeaponPose>().First(p=>p.weapon&&p.weapon.Find("Right grip"));float t=0;bool released=false;
   while(t<1.1f&&enemy&&enemy.hp>0){if(game.Running){t+=Time.deltaTime;pose.scriptedThrow=t/1.1f;if(!released&&t>=.61f){released=true;OpeningGrenadesThrown++;HostileGrenade.Launch(game,pose.RightHand?pose.RightHand.position:enemy.transform.position+Vector3.up*1.5f,target);}}yield return null;}
   if(pose)pose.scriptedThrow=-1;
  }
  public bool CombatHit(float amount,bool grenade){
   if(!game.Running||Time.time<nextCombatDamage)return false;
   nextCombatDamage=Time.time+(grenade?.7f:.35f);game.Damage(Mathf.Min(amount,grenade?18:6));return true;
  }
  public bool TryEmergencyRetreat(){
   if(!Suppressing||game.state.hp>=50||AutoRetreat)return false;
   StartCoroutine(Descend());return true;
  }

  void BeginRetreat(){State=Encounter.Retreat;OpenObject("Retreat gate");game.Objective("retreat","Hai mươi cận vệ đang bắn áp chế, Victor ở phía sau! Bám vật che, rút ngay xuống tầng cuối.");game.ui.Subtitle("ALEX","Quá nhiều hỏa lực! Không thể đấu trực diện. Phải rút xuống tầng cuối!");}
  IEnumerator ReinforcementArrival(){
   Playing=true;OpenBreach();game.player.ResetTouch();game.LockCursor(false);Caption="VICTOR: Đội áp chế, tiến lên! Bắn liên tục!";
   Shot(new Vector3(1.3f,4.3f,-5),new Vector3(0,4,-13),58);
   // Door 006 is already destroyed. All twenty escorts enter before Victor.
   for(int i=0;i<20;i++){Spawn(scheduled++);yield return Wait(.55f);}SpawnBoss();yield return Wait(1.5f);
   cinematicCamera=false;Playing=false;game.LockCursor(true);BeginRetreat();
  }
  void SpawnBoss(){if(game.enemies.Any(e=>e&&e.combat&&e.boss&&e.hp>0))return;var boss=Spawn(-1,true);boss.hp=boss.maxHp=game.state.stage==2?(game.state.victorWounded?990:1350):450;}
  EnemyController Spawn(int id,bool boss=false){
   var go=new GameObject(boss?"Victor Hale":"Người mặc vest / "+id);go.transform.SetParent(transform);go.transform.position=game.Marker("enemy_"+(boss?0:id%4)).position;
   var e=go.AddComponent<EnemyController>();e.campaign=game;e.combat=true;e.combatId=id;e.boss=boss;e.nightSight=boss;e.actorPrefab=boss?"Victor":"BlackSuit";e.elite=!boss&&(Suppressing||game.state.stage==2);e.hp=e.maxHp=boss?450:e.elite?140:80;e.damage=boss?10:e.elite?8:6;e.speed=boss&&game.state.stage==1?4.6f:boss?3.1f:game.state.stage==1?6.2f:2.8f;e.Setup();game.enemies.Add(e);return e;
  }
  public bool FireSlot(EnemyController enemy){int limit=Suppressing?8:6;var shooters=game.enemies.Where(e=>e&&e.combat&&e.hp>0&&e.tactical&&e.tactical.CanSeePlayer).ToArray();if(shooters.Length<=limit)return true;int idx=Array.IndexOf(shooters,enemy),start=(int)(Time.time/2.4f)%shooters.Length;return (idx-start+shooters.Length)%shooters.Length<limit;}
  public void EnemyFell(EnemyController e){
   if(e.boss){if(game.state.stage==1){game.state.victorWounded=true;game.ui.Subtitle("VICTOR","Yểm trợ! Ta sẽ chờ nó ở dưới!");}return;}
   if(State==Encounter.FirstWave){firstDeaths++;DropLoot(e.transform.position);game.Objective("first_wave","Đẩy lùi đội truy bắt: "+firstDeaths+" / 30. Nhóm sau chỉ xuống khi nhóm hiện tại bị hạ hết.");}
   else if(e.combatId>=0)game.state.reinforcementDeaths|=1<<e.combatId;
  }
  public void BreakLights(){if(State!=Encounter.FinalBattle||Dark)return;game.state.lightsDestroyed=true;game.security.SetDark(true);game.sound.Play("power_down",.75f);Shake(.35f);foreach(var e in game.enemies)if(e&&e.combat&&e.hp>0)e.tactical?.Blackout();game.Objective("boss","Điện đã tắt. Bật kính nhìn đêm [N], hạ Victor và các thuộc hạ còn lại.");game.ui.Subtitle("VICTOR","Tao cũng có kính nhìn đêm. Bóng tối không cứu được mày đâu!");}
  public void ReadFinal(){if(State!=Encounter.Decrypt||!game.state.keycard||!game.state.codeRecovered)return;game.ui.Document("ORDER 71 / PHỤ LỤC MẬT",StoryData.FinalFile,()=>{game.state.evidenceCopied=true;Checkpoint(4);State=Encounter.Escape;EscapeRemaining=90;game.Objective("escape","Đã gửi bằng chứng! Quay lại cầu thang đã đi xuống để ra ngoài trước khi căn cứ bị tiêu hủy.");game.sound.Play("alarm",.65f);OpenObject("Maintenance gate");},"final-file-v2");}
  void Update(){
   if(!game || !game.Running)return;
   if(State==Encounter.Resupply){RestRemaining=Mathf.Max(0,RestRemaining-Time.deltaTime);if(restShown!=Mathf.CeilToInt(RestRemaining)){restShown=Mathf.CeilToInt(RestRemaining);var nearest=game.level.GetComponentsInChildren<Interaction>().Where(i=>i.id.StartsWith("corpse_loot_")&&!i.consumed).OrderBy(i=>Vector3.Distance(i.transform.position,game.player.transform.position)).FirstOrDefault();game.Objective(nearest?nearest.id:"retreat","Tạm yên: còn "+restShown+" giây. Tìm túi chiến lợi phẩm cạnh xác địch, lấy máu và đạn rồi nạp súng.");}if(RestRemaining<=0)StartAssault(true);return;}
   if(Overwhelming){if(overwhelmingSince<0){overwhelmingSince=Time.time;game.ui.Subtitle("VICTOR","Đội xung kích, dồn hỏa lực! Phá chỗ nấp của nó!");}if(Time.time-overwhelmingSince>10&&game.state.hp>=50){game.ui.Subtitle("ALEX","Sức ép vụ nổ quá lớn! Phải rút ngay!");game.sound.Play("explosion",.7f);BlastEffects.Explode(game.player.transform.position+Vector3.right*2,3);Shake(.8f);game.state.hp=49;}}
   if(TryEmergencyRetreat())return;
   if(State==Encounter.FirstWave&&volleyPending){spawnClock-=Time.deltaTime;if(spawnClock<=0){VolleyActive=true;for(int i=0;i<5;i++)Spawn(scheduled++);StartCoroutine(OpeningVolley());}return;}
   if(VolleyActive)return;
   if(State==Encounter.FirstWave || State==Encounter.Assault || State==Encounter.Retreat || State==Encounter.FinalBattle){
    waveClock+=Time.deltaTime;spawnClock-=Time.deltaTime;
    if(spawnClock<=0 && Living<(Suppressing?21:6) && scheduled<total && (State!=Encounter.FirstWave || scheduled%5!=0 || Living==0)){int id=scheduled++;if(State==Encounter.FirstWave || (game.state.reinforcementDeaths&(1<<id))==0)Spawn(id);spawnClock=game.state.stage==1?(Suppressing?.6f:scheduled%3==0?5:1.2f):1.15f;}
    if(State==Encounter.FirstWave && firstDeaths==30 && Living==0)StartResupply();
    if(game.state.stage==1&&State==Encounter.Retreat&&scheduled==total&&!game.enemies.Any(e=>e&&e.boss))SpawnBoss();
    if(State==Encounter.FinalBattle && scheduled==total && Living==0 && Dark){State=Encounter.Decrypt;game.flags.Add("boss_dead");Checkpoint(4);game.Objective("final_terminal","Victor đã bị hạ. Cắm USB vào máy tính trung tâm để giải mã Order 71.");}
   }
   if(State==Encounter.Escape && !endingStarted){var exit=game.level.GetComponentsInChildren<Interaction>().FirstOrDefault(i=>i.id=="escape");if(exit&&Vector3.Distance(game.player.transform.position,exit.transform.position)<3){StartCoroutine(Ending());return;}EscapeRemaining=Mathf.Max(0,EscapeRemaining-Time.deltaTime);if(EscapeRemaining<=0){game.Damage(1000);}}
  }
  void StartResupply(){State=Encounter.Resupply;foreach(var grenade in game.level.GetComponentsInChildren<HostileGrenade>())Destroy(grenade.gameObject);RestRemaining=45;restShown=-1;if(!game.state.corpseLootRecorded){game.state.corpseLootRecorded=true;for(int i=0;i<6;i++){var point=game.Marker("combat_focus").position+new Vector3((i%3-1)*2,0,i/3*3);if(NavMesh.SamplePosition(point,out var snap,3,NavMesh.AllAreas))game.state.corpseLoot.Add(snap.position+Vector3.up*.12f);}}Checkpoint(6);game.ui.Subtitle("ALEX","Từng ấy người chỉ để giữ một căn cứ? Chú đã phát hiện ra điều gì? Mình cần nhặt thêm đạn, băng bó rồi tìm tới trung tâm.");if(!game.level.GetComponentsInChildren<Interaction>().Any(i=>i.id.StartsWith("corpse_loot_")))foreach(var point in game.state.corpseLoot){CreateLoot(point);var corpse=Instantiate(Resources.Load<GameObject>("Actors/BlackSuit"),transform);corpse.name="Thi thể / khôi phục điểm tiếp tế";corpse.transform.position=point-Vector3.up*.12f;var animation=corpse.GetComponentInChildren<Animation>();if(animation)animation.Stop();corpse.AddComponent<GroundedDeath>().Fall(corpse.transform);}}
  void DropLoot(Vector3 point){if(NavMesh.SamplePosition(point,out var hit,2,NavMesh.AllAreas))point=hit.position;point+=Vector3.up*.12f;game.state.corpseLoot.Add(point);CreateLoot(point);}
  void CreateLoot(Vector3 point){var bag=GameObject.CreatePrimitive(PrimitiveType.Cube);bag.name="Chiến lợi phẩm cạnh xác địch";bag.transform.SetParent(transform);bag.transform.position=point;bag.transform.localScale=new Vector3(.42f,.22f,.3f);bag.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("Materials/Black");Destroy(bag.GetComponent<Collider>());var item=bag.AddComponent<Interaction>();item.id="corpse_loot_"+bag.GetInstanceID();item.title="THU CHIẾN LỢI PHẨM / MÁU, ĐẠN, LỰU ĐẠN";var glow=new GameObject("Dấu chiến lợi phẩm");glow.transform.SetParent(bag.transform,false);glow.transform.localPosition=Vector3.up*2;var mesh=glow.AddComponent<TextMesh>();mesh.text="+";mesh.characterSize=.5f;mesh.fontSize=32;mesh.color=new Color(.5f,1,.65f);mesh.anchor=TextAnchor.MiddleCenter;}
  public bool TryTacticalGrenade(EnemyController enemy,Vector3 target){
   if(!game.Running||VolleyActive||Time.time<nextGrenade||State==Encounter.Resupply||Dark&&!enemy.nightSight)return false;
   if(game.level.GetComponentsInChildren<HostileGrenade>().Length>=4)return false;
   nextGrenade=Time.time+(enemy.boss?1.8f:2.6f);StartCoroutine(ThrowOpeningGrenade(enemy,target));game.ui.Toast("LỰU ĐẠN! Rời vòng cam, đổi chỗ nấp!");return true;
  }
  IEnumerator ControlArrival(){
   Playing=true;game.player.ResetTouch();game.LockCursor(false);OpenBreach();Caption="TẦNG HẦM CUỐI / TRUNG TÂM ĐIỀU PHỐI — ĐÃ BĂNG BÓ TẠM VÀ BỔ SUNG ĐẠN";
   var p=game.player;var path=new NavMeshPath();var goal=new Vector3(0,0,44);
   if(NavMesh.CalculatePath(p.transform.position,goal,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete){p.body.enabled=false;var anim=p.visual.GetComponentInChildren<Animation>();if(anim)anim.CrossFade("run",.2f);
    foreach(var point in path.corners)while(Vector3.Distance(p.transform.position,point)>.08f){if(!game.paused&&game.active){var d=point-p.transform.position;p.transform.position=Vector3.MoveTowards(p.transform.position,point,6.5f*Time.deltaTime);d.y=0;if(d.sqrMagnitude>.01f)p.visual.rotation=Quaternion.LookRotation(d);Shot(p.transform.position+new Vector3(0,4,-3),p.transform.position+Vector3.up,65);}yield return null;}p.body.enabled=true;
   }
   Shot(new Vector3(7,4,41),new Vector3(0,3,51),65);Caption="ALEX: Đây là trung tâm điều phối… Mọi mệnh lệnh Order 71 đều đi qua đây. Chú đã giấu bằng chứng trong máy chủ này.";yield return Wait(7);
   cinematicCamera=false;Playing=false;game.state.checkpoint=3;game.Save();BeginFinal(true);
  }
  IEnumerator FinalArrival(){
   Playing=true;game.player.ResetTouch();game.LockCursor(false);OpenBreach();Caption="VICTOR: Marcus đã trả giá vì phản bội. Đặt USB xuống! Bao vây máy chủ, không cho nó rời khỏi đây!";
   Shot(new Vector3(3,3,8),new Vector3(0,1,-3),60);SpawnBoss();yield return Wait(5.5f);
   for(int i=0;i<3&&scheduled<total;i++){int id=scheduled++;if((game.state.reinforcementDeaths&(1<<id))==0)Spawn(id);yield return Wait(.45f);}
   cinematicCamera=false;Playing=false;game.LockCursor(true);nextGrenade=Time.time+6;
   game.Objective("breaker","BẮN TỦ ĐIỆN cạnh màn hình trung tâm, bật kính nhìn đêm [N]! Di chuyển khỏi vòng cam; phản công khi Victor ngừng bắn để nạp đạn.");
  }
  IEnumerator Descend(){
   if(AutoRetreat)yield break;AutoRetreat=true;Playing=true;OpenObject("Retreat gate");game.player.ResetTouch();game.LockCursor(false);
   Caption="ALEX: Không thể trụ thêm! Phải xuống tầng cuối!";var p=game.player;p.crouch=false;p.body.enabled=false;
   var anim=p.visual.GetComponentInChildren<Animation>();if(anim){anim["run"].speed=1;anim.CrossFade("run",.15f);}
   // Route through room doors and around furniture before entering the stairs.
   var path=new NavMeshPath();var destination=new Vector3(0,0,61);
   bool routed=NavMesh.SamplePosition(p.transform.position,out var origin,3,NavMesh.AllAreas)&&NavMesh.CalculatePath(origin.position,destination,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
   if(!routed){AutoRetreat=false;Playing=false;p.body.enabled=true;game.state.hp=Mathf.Max(25,game.state.hp);game.ui.Toast("Lối rút đang bị chắn. Di chuyển ra lối đi rồi tới cửa tầng cuối.");game.LockCursor(true);yield break;}
   var points=path.corners.Concat(new[]{new Vector3(0,0,64),game.Marker("retreat_end").position}).ToArray();
   var fade=game.gameObject.AddComponent<DescentBlackout>();float distance=0;var last=p.transform.position;foreach(var point in points){distance+=Vector3.Distance(last,point);last=point;}float travelled=0;
   foreach(var point in points){while(Vector3.Distance(p.transform.position,point)>.06f){if(!game.paused&&game.active){var before=p.transform.position;var direction=point-before;p.transform.position=Vector3.MoveTowards(before,point,6.5f*Time.deltaTime);travelled+=Vector3.Distance(before,p.transform.position);var flat=Vector3.ProjectOnPlane(direction,Vector3.up);if(flat.sqrMagnitude>.001f)p.visual.rotation=Quaternion.Slerp(p.visual.rotation,Quaternion.LookRotation(flat),Time.deltaTime*12);
     Shot(p.transform.position+new Vector3(0,4.4f,-2.5f),p.transform.position+Vector3.up*.7f,65);fade.opacity=Mathf.Clamp01((travelled-(distance-3))/3);
    }yield return null;}}
   fade.opacity=1;game.state.checkpoint=0;game.Advance();fade.FadeOutAfterLoad();
  }

  IEnumerator Ending(){
   if(endingStarted)yield break;endingStarted=true;Playing=true;game.player.ResetTouch();game.LockCursor(false);Caption="CẦU THANG / RỜI KHỎI NORTH POINT";game.sound.Stop("alarm");var fade=game.gameObject.AddComponent<DescentBlackout>();float fadeTime=0;while(fadeTime<2){if(!game.paused){fadeTime+=Time.deltaTime;fade.opacity=Mathf.SmoothStep(0,1,fadeTime/2);}yield return null;}
   // The exterior set is reused without its gameplay components; the saved shop is untouched.
   game.security.SetDark(false);var moon=GameObject.Find("Moonlight");if(moon&&moon.GetComponent<Light>())moon.GetComponent<Light>().enabled=true;foreach(Transform child in transform)child.gameObject.SetActive(false);
   var exterior=Instantiate(Resources.Load<GameObject>("Worlds/NorthPointShop"));exterior.name="North Point / hoạt cảnh kết";EndingNeighborhood.Build(exterior.transform);foreach(var c in exterior.GetComponentsInChildren<Collider>())c.enabled=false;foreach(var n in exterior.GetComponentsInChildren<Unity.AI.Navigation.NavMeshSurface>())n.enabled=false;foreach(var light in exterior.GetComponentsInChildren<Light>())light.enabled=true;
   game.player.visual.gameObject.SetActive(false);var actor=Instantiate(Resources.Load<GameObject>("Actors/Alex"),transform);var bike=Instantiate(Resources.Load<GameObject>("Opening/AlexMotorcycle"),transform);bike.transform.SetPositionAndRotation(PrologueSequence.ParkPosition,PrologueSequence.ParkRotation);actor.transform.position=new Vector3(9,0,-1);var anim=actor.GetComponentInChildren<Animation>();if(anim)anim.Play("walk");
   Shot(new Vector3(11,3,-9),new Vector3(3,1,-1),60);fade.FadeOutAfterLoad();float t=0;var start=actor.transform.position;var mount=PrologueSequence.ParkPosition+new Vector3(0,0,1);while(t<3){if(!game.paused&&game.active){t+=Time.deltaTime;actor.transform.position=Vector3.Lerp(start,mount,t/3);actor.transform.rotation=Quaternion.LookRotation(mount-start);Shot(new Vector3(11,3,-9),new Vector3(3,1,-1),60);}yield return null;}
   var pose=actor.AddComponent<IntroActorPose>();pose.pose=IntroActorPose.Pose.Ride;pose.leftGrip=bike.transform.Find("Left grip");pose.rightGrip=bike.transform.Find("Right grip");pose.weight=0;if(anim)anim.CrossFade("idle",.2f);start=actor.transform.position;var seated=bike.transform.TransformPoint(new Vector3(0,0,-.35f));t=0;
   while(t<2){if(!game.paused&&game.active){t+=Time.deltaTime;float q=Mathf.SmoothStep(0,1,t/2);pose.weight=q;actor.transform.position=Vector3.Lerp(start,seated,q);actor.transform.rotation=Quaternion.Slerp(actor.transform.rotation,bike.transform.rotation,Time.deltaTime*5);}yield return null;}
   game.sound.Play("alex_motorcycle",.5f);Caption="BẢN SAO BẰNG CHỨNG ĐÃ ĐƯỢC GỬI RA NGOÀI";t=0;while(t<7){if(!game.paused&&game.active){t+=Time.deltaTime;bike.transform.position=PrologueSequence.ParkPosition+Vector3.right*t*t*.7f;actor.transform.SetPositionAndRotation(bike.transform.TransformPoint(new Vector3(0,0,-.35f)),bike.transform.rotation);foreach(var wheel in new[]{bike.transform.Find("Bánh xe trước"),bike.transform.Find("Bánh xe sau")})if(wheel)wheel.Rotate(Vector3.right,Time.deltaTime*650,Space.Self);Shot(new Vector3(12,4,-15),Vector3.Lerp(new Vector3(0,1,0),actor.transform.position+Vector3.up,.25f),65);}yield return null;}
   game.sound.Stop("alex_motorcycle");Caption="NORTH POINT / QUY TRÌNH TIÊU HỦY";Shot(new Vector3(0,6.5f,-17),new Vector3(0,3,16),78);
   for(int i=0;i<5;i++){BlastEffects.Explode(new Vector3(-9+i*4.5f,2,-.6f+i%2*2),9);game.sound.Play("explosion",.85f);Shake(1.5f);yield return Wait(.35f);}foreach(var renderer in exterior.GetComponentsInChildren<Renderer>())if(renderer.bounds.center.z>-1.2f&&Mathf.Abs(renderer.bounds.center.x)<15)renderer.enabled=false;var ruin=new GameObject("Décombres / North Point");for(int r=0;r<32;r++){var piece=GameObject.CreatePrimitive(PrimitiveType.Cube);piece.transform.SetParent(ruin.transform);piece.transform.SetPositionAndRotation(new Vector3(UnityEngine.Random.Range(-13,13),UnityEngine.Random.Range(0,1.1f),UnityEngine.Random.Range(0,25)),UnityEngine.Random.rotation);piece.transform.localScale=new Vector3(UnityEngine.Random.Range(1,4),.4f,UnityEngine.Random.Range(1,3));piece.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("Materials/Black");Destroy(piece.GetComponent<Collider>());}yield return Wait(2.5f);
   Destroy(exterior);Destroy(ruin);Destroy(actor);Destroy(bike);cinematicCamera=false;Playing=false;State=Encounter.Complete;game.state.campaignComplete=true;Checkpoint(5);game.active=false;game.ui.Story("KẾT THÚC / SỰ THẬT ĐƯỢC GIỮ LẠI",StoryData.DestroyEnding+"\n\n"+game.state.kills+" đối thủ bị hạ.",game.Menu,"ending-v2");
  }
  public void Shake(float strength){shake=Mathf.Max(shake,strength);}
  void Shot(Vector3 eye,Vector3 target,float fov){cinematicCamera=true;shotEye=eye;shotTarget=target;shotFov=fov;}
  void LateUpdate(){if(!game || game.paused || !game.active)return;if(cinematicCamera){game.view.transform.SetPositionAndRotation(shotEye,Quaternion.LookRotation(shotTarget-shotEye));game.view.fieldOfView=shotFov;}if(shake>0){shake=Mathf.MoveTowards(shake,0,Time.deltaTime);game.view.transform.position+=UnityEngine.Random.insideUnitSphere*Mathf.Min(.15f,shake*.12f);}}
  IEnumerator Wait(float seconds){float t=0;while(t<seconds){if(!game.paused&&game.active)t+=Time.deltaTime;yield return null;}}
 }
}
