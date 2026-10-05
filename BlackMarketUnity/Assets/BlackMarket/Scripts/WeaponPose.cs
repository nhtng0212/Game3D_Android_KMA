using UnityEngine;
namespace BlackMarket {
    // Native locomotion first; blend low-ready, aim and NPC draw over the right arm.
    public class WeaponPose:MonoBehaviour {
        public float weight=1;public Transform weapon;Transform upper,forearm,hand,leftUpper,leftForearm,leftHand;PlayerMotor player;float raised=1;
        public float RaisedAmount=>raised;
        void Start(){player=GetComponentInParent<PlayerMotor>();if(player)raised=0;foreach(var t in GetComponentsInChildren<Transform>()){if(t.name=="Bip01 R UpperArm")upper=t;else if(t.name=="Bip01 R Forearm")forearm=t;else if(t.name=="Bip01 R Hand")hand=t;else if(t.name=="Bip01 L UpperArm")leftUpper=t;else if(t.name=="Bip01 L Forearm")leftForearm=t;else if(t.name=="Bip01 L Hand")leftHand=t;}}
        void LateUpdate(){if(!weapon || !weapon.gameObject.activeSelf || !upper || !forearm || !hand)return;
            raised=Mathf.MoveTowards(raised,player?(player.WeaponRaised && player.reloadTime<=0?1:0):1,Time.deltaTime*5);
            var forward=Vector3.Slerp((transform.forward*.35f-Vector3.up).normalized,transform.forward,raised);
            upper.rotation=Quaternion.Slerp(upper.rotation,Quaternion.FromToRotation(forearm.position-upper.position,(forward-Vector3.up*.35f).normalized)*upper.rotation,weight);
            forearm.rotation=Quaternion.Slerp(forearm.rotation,Quaternion.FromToRotation(hand.position-forearm.position,forward)*forearm.rotation,weight);
            weapon.SetPositionAndRotation(Vector3.Lerp(transform.position+Vector3.up*.9f+transform.right*.25f,hand.position+forward*.08f,weight),Quaternion.LookRotation(forward,transform.up));
            if(player && player.UsingAK && leftUpper && leftForearm && leftHand){Vector3 support=weapon.TransformPoint(new Vector3(-.025f,-.025f,.27f));Vector3 elbow=Vector3.Lerp(leftUpper.position,support,.5f)-transform.up*.17f;leftUpper.rotation=Quaternion.FromToRotation(leftForearm.position-leftUpper.position,elbow-leftUpper.position)*leftUpper.rotation;leftForearm.rotation=Quaternion.FromToRotation(leftHand.position-leftForearm.position,support-leftForearm.position)*leftForearm.rotation;}
        }
    }
}
