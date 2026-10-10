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
        public PrologueSequence prologue;
        public BasementAirlock airlock;
        public UndergroundCampaign underground;
        public bool HasSeenStory(string id)=>!string.IsNullOrEmpty(id) && StoryMemory.Seen(SavePath,id);
        public void MarkStorySeen(string id)=>StoryMemory.Mark(SavePath,id);
        public string objective, objectiveId;
        public string ObjectivePurpose => StoryData.Purpose(objectiveId,state.stage);
        public string ApproachHint => StealthGuidance.Current(this);
        public readonly List<string> journal = new List<string>();
        public float upload = -1;
        public bool paused => ui != null && ui.modal != "";
        public bool Running => active && !paused && !(underground && underground.Playing) && !(prologue && prologue.Playing) && !(opening && opening.Playing) && !(airlock && airlock.Scanning) && state.hp > 0;
        public bool IsSelfTest => Debug.isDebugBuild && Array.Exists(Environment.GetCommandLineArgs(),a=>a.EndsWith("-check") || a=="--self-test");
        public string SavePath => Path.Combine(IsSelfTest?Application.temporaryCachePath:Application.persistentDataPath,IsSelfTest?"north-point-v2-test.json":"north-point-unity-v1.json");
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
            LoadWorld(0);prologue=gameObject.AddComponent<PrologueSequence>();prologue.Begin(this);
        }
        public bool HasSave() { return ReadSave()!=null; }
        CampaignSave ReadSave() {
            try {
                if(!File.Exists(SavePath)) return null;
                var s=JsonUtility.FromJson<CampaignSave>(File.ReadAllText(SavePath));
                if(s==null || (s.version!=1 && s.version!=2) || s.stage<0 || s.stage>6 || !float.IsFinite(s.hp) || !float.IsFinite(s.elapsed))return null;
                if(s.version==1){s.version=2;s.stage=s.stage==0?0:1;s.checkpoint=0;s.armed=s.hasAK=s.hasNightVision=s.tablet=false;s.grenades=0;s.rifleAmmo=s.rifleReserve=0;s.hp=100;}
                if(s.stage>2 || s.checkpoint<0 || s.checkpoint>6 || (s.checkpoint==6 && s.stage!=1))return null;
                s.hp=Mathf.Clamp(s.hp,1,100);s.ammo=Mathf.Clamp(s.ammo,0,8);s.reserve=Mathf.Clamp(s.reserve,0,96);s.rifleAmmo=Mathf.Clamp(s.rifleAmmo,0,30);s.rifleReserve=Mathf.Clamp(s.rifleReserve,0,450);s.grenades=Mathf.Clamp(s.grenades,0,12);s.selectedWeapon=s.hasSniper&&s.selectedWeapon==2?2:s.hasAK?1:0;s.sniperAmmo=Mathf.Clamp(s.sniperAmmo,0,5);s.sniperReserve=Mathf.Clamp(s.sniperReserve,0,80);
                return s;
            } catch { return null; }
        }
        public void Save() {
            try { Directory.CreateDirectory(Path.GetDirectoryName(SavePath)); File.WriteAllText(SavePath+".tmp",JsonUtility.ToJson(state,true)); if(File.Exists(SavePath))File.Replace(SavePath+".tmp",SavePath,null);else File.Move(SavePath+".tmp",SavePath); }
            catch(Exception e) { ui?.Toast("Không thể lưu điểm lưu: "+e.Message); }
        }
        public void Continue() { var s=ReadSave(); if(s==null) {ui.Toast("Không có điểm lưu hợp lệ.");return;} state=s;if(s.campaignComplete){ui.Story("KẾT THÚC / ORDER 71",StoryData.DestroyEnding,Menu,"ending-v2");return;} LoadWorld(s.stage);if(s.stage==0 && !s.introCompleted){prologue=gameObject.AddComponent<PrologueSequence>();prologue.Begin(this);} }
        public void Advance() {
            state.stage=Mathf.Min(2,state.stage+1);state.checkpoint=0;
            if(state.armed) state.reserve=Mathf.Max(24,state.reserve);
            Save(); LoadWorld(state.stage); ui.Toast("ĐIỂM LƯU / "+StoryData.Chapters[state.stage]);
        }
        public void LoadWorld(int stage) {
            if(prologue){prologue.Cancel();Destroy(prologue);prologue=null;}
            Time.timeScale=1; flags.Clear(); enemies.Clear(); journal.Clear(); upload=-1;
            security.Close();opening=null;airlock=null;underground=null;
            if(level) {level.SetActive(false);Destroy(level);}
            if(player) {player.gameObject.SetActive(false);Destroy(player.gameObject);}
            stage=Mathf.Clamp(stage,0,2);state.stage=stage;
            var prefab=Resources.Load<GameObject>("Worlds/"+StoryData.Worlds[stage]);
            if(!prefab) {Debug.LogError("Build North Point scenes using BLACK MARKET / Prepare project.");return;}
            level=Instantiate(prefab); level.name=prefab.name;VietnameseText.Apply(level);airlock=level.GetComponentInChildren<BasementAirlock>();if(stage==0){level.AddComponent<ExteriorRain>();opening=level.AddComponent<OpeningSequence>();opening.Setup(this);}
            foreach(var lamp in level.GetComponentsInChildren<Light>()) lamp.enabled=true;
            var go=new GameObject("Alex Carter");go.transform.SetPositionAndRotation(Marker(stage==1&&state.checkpoint>0?"combat_spawn":"spawn").position,Marker("spawn").rotation);
            player=go.AddComponent<PlayerMotor>();player.campaign=this;
            player.Setup();view=player.camera;
            security.Setup();ui.modal="";active=true;LockCursor(true);

            if(stage==0){PrologueSequence.EnsureBike(this);Objective("drawers","Tìm USB đỏ trong các ngăn tủ sát tường tại văn phòng Marcus.");PrologueSequence.AddJournal(this);}
            if(stage>0){underground=level.AddComponent<UndergroundCampaign>();underground.Setup(this);}
        }

        public void SpawnGuards(int count,bool scout=false){for(int i=0;i<count;i++)Spawn("enemy_"+(char)('a'+i),scout || i%3==2);}
        public EnemyController Spawn(string id,bool scout=false,bool boss=false) {
            var go=new GameObject(boss?"Victor Hale":scout?"Scout":"Purge đặc vụ");go.transform.SetParent(level.transform);go.transform.position=Marker(id).position;
            var e=go.AddComponent<EnemyController>();e.campaign=this;e.boss=boss;e.hp=boss?300:scout?50:100;e.damage=boss?15:scout?10:15;e.speed=scout?4.5f:3;
            e.actorPrefab=boss?"Victor":EncounterData.Guard(state.stage,enemies.FindAll(x=>x && !x.boss).Count);e.routeId=id;e.Setup();enemies.Add(e);return e;
        }
        public void Objective(string id,string text) {objectiveId=id;objective=text;journal.Add(text+"\n→ "+ObjectivePurpose);}
        public bool Available(string id) {
            if(underground)return underground.Available(id);
            switch(id){
                case "keycard":return !state.keycard;
                case "computer":return !flags.Contains("computer");
                case "face_scan":return airlock && airlock.OuterUnlocked && !airlock.FaceVerified && !airlock.Scanning;
                default:return true;
            }
        }
        public void Interact(Interaction p) {
            if(!Running || !p.Available)return;
            sound.Play("beep",.2f);
            if(underground){underground.Interact(p);return;}
            switch(p.id){
                case "frontdoor":ui.Toast(opening && opening.PursuitStarted?"Lối ra bị chúng chặn. Chạy tới Cửa 006 trong kho!":"Tới văn phòng tìm USB đỏ của Marcus rồi mở máy tính.");break;
                case "note":ui.Story("MARCUS / GHI CHÚ","Alex, USB đỏ của chú ở trong một ngăn kéo tại văn phòng. Đừng ở lại sau 23 giờ. Và dù chuyện gì xảy ra… đừng mở Cửa 006.",null);break;
                case "keycard":state.keycard=true;p.consumed=true;p.gameObject.SetActive(false);Objective("computer","Cắm USB đỏ vào máy tính của Marcus, rồi bấm tệp Order 71.");ui.Subtitle("ALEX","USB của chú… Đây là khóa truy cập, không chỉ là một ổ lưu trữ.");break;
                case "computer":if(!state.keycard){ui.Toast("Máy tính bị khóa. Tìm USB đỏ trong các ngăn tủ sát tường trước.");break;}ui.OpenComputerDesktop();break;
                case "face_scan":airlock?.StartScan();break;
                case "door06":if(!flags.Contains("computer") || !opening || !opening.PursuitStarted){ui.Toast("Đọc hết phần mở của Order 71 trên máy tính trước.");break;}airlock.UnlockOuter();if(state.promisedMarcus)ui.Subtitle("ALEX","Cháu xin lỗi, chú Marcus… cháu không còn đường nào khác.");break;
            }
        }
        public void Noise(Vector3 p,float radius) {foreach(var e in enemies)if(e)e.Hear(p,radius);}
        public bool EnemyShot(float normalDamage){if(!Running)return false;if(state.stage>0&&underground)return underground.CombatHit(normalDamage,false);Damage(normalDamage);return true;}
        public void Damage(float amount) {
            if(!Running)return;state.hp=Mathf.Max(underground&&underground.Suppressing?1:0,state.hp-amount);ui.hurt=1;sound.Play("hit",.35f);
            if(underground&&underground.TryEmergencyRetreat())return;
            if(state.hp<=0){security.Close();ui.modal="death";player.ResetTouch();LockCursor(false);}
        }
        public void Pause(){if(!active || paused)return;security.Close();ui.modal="pause";player.ResetTouch();LockCursor(false);}
        public void Resume(){player?.ResetTouch();ui.modal="";Time.timeScale=1;LockCursor(!(underground && underground.Playing) && !(prologue && prologue.Playing) && !(opening && opening.Playing) && !(airlock && airlock.Scanning));}
        public void Menu(){player?.ResetTouch();security.Close();active=false;ui.modal="menu";LockCursor(false);}
        void Update() {
            if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame) {if(security.opened)security.Close();else if(paused)ui.Back();else if(active)Pause();}
            Time.timeScale=paused?0:1;
            if(Running && Keyboard.current!=null && Keyboard.current.tabKey.wasPressedThisFrame)security.Toggle();
            if(active && Keyboard.current!=null && Keyboard.current.jKey.wasPressedThisFrame){if(ui.modal=="journal")Resume();else if(Running)ui.OpenJournal();}
            if(!Running)return;state.elapsed+=Time.deltaTime;
            if(upload>0){upload-=Time.deltaTime;if(upload<=0){flags.Add("uploaded");Objective("exit","Bằng chứng đã truyền. Đến phòng điều hành đối mặt Victor.");}}
        }
        void OnApplicationFocus(bool focus){if(!focus && (Running || underground && underground.Playing || prologue && prologue.Playing || opening && opening.Playing || airlock && airlock.Scanning) && !IsSelfTest)Pause();}
        void OnApplicationPause(bool value){if(value && (Running || underground && underground.Playing || prologue && prologue.Playing || opening && opening.Playing || airlock && airlock.Scanning) && !IsSelfTest)Pause();}
        void OnDestroy(){Time.timeScale=1;Cursor.lockState=CursorLockMode.None;if(Instance==this)Instance=null;}
    }
}
