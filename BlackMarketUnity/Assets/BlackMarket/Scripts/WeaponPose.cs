using UnityEngine;
namespace BlackMarket {
    // Native locomotion plays first; adjust the right arm toward the current firing direction.
    public class WeaponPose:MonoBehaviour {
        public Transform weapon;Transform upper,forearm,hand;
        void Start(){foreach(var t in GetComponentsInChildren<Transform>()){if(t.name=="Bip01 R UpperArm")upper=t;else if(t.name=="Bip01 R Forearm")forearm=t;else if(t.name=="Bip01 R Hand")hand=t;}}
        void LateUpdate(){if(!weapon || !weapon.gameObject.activeSelf || !upper || !forearm || !hand)return;
            var forward=transform.forward;upper.rotation=Quaternion.FromToRotation(forearm.position-upper.position,(forward-Vector3.up*.35f).normalized)*upper.rotation;
            forearm.rotation=Quaternion.FromToRotation(hand.position-forearm.position,forward)*forearm.rotation;
            weapon.SetPositionAndRotation(hand.position+forward*.08f,Quaternion.LookRotation(forward));
        }
    }
}
