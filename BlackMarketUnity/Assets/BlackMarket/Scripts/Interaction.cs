using UnityEngine;
namespace BlackMarket {
    public class Interaction : MonoBehaviour {
        public string id, title;
        public bool consumed;
        public bool Available => !consumed && Campaign.Instance && Campaign.Instance.Available(id);
        public void Use() { if (Available) Campaign.Instance.Interact(this); }
    }
}
