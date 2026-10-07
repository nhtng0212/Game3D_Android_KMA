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
        public string WeaponName=>UsingAK?"AK":"SÚNG NGẮN";
        public int MagazineSize=>UsingAK?30:8;
        public int Ammo {get=>UsingAK?campaign.state.rifleAmmo:campaign.state.ammo;set{if(UsingAK)campaign.state.rifleAmmo=value;else campaign.state.ammo=value;}}
        public int Reserve {get=>UsingAK?campaign.state.rifleReserve:campaign.state.reserve;set{if(UsingAK)campaign.state.rifleReserve=value;else campaign.state.reserve=value;}}
        public void Setup() {
            gameObject.layer=2;
            body=gameObject.AddComponent<CharacterController>();body.height=1.8f;body.radius=.3f;body.center=new Vector3(0,.9f,0);body.stepOffset=.25f;
            var model=Resources.Load<GameObject>("Actors/Alex");
            visual=Instantiate(model,transform).transform;visual.name="Alex visual (replaceable prefab)";
            animationPlayer=visual.GetComponentInChildren<Animation>();if(animationPlayer)animationPlayer.cullingType=AnimationCullingType.AlwaysAnimate;visual.gameObject.AddComponent<CrouchPose>().player=this;
            weapon=Instantiate(Resources.Load<GameObject>("Actors/"+(UsingAK?"AK":"Pistol")),visual);weapon.name=WeaponName;weapon.SetActive(campaign.state.armed);
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
        public void SwitchWeapon(int slot){if(!campaign.Running || campaign.security.opened)return;if(slot==1 && !campaign.state.hasAK){campaign.ui.Toast("AK nằm trong phòng QUÂN NHU ở B6.");return;}campaign.sound.Stop("reload");reloadTime=0;campaign.state.selectedWeapon=slot==1?1:0;if(weapon)Destroy(weapon);weapon=Instantiate(Resources.Load<GameObject>("Actors/"+(UsingAK?"AK":"Pistol")),visual);weapon.name=WeaponName;foreach(var t in weapon.GetComponentsInChildren<Transform>())t.gameObject.layer=2;weaponPose.weapon=weapon.transform;muzzle=weapon.transform;}
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
                if(keys.qKey.wasPressedThisFrame)SwitchWeapon(UsingAK?0:1);
                if(keys.nKey.wasPressedThisFrame)nightVision.Toggle();
            }
            if(!campaign.Running || campaign.security.opened){ResetTouch();return;}
            if(mouse!=null && Cursor.lockState==CursorLockMode.Locked) {
                var delta=mouse.delta.ReadValue();yaw+=delta.x*.095f*campaign.ui.sensitivity;pitch-=delta.y*.075f*campaign.ui.sensitivity;
            }
            pitch=Mathf.Clamp(pitch,-30,55);
            aiming=campaign.state.armed && (touchAim || (mouse!=null && Cursor.lockState==CursorLockMode.Locked && mouse.rightButton.isPressed));
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
            camera.fieldOfView=Mathf.Lerp(camera.fieldOfView,aiming?55:68,Time.unscaledDeltaTime*10);
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
        public void Reload(){if(!campaign.Running || campaign.security.opened || !campaign.state.armed || reloadTime>0 || Ammo>=MagazineSize)return;if(Reserve==0){campaign.ui.Toast("Hết đạn dự trữ. Tìm hộp tiếp tế.");return;}reloadTime=UsingAK?2.2f:1.5f;campaign.sound.Play("reload",.35f);}
        public void Fire() {
            if(!campaign.Running || campaign.security.opened || !campaign.state.armed || Time.time<shotTime || reloadTime>0)return;
            if(Ammo==0){Reload();shotTime=Time.time+.4f;return;}
            RaiseWeaponUntil=Time.time+1.2f;shotTime=Time.time+(UsingAK?.11f:.4f);Ammo--;campaign.Noise(transform.position,UsingAK?35:25);campaign.sound.Play(UsingAK?"rifle":"pistol",UsingAK?.56f:.62f,transform.position+Vector3.up*1.4f);
            Vector3 origin=camera.transform.position,dir=camera.transform.forward;
            if(campaign.ui.assist){float best=.992f;foreach(var e in campaign.enemies){if(!e || e.hp<=0)continue;var to=e.transform.position+Vector3.up*1.2f-origin;float dot=Vector3.Dot(dir,to.normalized);if(dot>best && Physics.Raycast(origin,to.normalized,out var aimHit,60,~(1<<2)) && aimHit.collider.GetComponentInParent<EnemyController>()==e){dir=to.normalized;best=dot;}}}
            Vector3 end=origin+dir*60;
            if(Physics.Raycast(origin,dir,out var hit,60,~(1<<2)))end=hit.point;
            var start=transform.position+Vector3.up*(crouch?.9f:1.35f)+Quaternion.Euler(0,yaw,0)*new Vector3(.3f,0,.45f);
            if(Physics.Linecast(start,end,out var close,~(1<<2)))hit=close;
            if(hit.collider){end=hit.point;var enemy=hit.collider.GetComponentInParent<EnemyController>();if(enemy){enemy.TakeDamage(UsingAK?22:25);campaign.ui.hit=.18f;}else {Effects.Impact(hit.point,hit.normal);campaign.sound.Impact(hit);}}
            Effects.Shot(start,end,false);pitch-=UsingAK?.65f:.45f;
        }
        void OnDestroy(){if(camera)Destroy(camera.gameObject);}
    }
}
