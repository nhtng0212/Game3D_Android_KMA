using System.Collections;
using UnityEngine;
namespace BlackMarket {
    public class PrologueSequence:MonoBehaviour {
        public bool Playing {get;private set;}
        public string Phase {get;private set;}
        public string NodeId {get;private set;}
        public PhoneDialogue.Node Node=>NodeId!=null && PhoneDialogue.Nodes.TryGetValue(NodeId,out var n)?n:null;
        public float ReadTime {get;private set;}
        public bool Ready=>Node!=null && ReadTime>=Node.text.Length/42f+3;
        public string VisibleText=>Node==null?Caption:Node.text.Substring(0,Mathf.Min(Node.text.Length,Mathf.FloorToInt(ReadTime*42)));
        public string Caption {get;private set;}
        public bool CanSkip=>game && game.HasSeenStory("alex-intro");
        Campaign game;GameObject bedroom,actor,phone;Transform bike;IntroActorPose pose;Animation anim;AudioSource engine;bool cancelled;
        Vector3 oldCamera;Quaternion oldRotation;float oldFov;
        static readonly Vector3 RoomOrigin=new Vector3(70,0,0);
        public static readonly Vector3 ParkPosition=new Vector3(3.8f,0,-2.3f);
        public static readonly Quaternion ParkRotation=Quaternion.Euler(0,90,0);
        public static Transform EnsureBike(Campaign game){var existing=game.level.transform.Find("Xe máy Alex đã đỗ");if(existing)return existing;var prefab=Resources.Load<GameObject>("Opening/AlexMotorcycle");if(!prefab)return null;var t=Instantiate(prefab,game.level.transform).transform;t.name="Xe máy Alex đã đỗ";t.SetPositionAndRotation(ParkPosition,ParkRotation);return t;}
        public void Begin(Campaign owner){game=owner;Playing=true;cancelled=false;oldCamera=game.view.transform.position;oldRotation=game.view.transform.rotation;oldFov=game.view.fieldOfView;game.player.ResetTouch();game.player.visual.gameObject.SetActive(false);game.LockCursor(false);bike=EnsureBike(game);if(bike)bike.gameObject.SetActive(false);StartCoroutine(Bedroom());}
        void Update(){if(Playing && !game.paused && game.active && Node!=null)ReadTime+=Time.deltaTime;if(engine){if(game.paused||!game.active)engine.Pause();else engine.UnPause();}}
        IEnumerator Bedroom(){
            Phase="bedroom";Caption="02:17 / PHÒNG NGỦ CỦA ALEX";
            bedroom=Instantiate(Resources.Load<GameObject>("Opening/AlexBedroom"),RoomOrigin,Quaternion.identity,transform);
            actor=Instantiate(Resources.Load<GameObject>("Actors/Alex"),RoomOrigin+new Vector3(.1f,0,.8f),Quaternion.identity,transform);actor.name="Alex — diễn viên hoạt cảnh";anim=actor.GetComponentInChildren<Animation>();if(anim){anim.cullingType=AnimationCullingType.AlwaysAnimate;anim.Play("idle");}pose=actor.AddComponent<IntroActorPose>();pose.pose=IntroActorPose.Pose.Desk;
            Shot(RoomOrigin+new Vector3(2.55f,1.85f,-2),RoomOrigin+new Vector3(-.35f,1,1),53);
            yield return Wait(2.5f);game.sound.Play("ring",.35f);Caption="Điện thoại reo… Chú Marcus đang gọi.";
            var hand=FindBone("Bip01 L Hand");phone=GameObject.CreatePrimitive(PrimitiveType.Cube);phone.name="Điện thoại cầm tay";Destroy(phone.GetComponent<Collider>());phone.transform.SetParent(hand,false);phone.transform.localPosition=new Vector3(.02f,.025f,0);phone.transform.localScale=new Vector3(.065f,.13f,.014f);phone.GetComponent<Renderer>().sharedMaterial=Resources.Load<GameObject>("Opening/AlexBedroom").transform.Find("Màn hình máy tính").GetComponent<Renderer>().sharedMaterial;
            pose.phone=phone.transform;pose.pose=IntroActorPose.Pose.Phone;pose.phoneLift=0;
            float t=0;while(t<1.3f){if(!game.paused&&game.active){t+=Time.deltaTime;pose.phoneLift=Mathf.SmoothStep(0,1,t/1.3f);}yield return null;}
            game.sound.Stop("ring");Shot(RoomOrigin+new Vector3(-1.1f,1.35f,2.15f),RoomOrigin+new Vector3(.1f,1.05f,.8f),52);Phase="dialogue";SetNode("start");
        }
        Transform FindBone(string name){foreach(var t in actor.GetComponentsInChildren<Transform>())if(t.name==name)return t;return actor.transform;}
        void SetNode(string id){NodeId=id;ReadTime=0;}
        public void Choose(int index){if(!Playing||!Ready||game.paused||index<0||index>=Node.choices.Length)return;var choice=Node.choices[index];PhoneDialogue.Apply(game.state,choice.effect);if(choice.next=="end"){NodeId=null;Phase="news";if(phone)phone.SetActive(false);pose.pose=IntroActorPose.Pose.Desk;game.ui.Story("CUỘC GỌI CUỐI CÙNG","Sáng hôm sau, Alex nhận được tin Marcus đã chết trong một vụ tai nạn.\n\nBa ngày sau — 21:30.\n\nKhông tin vào lời giải thích đó, Alex quyết định tới North Point để tìm chiếc thẻ đỏ và đọc tệp Order 71 mà chú đã nhắc tới.",()=>{if(Playing)StartCoroutine(Arrival());},"alex-intro-news");}else SetNode(choice.next);}
        IEnumerator Arrival(){
            Phase="ride";Caption="BA NGÀY SAU / 21:30 — NORTH POINT";if(bedroom)bedroom.SetActive(false);if(phone)phone.SetActive(false);bike.gameObject.SetActive(true);pose.pose=IntroActorPose.Pose.Ride;pose.weight=1;pose.leftGrip=bike.Find("Left grip");pose.rightGrip=bike.Find("Right grip");engine=bike.gameObject.AddComponent<AudioSource>();engine.clip=Resources.Load<AudioClip>("Audio/alex_motorcycle");engine.loop=true;engine.volume=.22f;engine.Play();
            float t=0;while(t<7){if(!game.paused&&game.active){t+=Time.deltaTime;float q=Mathf.SmoothStep(0,1,t/7);foreach(var wheel in new[]{bike.Find("Bánh xe trước"),bike.Find("Bánh xe sau")})if(wheel)wheel.Rotate(Vector3.right,Time.deltaTime*500*(1-q),Space.Self);engine.pitch=Mathf.Lerp(1.25f,.7f,q);bike.SetPositionAndRotation(Vector3.Lerp(new Vector3(-15,0,-3.4f),ParkPosition,q),ParkRotation);actor.transform.SetPositionAndRotation(bike.TransformPoint(new Vector3(0,0,-.35f)),ParkRotation);Shot(actor.transform.position+new Vector3(3.1f,1.75f,-4.2f),actor.transform.position+Vector3.up*.85f,50);}yield return null;}
            if(engine){engine.Stop();Destroy(engine);}
            Phase="dismount";Caption="ALEX: Cửa hàng vẫn tối… Mình phải tìm thứ chú để lại.";var start=actor.transform.position;var end=ParkPosition+new Vector3(0,0,1);
            t=0;while(t<2.5f){if(!game.paused&&game.active){t+=Time.deltaTime;float q=Mathf.SmoothStep(0,1,t/2.5f);pose.weight=1-q;actor.transform.position=Vector3.Lerp(start,end,q)+Vector3.up*(Mathf.Sin(q*Mathf.PI)*.12f);actor.transform.rotation=Quaternion.Slerp(ParkRotation,Quaternion.identity,q);Shot(new Vector3(6.5f,2.2f,-4.6f),actor.transform.position+Vector3.up*.9f,55);}yield return null;}
            pose.enabled=false;bike.SetPositionAndRotation(ParkPosition,ParkRotation);Phase="walk";Entrance(true);if(anim)anim.CrossFade("walk",.2f);
            yield return WalkTo(new Vector3(0,0,-1.3f),2.5f);yield return WalkTo(game.Marker("spawn").position,3);
            Finish(true);
        }
        IEnumerator WalkTo(Vector3 end,float seconds){var start=actor.transform.position;actor.transform.rotation=Quaternion.LookRotation(end-start);float t=0;while(t<seconds){if(!game.paused&&game.active){t+=Time.deltaTime;actor.transform.position=Vector3.Lerp(start,end,t/seconds);Shot(new Vector3(5.5f,2.5f,-5.5f),actor.transform.position+Vector3.up,57);}yield return null;}}
        IEnumerator Wait(float seconds){float t=0;while(t<seconds){if(!game.paused&&game.active)t+=Time.deltaTime;yield return null;}}
        void Shot(Vector3 position,Vector3 target,float fov){game.view.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position));game.view.fieldOfView=fov;}
        void Entrance(bool open){foreach(var t in game.level.GetComponentsInChildren<Transform>(true))if(t.name=="Locked entrance glass"||t.name=="Entrance handle")t.gameObject.SetActive(!open);}
        public void Skip(){if(Playing&&CanSkip){StopAllCoroutines();Finish(false);}}
        void Finish(bool seen){Playing=false;Phase="complete";NodeId=null;Cleanup();if(bike){bike.gameObject.SetActive(true);bike.SetPositionAndRotation(ParkPosition,ParkRotation);}Entrance(false);game.player.visual.gameObject.SetActive(true);game.view.transform.SetPositionAndRotation(oldCamera,oldRotation);game.view.fieldOfView=oldFov;game.Resume();if(seen)game.MarkStorySeen("alex-intro");game.state.introCompleted=true;game.Save();AddJournal(game);game.ui.Subtitle("ALEX","Mình đã khóa cửa trước. Tìm thẻ đỏ trong văn phòng của chú trước đã.");}
        public static void AddJournal(Campaign game){if(game.state.askedAboutCard)game.journal.Add("Lời Marcus: mở từng ngăn kéo trong các tủ sát tường văn phòng. Thẻ đỏ mở máy tính; tệp cần đọc là Order 71.");if(game.state.askedAboutHunters)game.journal.Add("Marcus cảnh báo: nhóm người mặc vest đen đang truy tìm chú. Họ không phải khách hàng.");if(game.state.promisedMarcus)game.journal.Add("Alex đã hứa với Marcus sẽ không xuống Cửa 006.");}
        void Cleanup(){if(engine){engine.Stop();Destroy(engine);}if(bedroom){bedroom.SetActive(false);Destroy(bedroom);}if(actor){actor.SetActive(false);Destroy(actor);}if(game && game.sound)game.sound.Stop("ring");}
        public void Cancel(){if(cancelled)return;cancelled=true;StopAllCoroutines();Playing=false;NodeId=null;Cleanup();if(game&&game.player)game.player.visual.gameObject.SetActive(true);}
        void OnDestroy(){Cancel();}
    }
}
