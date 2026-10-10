using System.Collections.Generic;
using UnityEngine;
namespace BlackMarket {
 [DefaultExecutionOrder(20)]
 public class WeaponPose:MonoBehaviour {
  public float scriptedThrow=-1;public float weight=1;public Transform weapon;
  Transform upper,forearm,hand,leftUpper,leftForearm,leftHand;PlayerMotor player;float raised=1;GameObject grenade;
  readonly Dictionary<string,Transform> bones=new Dictionary<string,Transform>();
  readonly Dictionary<Transform,Quaternion> fingerRest=new Dictionary<Transform,Quaternion>();
  Vector3 rightPalm,leftPalm;Quaternion rightFrame,leftFrame;
  public Transform RightHand=>hand;public float RaisedAmount=>raised;
  public Vector3 RightPalmPosition=>hand?hand.TransformPoint(rightPalm):Vector3.zero;
  public Vector3 LeftPalmPosition=>leftHand?leftHand.TransformPoint(leftPalm):Vector3.zero;
  Transform Bone(string name)=>bones.TryGetValue("Bip01 "+name,out var t)?t:null;
  void Start(){player=GetComponentInParent<PlayerMotor>();if(player)raised=0;foreach(var t in GetComponentsInChildren<Transform>()){bones[t.name]=t;if(t.name.Contains(" Finger"))fingerRest[t]=t.localRotation;}upper=Bone("R UpperArm");forearm=Bone("R Forearm");hand=Bone("R Hand");leftUpper=Bone("L UpperArm");leftForearm=Bone("L Forearm");leftHand=Bone("L Hand");Calibrate(hand,"R",out rightPalm,out rightFrame);Calibrate(leftHand,"L",out leftPalm,out leftFrame);}
  void Calibrate(Transform palm,string side,out Vector3 center,out Quaternion frame){center=Vector3.zero;frame=Quaternion.identity;var index=Bone(side+" Finger1");var little=Bone(side+" Finger4");var middle=Bone(side+" Finger2");if(!palm||!index||!little||!middle)return;var a=palm.InverseTransformPoint(index.position);var b=palm.InverseTransformPoint(little.position);center=(a+b)*.35f;var forward=palm.InverseTransformPoint(middle.position).normalized;var normal=Vector3.Cross(forward,(a-b).normalized).normalized;frame=Quaternion.LookRotation(forward,normal);}
  Vector3 Wrist(Transform grip,Quaternion rotation,Vector3 palm)=>grip.position-rotation*Vector3.Scale(palm,grip.name=="Left grip"?leftHand.lossyScale:hand.lossyScale);
  void Fit(Vector3 target,Transform a,Transform b,Transform c){float reach=Vector3.Distance(a.position,b.position)+Vector3.Distance(b.position,c.position)-.02f;var delta=target-a.position;if(delta.magnitude>reach)weapon.position+=a.position+delta.normalized*reach-target;}
  void LateUpdate(){if(!weapon||!weapon.gameObject.activeSelf||!hand||!leftHand)return;
   raised=Mathf.MoveTowards(raised,player?(player.WeaponRaised&&player.reloadTime<=0?1:0):1,Time.deltaTime*5);
   var right=weapon.Find("Right grip");var left=weapon.Find("Left grip");var stock=weapon.Find("Stock");bool rifle=right&&left;
   float pitch=player?player.pitch:0;var rotation=transform.rotation*Quaternion.Euler(Mathf.Lerp(30,pitch,raised),0,0);
   weapon.rotation=rotation;var shoulder=upper.position-transform.forward*.035f-transform.right*.035f-Vector3.up*.055f;
   weapon.position=shoulder-rotation*(stock?stock.localPosition:new Vector3(0,0,-.23f));
   Quaternion rightRot=Quaternion.LookRotation((weapon.forward*.9f+weapon.up*.4f).normalized,-weapon.right)*Quaternion.Inverse(rightFrame);
   Quaternion leftRot=Quaternion.LookRotation(weapon.right,-weapon.up)*Quaternion.Inverse(leftFrame);
   if(rifle){for(int pass=0;pass<5;pass++){Fit(Wrist(left,leftRot,leftPalm),leftUpper,leftForearm,leftHand);Fit(Wrist(right,rightRot,rightPalm),upper,forearm,hand);}}
   bool throwing=player?player.Throwing:scriptedThrow>=0;if(throwing)weapon.position-=Vector3.up*.16f;
   Vector3 rightTarget=rifle?Wrist(right,rightRot,rightPalm):weapon.position;
   if(throwing){float t=player?player.ThrowProgress:scriptedThrow;var back=upper.position-transform.forward*.2f+Vector3.up*.28f;var release=upper.position+transform.forward*.53f+Vector3.up*.2f;rightTarget=t<.45f?Vector3.Lerp(rightTarget,back,Mathf.SmoothStep(0,1,t/.45f)):t<.7f?Vector3.Lerp(back,release,(t-.45f)/.25f):Vector3.Lerp(release,rightTarget,(t-.7f)/.3f);}
   IntroActorPose.Solve(upper,forearm,hand,Vector3.Lerp(hand.position,rightTarget,weight),-transform.up+transform.right*.45f);
   if(rifle){IntroActorPose.Solve(leftUpper,leftForearm,leftHand,Vector3.Lerp(leftHand.position,Wrist(left,leftRot,leftPalm),weight),-transform.up-transform.right*.4f);leftHand.rotation=Quaternion.Slerp(leftHand.rotation,leftRot,weight);if(!throwing)hand.rotation=Quaternion.Slerp(hand.rotation,rightRot,weight);foreach(var pair in fingerRest)pair.Key.localRotation=pair.Value;Curl("L",left.position,weapon.up);if(!throwing)Curl("R",right.position,-weapon.right);}
   if(player||throwing||grenade){if(!grenade)grenade=GrenadeVisual.Create(hand);grenade.SetActive(throwing&&(player?player.ThrowProgress:scriptedThrow)<.55f);}
  }
  // Flex in the anatomical palm plane instead of aiming every joint at one
  // point, which folded fingers backwards and produced pinched, broken wrists.
  void Curl(string side,Vector3 center,Vector3 inward){
   for(int i=0;i<5;i++){
    var baseBone=Bone(side+" Finger"+i);if(!baseBone||baseBone.childCount==0)continue;
    var palmFrame=side=="L"?leftHand.rotation*leftFrame:hand.rotation*rightFrame;
    var axis=Vector3.Cross(palmFrame*Vector3.forward,inward).normalized;
    for(int segment=0;segment<3;segment++){
    var finger=Bone(side+" Finger"+i+(segment==0?"":segment.ToString()));
    if(!finger)continue;
    float angle=segment==0?48:segment==1?70:45;
    if(i==0)angle*=.45f;
    if(side=="R"&&i==1)angle*=.45f;
    finger.rotation=Quaternion.AngleAxis(angle,axis)*finger.rotation;
    }
   }
  }
 }
}
