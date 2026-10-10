using UnityEngine;
namespace BlackMarket {
 public class ChargePlacementPose:MonoBehaviour {
  public Vector3 target;Transform upper,forearm,hand;
  void Start(){foreach(var t in GetComponentsInChildren<Transform>()){if(t.name=="Bip01 R UpperArm")upper=t;if(t.name=="Bip01 R Forearm")forearm=t;if(t.name=="Bip01 R Hand")hand=t;}}
  void LateUpdate(){if(upper&&forearm&&hand)IntroActorPose.Solve(upper,forearm,hand,target+transform.right*Mathf.Sin(Time.time*6)*.025f,-transform.up+transform.right);}
 }
}
