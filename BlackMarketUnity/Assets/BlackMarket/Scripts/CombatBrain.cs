using UnityEngine;
using UnityEngine.AI;
using System.Linq;
namespace BlackMarket {
 public class CombatBrain:MonoBehaviour {
  EnemyController e;NavMeshAgent agent;Transform visual;Animation anim;WeaponPose pose;Vector3 known;float confused,think,shot,moveAt,acquired,grace,searchAt;int burst,searchStep;
  WorldMarker[] covers;Vector3 tacticalPosition;float repositionAt,grenadeAt;
  public bool CanSeePlayer {get;private set;}
  public void Setup(EnemyController owner,NavMeshAgent nav,Transform model,Animation animation,WeaponPose weapon){e=owner;covers=e.campaign.level.GetComponentsInChildren<WorldMarker>().Where(m=>m.id.StartsWith("cover_")).ToArray();tacticalPosition=transform.position;grenadeAt=Time.time+4;grace=Time.time+5;agent=nav;visual=model;anim=animation;pose=weapon;known=e.campaign.Marker("combat_focus").position+Vector3.right*((e.combatId%3-1)*2);if(e.campaign.underground.Dark&&!e.nightSight)Blackout();if(e.nightSight){var prefab=Resources.Load<GameObject>("Actors/NightVisionGoggles");if(prefab){var goggles=Instantiate(prefab,visual);goggles.transform.localPosition=Vector3.up*1.64f;}}}
  public void Blackout(){if(e.nightSight)return;confused=4;pose.weight=0;CanSeePlayer=false;}
  public void Hear(Vector3 point,float radius){if(Vector3.Distance(transform.position,point)>radius)return;known=point;moveAt=0;searchAt=Time.time+10;}
  bool Clear(Vector3 from,Vector3 to){foreach(var h in Physics.RaycastAll(from,(to-from).normalized,Vector3.Distance(from,to),~(1<<2),QueryTriggerInteraction.Ignore))if(h.collider.GetComponentInParent<EnemyController>()!=e)return false;return true;}
  public static float HitProbability(float distance,float moveSpeed,bool crouched,bool pressure){
   float baseChance=Mathf.Lerp(pressure?.58f:.42f,pressure?.35f:.26f,Mathf.Clamp01(distance/85));
   float movement=Mathf.Lerp(1,.45f,Mathf.Clamp01(moveSpeed/6.5f));
   return baseChance*movement*(crouched?.72f:1);
  }
  public void Tick(){
   if(e.hp<=0)return;var g=e.campaign;bool pressure=g.underground.Suppressing||e.boss;
   if(g.state.stage==1&&g.underground.VolleyActive){if(agent.isOnNavMesh)agent.isStopped=true;CanSeePlayer=false;Play("idle");return;}
   if(g.underground.Playing&&!g.paused){if(agent.isOnNavMesh){agent.isStopped=false;agent.speed=e.boss?4.6f:6.2f;agent.acceleration=18;agent.SetDestination(g.underground.AutoRetreat?g.player.transform.position:g.Marker("combat_focus").position+Vector3.right*((e.combatId%3-1)*2));}Face(agent.velocity);Play("run");return;}
   if(agent.isOnNavMesh)agent.isStopped=!g.Running||e.stun>0||confused>0;if(!g.Running)return;e.stun=Mathf.Max(0,e.stun-Time.deltaTime);if(e.stun>0)return;
   if(confused>0){confused-=Time.deltaTime;visual.Rotate(0,Mathf.Sin(Time.time*3)*Time.deltaTime*80,0);Play("idle");return;}pose.weight=Mathf.MoveTowards(pose.weight,1,Time.deltaTime*2);
   var target=g.player.transform.position+Vector3.up*(g.player.crouch?.8f:1.3f);var origin=transform.position+Vector3.up*1.45f;var delta=target-origin;
   if(Time.time>=think){think=Time.time+.12f;CanSeePlayer=(!g.underground.Dark||e.nightSight)&&delta.magnitude<(g.state.stage==1?85:42)&&(g.state.stage!=1||Vector3.Angle(visual.forward,delta)<75)&&Clear(origin,target);
    if(CanSeePlayer){known=g.player.transform.position;e.suspicion=1;searchAt=Time.time+6;}
    if(agent.isOnNavMesh&&Time.time>=moveAt){moveAt=Time.time+.7f;agent.speed=e.boss?3.5f:3.2f;agent.acceleration=18;
     if(!CanSeePlayer&&g.state.stage==1&&Vector3.Distance(transform.position,known)<3&&Time.time>searchAt){searchAt=Time.time+5;var rooms=g.level.GetComponentsInChildren<WorldMarker>().Where(m=>m.id.StartsWith("room_")).ToArray();if(rooms.Length>0)known=rooms[(Mathf.Abs(e.combatId)+searchStep++)%rooms.Length].transform.position;}
     var destination=known;if(g.underground.Dark&&!e.nightSight){var cover=g.level.GetComponentsInChildren<WorldMarker>().Where(m=>m.id.StartsWith("cover_")).OrderBy(m=>Vector3.Distance(m.transform.position,transform.position)).FirstOrDefault();if(cover)destination=cover.transform.position;}
     if(CanSeePlayer||e.suspicion>0)destination=TacticalPosition(known);
     if(transform.position.z<3)destination=new Vector3((e.combatId%3-1)*1.2f,0,5);
     if(NavMesh.SamplePosition(destination,out var snap,5,NavMesh.AllAreas))agent.SetDestination(snap.position);
    }
   }
   if(Time.time>grenadeAt&&(CanSeePlayer||e.suspicion>0)&&Vector3.Distance(transform.position,known)>8&&Vector3.Distance(transform.position,known)<48){
    if(g.underground.TryTacticalGrenade(e,known+Vector3.right*Random.Range(-1.2f,1.2f)))grenadeAt=Time.time+(e.boss?2.2f:5);
   }
   acquired=CanSeePlayer?acquired+Time.deltaTime:0;e.mode=CanSeePlayer?EnemyController.Mode.Attack:EnemyController.Mode.Search;Face(CanSeePlayer?delta:agent.velocity);
   if(pose.scriptedThrow<0&&CanSeePlayer&&acquired>(pressure?.25f:.65f)&&Time.time>grace&&Time.time>=shot&&g.underground.FireSlot(e)&&Clear(origin,target)){
    int rounds=pressure?16:9;burst++;shot=Time.time+(burst<rounds?(pressure?.085f:.11f):(e.boss?2.2f:1.1f));if(burst>=rounds)burst=0;
    var barrel=pose.weapon?pose.weapon.Find("Muzzle"):null;var muzzle=barrel?barrel.position:origin;bool clear=Clear(muzzle,target);bool hit=clear&&Random.value<Mathf.Clamp01(HitProbability(delta.magnitude,g.player.MoveSpeed,g.player.crouch,pressure)*(e.elite?1.2f:1)*(g.underground.Overwhelming?2.5f:1));
    // All shooters share a damage interval. Rejected hits become visible near misses.
    if(clear){hit=hit&&g.EnemyShot(e.damage);var across=Vector3.Cross(delta.normalized,Vector3.up).normalized;
     var end=hit?target:target+across*(Random.value<.5f?-1:1)*Random.Range(.8f,2.2f)+Vector3.up*Random.Range(-.35f,.8f)+delta.normalized*Random.Range(1,4);
     foreach(var h in Physics.RaycastAll(muzzle,(end-muzzle).normalized,Vector3.Distance(muzzle,end),~(1<<2),QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance))if(h.collider.GetComponentInParent<EnemyController>()!=e){end=h.point;break;}
     Effects.Shot(muzzle,end,true);g.sound.Play("rifle",pressure?.32f:.25f,origin);
    }
   }
   Play(agent.velocity.sqrMagnitude>.1f?"run":"idle");
  }
  Vector3 TacticalPosition(Vector3 target){
   if(Time.time<repositionAt){
    if(Time.time%4>2){var side=Vector3.Cross((target-tacticalPosition).normalized,Vector3.up);var peek=tacticalPosition+side*(e.combatId%2==0?1.1f:-1.1f);if(NavMesh.SamplePosition(peek,out var hit,1,NavMesh.AllAreas)&&!NavMesh.Raycast(tacticalPosition,hit.position,out var blocked,NavMesh.AllAreas))return hit.position;}
    return tacticalPosition;
   }
   // Only one fire team bounds forward while the other teams hold and fire.
   if((int)(Time.time/4)%3!=Mathf.Abs(e.combatId)%3&&transform.position.z>4)return transform.position;
   repositionAt=Time.time+Random.Range(4,6);float best=float.PositiveInfinity;Vector3 chosen=transform.position;
   foreach(var m in covers){float travel=Vector3.Distance(transform.position,m.transform.position);if(travel>13)continue;
    var away=(m.transform.position-target).normalized;away.y=0;var candidate=m.transform.position+away*.65f;
    if(Vector3.Distance(candidate,target)<9||e.campaign.enemies.Any(other=>other&&other!=e&&other.hp>0&&Vector3.Distance(other.transform.position,candidate)<1.8f))continue;
    if(!NavMesh.SamplePosition(candidate,out var snap,1.5f,NavMesh.AllAreas))continue;
    var path=new NavMeshPath();if(!agent.CalculatePath(snap.position,path)||path.status!=NavMeshPathStatus.PathComplete)continue;
    bool protectedSpot=!Clear(snap.position+Vector3.up*.8f,target+Vector3.up);float score=travel*.5f+Mathf.Abs(Vector3.Distance(candidate,target)-22)+(protectedSpot?0:9)+Random.value*3;
    if(score<best){best=score;chosen=snap.position;}
   }
   tacticalPosition=chosen;return chosen;
  }
  void Face(Vector3 direction){direction.y=0;if(direction.sqrMagnitude>.01f)visual.rotation=Quaternion.Slerp(visual.rotation,Quaternion.LookRotation(direction),Time.deltaTime*7);}
  void Play(string name){if(anim&&anim[name]!=null&&!anim.IsPlaying(name))anim.CrossFade(name,.2f);}
 }
}
