using System;
using UnityEngine;
using UnityEngine.InputSystem;
namespace BlackMarket {
    public class GameInterface : MonoBehaviour {
        public string modal="menu";public bool assist=true;public float sensitivity=1,volume=.65f,brightness=.7f,hurt,hit;
        string title,body,toast,speaker,subtitle;float toastUntil,subtitleUntil;Action continuation;Vector2 storyScroll;
        Campaign game;Font regular,bold,mono;GUIStyle text,heading,small,button;
        readonly Color ink=new Color(.025f,.036f,.043f,.96f), ivory=new Color(.87f,.87f,.81f),accent=new Color(.77f,.57f,.33f),muted=new Color(.54f,.62f,.64f);
        const float W=1280,H=720;
        void Awake(){game=GetComponent<Campaign>();regular=Resources.Load<Font>("Fonts/Regular");bold=Resources.Load<Font>("Fonts/Bold");mono=Resources.Load<Font>("Fonts/Mono");assist=PlayerPrefs.GetInt("assist",1)==1;sensitivity=PlayerPrefs.GetFloat("sensitivity",1);volume=PlayerPrefs.GetFloat("volume",.65f);AudioListener.volume=volume;brightness=PlayerPrefs.GetFloat("brightness",.7f);ApplyBrightness();}
        void ApplyBrightness(){var v=FindFirstObjectByType<UnityEngine.Rendering.Volume>();if(v && v.profile.TryGet<UnityEngine.Rendering.Universal.ColorAdjustments>(out var color))color.postExposure.Override(brightness);}
        public void Story(string caption,string content,Action next){game.security.Close();title=caption;body=content;storyScroll=Vector2.zero;continuation=next;modal="story";game.LockCursor(false);game.player?.ResetTouch();}
        public void Toast(string value){toast=value;toastUntil=Time.unscaledTime+4;}
        public void Subtitle(string who,string value){speaker=who;subtitle=value;subtitleUntil=Time.unscaledTime+Mathf.Max(6,value.Length*.055f);}
        public void ContinueStory(){if(modal!="story")return;var next=continuation;continuation=null;game.Resume();next?.Invoke();}
        void Init(){if(text!=null)return;text=new GUIStyle(GUI.skin.label){font=regular,fontSize=20,wordWrap=true};text.normal.textColor=ivory;heading=new GUIStyle(text){font=bold,fontSize=48};small=new GUIStyle(text){font=mono,fontSize=13};small.normal.textColor=muted;button=new GUIStyle(GUI.skin.button){font=mono,fontSize=16,alignment=TextAnchor.MiddleLeft,padding=new RectOffset(20,12,5,5)};}
        void Panel(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
        void Label(string s,float x,float y,float w=900,float h=40,int size=20,Color? color=null){var st=new GUIStyle(text){fontSize=size};st.normal.textColor=color??ivory;GUI.Label(new Rect(x,y,w,h),s,st);}
        bool Button(string s,float x,float y,float w=340,bool primary=false){GUI.backgroundColor=primary?accent:new Color(.15f,.21f,.23f);bool click=GUI.Button(new Rect(x,y,w,49),s,button);GUI.backgroundColor=Color.white;if(click)game.sound.Play("beep",.12f);return click;}
        public Rect TouchRect(string id){switch(id){case "move":return new Rect(35,475,220,210);case "fire":return new Rect(1120,505,110,100);case "aim":return new Rect(1120,395,110,80);case "interact":return new Rect(950,530,135,65);case "reload":return new Rect(1000,620,105,55);case "crouch":return new Rect(275,620,100,55);case "run":return new Rect(275,545,100,55);case "tablet":return new Rect(820,620,150,55);case "pause":return new Rect(1190,22,60,45);default:return new Rect();}}
        Vector2 GuiPoint(Vector2 p)=>new Vector2(p.x/Screen.width*W,(Screen.height-p.y)/Screen.height*H);
        void Update(){
            hurt=Mathf.Max(0,hurt-Time.unscaledDeltaTime*2);hit=Mathf.Max(0,hit-Time.unscaledDeltaTime);
            if(!Application.isMobilePlatform || !game.Running || game.security.opened || Touchscreen.current==null || !game.player)return;
            var p=game.player;p.touchMove=Vector2.zero;p.touchAim=false;p.touchFire=false;
            foreach(var t in Touchscreen.current.touches){if(!t.press.isPressed)continue;var pos=GuiPoint(t.position.ReadValue());var start=GuiPoint(t.startPosition.ReadValue());
                if(TouchRect("move").Contains(start)){p.touchMove=new Vector2((pos.x-145)/70,(580-pos.y)/70);continue;}
                if(TouchRect("fire").Contains(start)){p.touchFire=game.state.armed;continue;}
                if(TouchRect("aim").Contains(start)){p.touchAim=game.state.armed;continue;}
                bool action=false;foreach(var id in new[]{"interact","reload","crouch","run","tablet","pause"})if(TouchRect(id).Contains(start)){action=true;if(t.press.wasPressedThisFrame){switch(id){case "interact":p.Interact();break;case "reload":p.Reload();break;case "crouch":p.crouch=!p.crouch;break;case "run":p.touchRun=!p.touchRun;break;case "tablet":game.security.Toggle();break;case "pause":game.Pause();break;}}break;}
                if(!action && start.x>W*.4f){var d=t.delta.ReadValue();p.yaw+=d.x*.11f*sensitivity;p.pitch-=d.y*.09f*sensitivity;}
            }
        }
        void OnGUI(){
            Init();GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/W,Screen.height/H,1));
            if(modal=="menu"){
                Panel(new Rect(0,0,570,H),new Color(.025f,.035f,.044f,.91f));Panel(new Rect(57,79,40,3),accent);
                Label("NORTH POINT / 21:30",58,98,440,30,14,accent);
                var hs=new GUIStyle(heading){fontSize=74};GUI.Label(new Rect(51,163,515,180),"BLACK\nMARKET",hs);
                Label("THE BUILDING IS YOUR WEAPON",58,350,470,32,15,accent);
                Label("Di sản của Marcus đang chờ sau Door 06.",58,394,430,60,19,muted);
                if(Button("BẮT ĐẦU CHIẾN DỊCH   →",58,485,420,true))game.NewGame();
                GUI.enabled=game.HasSave();if(Button("TIẾP TỤC CHECKPOINT",58,547,420))game.Continue();GUI.enabled=true;
                if(Button("CÀI ĐẶT",58,609,202))modal="settings";
                if(Button("ĐIỀU KHIỂN",276,609,202))modal="help";
                Label("01—07   /   SINGLE PLAYER   /   OFFLINE",60,684,490,22,11,muted);
                Label("NORTH POINT SUPPLY\nELECTRONICS / REPAIR / EST. 1998",865,570,355,70,14,ivory);if(!Application.isMobilePlatform && Button("THOÁT GAME",1015,644,205))Application.Quit();return;
            }
            if(game.active){
                Panel(new Rect(24,22,232,74),ink);Label("ALEX CARTER",40,30,200,20,12,muted);Label($"{game.state.hp:000} HP",40,54,110,29,23);Panel(new Rect(143,68,93,3),muted);Panel(new Rect(143,68,93*game.state.hp/100,3),accent);
                Panel(new Rect(282,22,860,74),ink);Label($"{game.state.stage+1:00} / {StoryData.Chapters[game.state.stage]}",300,30,820,25,12,accent);Label(game.objective,300,54,820,34,17);
                if(!Application.isMobilePlatform && Button("Ⅱ",1185,22,65))game.Pause();
                if(game.upload>0)Label($"UPLINK / {Mathf.CeilToInt(game.upload):00}s",40,112,300,40,20,accent);
                if(!game.security.opened){
                    ObjectiveMarker();
                    Panel(new Rect(632,360,16,1),hit>0?accent:ivory);Panel(new Rect(640,352,1,16),hit>0?accent:ivory);
                    if(game.player && game.player.target){Panel(new Rect(435,440,410,45),ink);Label("[ E ]  "+game.player.target.title,453,449,380,32,17);}
                    Label(game.state.armed?(game.player.reloadTime>0?"NẠP ĐẠN…":$"{game.state.ammo:00} / {game.state.reserve:00}"):"UNARMED",1080,672,180,35,22,accent);
                    if(!Application.isMobilePlatform)Label("WASD  DI CHUYỂN   •   E  DÙNG   •   C  KHOM   •   TAB  TABLET   •   ESC  MENU",28,679,970,25,12,muted);
                    else TouchHUD();
                    float danger=0;foreach(var e in game.enemies)if(e && e.hp>0)danger=Mathf.Max(danger,e.suspicion);
                    Label(danger>=1?"ĐÃ BỊ PHÁT HIỆN":danger>.1f?"NGHI NGỜ":"ẨN MÌNH",40,111,300,25,12,danger>.1f?accent:muted);
                }else Tablet();
                foreach(var e in game.enemies)if(e && e.boss && e.hp>0){Panel(new Rect(465,117,350,4),muted);Panel(new Rect(465,117,350*e.hp/300,4),accent);Label("VICTOR / PHASE "+e.phase+(e.shielded?" / ACCESS REVOKED":""),465,128,420,25,12,accent);}
                if(Time.unscaledTime<subtitleUntil && !game.paused){Panel(new Rect(310,530,660,108),ink);Label(speaker,332,541,616,24,13,accent);Label(subtitle,332,568,616,60,18);}
                if(Time.unscaledTime<toastUntil){Panel(new Rect(340,170,600,60),ink);Label(toast,357,182,566,45,16,accent);}
                if(hurt>0)Panel(new Rect(0,0,W,H),new Color(.55f,.04f,.015f,hurt*.2f));
            }
            if(modal=="")return;
            Panel(new Rect(0,0,W,H),new Color(.02f,.028f,.035f,.96f));Panel(new Rect(165,90,50,3),accent);
            if(modal=="story"){
                Label("NORTH POINT / SECURE ARCHIVE",165,110,940,30,13,accent);GUI.Label(new Rect(160,155,970,75),title,heading);var bodyStyle=new GUIStyle(text){fontSize=21};float contentHeight=Mathf.Max(330,bodyStyle.CalcHeight(new GUIContent(body),905));storyScroll=GUI.BeginScrollView(new Rect(165,247,945,350),storyScroll,new Rect(0,0,905,contentHeight));GUI.Label(new Rect(0,0,905,contentHeight),body,bodyStyle);GUI.EndScrollView();
                if(Button("TIẾP TỤC   →",165,625,350,true))ContinueStory();
            } else if(modal=="pause" || modal=="death"){
                GUI.Label(new Rect(165,150,1000,90),modal=="death"?"SIGNAL LOST":"TẠM DỪNG",heading);Label("Checkpoint lưu ở đầu mỗi chương. Hãy thử một cách tiếp cận khác.",165,255,850,60,21,muted);
                if(modal=="pause" && Button("TIẾP TỤC",165,365,450,true))game.Resume();
                if(Button("CHƠI LẠI CHECKPOINT",165,430,450,modal=="death"))game.Continue();
                if(Button("CÀI ĐẶT",165,495,450))modal="settings";
                if(Button("VỀ MENU",165,560,450))game.Menu();
            } else if(modal=="choice"){
                Label("SUCCESSOR VERIFIED / ALEX CARTER",165,114,900,40,15,accent);GUI.Label(new Rect(160,184,1000,80),"THE NEXT KEEPER",heading);
                Label("Victor đã im lặng. Nhưng North Point vẫn đang chờ mệnh lệnh của bạn.",165,296,920,90,24);
                if(Button("DESTROY / PHÁ HỦY",165,442,450,true))Ending(false);
                if(Button("ACCEPT / TIẾP QUẢN",650,442,450))Ending(true);
                Label("Phơi bày The Market. Từ bỏ quyền lực.",165,514,450,90,19,muted);Label("Trở thành Keeper. Giữ cơ sở hoạt động.",650,514,450,90,19,muted);
            } else if(modal=="settings"){
                GUI.Label(new Rect(160,150,1000,80),"CÀI ĐẶT",heading);Label("Âm lượng",165,270);volume=GUI.HorizontalSlider(new Rect(480,282,540,30),volume,0,1);AudioListener.volume=volume;
                Label("Độ nhạy camera",165,350);sensitivity=GUI.HorizontalSlider(new Rect(480,362,540,30),sensitivity,.4f,2);
                Label("Độ sáng",165,430);brightness=GUI.HorizontalSlider(new Rect(480,442,540,30),brightness,0,1.8f);ApplyBrightness();
                assist=GUI.Toggle(new Rect(165,510,850,50),assist,"  Hỗ trợ ngắm và giảm sát thương",text);
                if(Button("LƯU & QUAY LẠI",165,595,420,true)){PlayerPrefs.SetInt("assist",assist?1:0);PlayerPrefs.SetFloat("volume",volume);PlayerPrefs.SetFloat("brightness",brightness);PlayerPrefs.SetFloat("sensitivity",sensitivity);PlayerPrefs.Save();modal=game.active?"pause":"menu";}
            } else if(modal=="help"){
                GUI.Label(new Rect(160,150,1000,80),"FIELD MANUAL / 071",heading);
                Label("WASD di chuyển • Chuột xoay camera • Shift chạy • C đi khom\nE tương tác • Chuột phải ngắm • Chuột trái bắn • R nạp đạn\nTab mở Tablet • Esc tạm dừng\n\nAndroid: joystick trái, vuốt phải để nhìn; giữ NGẮM / BẮN.\n\nTablet không tạm dừng thời gian. Nấp trước khi mở. Tường chắn tầm nhìn; bóng tối và đi khom giúp tránh bị phát hiện. Dùng báo động để kéo lính khỏi lối đi và cửa để chia cắt chúng.",165,265,940,310,22);
                if(Button("ĐÃ HIỂU",165,625,420,true))modal=game.active?"pause":"menu";
            }
        }
        void ObjectiveMarker(){
            if(!game.player || game.paused)return;
            Transform target=null;foreach(var p in game.level.GetComponentsInChildren<Interaction>())if(p.id==game.objectiveId && p.Available){target=p.transform;break;}
            if(!target)return;var distance=Vector3.Distance(game.player.transform.position,target.position);if(distance<2.6f)return;
            var screen=game.player.camera.WorldToViewportPoint(target.position+Vector3.up*.8f);
            if(screen.z>0){float x=Mathf.Clamp(screen.x*W,95,W-95),y=Mathf.Clamp((1-screen.y)*H,210,490);Label("◇ "+Mathf.CeilToInt(distance)+" m",x-50,y,120,30,16,accent);}
            else Label("Mục tiêu phía sau · "+Mathf.CeilToInt(distance)+" m",490,620,340,30,14,accent);
        }
        public void Ending(bool accept){game.active=false;Story(accept?"ENDING B / THE KEEPER":"ENDING A / ORDER #072",(accept?StoryData.AcceptEnding:StoryData.DestroyEnding)+$"\n\n{(int)(game.state.elapsed/60)} phút / {game.state.kills} đối thủ bị hạ",game.Menu);}
        void TouchHUD(){foreach(var id in new[]{"move","fire","aim","interact","reload","crouch","run","tablet","pause"}){if((id=="fire"||id=="aim"||id=="reload")&&!game.state.armed)continue;if(id=="tablet"&&!game.state.tablet)continue;var r=TouchRect(id);Panel(r,new Color(.03f,.07f,.09f,.55f));string s=id=="move"?"DI CHUYỂN":id=="fire"?"BẮN":id=="aim"?"NGẮM":id=="interact"?"E / DÙNG":id=="reload"?"NẠP":id=="crouch"?"KHOM":id=="run"?"CHẠY":id=="pause"?"Ⅱ":"TABLET";Label(s,r.x+10,r.y+r.height/2-12,r.width-10,30,14);}}
        void Tablet(){Panel(new Rect(928,0,352,H),ink);Label("KEEPER OS / LIVE",950,133,320,45,22,accent);Label("KHÔNG TẠM DỪNG",38,175,350,40,16,accent);
            for(int i=0;i<3;i++)if(Button($"CAM 0{i+1} / KHU {(char)('A'+i)}",950,201+i*58,305))game.security.View(i);
            if(Button("BÁO ĐỘNG KHU C",950,396,305))game.security.Alarm();
            if(Button(game.security.locked?"MỞ CỬA B":"ĐÓNG CỬA B",950,456,305))game.security.Door();
            if(Button("ĐÈN / XUNG EMP",950,516,305))game.security.Light();
            Label($"ALARM {game.security.alarmCooldown:0}s / EMP {game.security.empCooldown:0}s",950,574,305,35,13,muted);
            if(Button("ĐÓNG TABLET [TAB]",950,625,305,true))game.security.Close();
        }
    }
}
