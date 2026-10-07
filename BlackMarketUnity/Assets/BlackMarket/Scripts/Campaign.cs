using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
namespace BlackMarket {
    public class Campaign : MonoBehaviour {
        public static Campaign Instance;
        public CampaignSave state = new CampaignSave();
        public readonly HashSet<string> flags = new HashSet<string>();
        public readonly List<EnemyController> enemies = new List<EnemyController>();
        public GameObject level;
        public PlayerMotor player;
        public Camera view;
        public SecurityConsole security;
        public GameInterface ui;
        public Soundscape sound;
        public bool active;
        public OpeningSequence opening;
        public BasementAirlock airlock;
        public bool HasSeenStory(string id)=>!string.IsNullOrEmpty(id) && StoryMemory.Seen(SavePath,id);
        public void MarkStorySeen(string id)=>StoryMemory.Mark(SavePath,id);
        public string objective, objectiveId;
        public string ObjectivePurpose => StoryData.Purpose(objectiveId,state.stage);
        public string ApproachHint => StealthGuidance.Current(this);
        public readonly List<string> journal = new List<string>();
        public float upload = -1;
        public bool paused => ui != null && ui.modal != "";
        public bool Running => active && !paused && !(opening && opening.Playing) && !(airlock && airlock.Scanning) && state.hp > 0;
        public string SavePath => Path.Combine(Array.IndexOf(Environment.GetCommandLineArgs(),"--self-test")>=0 || Array.IndexOf(Environment.GetCommandLineArgs(),"--shop-check")>=0 || Array.IndexOf(Environment.GetCommandLineArgs(),"--opening-check")>=0 || (Array.IndexOf(Environment.GetCommandLineArgs(),"--mission-check")>=0 || Array.IndexOf(Environment.GetCommandLineArgs(),"--revision-check")>=0) ? Application.temporaryCachePath : Application.persistentDataPath,Array.IndexOf(Environment.GetCommandLineArgs(),"--opening-check")>=0 || (Array.IndexOf(Environment.GetCommandLineArgs(),"--mission-check")>=0 || Array.IndexOf(Environment.GetCommandLineArgs(),"--revision-check")>=0)?"north-point-opening-test.json":"north-point-unity-v1.json");
        public Transform Marker(string id) {
            foreach(var p in level.GetComponentsInChildren<WorldMarker>()) if(p.id==id) return p.transform;
            return level.transform;
        }
        void Awake() {
            Instance=this; Application.targetFrameRate=60;
            sound=gameObject.AddComponent<Soundscape>();
            gameObject.AddComponent<TensionScore>();
            ui=gameObject.AddComponent<GameInterface>();
            security=gameObject.AddComponent<SecurityConsole>();
            LoadWorld(0); active=false; ui.modal="menu"; LockCursor(false);
        }
        public void LockCursor(bool locked) {
            Cursor.lockState=locked && !Application.isMobilePlatform ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible=!locked || Application.isMobilePlatform;
        }
        public void NewGame() {
            state=new CampaignSave(); flags.Clear(); Save();
            ui.Story("CUỘC GỌI CUỐI CÙNG",StoryData.Prologue,()=>LoadWorld(0),"prologue");
        }
        public bool HasSave() { return ReadSave()!=null; }
        CampaignSave ReadSave() {
            try {
                if(!File.Exists(SavePath)) return null;
                var s=JsonUtility.FromJson<CampaignSave>(File.ReadAllText(SavePath));
                if(s==null || s.version!=1 || s.stage<0 || s.stage>6 || !float.IsFinite(s.hp) || !float.IsFinite(s.elapsed)) return null;
                s.hp=Mathf.Clamp(s.hp,1,100); s.ammo=Mathf.Clamp(s.ammo,0,8); s.reserve=Mathf.Clamp(s.reserve,0,32);
                s.rifleAmmo=Mathf.Clamp(s.rifleAmmo,0,30);s.rifleReserve=Mathf.Clamp(s.rifleReserve,0,90);s.selectedWeapon=s.hasAK && s.selectedWeapon==1?1:0;
                return s;
            } catch { return null; }
        }
        public void Save() {
            try { Directory.CreateDirectory(Path.GetDirectoryName(SavePath)); File.WriteAllText(SavePath+".tmp",JsonUtility.ToJson(state,true)); if(File.Exists(SavePath))File.Replace(SavePath+".tmp",SavePath,null);else File.Move(SavePath+".tmp",SavePath); }
            catch(Exception e) { ui?.Toast("Không thể lưu điểm lưu: "+e.Message); }
        }
        public void Continue() { var s=ReadSave(); if(s==null) {ui.Toast("Không có điểm lưu hợp lệ.");return;} state=s; LoadWorld(s.stage); }
        public void Advance() {
            state.stage=Mathf.Min(6,state.stage+1);state.hp=Mathf.Max(70,state.hp);
            if(state.armed) state.reserve=Mathf.Max(24,state.reserve);
            Save(); LoadWorld(state.stage); ui.Toast("ĐIỂM LƯU / "+StoryData.Chapters[state.stage]);
        }
        public void LoadWorld(int stage) {
            Time.timeScale=1; flags.Clear(); enemies.Clear(); journal.Clear(); upload=-1;
            security.Close();opening=null;airlock=null;
            if(level) {level.SetActive(false);Destroy(level);}
            if(player) {player.gameObject.SetActive(false);Destroy(player.gameObject);}
            state.stage=stage;
            var prefab=Resources.Load<GameObject>("Worlds/"+StoryData.Worlds[stage]);
            if(!prefab) {Debug.LogError("Build North Point scenes using BLACK MARKET / Prepare project.");return;}
            level=Instantiate(prefab); level.name=prefab.name;VietnameseText.Apply(level);airlock=level.GetComponentInChildren<BasementAirlock>();if(stage==0){level.AddComponent<ExteriorRain>();opening=level.AddComponent<OpeningSequence>();opening.Setup(this);}
            foreach(var lamp in level.GetComponentsInChildren<Light>()) lamp.enabled=true;
            var go=new GameObject("Alex Carter");go.transform.SetPositionAndRotation(Marker("spawn").position,Marker("spawn").rotation);
            player=go.AddComponent<PlayerMotor>();player.campaign=this;
            player.Setup();view=player.camera;
            security.Setup();ui.modal="";active=true;LockCursor(true);

            if(stage==0) Objective("drawers","Tìm thẻ đỏ trong các ngăn tủ sát tường tại văn phòng Marcus.");
            if(stage==1) {
                Objective("radio","Chưa có vũ khí. Bật đài phát thanh ở xưởng kỹ thuật để đánh lạc hướng lính.");
                SpawnGuards(4,true);
                ui.Subtitle("LOA NỘI BỘ","Thu hồi hồ sơ bảy mươi mốt. Không để nhân chứng rời khỏi North Point.");
                security.SetDark(true);
            }
            if(stage==2) Objective("pistol","Xuống cầu thang. Nhận súng trong phòng kho vũ khí bên trái.");
            if(stage==3) {Objective("security","Mở máy tính bảng: quan sát máy quay, bật báo động và điều khiển cửa B.");SpawnGuards(4);}
            if(stage==4) {Objective("security","Qua kho hàng và khu y tế. Dùng đèn và cửa để chia cắt đối thủ.");SpawnGuards(6);}
            if(stage==5) Objective("upload","Đến trạm truyền dữ liệu. Phát tán bằng chứng HỒ SƠ 071.");
            if(stage==6) {Objective("boss","Chuẩn bị trong hai phòng trang bị, rồi đối mặt Victor.");Spawn("boss",false,true);Spawn("enemy_a");}
            foreach(var p in level.GetComponentsInChildren<Interaction>()) {
                bool hide=(p.id=="pistol" || p.id=="order" || p.id=="recording") && stage!=2;
                hide|=p.id=="upload" && stage!=5;
                hide|=(p.id=="ak" && state.hasAK) || (p.id=="nightvision" && state.hasNightVision);
                hide|=p.id=="exit" && (stage<2 || stage>5);
                p.gameObject.SetActive(!hide);
            }
        }
        public void SpawnGuards(int count,bool scout=false){for(int i=0;i<count;i++)Spawn("enemy_"+(char)('a'+i),scout || i%3==2);}
        public EnemyController Spawn(string id,bool scout=false,bool boss=false) {
            var go=new GameObject(boss?"Victor Hale":scout?"Scout":"Purge đặc vụ");go.transform.SetParent(level.transform);go.transform.position=Marker(id).position;
            var e=go.AddComponent<EnemyController>();e.campaign=this;e.boss=boss;e.hp=boss?300:scout?50:100;e.damage=boss?15:scout?10:15;e.speed=scout?4.5f:3;
            e.actorPrefab=boss?"Victor":EncounterData.Guard(state.stage,enemies.FindAll(x=>x && !x.boss).Count);e.routeId=id;e.Setup();enemies.Add(e);return e;
        }
        public void Objective(string id,string text) {objectiveId=id;objective=text;journal.Add(text+"\n→ "+ObjectivePurpose);}
        public bool Available(string id) {
            switch(id) {
                case "ak":return state.stage==6 && !state.hasAK;
                case "nightvision":return state.stage==6 && !state.hasNightVision;
                case "keycard":return state.stage==0 && !state.keycard;
                case "computer":return state.stage==0 && !flags.Contains("computer");
                case "face_scan":return state.stage==0 && airlock && airlock.OuterUnlocked && !airlock.FaceVerified && !airlock.Scanning;
                case "radio":return state.stage==1;
                case "pistol":return state.stage==2 && !state.armed;
                case "order":return state.stage==2 && state.armed && !state.drive;
                case "recording":return state.stage==2 && state.drive && !state.tablet;
                case "upload":return state.stage==5 && upload<0 && !flags.Contains("uploaded");
                case "override":return state.stage==6 && security.revoked;
                case "final":return flags.Contains("boss_dead");
                default:return true;
            }
        }
        public void Interact(Interaction p) {
            if(!Running || !p.Available)return;
            sound.Play("beep",.2f);
            switch(p.id) {
                case "ak":state.hasAK=true;state.rifleAmmo=30;state.rifleReserve=90;player.SwitchWeapon(1);p.consumed=true;p.gameObject.SetActive(false);ui.Toast("AK / 30 + 90 viên. [Q] đổi súng, giữ BẮN để bắn tự động.");journal.Add("Đã lấy AK ở phòng quân nhu: bắn tự động, nạp 2,2 giây; vẫn cần máy tính để gỡ lá chắn Victor.");break;
                case "nightvision":state.hasNightVision=true;p.consumed=true;p.gameObject.SetActive(false);player.nightVision.Toggle();ui.Toast("KÍNH ĐÊM / [N] bật tắt. Dùng máy tính bảng tắt đèn để tận dụng lợi thế.");journal.Add("Đã nhặt kính đêm ở phòng quang học. Kính khuếch đại hình ảnh, không soi xuyên tường và không vô hiệu hóa đèn pin của đối thủ.");break;
                case "frontdoor":ui.Toast(opening && opening.PursuitStarted?"Lối ra bị chúng chặn. Chạy tới Cửa 006 trong kho!":"Cửa trước đã khóa. Tới văn phòng lấy thẻ của chú rồi mở máy tính.");break;
                case "note":ui.Story("MARCUS / GHI CHÚ","Alex, thẻ đỏ của chú ở trong một ngăn kéo tại văn phòng. Đừng ở lại sau 23 giờ. Và dù chuyện gì xảy ra… đừng mở Cửa 006.",null);break;
                case "keycard":state.keycard=true;p.consumed=true;p.gameObject.SetActive(false);Objective("computer","Dùng thẻ đỏ mở máy tính của Marcus, rồi bấm tệp Order 71.");ui.Subtitle("ALEX","Thẻ của chú… Sao thẻ cửa hàng lại có quyền xuống sáu tầng hầm?");break;
                case "computer":if(!state.keycard){ui.Toast("Máy tính bị khóa. Tìm thẻ đỏ trong các ngăn tủ sát tường trước.");break;}ui.OpenComputerDesktop();break;
                case "face_scan":airlock?.StartScan();break;
                case "door06":
                    if(state.stage==0) {
                        if(!flags.Contains("computer")){ui.Toast("TRUY CẬP BỊ TỪ CHỐI / Kiểm tra máy tính Marcus trước.");break;}
                        if(!opening || !opening.PursuitStarted){ui.Toast("Đọc hết hồ sơ trên máy tính của chú trước.");break;}
                        airlock.UnlockOuter();
                    } else {
                        if(!flags.Contains("radio")){ui.Toast("Bật đài phát thanh trong xưởng để kéo lính khỏi cầu thang xuống B2.");break;}
                        ui.Story("LỐI XUỐNG KHO HỒ SƠ","Quyền truy cập của Alex đã được xác nhận tại Cửa 006. Lối xuống B2 mở ra sau khi đội lục soát bị đánh lạc hướng.\n\nB2: lấy súng tự vệ tại kho vũ khí, tìm Tủ hồ sơ 071 rồi mở bản ghi GỬI ALEX. Đây là nơi Marcus giấu sự thật.",Advance);
                    }break;
                case "radio":flags.Add("radio");Noise(p.transform.position,12);sound.Play("ring",.5f,p.transform.position);Objective("door06","Đi khom tới máy quét Carter ở cầu thang cuối tầng B1.");break;
                case "pistol":state.armed=true;p.consumed=true;Objective("order","Tìm Tủ hồ sơ 071. Dùng thẻ Marcus và sinh trắc Carter.");ui.Subtitle("KHO VŨ KHÍ","Súng ngắn / 8 viên. Chuột phải ngắm, chuột trái bắn, R nạp đạn.");break;
                case "order":state.drive=true;p.consumed=true;Objective("recording","Đưa ổ dữ liệu tới máy tính GỬI ALEX ở phòng lưu trữ giữa tầng.");break;
                case "recording":state.tablet=true;p.consumed=true;Objective("exit","Mở cửa cầu thang cuối cánh phải để xuống B3 / An ninh.");ui.Story("GỬI ALEX / MARCUS CARTER",StoryData.Recording+"\n\nBƯỚC TIẾP THEO: Mang máy tính bảng xuống B3. Dùng an ninh để vượt đội thanh trừng và đưa HỒ SƠ 071 tới trạm truyền dữ liệu tại B5.",null);break;
                case "exit":
                    if(state.stage==2 && !state.tablet){ui.Toast("Đọc GỬI ALEX để nhận máy tính bảng và hiểu bằng chứng trước.");break;}
                    if(state.stage>=3 && state.stage<5 && !flags.Contains("security_done")){ui.Toast("Hoàn thành mục tiêu an ninh trước khi chuyển khu.");break;}
                    if(state.stage==5 && !flags.Contains("uploaded")){ui.Toast("Chờ trạm truyền dữ liệu truyền xong bằng chứng.");break;}
                    ui.Story("CẦU THANG / "+StoryData.Locations[state.stage+1],StoryData.Briefings[state.stage+1],Advance);break;
                case "upload":upload=35;SpawnGuards(6);Noise(player.transform.position,40);Objective("survive","Bảo vệ trạm truyền dữ liệu trong 35 giây. Tận dụng cửa và vật che chắn.");break;
                case "override":security.revoked=false;foreach(var e in enemies)if(e.boss){e.shielded=false;e.stun=4;}Objective("boss","Quyền quản lý đã khôi phục. Dùng xung điện từ và đánh bại Victor.");break;
                case "final":ui.modal="choice";LockCursor(false);break;
                default:
                    if(p.id.StartsWith("supply")) {
                        if(state.hp>=100 && state.reserve>=32 && (!state.hasAK || state.rifleReserve>=90)){ui.Toast("Máu và đạn đã đầy.");break;}
                        state.hp=Mathf.Min(100,state.hp+45);state.reserve=Mathf.Min(32,state.reserve+16);if(state.hasAK)state.rifleReserve=Mathf.Min(90,state.rifleReserve+30);p.consumed=true;ui.Toast(state.hasAK?"+45 MÁU / ĐẠN SÚNG NGẮN & AK":"+45 MÁU / +16 ĐẠN");
                    }break;
            }
        }
        public void Noise(Vector3 p,float radius) {foreach(var e in enemies)if(e)e.Hear(p,radius);}
        public void EnemyShot(float normalDamage){Damage(state.stage<6?50:normalDamage);}
        public void Damage(float amount) {
            if(!Running)return;state.hp=Mathf.Max(0,state.hp-amount);ui.hurt=1;sound.Play("hit",.35f);
            if(state.hp<=0){security.Close();ui.modal="death";player.ResetTouch();LockCursor(false);}
        }
        public void Pause(){if(!active || paused)return;security.Close();ui.modal="pause";player.ResetTouch();LockCursor(false);}
        public void Resume(){player?.ResetTouch();ui.modal="";Time.timeScale=1;LockCursor(!(opening && opening.Playing) && !(airlock && airlock.Scanning));}
        public void Menu(){player?.ResetTouch();security.Close();active=false;ui.modal="menu";LockCursor(false);}
        void Update() {
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) {if(security.opened)security.Close();else if(paused)ui.Back();else if(active)Pause();}
            Time.timeScale=paused?0:1;
            if(Running && Keyboard.current!=null && Keyboard.current.tabKey.wasPressedThisFrame)security.Toggle();
            if(active && Keyboard.current!=null && Keyboard.current.jKey.wasPressedThisFrame){if(ui.modal=="journal")Resume();else if(Running)ui.OpenJournal();}
            if(!Running)return;state.elapsed+=Time.deltaTime;
            if(upload>0){upload-=Time.deltaTime;if(upload<=0){flags.Add("uploaded");Objective("exit","Bằng chứng đã truyền. Đến phòng điều hành đối mặt Victor.");}}
        }
        bool IsSelfTest => Debug.isDebugBuild && (Array.IndexOf(Environment.GetCommandLineArgs(),"--self-test")>=0 || Array.IndexOf(Environment.GetCommandLineArgs(),"--shop-check")>=0 || Array.IndexOf(Environment.GetCommandLineArgs(),"--opening-check")>=0 || (Array.IndexOf(Environment.GetCommandLineArgs(),"--mission-check")>=0 || Array.IndexOf(Environment.GetCommandLineArgs(),"--revision-check")>=0));
        void OnApplicationFocus(bool focus){if(!focus && (Running || opening && opening.Playing || airlock && airlock.Scanning) && !IsSelfTest)Pause();}
        void OnApplicationPause(bool value){if(value && (Running || opening && opening.Playing || airlock && airlock.Scanning) && !IsSelfTest)Pause();}
        void OnDestroy(){Time.timeScale=1;Cursor.lockState=CursorLockMode.None;if(Instance==this)Instance=null;}
    }
}
