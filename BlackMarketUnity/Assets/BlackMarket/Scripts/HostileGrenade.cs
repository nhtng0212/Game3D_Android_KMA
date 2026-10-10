using UnityEngine;
using System.Linq;
namespace BlackMarket {
 // A visible, telegraphed opening volley; no homing after release.
 public class HostileGrenade:MonoBehaviour {
  Campaign game;Vector3 start,landing;float elapsed;bool landed;LineRenderer ring;Material warningMaterial;
  public const float Fuse=3.8f,Radius=4.5f;
  public static HostileGrenade Launch(Campaign g,Vector3 from,Vector3 target){
   var o=new GameObject("Lựu đạn địch / tránh vòng cam");o.transform.SetParent(g.level.transform);o.transform.position=from;
   var h=o.AddComponent<HostileGrenade>();h.game=g;h.start=from;h.landing=target;GrenadeVisual.Create(o.transform);
   var warning=new GameObject("Vùng nổ / cảnh báo");warning.transform.SetParent(o.transform);h.ring=warning.AddComponent<LineRenderer>();h.ring.useWorldSpace=true;h.ring.loop=true;h.ring.positionCount=48;h.ring.widthMultiplier=.055f;h.warningMaterial=new Material(Shader.Find("Universal Render Pipeline/Unlit"));h.ring.sharedMaterial=h.warningMaterial;h.DrawRing();return h;
  }
  void DrawRing(){var color=Color.Lerp(new Color(1,.55f,.05f),Color.red,elapsed/Fuse);ring.startColor=ring.endColor=color;warningMaterial.SetColor("_BaseColor",color);for(int i=0;i<48;i++){float a=i*Mathf.PI*2/48;ring.SetPosition(i,landing+new Vector3(Mathf.Cos(a)*Radius,.08f,Mathf.Sin(a)*Radius));}}
  void OnDestroy(){if(warningMaterial)Destroy(warningMaterial);}
  void Update(){if(!game||!game.Running)return;elapsed+=Time.deltaTime;
   if(!landed){float t=Mathf.Clamp01(elapsed/1.25f);var next=Vector3.Lerp(start,landing+Vector3.up*.08f,t)+Vector3.up*Mathf.Sin(t*Mathf.PI)*.75f;
    var step=next-transform.position;foreach(var h in Physics.SphereCastAll(transform.position,.07f,step.normalized,step.magnitude,~(1<<2),QueryTriggerInteraction.Ignore).OrderBy(h=>h.distance)){
     if(h.collider.GetComponentInParent<EnemyController>())continue;
     next=h.point+h.normal*.09f;landing=next;landed=true;break;
    }
    transform.position=next;if(t>=1)landed=true;
   }
   DrawRing();if(elapsed<Fuse)return;var position=transform.position;BlastEffects.Explode(position,Radius);game.sound.Play("explosion",.65f,position);game.underground.Shake(.45f);
   var target=game.player.transform.position+Vector3.up*.8f;float distance=Vector3.Distance(position,target);bool clear=true;
   foreach(var h in Physics.RaycastAll(position,(target-position).normalized,distance,~(1<<2),QueryTriggerInteraction.Ignore))if(!h.collider.GetComponentInParent<EnemyController>()){clear=false;break;}
   if(clear&&distance<Radius)game.underground.CombatHit(Mathf.Lerp(18,4,distance/Radius),true);
   Destroy(gameObject);
  }
 }
}
