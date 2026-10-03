using UnityEngine;
namespace BlackMarket {
    public class WorldMarker : MonoBehaviour {
        public string id;
        void OnDrawGizmos(){Gizmos.color=new Color(.4f,1,.8f);Gizmos.DrawWireSphere(transform.position,.22f);Gizmos.DrawRay(transform.position,transform.forward);}
    }
}
