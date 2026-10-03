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
        Animation animationPlayer;
        GameObject weapon;
        public void Setup() {
            gameObject.layer=2;
            body=gameObject.AddComponent<CharacterController>();body.height=1.8f;body.radius=.3f;body.center=new Vector3(0,.9f,0);body.stepOffset=.25f;
            var model=Resources.Load<GameObject>("Actors/Alex");
            visual=Instantiate(model,transform).transform;visual.name="Alex visual (replaceable prefab)";
            animationPlayer=visual.GetComponentInChildren<Animation>();visual.gameObject.AddComponent<CrouchPose>().player=this;
            weapon=Instantiate(Resources.Load<GameObject>("Actors/Pistol"),visual);weapon.name="Pistol";weapon.SetActive(campaign.state.armed);
            weapon.transform.localPosition=new Vector3(.28f,1.15f,.4f);
            foreach(var child in GetComponentsInChildren<Transform>())child.gameObject.layer=2;
            muzzle=weapon.transform;visual.gameObject.AddComponent<WeaponPose>().weapon=weapon.transform;
            var cam=new GameObject("Over shoulder camera");camera=cam.AddComponent<Camera>();cam.AddComponent<AudioListener>();
            camera.nearClipPlane=.08f;camera.farClipPlane=130;camera.fieldOfView=68;camera.tag="MainCamera";
            camera.backgroundColor=new Color(.022f,.032f,.046f);camera.clearFlags=CameraClearFlags.SolidColor;
            camera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>().renderPostProcessing=true;
            yaw=transform.eulerAngles.y;LateUpdate();
        }
        public void ResetTouch(){touchMove=Vector2.zero;touchFire=false;touchAim=false;touchRun=false;}
        void Update() {
            if(!campaign.Running || campaign.security.opened)return;
            var keys=Keyboard.current;var mouse=Mouse.current;
            Vector2 move=touchMove;
            if(keys!=null){
                move+=new Vector2((keys.dKey.isPressed?1:0)-(keys.aKey.isPressed?1:0),(keys.wKey.isPressed?1:0)-(keys.sKey.isPressed?1:0));
                if(keys.cKey.wasPressedThisFrame)crouch=!crouch;
                if(keys.eKey.wasPressedThisFrame)Interact();
                if(keys.rKey.wasPressedThisFrame)Reload();
            }
            if(mouse!=null && Cursor.lockState==CursorLockMode.Locked) {
                var delta=mouse.delta.ReadValue();yaw+=delta.x*.095f*campaign.ui.sensitivity;pitch-=delta.y*.075f*campaign.ui.sensitivity;
            }
            pitch=Mathf.Clamp(pitch,-30,55);
            aiming=campaign.state.armed && (touchAim || (mouse!=null && mouse.rightButton.isPressed));
            float speed=crouch?2:aiming?2.5f:touchRun || (keys!=null && keys.leftShiftKey.isPressed)?6.5f:4;
            move=Vector2.ClampMagnitude(move,1);
            var direction=Quaternion.Euler(0,yaw,0)*new Vector3(move.x,0,move.y);
            if(body.isGrounded && vertical<0)vertical=-2;vertical-=22*Time.deltaTime;
            body.Move((direction*speed+Vector3.up*vertical)*Time.deltaTime);
            if(direction.sqrMagnitude>.01f || aiming)visual.rotation=Quaternion.Slerp(visual.rotation,aiming?Quaternion.Euler(0,yaw,0):Quaternion.LookRotation(direction),Time.deltaTime*12);
            if(!crouch && body.height<1.7f && Physics.CheckCapsule(transform.position+Vector3.up*.32f,transform.position+Vector3.up*1.48f,.29f,~(1<<2),QueryTriggerInteraction.Ignore))crouch=true;
            body.height=crouch?1.25f:1.8f;body.center=Vector3.up*body.height*.5f;visual.localPosition=Vector3.zero;
            weapon.SetActive(campaign.state.armed);
            if(animationPlayer){string clip=direction.sqrMagnitude>.01f?(speed>4?"run":"walk"):"idle";if(!animationPlayer.IsPlaying(clip))animationPlayer.CrossFade(clip,.2f);}
            if(direction.sqrMagnitude>.1f && Time.time>stepTime){stepTime=Time.time+(speed>4? .33f:.52f);campaign.sound.Play(speed>4?"run":"step",crouch? .045f:speed>4?.27f:.14f,transform.position);if(speed>4)campaign.Noise(transform.position,5);}
            if(reloadTime>0){reloadTime-=Time.deltaTime;if(reloadTime<=0){int n=Mathf.Min(8-campaign.state.ammo,campaign.state.reserve);campaign.state.ammo+=n;campaign.state.reserve-=n;}}
            if(campaign.state.armed && (touchFire || (mouse!=null && mouse.leftButton.isPressed && Cursor.lockState==CursorLockMode.Locked)))Fire();
            FindTarget();
        }
        void LateUpdate() {
            if(!camera || campaign.security.opened)return;
            var rotation=Quaternion.Euler(pitch,yaw,0);
            var pivot=transform.position+Vector3.up*(crouch?1.3f:1.6f);
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
        public void Interact(){FindTarget();if(target)target.Use();}
        public void Reload(){if(!campaign.Running || !campaign.state.armed || reloadTime>0 || campaign.state.ammo>=8)return;if(campaign.state.reserve==0){campaign.ui.Toast("Hết đạn dự trữ. Tìm hộp tiếp tế.");return;}reloadTime=1.5f;campaign.sound.Play("reload",.35f);}
        public void Fire() {
            if(!campaign.Running || !campaign.state.armed || Time.time<shotTime || reloadTime>0)return;
            if(campaign.state.ammo==0){Reload();shotTime=Time.time+.4f;return;}
            shotTime=Time.time+.4f;campaign.state.ammo--;campaign.Noise(transform.position,25);campaign.sound.Play("shot",.55f,transform.position);
            Vector3 origin=camera.transform.position,dir=camera.transform.forward;
            if(campaign.ui.assist){float best=.992f;foreach(var e in campaign.enemies){if(!e || e.hp<=0)continue;var to=e.transform.position+Vector3.up*1.2f-origin;float dot=Vector3.Dot(dir,to.normalized);if(dot>best && Physics.Raycast(origin,to.normalized,out var aimHit,60,~(1<<2)) && aimHit.collider.GetComponentInParent<EnemyController>()==e){dir=to.normalized;best=dot;}}}
            Vector3 end=origin+dir*60;
            if(Physics.Raycast(origin,dir,out var hit,60,~(1<<2)))end=hit.point;
            var start=transform.position+Vector3.up*1.35f+Quaternion.Euler(0,yaw,0)*new Vector3(.3f,0,.45f);
            if(Physics.Linecast(start,end,out var close,~(1<<2)))hit=close;
            if(hit.collider){end=hit.point;var enemy=hit.collider.GetComponentInParent<EnemyController>();if(enemy){enemy.TakeDamage(25);campaign.ui.hit=.18f;}else Effects.Impact(hit.point,hit.normal);}
            Effects.Shot(start,end,false);pitch-=.45f;
        }
        void OnDestroy(){if(camera)Destroy(camera.gameObject);}
    }
}
