using System.Collections;
using UnityEngine;
namespace BlackMarket {
 public class GroundedDeath:MonoBehaviour {
  public void Fall(Transform model){StartCoroutine(Settle(model));}
  IEnumerator Settle(Transform model){
   foreach(var pose in model.GetComponents<WeaponPose>())pose.enabled=false;
   var start=model.position;var original=model.rotation;var end=Quaternion.Euler(0,model.eulerAngles.y,0)*Quaternion.Euler(0,0,90);
   float ground=transform.position.y;if(Physics.Raycast(transform.position+Vector3.up*.2f,Vector3.down,out var hit,3,~(1<<2),QueryTriggerInteraction.Ignore))ground=hit.point.y;
   model.rotation=end;float min=float.PositiveInfinity;var mesh=new Mesh();foreach(var skin in model.GetComponentsInChildren<SkinnedMeshRenderer>()){skin.BakeMesh(mesh);foreach(var v in mesh.vertices)min=Mathf.Min(min,skin.transform.TransformPoint(v).y);}Destroy(mesh);
   Vector3 target=start+Vector3.up*(ground+.018f-min);if(float.IsInfinity(min))target=start;model.rotation=original;
   float t=0;while(t<1){if(Campaign.Instance&&!Campaign.Instance.paused){t+=Time.deltaTime*2.5f;float q=Mathf.SmoothStep(0,1,t);model.SetPositionAndRotation(Vector3.Lerp(start,target,q),Quaternion.Slerp(original,end,q));}yield return null;}model.SetPositionAndRotation(target,end);
  }
 }
}
