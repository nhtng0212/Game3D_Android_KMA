using UnityEngine;
using UnityEngine.AI;
namespace BlackMarket {
    // Sliding pocket door: the leaf remains solid and occludes NPC sight throughout travel.
    public class RoomDoor:MonoBehaviour {
        public Transform leaf;
        public string roomName;
        public bool IsOpen {get;private set;}
        public bool Moving=>leaf && Mathf.Abs(leaf.localPosition.x-(IsOpen?2.25f:0))>.01f;
        NavMeshObstacle obstacle;bool wasMoving;
        void Awake(){obstacle=leaf.GetComponent<NavMeshObstacle>();if(obstacle)obstacle.enabled=true;Refresh();}
        bool Occupied(){
            var center=transform.TransformPoint(new Vector3(1.1f,.2f,0));
            foreach(var hit in Physics.OverlapBox(center+Vector3.up, new Vector3(2.25f,1.2f,.65f),transform.rotation,~0,QueryTriggerInteraction.Ignore))
                if(hit.GetComponentInParent<PlayerMotor>() || hit.GetComponentInParent<EnemyController>())return true;
            return false;
        }
        public void Toggle(){if(!Campaign.Instance.Running || Moving)return;if(IsOpen && Occupied()){Campaign.Instance.ui.Toast("Lùi khỏi ngưỡng cửa để đóng cửa.");return;}IsOpen=!IsOpen;Refresh();wasMoving=true;Campaign.Instance.sound.Play("door_move",.3f,transform.position+Vector3.up);}
        public void SetOpenImmediate(bool open){IsOpen=open;leaf.localPosition=new Vector3(open?2.25f:0,1.25f,0);Refresh();Physics.SyncTransforms();}
        void Refresh(){var interaction=GetComponent<Interaction>();if(interaction)interaction.title=(IsOpen?"ĐÓNG CỬA / ":"MỞ CỬA / ")+roomName;}
        void Update(){if(!Campaign.Instance || !Campaign.Instance.Running)return;if(!IsOpen && Moving && Occupied()){IsOpen=true;Refresh();}leaf.localPosition=Vector3.MoveTowards(leaf.localPosition,new Vector3(IsOpen?2.25f:0,1.25f,0),Time.deltaTime*3);if(wasMoving && !Moving){wasMoving=false;Campaign.Instance.sound.Play("door_latch",.2f,transform.position+Vector3.up);}}
    }
}
