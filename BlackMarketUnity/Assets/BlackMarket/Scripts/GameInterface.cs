using System;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;
using FingerTouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine;
using UnityEngine.InputSystem;
namespace BlackMarket {
    [DefaultExecutionOrder(-10)]
    public class GameInterface : MonoBehaviour {
        public string modal="menu";public bool assist=true;public float sensitivity=1,volume=.65f,brightness=.7f,hurt,hit;
        string title,body,toast,speaker,subtitle,settingsReturn="menu";float toastUntil,subtitleUntil;Action continuation;Vector2 storyScroll,journalScroll;
        string storyKey; float readRemaining,storyContentHeight; bool readEnd;
        string[] terminalPages; int terminalPage; float typedCharacters;
        public bool ComputerScreen => modal=="story" && (storyKey=="terminal071" || storyKey=="base-file-v2" || storyKey=="final-file-v2");
        public int TerminalPage => terminalPage;
        public int TerminalPageCount => terminalPages==null?0:terminalPages.Length;
        public bool TerminalPageReady => ComputerScreen && (game.HasSeenStory(storyKey) || (readRemaining<=0 && typedCharacters>=terminalPages[terminalPage].Length));
        public bool StoryReady => string.IsNullOrEmpty(storyKey) || game.HasSeenStory(storyKey) || (ComputerScreen ? TerminalPageReady : readRemaining<=0 && typedCharacters>=(body??"").Length);
        public float ReadingRemaining => readRemaining;
        public bool TextComplete => modal=="story" && typedCharacters>=(ComputerScreen?terminalPages[terminalPage].Length:(body??"").Length);
        Campaign game;Font regular,bold,mono;GUIStyle text,heading,small,button;
        readonly Color ink=new Color(.025f,.036f,.043f,.96f), ivory=new Color(.87f,.87f,.81f),accent=new Color(.77f,.57f,.33f),muted=new Color(.54f,.62f,.64f);
        const float W=1280,H=720;
        void Awake(){game=GetComponent<Campaign>();regular=Resources.Load<Font>("Fonts/Regular");bold=Resources.Load<Font>("Fonts/Bold");mono=Resources.Load<Font>("Fonts/Mono");assist=PlayerPrefs.GetInt("assist",1)==1;sensitivity=PlayerPrefs.GetFloat("sensitivity",1);volume=PlayerPrefs.GetFloat("volume",.65f);AudioListener.volume=volume;brightness=PlayerPrefs.GetFloat("brightness",.7f);ApplyBrightness();}
        void ApplyBrightness(){var v=FindFirstObjectByType<UnityEngine.Rendering.Volume>();if(v && v.profile.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out var color))color.postExposure.Override(brightness);}
        public void Story(string caption,string content,Action next,string key=null){key=key??"document:"+caption;game.security.Close();storyKey=key;readRemaining=game.HasSeenStory(key)?0:3;typedCharacters=game.HasSeenStory(key)?content.Length:0;readEnd=false;title=caption;body=content;storyContentHeight=Mathf.Max(330,new GUIStyle{font=regular,fontSize=21,wordWrap=true}.CalcHeight(new GUIContent(content),905));readEnd=storyContentHeight<=350;storyScroll=Vector2.zero;continuation=next;modal="story";game.LockCursor(false);game.player?.ResetTouch();
            if(key=="terminal071" || key=="base-file-v2" || key=="final-file-v2"){
                terminalPages=content.Trim().Split(new[]{"\n\n"},StringSplitOptions.RemoveEmptyEntries);terminalPage=0;StartTerminalPage();
            }
        }
        public void Document(string caption,string content,Action next,string key){Story(caption,content,next,key);}
        public void OpenDecoder(){modal="decoder";game.LockCursor(false);game.player.ResetTouch();}
        public Rect OrderFileRect => new Rect(125,210,260,150);
        public void OpenComputerDesktop(){game.security.Close();modal="computer";game.LockCursor(false);game.player.ResetTouch();}
        public void OpenOrderFile(){if(modal!="computer" || !game.state.keycard)return;Story("Order 71",StoryData.Terminal,()=>{game.flags.Add("computer");game.opening.Begin();},"terminal071");}
        public bool TryOpenOrderFile(){if(modal!="computer" || !TryPointerClick(OrderFileRect))return false;OpenOrderFile();return true;}
        void DrawDesktop(){
            Panel(new Rect(65,42,1150,624),new Color(.13f,.15f,.16f));Panel(new Rect(80,57,1120,594),new Color(.025f,.075f,.09f));
            Label("MÁY TÍNH CỦA MARCUS / USB ĐỎ ĐÃ XÁC THỰC",110,85,1020,40,24,accent);
            Panel(OrderFileRect,new Color(.12f,.23f,.25f));Label("▤",215,220,100,60,44);Label("Order 71",170,295,210,45,28);
            Label("Bấm vào tệp Order 71 để mở và đọc nội dung.",110,400,1000,70,24);
            if(ConsumeClick(OrderFileRect))OpenOrderFile();
            if(Button("RỜI MÁY TÍNH",110,562,350))game.Resume();
        }
        void StartTerminalPage(){typedCharacters=game.HasSeenStory(storyKey)?terminalPages[terminalPage].Length:0;readRemaining=game.HasSeenStory(storyKey)?0:3;}
        public void NextTerminalPage(){if(!TerminalPageReady)return;if(terminalPage<terminalPages.Length-1){terminalPage++;StartTerminalPage();}else ContinueStory();}
        public void SkipReadStory(){if(modal!="story" || !game.HasSeenStory(storyKey))return;FinishStory();}
        public void ScrollStory(float amount){ScrollPage(amount);}
        public void OpenSettings(){settingsReturn=modal;modal="settings";}
        public void Back(){if(modal=="settings")modal=settingsReturn;else if(modal=="help")modal=game.active?"pause":"menu";else if(modal=="journal" || modal=="pause" || modal=="computer")game.Resume();}
        public void OpenJournal(){game.security.Close();game.player.ResetTouch();journalScroll=Vector2.zero;modal="journal";game.LockCursor(false);}
        public void Toast(string value){toast=value;toastUntil=Time.unscaledTime+Mathf.Max(4,value.Length*.055f);}
        public void Subtitle(string who,string value){speaker=who;subtitle=value;subtitleUntil=Time.unscaledTime+Mathf.Max(6,value.Length*.055f);}
        public void ContinueStory(){if(modal!="story" || !StoryReady || (ComputerScreen && terminalPage<terminalPages.Length-1))return;FinishStory();}
        void FinishStory(){if(!string.IsNullOrEmpty(storyKey))game.MarkStorySeen(storyKey);var next=continuation;continuation=null;game.Resume();next?.Invoke();}
        void Init(){if(text!=null)return;text=new GUIStyle(GUI.skin.label){font=regular,fontSize=20,wordWrap=true};text.normal.textColor=ivory;heading=new GUIStyle(text){font=bold,fontSize=48};small=new GUIStyle(text){font=mono,fontSize=13};small.normal.textColor=muted;button=new GUIStyle(GUI.skin.button){font=mono,fontSize=16,wordWrap=true,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(20,12,5,5)};}
        void Panel(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
        string ControlText(string value)=>TouchControls?value.Replace("[E]","[DÙNG]").Replace("[C]","[KHOM]").Replace("[N]","[KÍNH ĐÊM]").Replace("[Q]","[ĐỔI SÚNG]").Replace("[G]","[LỰU ĐẠN]").Replace("[chuột phải]","[NGẮM]").Replace("chuột phải","NGẮM").Replace("Chuột phải","NGẮM").Replace("chuột trái","BẮN").Replace("Q đổi","ĐỔI SÚNG để đổi").Replace("R nạp","NẠP để nạp").Replace(" [TAB]","").Replace(" [Tab]","").Replace(" [J]",""):value;
        void Label(string s,float x,float y,float w=900,float h=40,int size=20,Color? color=null){s=ControlText(s);var st=new GUIStyle(text){fontSize=size};st.normal.textColor=color??ivory;GUI.Label(new Rect(x,y,w,h),s,st);}
        bool Button(string s,float x,float y,float w=340,bool primary=false){s=ControlText(s);GUI.backgroundColor=primary?accent:new Color(.15f,.21f,.23f);var rect=new Rect(x,y,w,49);GUI.Box(rect,s,button);bool click=ConsumeClick(rect);GUI.backgroundColor=Color.white;if(click)game.sound.Play("beep",.12f);return click;}
        public Rect TouchRect(string id){switch(id){case "grenade":return new Rect(570,545,135,55);case "move":return new Rect(35,475,220,210);case "fire":return new Rect(1120,505,110,100);case "aim":return new Rect(1120,395,110,80);case "interact":return new Rect(950,530,135,65);case "reload":return new Rect(1000,620,105,55);case "crouch":return new Rect(275,620,100,55);case "run":return new Rect(275,545,100,55);case "tablet":return new Rect(820,620,150,55);case "switch":return new Rect(405,545,130,55);case "nightvision":return new Rect(405,620,150,55);case "journal":return new Rect(650,620,150,55);case "pause":return new Rect(1190,22,60,45);default:return new Rect();}}
        static readonly string[] TouchIds={"grenade","move","fire","aim","interact","reload","crouch","run","tablet","switch","nightvision","journal","pause"};
        readonly Dictionary<int,string> gestures=new Dictionary<int,string>();
        Vector2 moveOrigin;
        public bool TouchControls => Application.isMobilePlatform || (Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(),"--touch-test")>=0);
        public static Rect FitCanvas(Rect safe){float scale=Mathf.Min(safe.width/W,safe.height/H);return new Rect(safe.center-new Vector2(W,H)*scale*.5f,new Vector2(W,H)*scale);}
        Rect CanvasRect => TouchControls?FitCanvas(new Rect(Screen.safeArea.x,Screen.height-Screen.safeArea.yMax,Screen.safeArea.width,Screen.safeArea.height)):FitCanvas(new Rect(0,0,Screen.width,Screen.height));
        Vector2 GuiPoint(Vector2 p){var r=CanvasRect;return new Vector2((p.x-r.x)*W/r.width,(Screen.height-p.y-r.y)*H/r.height);}
        Vector2 GuiDelta(Vector2 delta){var r=CanvasRect;return new Vector2(delta.x*W/r.width,delta.y*H/r.height);}
        Vector2 pointerStart,pointerPosition;bool pointerHeld,pendingClick;string pointerContext;int menuFinger=-1;
        string InputContext=>modal+"/"+game.security.opened;
        public void ProcessMenuPointer(Vector2 position,bool down,bool up,bool canceled=false){
            pointerPosition=position;
            if(down){pointerStart=position;pointerContext=InputContext;pointerHeld=true;}
            if(up || canceled){pendingClick=!canceled && pointerHeld && pointerContext==InputContext && Vector2.Distance(pointerStart,position)<24;pointerHeld=false;}
        }
        public bool ConsumeClick(Rect rect){
            if(Event.current.type!=EventType.Repaint || !GUI.enabled)return false;
            return TryPointerClick(rect);
        }
        public bool TryPointerClick(Rect rect){
            if(!pendingClick || pointerContext!=InputContext || !rect.Contains(pointerStart) || !rect.Contains(pointerPosition))return false;
            pendingClick=false;return true;
        }
        float Slider(Rect rect,float value,float min,float max){
            GUI.HorizontalSlider(rect,value,min,max);
            var hitRect=new Rect(rect.x-12,rect.y-20,rect.width+24,rect.height+40);
            if(Event.current.type==EventType.Repaint && pointerHeld && pointerContext==InputContext && hitRect.Contains(pointerStart))value=Mathf.Lerp(min,max,Mathf.Clamp01((pointerPosition.x-rect.x)/rect.width));
            return value;
        }
        void ReadMenuPointer(){
            pendingClick=false;bool hadTouch=false;
            foreach(var t in FingerTouch.activeTouches){
                hadTouch=true;
                if(t.began && menuFinger<0){menuFinger=t.touchId;ProcessMenuPointer(GuiPoint(t.screenPosition),true,false);}
                if(t.touchId!=menuFinger)continue;
                ProcessMenuPointer(GuiPoint(t.screenPosition),false,t.ended,t.phase==UnityEngine.InputSystem.TouchPhase.Canceled);
                if(!t.ended)ScrollPage(GuiDelta(t.delta).y);
                else menuFinger=-1;
            }
            if(!hadTouch && menuFinger<0 && Mouse.current!=null && !Application.isMobilePlatform){var m=Mouse.current;ProcessMenuPointer(GuiPoint(m.position.ReadValue()),m.leftButton.wasPressedThisFrame,m.leftButton.wasReleasedThisFrame);ScrollPage(-m.scroll.ReadValue().y*.35f);}
        }
        void ScrollPage(float dy){if(modal=="story"){storyScroll.y=Mathf.Clamp(storyScroll.y+dy,0,Mathf.Max(0,storyContentHeight-350));if(storyScroll.y>=storyContentHeight-355)readEnd=true;}else if(modal=="journal")journalScroll.y=Mathf.Max(0,journalScroll.y+dy);}
        void OnApplicationFocus(bool focus){if(!focus){pointerHeld=pendingClick=false;menuFinger=-1;ResetTouchGestures();}}
        void OnEnable(){EnhancedTouchSupport.Enable();}
        void OnDisable(){EnhancedTouchSupport.Disable();ResetTouchGestures();if(game && game.player)game.player.ResetTouch();}
        public void ResetTouchGestures(){gestures.Clear();}
        public bool TouchVisible(string id){return !((id=="fire"||id=="aim"||id=="reload"||id=="grenade")&&!game.state.armed || id=="tablet"&&!game.state.tablet || id=="switch"&&!game.state.hasAK || id=="nightvision"&&!game.state.hasNightVision);}
        public void BeginTouchFrame(){var p=game.player;if(!p)return;p.touchMove=Vector2.zero;p.touchAim=p.touchFire=p.touchRun=false;}
        // A finger owns its initial control until release; returning from an overlay requires a fresh press.
        public void ProcessTouch(int finger,Vector2 pos,Vector2 delta,bool began,bool ended){
            var p=game.player;if(!p || !game.Running || game.security.opened){if(p)p.ResetTouch();return;}
            if(ended){gestures.Remove(finger);return;}
            if(began){
                string role="none";
                foreach(var id in TouchIds)if(TouchVisible(id) && TouchRect(id).Contains(pos)){role=id;break;}
                if(role=="none" && pos.x>W*.4f && pos.x<W && pos.y>190 && pos.y<H)role="look";
                if((role=="move" || role=="look") && gestures.ContainsValue(role))role="none";
                gestures[finger]=role;if(role=="move")moveOrigin=pos;
                switch(role){
                    case "interact":p.Interact();break;
                    case "reload":p.Reload();break;
                    case "grenade":p.ThrowGrenade();break;case "crouch":p.ToggleCrouch();break;
                    case "tablet":game.security.Toggle();break;
                    case "switch":p.SwitchWeapon(p.UsingAK&&p.campaign.state.hasSniper?2:1);break;
                    case "nightvision":p.nightVision.Toggle();break;
                    case "journal":OpenJournal();break;
                    case "pause":game.Pause();break;
                }
                if(!game.Running || game.security.opened){p.ResetTouch();return;}
            }
            if(!gestures.TryGetValue(finger,out var control))return;
            switch(control){
                case "move":var v=new Vector2(pos.x-moveOrigin.x,moveOrigin.y-pos.y)/70;float length=v.magnitude;p.touchMove=length<.12f?Vector2.zero:v.normalized*Mathf.Clamp01((length-.12f)/.88f);break;
                case "fire":p.touchFire=game.state.armed;break;
                case "aim":p.touchAim=game.state.armed;break;
                case "run":p.touchRun=true;break;
                case "look":p.yaw+=delta.x*.11f*sensitivity;p.pitch=Mathf.Clamp(p.pitch-delta.y*.09f*sensitivity,-30,55);break;
            }
        }
        void Update(){
            if(game.paused){subtitleUntil+=Time.unscaledDeltaTime;toastUntil+=Time.unscaledDeltaTime;}
            if(modal=="story" && (Application.isFocused || Application.isBatchMode || game.IsSelfTest)){
                int count=ComputerScreen?terminalPages[terminalPage].Length:(body??"").Length;
                float dt=Time.unscaledDeltaTime, typingTime=Mathf.Max(0,count-typedCharacters)/42f;
                typedCharacters=Mathf.Min(count,typedCharacters+42*dt);
                if(dt>typingTime)readRemaining=Mathf.Max(0,readRemaining-(dt-typingTime));
            }
            hurt=Mathf.Max(0,hurt-Time.unscaledDeltaTime*2);hit=Mathf.Max(0,hit-Time.unscaledDeltaTime);
            ReadMenuPointer();
            if(!TouchControls || !game.player)return;
            BeginTouchFrame();
            if(!game.Running || game.security.opened){game.player.ResetTouch();return;}
            foreach(var t in FingerTouch.activeTouches){ProcessTouch(t.touchId,GuiPoint(t.screenPosition),GuiDelta(t.delta),t.began,t.ended);if(!game.Running || game.security.opened)break;}
        }
        void OnGUI(){
            if(DescentBlackout.Visible&&game&&!game.paused)return;
            Init();var canvas=CanvasRect;GUI.matrix=Matrix4x4.TRS(new Vector3(canvas.x,canvas.y,0),Quaternion.identity,new Vector3(canvas.width/W,canvas.height/H,1));
            if(modal=="menu"){
                Panel(new Rect(0,0,570,H),new Color(.025f,.035f,.044f,.91f));Panel(new Rect(57,79,40,3),accent);
                Label("NORTH POINT / 21:30",58,98,440,30,14,accent);
                var hs=new GUIStyle(heading){fontSize=74};GUI.Label(new Rect(51,163,515,180),"CHỢ\nĐEN",hs);
                Label("BIẾN TÒA NHÀ THÀNH VŨ KHÍ",58,350,470,32,15,accent);
                Label("Di sản của Marcus đang chờ sau Cửa 006.",58,394,430,60,19,muted);
                if(Button("BẮT ĐẦU CHIẾN DỊCH   →",58,485,420,true))game.NewGame();
                GUI.enabled=game.HasSave();if(Button("TIẾP TỤC ĐIỂM LƯU",58,547,420))game.Continue();GUI.enabled=true;
                if(Button("CÀI ĐẶT",58,609,202))OpenSettings();
                if(Button("ĐIỀU KHIỂN",276,609,202))modal="help";
                Label("01—03   /   MỘT NGƯỜI CHƠI   /   KHÔNG CẦN MẠNG",60,684,490,22,11,muted);
                Label("CỬA HÀNG NORTH POINT\nĐIỆN TỬ / SỬA CHỮA / TỪ NĂM 1998",865,570,355,70,14,ivory);if(!TouchControls && Button("THOÁT TRÒ CHƠI",1015,644,205))Application.Quit();return;
            }
            if(game.prologue && game.prologue.Playing && modal==""){
                var intro=game.prologue;Panel(new Rect(0,0,W,64),ink);Label("CUỘC GỌI CUỐI CÙNG / ALEX CARTER",35,20,950,32,20,accent);
                if(Button("TẠM DỪNG",1045,8,205))game.Pause();
                if(intro.Node!=null){
                    Panel(new Rect(55,400,1170,305),ink);Label(intro.Node.speaker+" / ĐIỆN THOẠI",80,416,1100,30,18,accent);
                    Label(intro.VisibleText,80,452,1100,90,22);
                    Label(intro.Ready?"CHỌN CÂU TRẢ LỜI CỦA ALEX":"Đọc lời thoại… Các lựa chọn sáng sau khi chữ chạy hết 3 giây.",80,546,1090,26,15,muted);
                    GUI.enabled=intro.Ready;
                    for(int i=0;i<intro.Node.choices.Length;i++)if(Button(intro.Node.choices[i].text,80+i*370,582,355,true)){intro.Choose(i);break;}
                    GUI.enabled=true;
                }else{Panel(new Rect(0,H-120,W,120),ink);Label(intro.Caption,80,H-100,1120,65,23);}
                if(intro.CanSkip && Button("BỎ QUA PHẦN ĐÃ XEM",865,75,360))intro.Skip();return;
            }
            if(game.opening && game.opening.Playing && modal=="" ){
                Panel(new Rect(0,0,W,64),ink);Panel(new Rect(0,H-125,W,125),ink);Label("23:00 / NHỮNG VỊ KHÁCH KHÔNG MỜI",45,20,1050,35,20,accent);Label(game.opening.Caption,100,H-106,1050,60,23);
                if(Button("TẠM DỪNG",1045,8,205))game.Pause();
                if(game.opening.CanSkip && Button("BỎ QUA CẢNH ĐÃ XEM",890,H-53,350))game.opening.Skip();return;
            }
            if(game.airlock && game.airlock.Scanning && modal==""){
                if(game.airlock.AutoDescending){Panel(new Rect(300,642,680,55),ink);Label(game.airlock.Status,325,656,630,30,20,accent);}
                else {Panel(new Rect(250,540,780,140),ink);Label(game.airlock.Status,280,565,740,40,25,accent);Panel(new Rect(280,625,720,12),muted);Panel(new Rect(280,625,720*game.airlock.ScanProgress,12),accent);}
                if(Button("TẠM DỪNG",1045,8,205))game.Pause();return;
            }
            if(game.underground && game.underground.Playing && modal==""){Panel(new Rect(0,0,W,65),ink);Panel(new Rect(0,580,W,140),ink);Label(game.underground.Caption,60,602,1100,90,23);if(Button("TẠM DỪNG",1045,8,205))game.Pause();return;}
            if(modal=="decoder"){Panel(new Rect(65,42,1150,624),ink);Label("MÁY CHỦ TRUNG TÂM / USB ĐÃ XÁC THỰC",110,90,1050,50,27,accent);Label("Đã khôi phục mã 10 chữ số của Marcus:\n\n"+StoryData.UnlockCode,110,200,1000,160,30);Label("Dùng mã này để mở phần còn lại của Order 71 trên USB.",110,390,1000,90,23);if(Button("GIẢI KHÓA ORDER 71",110,550,500,true))game.underground.ReadFinal();return;}
            if(modal=="computer"){DrawDesktop();return;}
            if(game.active){
                Panel(new Rect(24,22,232,74),ink);Label("ALEX CARTER",40,30,200,20,12,muted);Label($"{game.state.hp:000} MÁU",40,54,110,29,23);Panel(new Rect(143,68,93,3),muted);Panel(new Rect(143,68,93*game.state.hp/100,3),accent);
                Panel(new Rect(282,22,860,166),ink);Label($"{game.state.stage+1:00} / {StoryData.Chapters[game.state.stage]}",300,30,820,25,12,accent);Label(game.objective,300,54,820,40,17);Label("VÌ SAO: "+game.ObjectivePurpose,300,96,820,42,14,accent);Label(game.ApproachHint,300,140,820,46,14);
                if(!TouchControls && Button("Ⅱ",1185,22,65))game.Pause();
                if(game.underground){Label("LỰU ĐẠN: "+game.state.grenades,40,220,230,30,15,accent);if(game.underground.State==UndergroundCampaign.Encounter.Escape)Label("THOÁT HIỂM: "+Mathf.CeilToInt(game.underground.EscapeRemaining)+" GIÂY",440,205,500,35,23,accent);}if(game.upload>0)Label($"TRUYỀN DỮ LIỆU / {Mathf.CeilToInt(game.upload):00}s",40,142,300,40,20,accent);
                if(!game.security.opened){
                    ObjectiveMarker();
                    Panel(new Rect(632,360,16,1),hit>0?accent:ivory);Panel(new Rect(640,352,1,16),hit>0?accent:ivory);
                    if(game.player && game.player.target){Panel(new Rect(360,430,560,60),ink);Label((TouchControls?"[ DÙNG ]  ":"[ E ]  ")+game.player.target.title,378,441,524,45,16);}
                    Label(game.state.armed?(game.player.reloadTime>0?"NẠP ĐẠN…":$"{game.player.WeaponName} {game.player.Ammo:00}/{game.player.Reserve:00}"):"CHƯA CÓ VŨ KHÍ",1020,672,245,35,20,accent);
                    if(game.state.stage<6 && game.upload<0)Label(game.state.stage==0?"BỊ ÁP SÁT = BỊ BẮT / HÃY CHẠY":"NÚP SAU CỘT / BÀN ĐỂ TRÁNH ĐẠN",40,142,230,38,11,accent);
                    if(game.player.nightVision.Active)Label("KÍNH ĐÊM / ĐANG BẬT",40,183,200,28,13,new Color(.4f,1,.6f));
                    if(!TouchControls)Label("WASD DI CHUYỂN • E DÙNG • C KHOM • G LỰU ĐẠN • J NHẬT KÝ • Q ĐỔI SÚNG • N KÍNH ĐÊM • ESC TẠM DỪNG",28,679,970,25,12,muted);
                    else TouchHUD();
                    float danger=0;foreach(var e in game.enemies)if(e && e.hp>0)danger=Mathf.Max(danger,e.suspicion);
                    Label(danger>=1?"ĐÃ BỊ PHÁT HIỆN":danger>.1f?"NGHI NGỜ":"ẨN MÌNH",40,111,300,25,12,danger>.1f?accent:muted);
                }else Tablet();
                foreach(var e in game.enemies)if(e && e.boss && e.hp>0){Panel(new Rect(465,196,350,4),muted);Panel(new Rect(465,196,350*e.hp/e.maxHp,4),accent);Label("VICTOR",465,202,420,25,12,accent);}
                if(Time.unscaledTime<subtitleUntil && !game.paused){var subtitleStyle=new GUIStyle(text){fontSize=18};float subtitleHeight=Mathf.Max(60,subtitleStyle.CalcHeight(new GUIContent(subtitle),616));float subtitleY=TouchControls?300:638-subtitleHeight-48;Panel(new Rect(310,subtitleY,660,subtitleHeight+48),ink);Label(speaker,332,subtitleY+11,616,24,13,accent);Label(subtitle,332,subtitleY+38,616,subtitleHeight,18);}
                if(Time.unscaledTime<toastUntil){Panel(new Rect(340,233,600,60),ink);Label(toast,357,245,566,45,16,accent);}
                if(hurt>0)Panel(new Rect(0,0,W,H),new Color(.55f,.04f,.015f,hurt*.2f));
            }
            if(modal=="")return;
            Panel(new Rect(0,0,W,H),new Color(.02f,.028f,.035f,.96f));Panel(new Rect(165,90,50,3),accent);
            if(modal=="journal"){
                Label("NHẬT KÝ / "+StoryData.Locations[game.state.stage],165,110,950,50,30,accent);
                string entries="MỤC TIÊU HIỆN TẠI\n"+game.objective+"\n"+game.ObjectivePurpose+"\n\nCÁCH TIẾP CẬN\n"+game.ApproachHint+"\n\nĐÃ BIẾT\n";
                for(int i=0;i<=game.state.stage;i++)entries+=StoryData.Locations[i]+"\n"+StoryData.Briefings[i]+"\n\n";
                entries+="TIẾN TRÌNH TẦNG NÀY\n"+string.Join("\n\n",game.journal);
                float height=text.CalcHeight(new GUIContent(entries),900);journalScroll=GUI.BeginScrollView(new Rect(165,180,950,410),journalScroll,new Rect(0,0,900,height));GUI.Label(new Rect(0,0,900,height),entries,text);GUI.EndScrollView();
                if(Button("QUAY LẠI [J]",165,625,420,true))game.Resume();
            } else if(modal=="story"){
                if(ComputerScreen){DrawComputer();return;}
                Label("NORTH POINT / HỒ SƠ BẢO MẬT",165,110,940,30,13,accent);GUI.Label(new Rect(160,155,970,75),title,heading);var bodyStyle=new GUIStyle(text){fontSize=21};float contentHeight=Mathf.Max(330,bodyStyle.CalcHeight(new GUIContent(body),905));storyScroll=GUI.BeginScrollView(new Rect(165,247,945,350),storyScroll,new Rect(0,0,905,contentHeight));GUI.Label(new Rect(0,0,905,contentHeight),body.Substring(0,Mathf.Min(body.Length,Mathf.FloorToInt(typedCharacters))),bodyStyle);GUI.EndScrollView();
                if(contentHeight<=350 || storyScroll.y>=contentHeight-355)readEnd=true;
                GUI.enabled=StoryReady;if(Button(game.HasSeenStory(storyKey)?"BỎ QUA / TIẾP TỤC →":"TIẾP TỤC →",165,625,350,true))ContinueStory();GUI.enabled=true;
                if(!StoryReady)Label(typedCharacters<body.Length?"Đang hiển thị nội dung…":$"Có thể tiếp tục sau {Mathf.CeilToInt(readRemaining)} giây.",540,635,600,55,17,accent);
            } else if(modal=="pause" || modal=="death"){
                GUI.Label(new Rect(165,150,1000,90),modal=="death"?(game.state.stage==0?"BẠN ĐÃ BỊ BẮT":"BẠN ĐÃ BỊ HẠ"):"TẠM DỪNG",heading);Label("Trò chơi lưu ở đầu mỗi tầng và các chặng chiến đấu. Nội dung đã đọc và cảnh đã xem có thể bỏ qua khi chơi lại.",165,255,850,60,21,muted);
                if(modal=="pause" && Button("TIẾP TỤC",165,365,450,true))game.Resume();
                if(Button("CHƠI LẠI ĐIỂM LƯU",165,430,450,modal=="death"))game.Continue();
                if(Button("CÀI ĐẶT",165,495,450))OpenSettings();
                if(Button("VỀ MÀN HÌNH CHÍNH",165,560,450))game.Menu();
            } else if(modal=="choice"){
                Label("ĐÃ XÁC NHẬN NGƯỜI KẾ THỪA / ALEX CARTER",165,114,900,40,15,accent);GUI.Label(new Rect(160,184,1000,80),"NGƯỜI QUẢN LÝ KẾ TIẾP",heading);
                Label("Victor đã im lặng. Nhưng North Point vẫn đang chờ mệnh lệnh của bạn.",165,296,920,90,24);
                if(Button("PHÁ HỦY CƠ SỞ",165,442,450,true))Ending(false);
                if(Button("TIẾP QUẢN CƠ SỞ",650,442,450))Ending(true);
                Label("Phơi bày Chợ Đen. Từ bỏ quyền lực.",165,514,450,90,19,muted);Label("Trở thành người quản lý. Giữ cơ sở hoạt động.",650,514,450,90,19,muted);
            } else if(modal=="settings"){
                GUI.Label(new Rect(160,150,1000,80),"CÀI ĐẶT",heading);Label("Âm lượng",165,270);volume=Slider(new Rect(480,282,540,30),volume,0,1);AudioListener.volume=volume;
                Label("Độ nhạy góc nhìn",165,350);sensitivity=Slider(new Rect(480,362,540,30),sensitivity,.4f,2);
                Label("Độ sáng",165,430);brightness=Slider(new Rect(480,442,540,30),brightness,0,1.8f);ApplyBrightness();
                var assistRect=new Rect(165,510,850,50);GUI.Label(assistRect,(assist?"☑":"□")+"  Hỗ trợ ngắm (không giảm sát thương)",text);if(ConsumeClick(assistRect))assist=!assist;
                if(Button("LƯU & QUAY LẠI",165,595,420,true)){PlayerPrefs.SetInt("assist",assist?1:0);PlayerPrefs.SetFloat("volume",volume);PlayerPrefs.SetFloat("brightness",brightness);PlayerPrefs.SetFloat("sensitivity",sensitivity);PlayerPrefs.Save();modal=settingsReturn;}
            } else if(modal=="help"){
                GUI.Label(new Rect(160,150,1000,80),"HƯỚNG DẪN SINH TỒN",heading);
                string controls=TouchControls?"DI CHUYỂN: kéo cần bên trái. NHÌN: vuốt vùng trống bên phải.\nTƯƠNG TÁC: đến gần rồi chạm DÙNG. Giữ CHẠY để chạy; chạm KHOM để cúi.\nKhi có súng: giữ NGẮM / BẮN; chạm NẠP để nạp đạn.":"DI CHUYỂN: W A S D. NHÌN: di chuột. CHẠY: giữ Shift. KHOM: C.\nTƯƠNG TÁC: đến gần rồi nhấn E. TẠM DỪNG: Esc. NHẬT KÝ: J.\nKhi có súng: chuột phải ngắm, chuột trái bắn, R nạp đạn; Q đổi súng.";
                Label(controls,165,255,950,120,20);
                Label("MỞ ĐẦU: vào văn phòng cuối cửa hàng bên trái → mở các ngăn tủ sát tường → nhặt USB đỏ → mở máy tính → bấm tệp Order 71 và đọc từng trang. USB là chìa khóa; hồ sơ trong máy tính mới là manh mối.",165,386,950,90,20,accent);
                Label("KHI CÓ NGƯỜI TỚI: chạy vào kho phía sau, vòng qua các kệ tới Cửa 006. Bị nhìn thấy sẽ bị truy đuổi; để chúng áp sát là bị bắt chết. Tường và kệ kín giúp cắt tầm nhìn.",165,479,950,90,20);
                Label("TẦNG NGẦM: lấy vũ khí, đọc máy tính, chống trả rồi rút xuống trung tâm. G ném lựu đạn; N bật kính đêm. Bắn tủ điện để cắt đèn.",165,570,950,48,16,muted);
                if(Button("ĐÃ HIỂU",165,625,420,true))modal=game.active?"pause":"menu";
            }
        }
        void DrawComputer(){
            Panel(new Rect(65,42,1150,624),new Color(.13f,.15f,.16f));
            Panel(new Rect(80,57,1120,594),new Color(.025f,.055f,.052f));
            Panel(new Rect(80,57,1120,60),new Color(.07f,.15f,.13f));
            Label("THIẾT BỊ ĐẦU CUỐI / HỒ SƠ BẢO MẬT",110,75,1030,32,22,new Color(.6f,.9f,.76f));
            Label("USB MARCUS: ĐÃ XÁC THỰC",110,135,900,30,15,accent);
            Label($"{title}  •  TRANG {terminalPage+1}/{terminalPages.Length}",110,176,1030,45,29,new Color(.75f,.95f,.85f));
            string page=terminalPages[terminalPage];int length=Mathf.Min(page.Length,Mathf.FloorToInt(typedCharacters));
            Label(page.Substring(0,length)+(length<page.Length && (int)(Time.unscaledTime*2)%2==0?"▌":""),110,241,1030,275,23,new Color(.8f,.95f,.86f));
            GUI.enabled=TerminalPageReady;
            if(Button(terminalPage==terminalPages.Length-1?"ĐÓNG HỒ SƠ / RỜI MÁY TÍNH":"ĐỌC TRANG TIẾP →",110,562,470,true))NextTerminalPage();GUI.enabled=true;
            if(game.HasSeenStory(storyKey)){if(Button("BỎ QUA HỒ SƠ ĐÃ ĐỌC",655,562,490))SkipReadStory();}
            else Label(TerminalPageReady?"Bạn có thể đọc tiếp khi sẵn sàng.":typedCharacters<page.Length?"Đang hiển thị nội dung…":$"Có thể tiếp tục sau {Mathf.CeilToInt(readRemaining)} giây.",610,574,530,55,16,accent);
            Panel(new Rect(540,666,200,14),new Color(.2f,.22f,.23f));
        }
        void ObjectiveMarker(){
            if(!game.player || game.paused)return;
            Transform target=null;foreach(var p in game.level.GetComponentsInChildren<Interaction>())if(p.id==game.objectiveId && p.Available){target=p.transform;break;}
            if(!target)foreach(var marker in game.level.GetComponentsInChildren<WorldMarker>())if(marker.id==game.objectiveId){target=marker.transform;break;}
            if(!target)return;var distance=Vector3.Distance(game.player.transform.position,target.position);if(distance<2.6f)return;
            var screen=game.player.camera.WorldToViewportPoint(target.position+Vector3.up*.8f);
            if(screen.z>0){float x=Mathf.Clamp(screen.x*W,95,W-95),y=Mathf.Clamp((1-screen.y)*H,210,490);Label("◇ "+Mathf.CeilToInt(distance)+" m",x-50,y,120,30,16,accent);}
            else Label("Mục tiêu phía sau · "+Mathf.CeilToInt(distance)+" m",490,620,340,30,14,accent);
        }
        public void Ending(bool accept){game.active=false;Story(accept?"KẾT THÚC B / NGƯỜI QUẢN LÝ":"KẾT THÚC A / LỆNH 072",(accept?StoryData.AcceptEnding:StoryData.DestroyEnding)+$"\n\n{(int)(game.state.elapsed/60)} phút / {game.state.kills} đối thủ bị hạ",game.Menu);}
        void TouchHUD(){
            if(!game.Running)return;
            foreach(var id in TouchIds){
                if(!TouchVisible(id))continue;var r=TouchRect(id);var p=game.player;
                bool selected=id=="crouch"?p.crouch:id=="run"?p.touchRun:id=="aim"?p.touchAim:id=="fire"?p.touchFire:id=="nightvision"?p.nightVision.Active:false;
                Panel(r,selected?new Color(.55f,.36f,.15f,.8f):new Color(.03f,.07f,.09f,.55f));
                if(id=="move"){var center=gestures.ContainsValue("move")?moveOrigin:r.center;Panel(new Rect(center.x-5,center.y-5,10,10),muted);var knob=center+new Vector2(p.touchMove.x,-p.touchMove.y)*70;Panel(new Rect(knob.x-22,knob.y-22,44,44),accent);Label("DI CHUYỂN",r.x+40,r.y+8,170,26,14);continue;}
                string label=id=="grenade"?"LỰU ĐẠN":id=="fire"?"BẮN":id=="aim"?"NGẮM":id=="interact"?"DÙNG":id=="reload"?"NẠP":id=="crouch"?(p.crouch?"ĐỨNG":"KHOM"):id=="run"?"GIỮ CHẠY":id=="pause"?"Ⅱ":id=="journal"?"NHẬT KÝ":id=="switch"?"ĐỔI SÚNG":id=="nightvision"?"KÍNH ĐÊM":"BẢNG AN NINH";
                Label(label,r.x+10,r.y+r.height/2-12,r.width-10,30,14,id=="interact"&&!p.target?muted:ivory);
            }
        }
        void Tablet(){Panel(new Rect(928,0,352,H),ink);Label("HỆ THỐNG AN NINH / TRỰC TIẾP",950,133,320,45,22,accent);Label("KHÔNG TẠM DỪNG",38,175,350,40,16,accent);
            for(int i=0;i<3;i++)if(Button($"MÁY QUAY 0{i+1} / KHU {(char)('A'+i)}",950,201+i*58,305))game.security.View(i);
            if(Button("BÁO ĐỘNG KHU C",950,396,305))game.security.Alarm();
            if(Button(game.security.locked?"MỞ CỬA B":"ĐÓNG CỬA B",950,456,305))game.security.Door();
            if(Button("ĐÈN / XUNG ĐIỆN TỪ",950,516,305))game.security.Light();
            Label($"BÁO ĐỘNG {game.security.alarmCooldown:0}s / XUNG {game.security.empCooldown:0}s",950,574,305,35,13,muted);
            if(Button("ĐÓNG BẢNG AN NINH [TAB]",950,625,305,true))game.security.Close();
        }
    }
}
