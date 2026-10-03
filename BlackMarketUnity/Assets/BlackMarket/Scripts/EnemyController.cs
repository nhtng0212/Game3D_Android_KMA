using UnityEngine;
using UnityEngine.AI;
namespace BlackMarket {
    public class EnemyController : MonoBehaviour {
        public Campaign campaign;public bool boss,shielded;public float hp=100,damage=15,speed=3,stun,suspicion;public int phase=1;
        public enum Mode {Patrol,Investigate,Chase,Attack,Search}
        public Mode mode;
        NavMeshAgent agent;Transform visual;Animation anim;Vector3 home,destination,lastSeen;float nextThink,nextShot,memory;bool patrolOther;
        public void Setup(){
            home=transform.position;destination=home;agent=gameObject.AddComponent<NavMeshAgent>();agent.radius=.32f;agent.height=1.8f;agent.speed=speed;agent.stoppingDistance=.5f;
            if(NavMesh.SamplePosition(home,out var snap,3,NavMesh.AllAreas))agent.Warp(snap.position);
            var capsule=gameObject.AddComponent<CapsuleCollider>();capsule.height=1.8f;capsule.radius=.32f;capsule.center=Vector3.up*.9f;
            visual=Instantiate(Resources.Load<GameObject>("Actors/Operator"),transform).transform;anim=visual.GetComponentInChildren<Animation>();var gun=Instantiate(Resources.Load<GameObject>("Actors/Pistol"),visual);visual.gameObject.AddComponent<WeaponPose>().weapon=gun.transform;
        }
        public bool CanSee(){
            if(!campaign.player)return false;
            var delta=campaign.player.transform.position-transform.position;
            float range=(speed>4?20:15)*(campaign.security.dark? .48f:1)*(campaign.player.crouch? .65f:1);
            if(delta.magnitude>range)return false;
            if(delta.magnitude>2 && Vector3.Angle(visual.forward,delta)> (speed>4?45:30))return false;
            var from=transform.position+Vector3.up*1.5f;var to=campaign.player.transform.position+Vector3.up*(campaign.player.crouch?1:1.3f);
            // Player is on Ignore Raycast layer. Walls and other geometry block this segment.
            return !Physics.Linecast(from,to,~(1<<2),QueryTriggerInteraction.Ignore);
        }
        public void Hear(Vector3 point,float radius){if(hp<=0 || Vector3.Distance(transform.position,point)>radius || mode==Mode.Attack || mode==Mode.Chase)return;destination=point;memory=7;mode=Mode.Investigate;}
        void Update(){
            if(hp<=0)return;
            bool stop=!campaign.Running || stun>0;
            if(agent && agent.isOnNavMesh)agent.isStopped=stop;
            if(!campaign.Running)return;
            stun=Mathf.Max(0,stun-Time.deltaTime);if(stun>0)return;memory-=Time.deltaTime;
            if(Time.time>=nextThink){nextThink=Time.time+.16f;bool seen=CanSee();
                suspicion=Mathf.Clamp01(suspicion+(seen?(campaign.player.crouch? .25f:.45f):-.07f));
                if(seen){lastSeen=campaign.player.transform.position;memory=5;}
                if(seen && suspicion>=1){destination=lastSeen;mode=Vector3.Distance(transform.position,lastSeen)<9?Mode.Attack:Mode.Chase;}
                else if(mode==Mode.Attack || mode==Mode.Chase){mode=Mode.Search;destination=lastSeen;}
                else if(memory<=0 && mode!=Mode.Patrol){mode=Mode.Patrol;destination=home;}
                if(mode==Mode.Patrol && Vector3.Distance(transform.position,destination)<1){patrolOther=!patrolOther;destination=home+(patrolOther?Vector3.forward*3:Vector3.zero);}
                if(agent.isOnNavMesh){agent.speed=mode==Mode.Patrol?speed*.55f:speed;agent.SetDestination(destination);}
            }
            if(mode==Mode.Attack){if(agent.isOnNavMesh)agent.isStopped=true;var to=campaign.player.transform.position-transform.position;to.y=0;if(to.sqrMagnitude>.01f)visual.rotation=Quaternion.Slerp(visual.rotation,Quaternion.LookRotation(to),Time.deltaTime*8);
                if(Time.time>=nextShot && CanSee()){nextShot=Time.time+(boss? .85f:1.2f);campaign.Damage(damage);campaign.sound.Play("shot",.3f,transform.position);Effects.Shot(transform.position+Vector3.up*1.4f,campaign.player.transform.position+Vector3.up*1.2f,true);}}
            else if(agent.velocity.sqrMagnitude>.05f)visual.rotation=Quaternion.Slerp(visual.rotation,Quaternion.LookRotation(agent.velocity),Time.deltaTime*10);
            if(anim){string clip=agent.velocity.sqrMagnitude>.05f?"run":"idle";if(!anim.IsPlaying(clip))anim.CrossFade(clip,.2f);}
        }
        public void TakeDamage(float amount){
            if(hp<=0)return;if(shielded){campaign.ui.Toast("ACCESS REVOKED / Khôi phục quyền tại terminal.");return;}
            hp=Mathf.Max(0,hp-amount);suspicion=1;memory=7;lastSeen=campaign.player.transform.position;destination=lastSeen;mode=Mode.Chase;
            if(boss && hp<=200 && phase==1){phase=2;shielded=true;campaign.security.revoked=true;campaign.security.Close();campaign.Objective("override","Victor đã khóa Tablet. Khôi phục quyền Keeper ở terminal.");campaign.ui.Subtitle("VICTOR HALE","Marcus nghĩ mình có thể thay đổi The Market. Cháu cũng vậy sao?");}
            if(boss && hp<=100 && phase==2){phase=3;damage=20;campaign.ui.Subtitle("KEEPER OS","Quyền được phục hồi. Dùng đèn / xung EMP để vô hiệu hóa Victor.");}
            if(hp>0)return;
            agent.enabled=false;GetComponent<Collider>().enabled=false;if(anim)anim.Stop();visual.localRotation=Quaternion.Euler(0,0,85);visual.localPosition=Vector3.up*.2f;
            campaign.state.kills++;
            if(boss){campaign.flags.Add("boss_dead");campaign.Objective("final","Đến Successor Terminal. Quyết định số phận North Point.");}
            else {var drop=new GameObject("Operator supply");drop.transform.SetParent(campaign.level.transform);drop.transform.position=transform.position;var point=drop.AddComponent<Interaction>();point.id="supply_drop";point.title="ĐẠN / CỨU THƯƠNG";}
        }
    }
}
