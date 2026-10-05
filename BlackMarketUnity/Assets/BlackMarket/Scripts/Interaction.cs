using UnityEngine;
namespace BlackMarket {
    public class Interaction : MonoBehaviour {
        public string id, title;
        public bool consumed;
        public bool Available => !consumed && Campaign.Instance && Campaign.Instance.Available(id);
        public void Use() { if (!Available) return; var door=GetComponent<RoomDoor>(); if(door)door.Toggle();else Campaign.Instance.Interact(this); }
    }
}
