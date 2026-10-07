using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
namespace BlackMarket {
    public partial class CampaignSelfTest {
        IEnumerator TestControls(){
            var ui=g.ui;var p=g.player;bool armed=g.state.armed,tablet=g.state.tablet;
            Vector2 Center(string id)=>ui.TouchRect(id).center;
            void Touch(int id,string role,bool began=true)=>ui.ProcessTouch(id,Center(role),Vector2.zero,began,false);
            g.state.armed=true;p.ResetTouch();ui.BeginTouchFrame();Touch(1,"move");
            Check(p.touchMove==Vector2.zero,"touch joystick has no jump on first contact");
            ui.ProcessTouch(1,Center("move")+new Vector2(70,-70),Vector2.zero,false,false);
            Touch(2,"fire");Touch(3,"aim");float yaw=p.yaw;ui.ProcessTouch(4,new Vector2(850,350),new Vector2(20,10),true,false);
            Check(Mathf.Abs(p.touchMove.magnitude-1)<.001f && p.touchFire && p.touchAim && p.yaw>yaw,"four-finger move + fire + aim + look");
            ui.ProcessTouch(4,Center("fire"),new Vector2(20,0),false,false);Check(p.yaw>yaw+3,"look finger keeps ownership across fire button");
            ui.BeginTouchFrame();Touch(5,"run");Check(p.touchRun,"run is held");ui.BeginTouchFrame();ui.ProcessTouch(5,Center("run"),Vector2.zero,false,true);Check(!p.touchRun && !p.touchFire && !p.touchAim && p.touchMove==Vector2.zero,"released contacts clear held controls");
            p.ResetTouch();ui.BeginTouchFrame();Touch(6,"crouch");Check(p.crouch,"crouch touch toggles on");Touch(6,"crouch",false);Check(p.crouch,"held crouch does not toggle every frame");p.crouch=false;
            g.state.armed=false;p.ResetTouch();ui.BeginTouchFrame();Touch(7,"fire");Check(!p.touchFire && !ui.TouchVisible("fire"),"locked weapon controls cannot fire");g.state.armed=true;
            p.ResetTouch();ui.BeginTouchFrame();Touch(8,"fire");Touch(9,"pause");Check(g.paused && !p.touchFire,"pause clears simultaneous fire");g.Resume();ui.BeginTouchFrame();Touch(8,"fire",false);Check(!p.touchFire,"held finger cannot fire through resume");
            g.state.tablet=true;p.ResetTouch();ui.BeginTouchFrame();Touch(10,"tablet");Check(g.security.opened,"touch opens tablet");int ammo=p.Ammo;p.Fire();Check(p.Ammo==ammo,"tablet blocks direct firing");g.security.Close();ui.BeginTouchFrame();Touch(8,"fire",false);Check(!p.touchFire,"tablet close requires a fresh contact");
            foreach(var safe in new[]{new Rect(0,0,1280,720),new Rect(90,0,2220,1080),new Rect(0,30,2048,1476)}){var fit=GameInterface.FitCanvas(safe);Check(fit.xMin>=safe.xMin-.01f && fit.xMax<=safe.xMax+.01f && fit.yMin>=safe.yMin-.01f && fit.yMax<=safe.yMax+.01f && Mathf.Abs(fit.width/fit.height-1280f/720)<.001f,"safe-area canvas "+safe.size);}
            g.state.armed=armed;g.state.tablet=tablet;p.ResetTouch();
            g.Pause();ui.OpenSettings();ui.Back();Check(ui.modal=="pause","settings returns to pause");g.Resume();
            ui.modal="death";ui.OpenSettings();ui.Back();Check(ui.modal=="death","settings preserves death screen instead of allowing invalid resume");g.Resume();
            ui.ProcessMenuPointer(new Vector2(200,500),true,false);ui.ProcessMenuPointer(new Vector2(200,500),false,false,true);Check(!ui.TryPointerClick(new Rect(100,450,300,100)),"canceled touch does not click menu");
            if(ui.TouchControls)yield return TestNativeTouch();
            yield return TestCrouchVisual();
            var score=g.GetComponent<TensionScore>();yield return new WaitForSeconds(.5f);Check(!score.PursuitActive,"no detection alarm while hidden");
            var enemy=g.Spawn("spawn");enemy.enabled=false;enemy.TakeDamage(1);yield return new WaitForSeconds(.5f);Check(score.PursuitActive && score.AlarmVolume>.2f,"confirmed detection raises urgent alarm");
            g.Pause();yield return new WaitForSecondsRealtime(.5f);Check(score.AlarmVolume<.01f,"pause fades detection alarm");g.Resume();enemy.hp=0;yield return new WaitForSeconds(3.5f);Check(!score.PursuitActive && score.AlarmVolume<.01f,"alarm fades when encounter ends");Destroy(enemy.gameObject);g.enemies.Remove(enemy);
        }
        IEnumerator TestNativeTouch(){
            var p=g.player;bool armed=g.state.armed;g.state.armed=true;p.enabled=false;
            var originalSettings=Instantiate(InputSystem.settings);var testSettings=Instantiate(originalSettings);
            testSettings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            testSettings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            InputSystem.settings=testSettings;
            var screen=InputSystem.AddDevice<Touchscreen>();
            var canvas=GameInterface.FitCanvas(new Rect(Screen.safeArea.x,Screen.height-Screen.safeArea.yMax,Screen.safeArea.width,Screen.safeArea.height));
            Vector2 Pixels(Vector2 logical)=>new Vector2(canvas.x+logical.x*canvas.width/1280,Screen.height-canvas.y-logical.y*canvas.height/720);
            void Send(int id,Vector2 position,UnityEngine.InputSystem.TouchPhase phase)=>InputSystem.QueueStateEvent(screen,new TouchState{touchId=id,position=Pixels(position),phase=phase});
            var origin=g.ui.TouchRect("move").center;var fire=g.ui.TouchRect("fire").center;
            Send(41,origin,UnityEngine.InputSystem.TouchPhase.Began);yield return null;
            Send(41,origin+Vector2.up*-70,UnityEngine.InputSystem.TouchPhase.Moved);yield return null;
            Check(p.touchMove.y>.95f,"EnhancedTouch device events reach movement control: "+p.touchMove+" contacts="+UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count);
            Send(42,fire,UnityEngine.InputSystem.TouchPhase.Began);yield return null;
            Check(p.touchMove.y>.95f && p.touchFire,"EnhancedTouch tracks moving and firing fingers together: "+p.touchMove+" fire="+p.touchFire);
            Send(42,fire,UnityEngine.InputSystem.TouchPhase.Canceled);Send(41,origin,UnityEngine.InputSystem.TouchPhase.Ended);yield return null;
            Check(!p.touchFire && p.touchMove==Vector2.zero,"EnhancedTouch end/cancel clears controls");
            InputSystem.RemoveDevice(screen);InputSystem.settings=originalSettings;Destroy(testSettings);p.ResetTouch();p.enabled=true;g.state.armed=armed;
        }
        IEnumerator TestCrouchVisual(){
            var p=g.player;var original=p.transform.position;var originalYaw=p.yaw;var originalPitch=p.pitch;
            var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="QA crouch floor";floor.transform.position=new Vector3(80,-.2f,0);floor.transform.localScale=new Vector3(20,.4f,20);
            p.body.enabled=false;p.transform.position=new Vector3(80,0,0);p.body.enabled=true;p.enabled=false;p.visual.rotation=Quaternion.identity;
            var head=p.visual.GetComponentsInChildren<Transform>().First(t=>t.name=="Bip01 Head");var hip=p.visual.GetComponentsInChildren<Transform>().First(t=>t.name=="Bip01 Pelvis");var foot=p.visual.GetComponentsInChildren<Transform>().First(t=>t.name=="Bip01 L Foot");
            var key=new GameObject("QA pose light").AddComponent<Light>();key.type=LightType.Point;key.range=10;key.intensity=12;key.transform.position=new Vector3(82,3,2);
            p.camera.transform.position=new Vector3(82,1.35f,1.8f);p.camera.transform.LookAt(new Vector3(80,.85f,0));
            p.crouch=false;yield return new WaitForSeconds(.3f);float standing=0,standingHip=0;yield return AfterPose(()=>{standing=head.position.y;standingHip=hip.position.y;});
            p.crouch=true;yield return new WaitForSeconds(.35f);yield return RenderedFrame();
            yield return AfterPose(()=>{Check(head.position.y<1.35f && head.position.y<standing-.3f,"crouch head remains below cover: "+head.position.y.ToString("F3"));
            Check(standingHip-hip.position.y<.4f,"crouch pelvis uses moderate knee bend, not seated half-metre drop");});
            yield return Capture("crouch-idle-side");
            var anim=p.visual.GetComponentInChildren<Animation>();anim.CrossFade("walk",.15f);anim["walk"].speed=.9f;
            float min=100,max=-100;for(int i=0;i<24;i++){yield return new WaitForSeconds(.04f);yield return RenderedFrame();yield return AfterPose(()=>{min=Mathf.Min(min,foot.position.z);max=Mathf.Max(max,foot.position.z);});if(i%6==0)yield return Capture("crouch-walk-"+i);}
            Check(max-min>.1f,"crouch keeps alternating animated steps");
            bool wasArmed=g.state.armed;g.state.armed=true;p.muzzle.gameObject.SetActive(true);p.aiming=true;
            yield return new WaitForSeconds(.35f);yield return Capture("crouch-armed-side");
            g.state.armed=wasArmed;p.muzzle.gameObject.SetActive(wasArmed);p.aiming=false;
            g.Pause();yield return RenderedFrame();var pausedHead=Vector3.zero;yield return AfterPose(()=>pausedHead=head.position);yield return new WaitForSecondsRealtime(.25f);yield return RenderedFrame();yield return AfterPose(()=>Check(Vector3.Distance(head.position,pausedHead)<.02f,"crouch pose does not accumulate while paused"));g.Resume();
            p.crouch=false;yield return new WaitForSeconds(.35f);yield return RenderedFrame();yield return AfterPose(()=>Check(head.position.y>1.5f,"standing pose restored after crouch"));
            p.body.enabled=false;p.transform.position=original;p.body.enabled=true;p.enabled=true;p.yaw=originalYaw;p.pitch=originalPitch;p.ResetTouch();Destroy(key.gameObject);Destroy(floor);yield return Frame();
        }
    }
}
