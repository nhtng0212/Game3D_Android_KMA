using UnityEngine;
using UnityEngine.InputSystem;
namespace BlackMarket {
    public class SecurityConsole : MonoBehaviour {
        public bool opened,dark,locked,revoked;public int selected;public float alarmCooldown,empCooldown;
        Campaign game;Transform shutter;Vector3 doorOpen,doorClosed;
        public void Setup(){game=GetComponent<Campaign>();opened=false;dark=false;locked=false;revoked=false;alarmCooldown=empCooldown=0;shutter=game.Marker("shutter");doorOpen=shutter.position;doorClosed=doorOpen-Vector3.up*3.5f;shutter.position=doorOpen;SetDark(false);}
        public void Toggle(){if(opened){Close();return;}if(!game.Running)return;if(!game.state.tablet){game.ui.Toast("máy tính bảng mở sau bản ghi GỬI ALEX.");return;}if(revoked){game.ui.Toast("QUYỀN TRUY CẬP BỊ THU HỒI / Tìm máy tính khôi phục.");return;}opened=true;game.player.ResetTouch();game.LockCursor(false);View(0);}
        public void View(int index){selected=index;var p=game.Marker("camera_"+index);game.view.transform.SetPositionAndRotation(p.position,p.rotation);game.flags.Add("camera_used");CheckObjective();}
        public void Close(){if(!opened)return;opened=false;if(game && game.player)game.player.ResetTouch();if(game)game.LockCursor(game.Running);}
        public void SetDark(bool value){dark=value;foreach(var light in game.level.GetComponentsInChildren<Light>())if(!light.name.StartsWith("Emergency") && !light.GetComponentInParent<EnemyController>())light.enabled=!value;RenderSettings.ambientIntensity=value? .2f:.8f;}
        public void Light(){if(!opened || revoked)return;SetDark(!dark);game.flags.Add("lights_used");CheckObjective();game.sound.Play("door",.2f);if(game.state.stage==6 && empCooldown<=0){empCooldown=12;foreach(var e in game.enemies)if(e && e.hp>0)e.stun=5;game.ui.Toast("XUNG ĐIỆN TỪ / Vô hiệu hóa địch 5 giây");}}
        public void Door(){
            if(!opened || revoked)return;
            if(!locked){Vector3 p=game.player.transform.position;if(Mathf.Abs(p.z-doorClosed.z)<1 && Mathf.Abs(p.x-doorClosed.x)<1.8f){game.ui.Toast("Cảm biến cửa: có người trong ngưỡng cửa.");return;}foreach(var e in game.enemies)if(e && e.hp>0 && Mathf.Abs(e.transform.position.z-doorClosed.z)<1 && Mathf.Abs(e.transform.position.x-doorClosed.x)<1.8f){game.ui.Toast("Cửa đang có người đi qua.");return;}}
            locked=!locked;shutter.position=locked?doorClosed:doorOpen;game.flags.Add("door_used");CheckObjective();game.sound.Play("door",.5f,shutter.position);
        }
        public void Alarm(){if(!opened || revoked || alarmCooldown>0)return;alarmCooldown=8;var p=game.Marker("alarm").position;game.Noise(p,30);game.sound.Play("alarm",.5f,p);game.flags.Add("alarm_used");CheckObjective();}
        void CheckObjective(){
            string next=null;
            if(game.state.stage==3){
                if(!game.flags.Contains("camera_used"))next="Mở máy tính bảng [Tab] và quan sát máy quay để tìm đường qua B3.";
                else if(!game.flags.Contains("alarm_used"))next="máy tính bảng: bật BÁO ĐỘNG KHU C để kéo lính khỏi tuyến cầu thang.";
                else if(!game.flags.Contains("door_used"))next="máy tính bảng: điều khiển CỬA B để học cách chia cắt đội thanh trừng.";
            }else if(game.state.stage==4){
                if(!game.flags.Contains("lights_used"))next="máy tính bảng: tắt đèn kho; quan sát đèn pin để chọn lúc đi qua.";
                else if(!game.flags.Contains("door_used"))next="máy tính bảng: điều khiển CỬA B để chia cắt lính trước khi xuống trạm truyền dữ liệu.";
            }else return;
            if(next!=null){if(game.objective!=next)game.Objective("security",next);return;}
            game.flags.Add("security_done");string exit="Tới cửa CẦU THANG xuống "+(game.state.stage==3?"B4 / Y tế & kho.":"B5 / trạm truyền dữ liệu.");
            if(game.objective!=exit)game.Objective("exit",exit);
        }
        void Update(){if(game && game.Running){alarmCooldown=Mathf.Max(0,alarmCooldown-Time.deltaTime);empCooldown=Mathf.Max(0,empCooldown-Time.deltaTime);}}
    }
}
