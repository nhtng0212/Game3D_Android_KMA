using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
namespace BlackMarket {
    public class EnemyController : MonoBehaviour {
        public Campaign campaign;public bool captureOnContact;public bool boss,shielded;public float hp=100,damage=15,speed=3,stun,suspicion;public int phase=1;
        public enum Mode {Patrol,Investigate,Chase,Attack,Search}
        public Mode mode;
        public string routeId,actorPrefab="Operator";
        public bool Armed=>gun && gun.activeSelf;
        public Light Flashlight=>torch;
        public Vector3 LastSeen=>lastSeen;
        public bool PatrolResting=>mode==Mode.Patrol && Time.time<patrolUntil;
        public float PatrolRestRemaining=>PatrolResting?patrolUntil-Time.time:0;
        RoomDoor[] roomDoors;float patrolUntil;bool waypointReached;readonly List<WorldMarker> routeMarkers=new List<WorldMarker>();
        NavMeshAgent agent;Transform visual;Animation anim;Vector3 home,destination,lastSeen,visibleAimPoint;float nextThink,nextShot,memory,drawUntil,stepTime,searchUntil;int routeIndex,searchIndex;bool alerted;
        WeaponPose gunPose;GameObject gun,flashlight;Light torch;readonly List<Vector3> route=new List<Vector3>();
        public void Setup(){
            roomDoors=campaign.level.GetComponentsInChildren<RoomDoor>();home=transform.position;destination=home;agent=gameObject.AddComponent<NavMeshAgent>();agent.radius=.32f;agent.height=1.8f;agent.speed=speed;agent.stoppingDistance=.35f;
            if(NavMesh.SamplePosition(home,out var snap,3,NavMesh.AllAreas))agent.Warp(snap.position);
            var capsule=gameObject.AddComponent<CapsuleCollider>();capsule.height=1.8f;capsule.radius=.32f;capsule.center=Vector3.up*.9f;
            visual=Instantiate(Resources.Load<GameObject>("Actors/"+actorPrefab),transform).transform;anim=visual.GetComponentInChildren<Animation>();
            gun=Instantiate(Resources.Load<GameObject>("Actors/Pistol"),visual);gunPose=visual.gameObject.AddComponent<WeaponPose>();gunPose.weapon=gun.transform;gun.SetActive(false);
            flashlight=new GameObject("Patrol flashlight");flashlight.transform.SetParent(visual);visual.gameObject.AddComponent<WeaponPose>().weapon=flashlight.transform;
            var casing=GameObject.CreatePrimitive(PrimitiveType.Cylinder);Destroy(casing.GetComponent<Collider>());casing.transform.SetParent(flashlight.transform);casing.transform.localPosition=Vector3.zero;casing.transform.localRotation=Quaternion.Euler(90,0,0);casing.transform.localScale=new Vector3(.055f,.11f,.055f);casing.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("Materials/Black");
            var lamp=new GameObject("Flashlight beam");lamp.transform.SetParent(flashlight.transform);lamp.transform.localPosition=Vector3.forward*.13f;torch=lamp.AddComponent<Light>();torch.type=LightType.Spot;torch.spotAngle=48;torch.innerSpotAngle=26;torch.range=14;torch.intensity=24;torch.color=new Color(.9f,.94f,1);torch.shadows=LightShadows.Soft;torch.shadowBias=.02f;torch.shadowNormalBias=.08f;lamp.AddComponent<FlashlightBeam>();
            if(!string.IsNullOrEmpty(routeId))for(int i=0;i<20;i++){Transform found=null;foreach(var m in campaign.level.GetComponentsInChildren<WorldMarker>())if(m.id=="patrol_"+routeId+"_"+i){found=m.transform;break;}if(!found)break;route.Add(found.position);routeMarkers.Add(found.GetComponent<WorldMarker>());}
            if(route.Count==0){route.Add(home);if(NavMesh.SamplePosition(home+Vector3.forward*3,out var patrol,2,NavMesh.AllAreas))route.Add(patrol.position);}
            destination=route[0];SetEquipment(false);
        }
        void SetEquipment(bool armed){if(gun)gun.SetActive(armed && hp>0 && !captureOnContact);if(flashlight)flashlight.SetActive(!armed && hp>0 && !captureOnContact);}
        void Arm(){if(alerted)return;alerted=true;drawUntil=Time.time+.7f;gunPose.weight=0;SetEquipment(true);campaign.sound.Play("draw",.22f,transform.position);}
        // Visibility and illumination use the same origin, range and spotlight angle while patrolling.
        public bool CanSeePoint(Vector3 target){
            Vector3 origin=Armed || captureOnContact?transform.position+Vector3.up*1.45f:torch.transform.position;
            Vector3 forward=Armed || captureOnContact?visual.forward:torch.transform.forward;Vector3 delta=target-origin;
            float range=captureOnContact?16:Armed?18:torch.range;
            if(delta.magnitude>range || (delta.magnitude>1.5f && Vector3.Angle(forward,delta)>(captureOnContact?55:Armed?48:torch.spotAngle*.5f)))return false;
            foreach(var hit in Physics.RaycastAll(origin,delta.normalized,delta.magnitude,~(1<<2),QueryTriggerInteraction.Ignore))if(hit.collider.GetComponentInParent<EnemyController>()!=this)return false;
            return true;
        }
        public bool CanSee(){if(!campaign.player)return false;var p=campaign.player;var torso=p.transform.position+Vector3.up*(p.crouch?.82f:1.25f);var head=p.transform.position+Vector3.up*(p.crouch?1.12f:1.65f);if(CanSeePoint(torso)){visibleAimPoint=torso;return true;}if(CanSeePoint(head)){visibleAimPoint=head;return true;}return false;}
        public void Hear(Vector3 point,float radius){if(hp<=0 || Vector3.Distance(transform.position,point)>radius || alerted)return;if(Vector3.Distance(transform.position,point)>radius*.55f){var start=transform.position+Vector3.up;var end=point+Vector3.up*.2f;foreach(var hit in Physics.RaycastAll(start,(end-start).normalized,Vector3.Distance(start,end),~(1<<2),QueryTriggerInteraction.Ignore))if(!hit.collider.GetComponentInParent<EnemyController>())return;}destination=point;memory=9;searchUntil=Time.time+9;mode=Mode.Investigate;suspicion=Mathf.Max(suspicion,.18f);}
        void Update(){
            if(hp<=0)return;if(Armed)gunPose.weight=Mathf.Clamp01(1-(drawUntil-Time.time)/.7f);bool stop=!campaign.Running || stun>0;if(agent && agent.isOnNavMesh)agent.isStopped=stop;if(!campaign.Running)return;
            stun=Mathf.Max(0,stun-Time.deltaTime);if(stun>0)return;memory-=Time.deltaTime;
            if(Time.time>=nextThink){nextThink=Time.time+.12f;Think(.12f);}
            if(agent.isOnNavMesh){agent.speed=alerted?speed+1:mode==Mode.Patrol?(captureOnContact?1.85f:1.5f):2.1f;agent.isStopped=mode==Mode.Attack || Time.time<drawUntil || PatrolResting;}
            Vector3 facing=mode==Mode.Attack?lastSeen-transform.position:agent.velocity;
            if(facing.sqrMagnitude>.03f){facing.y=0;visual.rotation=Quaternion.Slerp(visual.rotation,Quaternion.LookRotation(facing),Time.deltaTime*7);}
            else if(PatrolResting && routeIndex<routeMarkers.Count)visual.rotation=Quaternion.Slerp(visual.rotation,routeMarkers[routeIndex].transform.rotation,Time.deltaTime*4);
            else if(mode==Mode.Search || mode==Mode.Investigate)visual.Rotate(0,Time.deltaTime*40,0,Space.World);
            if(captureOnContact && suspicion>=1 && Vector3.Distance(transform.position,campaign.player.transform.position)<1.25f && CanSee()){campaign.ui.Subtitle("KẺ TRUY BẮT","Bắt được mày rồi!");campaign.Damage(1000);return;}
            if(!captureOnContact && mode==Mode.Attack && Time.time>=drawUntil && Time.time>=nextShot && CanSee()){
                nextShot=Time.time+(boss?.85f:1.25f);Vector3 start=transform.position+Vector3.up*1.4f,end=visibleAimPoint;
                bool blocked=false;foreach(var hit in Physics.RaycastAll(start,(end-start).normalized,Vector3.Distance(start,end),~(1<<2)))if(hit.collider.GetComponentInParent<EnemyController>()!=this){blocked=true;end=hit.point;break;}
                if(!blocked)campaign.EnemyShot(damage);campaign.sound.Play("pistol",.48f,start);Effects.Shot(start,end,true);
            }
            bool moving=agent.velocity.sqrMagnitude>.05f;if(anim){string clip=moving?(alerted?"run":"walk"):"idle";if(anim[clip]!=null && !anim.IsPlaying(clip))anim.CrossFade(clip,.2f);}
            if(moving && Time.time>stepTime){stepTime=Time.time+(alerted?.32f:.56f);campaign.sound.Play(alerted?"run":"step",alerted?.27f:.16f,transform.position);}
        }
        void Think(float dt){
            bool seen=CanSee();if(seen){lastSeen=campaign.player.transform.position;memory=12;float d=Vector3.Distance(transform.position,lastSeen);suspicion=Mathf.Clamp01(suspicion+dt*(d<2?4:campaign.player.crouch?.65f:1.2f));
                if(suspicion>=1){Arm();destination=lastSeen;mode=!captureOnContact && d<8?Mode.Attack:Mode.Chase;}
                else {mode=Mode.Investigate;destination=lastSeen;}
            }else{
                suspicion=Mathf.Max(0,suspicion-dt*.16f);
                if(mode==Mode.Attack || mode==Mode.Chase){mode=Mode.Search;destination=lastSeen;searchUntil=Time.time+2;}
                if((mode==Mode.Search || mode==Mode.Investigate) && Vector3.Distance(transform.position,destination)<.8f && Time.time>searchUntil){
                    mode=Mode.Search;searchUntil=Time.time+2.5f;Vector3 offset=Quaternion.Euler(0,searchIndex++*137,0)*Vector3.forward*2.5f;if(NavMesh.SamplePosition(destination+offset,out var spot,1.5f,NavMesh.AllAreas))destination=spot.position;
                }
                if(memory<=0 && mode!=Mode.Patrol){alerted=false;SetEquipment(false);mode=Mode.Patrol;destination=route[routeIndex];}
            }
            if(mode==Mode.Patrol && Vector3.Distance(transform.position,destination)<.65f){
                if(!waypointReached){waypointReached=true;patrolUntil=Time.time+(routeIndex<routeMarkers.Count?routeMarkers[routeIndex].patrolWait:0);}
                if(Time.time>=patrolUntil){waypointReached=false;routeIndex=(routeIndex+1)%route.Count;destination=route[routeIndex];}
            }
            if(mode!=Mode.Patrol){waypointReached=false;patrolUntil=0;}
            foreach(var door in roomDoors)if(!door.IsOpen && !door.Moving && Vector3.Distance(transform.position,door.transform.position)<2.2f)door.Toggle();
            if(agent.isOnNavMesh)agent.SetDestination(destination);
        }
        public void TakeDamage(float amount){
            if(hp<=0)return;if(shielded){campaign.ui.Toast("QUYỀN TRUY CẬP BỊ THU HỒI / Khôi phục quyền tại máy tính.");return;}
            hp=Mathf.Max(0,hp-amount);suspicion=1;Arm();memory=12;lastSeen=campaign.player.transform.position;destination=lastSeen;mode=Mode.Chase;
            if(boss && hp<=200 && phase==1){phase=2;shielded=true;campaign.security.revoked=true;campaign.security.Close();campaign.Objective("override","Victor đã khóa máy tính bảng. Khôi phục quyền quản lý ở máy tính.");campaign.ui.Subtitle("VICTOR HALE","Marcus nghĩ mình có thể thay đổi Chợ Đen. Cháu cũng vậy sao?");}
            if(boss && hp<=100 && phase==2){phase=3;damage=20;campaign.ui.Subtitle("HỆ THỐNG AN NINH","Quyền được phục hồi. Dùng đèn / xung điện từ để vô hiệu hóa Victor.");}
            if(hp>0)return;
            SetEquipment(false);agent.enabled=false;GetComponent<Collider>().enabled=false;if(anim)anim.Stop();visual.localRotation=Quaternion.Euler(0,0,85);visual.localPosition=Vector3.up*.2f;
            campaign.state.kills++;
            if(boss){campaign.flags.Add("boss_dead");campaign.Objective("final","Đến máy tính kế thừa. Quyết định số phận North Point.");}
            else {var drop=new GameObject("đặc vụ supply");drop.transform.SetParent(campaign.level.transform);drop.transform.position=transform.position;var point=drop.AddComponent<Interaction>();point.id="supply_drop";point.title="ĐẠN / CỨU THƯƠNG";}
        }
    }
}
