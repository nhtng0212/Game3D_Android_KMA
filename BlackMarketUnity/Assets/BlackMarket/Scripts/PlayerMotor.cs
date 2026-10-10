using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
namespace BlackMarket {
    public class PlayerMotor : MonoBehaviour {
        public Campaign campaign;
        public Camera camera;
        public CharacterController body;
        public Transform visual, muzzle;
        public Interaction target;
        public bool crouch, aiming, touchFire, touchAim, touchRun;
        public Vector2 touchMove;
        public float yaw,pitch=12,reloadTime;
        float vertical,shotTime,stepTime;
        public float MoveSpeed {get;private set;}
        public float RaiseWeaponUntil {get;private set;}
        public bool WeaponRaised => aiming || Time.time<RaiseWeaponUntil;
        Vector3 safePosition;
        Animation animationPlayer;
        GameObject weapon;WeaponPose weaponPose;
        public NightVision nightVision;
        public bool UsingAK=>campaign.state.hasAK && campaign.state.selectedWeapon==1;
        public bool UsingSniper=>campaign.state.hasSniper && campaign.state.selectedWeapon==2;
        public bool UsingRifle=>UsingAK||UsingSniper;
        public string WeaponName=>UsingSniper?"M700 / SÚNG NGẮM":UsingAK?"AK":"SÚNG NGẮN";
        public int MagazineSize=>UsingSniper?5:UsingAK?30:8;
        public int Ammo {get=>UsingSniper?campaign.state.sniperAmmo:UsingAK?campaign.state.rifleAmmo:campaign.state.ammo;set{if(UsingSniper)campaign.state.sniperAmmo=value;else if(UsingAK)campaign.state.rifleAmmo=value;else campaign.state.ammo=value;}}
        public int Reserve {get=>UsingSniper?campaign.state.sniperReserve:UsingAK?campaign.state.rifleReserve:campaign.state.reserve;set{if(UsingSniper)campaign.state.sniperReserve=value;else if(UsingAK)campaign.state.rifleReserve=value;else campaign.state.reserve=value;}}
        public void Setup() {
            gameObject.layer=2;
            body=gameObject.AddComponent<CharacterController>();body.height=1.8f;body.radius=.3f;body.center=new Vector3(0,.9f,0);body.stepOffset=.25f;
            var model=Resources.Load<GameObject>("Actors/Alex");
            visual=Instantiate(model,transform).transform;visual.name="Alex visual (replaceable prefab)";
            animationPlayer=visual.GetComponentInChildren<Animation>();if(animationPlayer)animationPlayer.cullingType=AnimationCullingType.AlwaysAnimate;visual.gameObject.AddComponent<CrouchPose>().player=this;
            weapon=Instantiate(Resources.Load<GameObject>("Actors/"+(UsingSniper?"Sniper":UsingAK?"AK":"Pistol")),visual);weapon.name=WeaponName;weapon.SetActive(campaign.state.armed);
            weapon.transform.localPosition=new Vector3(.28f,1.15f,.4f);
            foreach(var child in GetComponentsInChildren<Transform>())child.gameObject.layer=2;
            muzzle=weapon.transform;weaponPose=visual.gameObject.AddComponent<WeaponPose>();weaponPose.weapon=weapon.transform;
            var cam=new GameObject("Over shoulder máy quay");camera=cam.AddComponent<Camera>();cam.AddComponent<AudioListener>();
            camera.nearClipPlane=.08f;camera.farClipPlane=130;camera.fieldOfView=68;camera.tag="MainMáy quay";
            camera.backgroundColor=new Color(.022f,.032f,.046f);camera.clearFlags=CameraClearFlags.SolidColor;
            camera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().renderPostProcessing=true;
            nightVision=camera.gameObject.AddComponent<NightVision>();nightVision.Setup(campaign);
            safePosition=campaign.Marker("spawn").position;yaw=transform.eulerAngles.y;UpdateCamera();
        }
        public void RecoverIfFallen(){if(transform.position.y < -4){body.enabled=false;transform.position=safePosition;body.enabled=true;vertical=0;ResetTouch();campaign.ui.Toast("Đã đưa Alex về chiếu nghỉ an toàn.");}}
        public void SwitchWeapon(int slot){if(!campaign.Running || campaign.security.opened || Throwing)return;if(slot==1 && !campaign.state.hasAK){campaign.ui.Toast("Tìm AK tại giá vũ khí ở tầng quân giới.");return;}campaign.sound.Stop("reload");reloadTime=0;if(slot==2&&!campaign.state.hasSniper)return;if(campaign.state.stage==1&&slot==0)slot=campaign.state.hasSniper?2:1;campaign.state.selectedWeapon=Mathf.Clamp(slot,0,2);if(weapon)Destroy(weapon);weapon=Instantiate(Resources.Load<GameObject>("Actors/"+(UsingSniper?"Sniper":UsingAK?"AK":"Pistol")),visual);weapon.name=WeaponName;foreach(var t in weapon.GetComponentsInChildren<Transform>())t.gameObject.layer=2;weaponPose.weapon=weapon.transform;muzzle=weapon.transform;}
        public void ResetTouch(){touchMove=Vector2.zero;touchFire=false;touchAim=false;touchRun=false;aiming=false;campaign.ui?.ResetTouchGestures();}
        public void ToggleCrouch(){if(!campaign.Running || campaign.security.opened)return;crouch=!crouch;touchRun=false;}
        void Update() {
            if(!campaign.Running || campaign.security.opened){MoveSpeed=0;ResetTouch();return;}
            RecoverIfFallen();
            var keys=Keyboard.current;var mouse=Mouse.current;
            Vector2 move=touchMove;
            if(keys!=null){
                move+=new Vector2((keys.dKey.isPressed?1:0)-(keys.aKey.isPressed?1:0),(keys.wKey.isPressed?1:0)-(keys.sKey.isPressed?1:0));
                if(keys.cKey.wasPressedThisFrame)ToggleCrouch();
                if(keys.eKey.wasPressedThisFrame)Interact();
                if(keys.rKey.wasPressedThisFrame)Reload();
                if(keys.qKey.wasPressedThisFrame)SwitchWeapon(UsingAK&&campaign.state.hasSniper?2:1);
                if(keys.gKey.wasPressedThisFrame)ThrowGrenade();
                if(keys.nKey.wasPressedThisFrame)nightVision.Toggle();
            }
            if(!campaign.Running || campaign.security.opened){ResetTouch();return;}
            if(mouse!=null && Cursor.lockState==CursorLockMode.Locked) {
                var delta=mouse.delta.ReadValue()*(UsingSniper&&aiming?.38f:1);yaw+=delta.x*.095f*campaign.ui.sensitivity;pitch-=delta.y*.075f*campaign.ui.sensitivity;
            }
            pitch=Mathf.Clamp(pitch,-30,55);
            aiming=!Throwing && campaign.state.armed && (touchAim || (mouse!=null && Cursor.lockState==CursorLockMode.Locked && mouse.rightButton.isPressed));
            float speed=crouch?2:aiming?2.5f:touchRun || (keys!=null && keys.leftShiftKey.isPressed)?6.5f:4;
            move=Vector2.ClampMagnitude(move,1);
            var direction=Quaternion.Euler(0,yaw,0)*new Vector3(move.x,0,move.y);
            if(body.isGrounded && vertical<0)vertical=-2;vertical-=22*Time.deltaTime;
            var before=transform.position;body.Move((direction*speed+Vector3.up*vertical)*Time.deltaTime);
            MoveSpeed=Vector3.ProjectOnPlane(transform.position-before,Vector3.up).magnitude/Mathf.Max(Time.deltaTime,.0001f);
            if(direction.sqrMagnitude>.01f || WeaponRaised)visual.rotation=Quaternion.Slerp(visual.rotation,WeaponRaised?Quaternion.Euler(0,yaw,0):Quaternion.LookRotation(direction),Time.deltaTime*12);
            if(!crouch && body.height<1.7f && Physics.CheckCapsule(transform.position+Vector3.up*.32f,transform.position+Vector3.up*1.51f,.29f,~(1<<2),QueryTriggerInteraction.Ignore))crouch=true;
            body.height=crouch?1.25f:1.8f;body.center=Vector3.up*body.height*.5f;visual.localPosition=Vector3.zero;
            weapon.SetActive(campaign.state.armed);
            if(animationPlayer){string clip=MoveSpeed>.08f?(speed>4?"run":"walk"):"idle";var state=animationPlayer[clip];if(state!=null){state.speed=clip=="idle"?1:Mathf.Clamp(MoveSpeed/(clip=="run"?6.5f:crouch?2.2f:4f),.15f,1.6f);if(!animationPlayer.IsPlaying(clip))animationPlayer.CrossFade(clip,.2f);}}
            if(MoveSpeed>.2f && body.isGrounded && Time.time>stepTime){stepTime=Time.time+(speed>4? .33f:crouch?.6f:.52f)*Mathf.Clamp(speed/MoveSpeed,1,3);campaign.sound.Play(speed>4?"run":"step",crouch? .045f:speed>4?.27f:.14f,transform.position);if(speed>4)campaign.Noise(transform.position,5);}
            if(reloadTime>0){reloadTime-=Time.deltaTime;if(reloadTime<=0){int n=Mathf.Min(MagazineSize-Ammo,Reserve);Ammo+=n;Reserve-=n;campaign.sound.Play("draw",.14f);}}
            if(campaign.state.armed && (touchFire || (mouse!=null && mouse.leftButton.isPressed && Cursor.lockState==CursorLockMode.Locked)))Fire();
            FindTarget();
        }
        void LateUpdate() {
            if(!camera || campaign.security.opened || !campaign.Running)return;
            UpdateCamera();
        }
        void UpdateCamera(){
            var rotation=Quaternion.Euler(pitch,yaw,0);
            var pivot=transform.position+Vector3.up*(crouch?1.15f:1.6f);
            var offset=rotation*new Vector3(aiming? .55f:.4f, .1f,aiming?-1.6f:-3.1f);
            var desired=pivot+offset;
            if(Physics.SphereCast(pivot,.17f,offset.normalized,out var hit,offset.magnitude,~(1<<2),QueryTriggerInteraction.Ignore))desired=pivot+offset.normalized*Mathf.Max(.2f,hit.distance-.08f);
            camera.transform.SetPositionAndRotation(desired,rotation);
            camera.cullingMask=UsingSniper&&aiming?~(1<<2):~0;
            camera.fieldOfView=Mathf.Lerp(camera.fieldOfView,aiming?(UsingSniper?20:50):68,Time.unscaledDeltaTime*10);
        }
        public void FindTarget() {
            target=null;float best=2.6f;
            foreach(var p in campaign.level.GetComponentsInChildren<Interaction>()) {
                if(!p.Available)continue;var d=Vector3.Distance(transform.position,p.transform.position);
                if(d>=best)continue;
                var from=transform.position+Vector3.up*1.4f;var to=p.transform.position+Vector3.up*.2f;
                if(Physics.Linecast(from,to,out var hit,~(1<<2),QueryTriggerInteraction.Ignore) && Vector3.Distance(hit.point,to)>.5f)continue;
                target=p;best=d;
            }
        }
        public void Interact(){if(!campaign.Running || campaign.security.opened)return;FindTarget();if(target)target.Use();}
        public void Reload(){if(!campaign.Running || campaign.security.opened || !campaign.state.armed || reloadTime>0 || Ammo>=MagazineSize)return;if(Reserve==0){campaign.ui.Toast("Hết đạn dự trữ. Tìm hộp tiếp tế.");return;}reloadTime=UsingSniper?2.6f:UsingAK?2.2f:1.5f;campaign.sound.Play("reload",.35f);}
        public void Fire() {
            if(!campaign.Running || campaign.security.opened || !campaign.state.armed || Time.time<shotTime || Throwing || reloadTime>0)return;
            if(Ammo==0){Reload();shotTime=Time.time+.4f;return;}
            RaiseWeaponUntil=Time.time+1.2f;shotTime=Time.time+(UsingSniper?1.25f:UsingAK?.11f:.4f);Ammo--;campaign.Noise(transform.position,UsingAK?35:25);campaign.sound.Play(UsingAK?"rifle":"pistol",UsingAK?.56f:.62f,transform.position+Vector3.up*1.4f);
            Vector3 origin=camera.transform.position,dir=camera.transform.forward;
            if(campaign.ui.assist&&!UsingSniper){float best=.992f;foreach(var e in campaign.enemies){if(!e || e.hp<=0)continue;var to=e.transform.position+Vector3.up*1.2f-origin;float dot=Vector3.Dot(dir,to.normalized);if(dot>best && Physics.Raycast(origin,to.normalized,out var aimHit,60,~(1<<2)) && aimHit.collider.GetComponentInParent<EnemyController>()==e){dir=to.normalized;best=dot;}}}
            Vector3 end=origin+dir*(UsingSniper?120:60);
            if(Physics.Raycast(origin,dir,out var hit,UsingSniper?120:60,~(1<<2)))end=hit.point;
            var barrel=muzzle?muzzle.Find("Muzzle"):null;var start=barrel?barrel.position:transform.position+Vector3.up*(crouch?.9f:1.35f)+Quaternion.Euler(0,yaw,0)*new Vector3(.3f,0,.45f);
            if(Physics.Linecast(start,end,out var close,~(1<<2)))hit=close;
            if(hit.collider){end=hit.point;var enemy=hit.collider.GetComponentInParent<EnemyController>();if(enemy){enemy.TakeDamage(UsingSniper?105:UsingAK?22:25);campaign.ui.hit=.18f;}else {hit.collider.GetComponentInParent<LightingBreaker>()?.Hit(UsingSniper?105:UsingAK?22:25);Effects.Impact(hit.point,hit.normal);campaign.sound.Impact(hit);}}
            Effects.Shot(start,end,false);pitch-=UsingSniper?2.2f:UsingAK?.65f:.45f;
        }
        float grenadeUntil;
        public bool Throwing {get;private set;}
        public float ThrowProgress {get;private set;}
        public void ThrowGrenade(){if(!campaign.Running || !campaign.state.armed || campaign.state.grenades<=0 || Time.time<grenadeUntil || Throwing)return;reloadTime=0;campaign.sound.Stop("reload");StartCoroutine(ThrowSequence());}
        System.Collections.IEnumerator ThrowSequence(){
            Throwing=true;ThrowProgress=0;grenadeUntil=Time.time+1.3f;bool released=false;
            while(ThrowProgress<1){if(campaign.Running){ThrowProgress+=Time.deltaTime/1.1f;if(!released&&ThrowProgress>=.55f){released=true;campaign.state.grenades--;var hand=weaponPose.RightHand;var origin=hand?hand.position:transform.position+Vector3.up*1.5f;ThrownGrenade.Launch(campaign,origin+camera.transform.forward*.18f,camera.transform.forward*13+Vector3.up*4);}}yield return null;}
            Throwing=false;
        }
        Texture2D scopeMask;
        void OnGUI(){if(!campaign || !campaign.Running || !UsingSniper || !aiming)return;var old=GUI.color;float cx=Screen.width*.5f,cy=Screen.height*.5f;float edge=Mathf.Max(0,cx-cy);GUI.color=Color.black;GUI.DrawTexture(new Rect(0,0,edge,Screen.height),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(cx+cy,0,edge,Screen.height),Texture2D.whiteTexture);
            if(!scopeMask){scopeMask=new Texture2D(256,256,TextureFormat.RGBA32,false);var pixels=new Color[256*256];for(int y=0;y<256;y++)for(int x=0;x<256;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(127.5f,127.5f))/127.5f;pixels[y*256+x]=new Color(0,0,0,Mathf.SmoothStep(0,1,(d-.91f)/.045f));}scopeMask.SetPixels(pixels);scopeMask.Apply();}
            GUI.color=Color.white;GUI.DrawTexture(new Rect(cx-cy,0,Screen.height,Screen.height),scopeMask);GUI.color=Color.black;GUI.DrawTexture(new Rect(cx-170,cy,340,1),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(cx,cy-170,1,340),Texture2D.whiteTexture);for(int i=-3;i<=3;i++){GUI.DrawTexture(new Rect(cx+i*35,cy-4,1,9),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(cx-4,cy+i*35,9,1),Texture2D.whiteTexture);}GUI.color=old;}
        void OnDestroy(){if(scopeMask)Destroy(scopeMask);if(camera)Destroy(camera.gameObject);}
    }
}
