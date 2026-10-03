using UnityEngine;
using UnityEngine.InputSystem;
namespace BlackMarket {
    public class SecurityConsole : MonoBehaviour {
        public bool opened,dark,locked,revoked;public int selected;public float alarmCooldown,empCooldown;
        Campaign game;Transform shutter;Vector3 doorOpen,doorClosed;
        public void Setup(){game=GetComponent<Campaign>();opened=false;dark=false;locked=false;revoked=false;alarmCooldown=empCooldown=0;shutter=game.Marker("shutter");doorOpen=shutter.position;doorClosed=doorOpen-Vector3.up*3.5f;shutter.position=doorOpen;SetDark(false);}
        public void Toggle(){if(opened){Close();return;}if(!game.Running)return;if(!game.state.tablet){game.ui.Toast("Tablet mở sau bản ghi FOR_ALEX.");return;}if(revoked){game.ui.Toast("ACCESS REVOKED / Tìm terminal khôi phục.");return;}opened=true;game.player.ResetTouch();game.LockCursor(false);View(0);}
        public void View(int index){selected=index;var p=game.Marker("camera_"+index);game.view.transform.SetPositionAndRotation(p.position,p.rotation);game.flags.Add("camera_used");CheckObjective();}
        public void Close(){if(!opened)return;opened=false;if(game && game.player)game.player.ResetTouch();if(game)game.LockCursor(game.Running);}
        public void SetDark(bool value){dark=value;foreach(var light in game.level.GetComponentsInChildren<Light>())if(!light.name.StartsWith("Emergency"))light.enabled=!value;RenderSettings.ambientIntensity=value? .2f:.8f;}
        public void Light(){if(!opened || revoked)return;SetDark(!dark);game.flags.Add("lights_used");CheckObjective();game.sound.Play("door",.2f);if(game.state.stage==6 && empCooldown<=0){empCooldown=12;foreach(var e in game.enemies)if(e && e.hp>0)e.stun=5;game.ui.Toast("XUNG EMP / Vô hiệu hóa địch 5 giây");}}
        public void Door(){
            if(!opened || revoked)return;
            if(!locked){Vector3 p=game.player.transform.position;if(Mathf.Abs(p.z-doorClosed.z)<1 && Mathf.Abs(p.x-doorClosed.x)<1.8f){game.ui.Toast("Cảm biến cửa: có người trong ngưỡng cửa.");return;}foreach(var e in game.enemies)if(e && e.hp>0 && Mathf.Abs(e.transform.position.z-doorClosed.z)<1 && Mathf.Abs(e.transform.position.x-doorClosed.x)<1.8f){game.ui.Toast("Cửa đang có người đi qua.");return;}}
            locked=!locked;shutter.position=locked?doorClosed:doorOpen;game.flags.Add("door_used");CheckObjective();game.sound.Play("door",.5f,shutter.position);
        }
        public void Alarm(){if(!opened || revoked || alarmCooldown>0)return;alarmCooldown=8;var p=game.Marker("alarm").position;game.Noise(p,30);game.sound.Play("alarm",.5f,p);game.flags.Add("alarm_used");CheckObjective();}
        void CheckObjective(){if(game.state.stage==3 && game.flags.Contains("camera_used") && game.flags.Contains("alarm_used") && game.flags.Contains("door_used") || game.state.stage==4 && game.flags.Contains("lights_used") && game.flags.Contains("door_used")){game.flags.Add("security_done");game.Objective("exit","Đến cửa chuyển khu. Bạn có thể lẻn qua đối thủ.");}}
        void Update(){if(game && game.Running){alarmCooldown=Mathf.Max(0,alarmCooldown-Time.deltaTime);empCooldown=Mathf.Max(0,empCooldown-Time.deltaTime);}}
    }
}
