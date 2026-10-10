using UnityEngine;
namespace BlackMarket {
    public class OfficeDrawer : MonoBehaviour {
        public Transform tray;
        public GameObject redCard;
        public int number;
        public bool IsOpen { get; private set; }
        public bool Revealed { get; private set; }
        Vector3 closed;
        void Awake(){closed=tray.localPosition;if(redCard)redCard.SetActive(false);}
        public void Open(){
            var game=Campaign.Instance;if(!game || !game.Running || IsOpen)return;
            IsOpen=true;GetComponent<Interaction>().consumed=true;
            game.sound.Play("door_move",.18f,transform.position);
        }
        void Update(){
            if(!IsOpen || Revealed || !Campaign.Instance.Running)return;
            tray.localPosition=Vector3.MoveTowards(tray.localPosition,closed+Vector3.back*.48f,Time.deltaTime*.8f);
            if(Vector3.Distance(tray.localPosition,closed+Vector3.back*.48f)>.005f)return;
            Revealed=true;var game=Campaign.Instance;
            if(redCard && !game.state.keycard){redCard.SetActive(true);game.Objective("keycard","Đã thấy USB đỏ! Nhặt USB trong ngăn kéo vừa mở.");game.ui.Toast("Có một chiếc USB đỏ trong ngăn kéo.");}
            else game.ui.Toast("Ngăn kéo "+number+" trống.");
        }
    }
}
