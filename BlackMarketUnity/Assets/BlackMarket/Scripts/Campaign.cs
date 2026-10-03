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
        public string objective, objectiveId;
        public float upload = -1;
        public bool paused => ui != null && ui.modal != "";
        public bool Running => active && !paused && state.hp > 0;
        public string SavePath => Path.Combine(Array.IndexOf(Environment.GetCommandLineArgs(),"--self-test")>=0 ? Application.temporaryCachePath : Application.persistentDataPath,"north-point-unity-v1.json");
        public Transform Marker(string id) {
            foreach(var p in level.GetComponentsInChildren<WorldMarker>()) if(p.id==id) return p.transform;
            return level.transform;
        }
        void Awake() {
            Instance=this; Application.targetFrameRate=60;
            sound=gameObject.AddComponent<Soundscape>();
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
            ui.Story("THE LAST CALL",StoryData.Prologue,()=>LoadWorld(0));
        }
        public bool HasSave() { return ReadSave()!=null; }
        CampaignSave ReadSave() {
            try {
                if(!File.Exists(SavePath)) return null;
                var s=JsonUtility.FromJson<CampaignSave>(File.ReadAllText(SavePath));
                if(s==null || s.version!=1 || s.stage<0 || s.stage>6 || !float.IsFinite(s.hp) || !float.IsFinite(s.elapsed)) return null;
                s.hp=Mathf.Clamp(s.hp,1,100); s.ammo=Mathf.Clamp(s.ammo,0,8); s.reserve=Mathf.Clamp(s.reserve,0,32);
                return s;
            } catch { return null; }
        }
        public void Save() {
            try { Directory.CreateDirectory(Path.GetDirectoryName(SavePath)); File.WriteAllText(SavePath+".tmp",JsonUtility.ToJson(state,true)); if(File.Exists(SavePath))File.Replace(SavePath+".tmp",SavePath,null);else File.Move(SavePath+".tmp",SavePath); }
            catch(Exception e) { ui?.Toast("Không thể lưu checkpoint: "+e.Message); }
        }
        public void Continue() { var s=ReadSave(); if(s==null) {ui.Toast("Không có checkpoint hợp lệ.");return;} state=s; LoadWorld(s.stage); }
        public void Advance() {
            state.stage=Mathf.Min(6,state.stage+1);state.hp=Mathf.Max(70,state.hp);
            if(state.armed) state.reserve=Mathf.Max(24,state.reserve);
            Save(); LoadWorld(state.stage); ui.Toast("CHECKPOINT / "+StoryData.Chapters[state.stage]);
        }
        public void LoadWorld(int stage) {
            Time.timeScale=1; flags.Clear(); enemies.Clear(); upload=-1;
            security.Close();
            if(level) {level.SetActive(false);Destroy(level);}
            if(player) {player.gameObject.SetActive(false);Destroy(player.gameObject);}
            state.stage=stage;
            var prefab=Resources.Load<GameObject>(stage<2 ? "Worlds/NorthPointShop" : stage==6 ? "Worlds/ControlRoom" : "Worlds/NorthPointBunker");
            if(!prefab) {Debug.LogError("Build North Point scenes using BLACK MARKET / Prepare project.");return;}
            level=Instantiate(prefab); level.name=prefab.name;if(stage<2)level.AddComponent<ExteriorRain>();
            foreach(var lamp in level.GetComponentsInChildren<Light>()) lamp.enabled=true;
            var go=new GameObject("Alex Carter");go.transform.SetPositionAndRotation(Marker("spawn").position,Marker("spawn").rotation);
            player=go.AddComponent<PlayerMotor>();player.campaign=this;
            player.Setup();view=player.camera;
            security.Setup();ui.modal="";active=true;LockCursor(true);
            if(stage==0) Objective("keycard","Tìm thẻ Marcus trong văn phòng phía sau cửa hàng.");
            if(stage==1) {
                Objective("radio","Chưa có vũ khí. Bật radio ở Workshop để đánh lạc hướng lính.");
                Spawn("enemy_a",true);Spawn("enemy_b",true);
                ui.Subtitle("INTERCOM","Collection seventy-one. Không để nhân chứng rời khỏi North Point.");
                security.SetDark(true);
            }
            if(stage==2) Objective("pistol","Khám phá tầng −03. Nhận Pistol tại Armory.");
            if(stage==3) {Objective("security","Mở Tablet: quan sát camera, bật báo động và điều khiển cửa B.");Spawn("enemy_a");Spawn("enemy_b",true);}
            if(stage==4) {Objective("security","Qua Storage và Medical. Dùng đèn và cửa để chia cắt đối thủ.");Spawn("enemy_a");Spawn("enemy_b");Spawn("enemy_c");}
            if(stage==5) Objective("upload","Đến Uplink. Phát tán bằng chứng ORDER 071.");
            if(stage==6) {Objective("boss","Đối mặt Victor Hale trong Control Room.");Spawn("boss",false,true);}
            foreach(var p in level.GetComponentsInChildren<Interaction>()) {
                bool hide=(p.id=="pistol" || p.id=="order" || p.id=="recording") && stage!=2;
                hide|=p.id=="upload" && stage!=5;
                hide|=p.id=="exit" && (stage<3 || stage>5);
                p.gameObject.SetActive(!hide);
            }
        }
        public EnemyController Spawn(string id,bool scout=false,bool boss=false) {
            var go=new GameObject(boss?"Victor Hale":scout?"Scout":"Purge Operator");go.transform.SetParent(level.transform);go.transform.position=Marker(id).position;
            var e=go.AddComponent<EnemyController>();e.campaign=this;e.boss=boss;e.hp=boss?300:scout?50:100;e.damage=boss?15:scout?10:15;e.speed=scout?4.5f:3;
            e.routeId=id;e.Setup();enemies.Add(e);return e;
        }
        public void Objective(string id,string text) {objectiveId=id;objective=text;}
        public bool Available(string id) {
            switch(id) {
                case "keycard":return state.stage==0 && !state.keycard;
                case "computer":return state.stage==0 && state.keycard && !flags.Contains("computer");
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
                case "note":ui.Story("MARCUS / GHI CHÚ","Alex, thẻ của chú ở trên bàn trong văn phòng. Đừng ở lại sau 23 giờ. Và dù chuyện gì xảy ra… đừng mở Door 06.",null);break;
                case "keycard":state.keycard=true;p.consumed=true;Objective("computer","Đọc terminal trên bàn Marcus — ORDER #071.");ui.Subtitle("ALEX","Thẻ của chú… Sao lại có quyền truy cập tầng −03?");break;
                case "computer":flags.Add("computer");Objective("door06","Kiểm tra Door 06 ở cuối Warehouse.");ui.Story("ORDER #071","COLLECTION 23:00\nKEEPER: MARCUS CARTER\nSTATUS: DECEASED\n\nĐây không phải một đơn hàng. Tên của chú nằm trong hồ sơ bị xóa.",null);break;
                case "door06":
                    if(state.stage==0) {
                        if(!flags.Contains("computer")){ui.Toast("ACCESS DENIED / Kiểm tra terminal Marcus trước.");break;}
                        ui.Story("22:58 / VISITORS ARRIVE","Một chiếc SUV dừng trước cửa.\n\n“Collection seventy-one.”\n\n23:00. Nguồn điện bị cắt. Liên lạc bên ngoài bị gây nhiễu. Tiếng giày vang lên trong kho. Alex chưa có vũ khí.",Advance);
                    } else {
                        if(!flags.Contains("radio")){ui.Toast("Dùng radio để kéo lính khỏi Door 06.");break;}
                        ui.Story("EMERGENCY SUCCESSION","KEYCARD REJECTED\nCARTER DNA: MATCH\n\nMột tiếng khóa nặng nề. Door 06 mở ra, để lộ thang máy xuống tầng −03. Marcus đã chuẩn bị cho ngày này.",Advance);
                    }break;
                case "radio":flags.Add("radio");Noise(p.transform.position,12);sound.Play("ring",.5f,p.transform.position);Objective("door06","Đi khom và lẻn đến máy quét Door 06.");break;
                case "pistol":state.armed=true;p.consumed=true;Objective("order","Tìm Locker 071. Dùng thẻ Marcus và sinh trắc Carter.");ui.Subtitle("ARMORY","Pistol / 8 viên. Chuột phải ngắm, chuột trái bắn, R nạp đạn.");break;
                case "order":state.drive=true;p.consumed=true;Objective("recording","Đưa Data Drive tới terminal FOR_ALEX trong Server Room.");break;
                case "recording":state.tablet=true;ui.Story("FOR_ALEX / MARCUS CARTER",StoryData.Recording,Advance);break;
                case "exit":
                    if(state.stage<5 && !flags.Contains("security_done")){ui.Toast("Hoàn thành mục tiêu an ninh trước khi chuyển khu.");break;}
                    if(state.stage==5 && !flags.Contains("uploaded")){ui.Toast("Chờ Uplink truyền xong bằng chứng.");break;}
                    Advance();break;
                case "upload":upload=35;Spawn("enemy_a");Spawn("enemy_b");Spawn("enemy_c",true);Noise(player.transform.position,40);Objective("survive","Bảo vệ Uplink trong 35 giây. Tận dụng cửa và vật che chắn.");break;
                case "override":security.revoked=false;foreach(var e in enemies)if(e.boss){e.shielded=false;e.stun=4;}Objective("boss","Quyền Keeper đã khôi phục. Dùng EMP và đánh bại Victor.");break;
                case "final":ui.modal="choice";LockCursor(false);break;
                default:
                    if(p.id.StartsWith("supply")) {
                        if(state.hp>=100 && state.reserve>=32){ui.Toast("Máu và đạn đã đầy.");break;}
                        state.hp=Mathf.Min(100,state.hp+45);state.reserve=Mathf.Min(32,state.reserve+16);p.consumed=true;ui.Toast("+45 HP / +16 ĐẠN");
                    }break;
            }
        }
        public void Noise(Vector3 p,float radius) {foreach(var e in enemies)if(e)e.Hear(p,radius);}
        public void Damage(float amount) {
            if(!Running)return;state.hp=Mathf.Max(0,state.hp-amount*(ui.assist? .65f:1));ui.hurt=1;sound.Play("hit",.35f);
            if(state.hp<=0){security.Close();ui.modal="death";LockCursor(false);}
        }
        public void Pause(){if(!active || paused)return;security.Close();ui.modal="pause";player.ResetTouch();LockCursor(false);}
        public void Resume(){ui.modal="";LockCursor(true);}
        public void Menu(){security.Close();active=false;ui.modal="menu";LockCursor(false);}
        void Update() {
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame && active) {if(security.opened)security.Close();else if(ui.modal=="pause")Resume();else Pause();}
            Time.timeScale=paused?0:1;
            if(Running && Keyboard.current!=null && Keyboard.current.tabKey.wasPressedThisFrame)security.Toggle();
            if(!Running)return;state.elapsed+=Time.deltaTime;
            if(upload>0){upload-=Time.deltaTime;if(upload<=0){flags.Add("uploaded");Objective("exit","Bằng chứng đã truyền. Đến Control Room đối mặt Victor.");}}
        }
        bool IsSelfTest => Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(),"--self-test")>=0;
        void OnApplicationFocus(bool focus){if(!focus && Running && !IsSelfTest)Pause();}
        void OnApplicationPause(bool value){if(value && Running && !IsSelfTest)Pause();}
        void OnDestroy(){Time.timeScale=1;Cursor.lockState=CursorLockMode.None;if(Instance==this)Instance=null;}
    }
}
