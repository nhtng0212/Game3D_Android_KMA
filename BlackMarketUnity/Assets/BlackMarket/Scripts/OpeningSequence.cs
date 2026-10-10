using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace BlackMarket {
    // Owns the opening only; floor transitions destroy it with the shop.
    public class OpeningSequence : MonoBehaviour {
        public bool Playing { get; private set; }
        public bool PursuitStarted { get; private set; }
        public bool CanSkip => game.HasSeenStory("arrival");
        public string Caption { get; private set; }
        Campaign game;
        readonly List<GameObject> actors=new List<GameObject>();
        readonly List<Transform> cars=new List<Transform>();
        bool skip;
        public void Setup(Campaign owner){game=owner;}
        public void Begin(){if(Playing || PursuitStarted)return;StartCoroutine(Arrival());}
        public void Skip(){if(Playing && CanSkip)skip=true;}
        IEnumerator Arrival(){
            Playing=true;skip=false;game.player.ResetTouch();game.LockCursor(false);
            var oldPosition=game.view.transform.position;var oldRotation=game.view.transform.rotation;float oldFov=game.view.fieldOfView;
            Caption="ALEX: Mã ư?";float beat=0;while(beat<2){if(!game.paused)beat+=Time.deltaTime;yield return null;}
            for(int i=0;i<3;i++){
                var car=Instantiate(Resources.Load<GameObject>("Opening/BlackSedan"),transform).transform;
                car.name="Xe của nhóm truy bắt "+(i+1);car.position=new Vector3(-10+i*9,-.1f,-7);car.rotation=Quaternion.Euler(0,90,0);cars.Add(car);
            }
            for(int i=0;i<10;i++){
                var actor=Instantiate(Resources.Load<GameObject>("Actors/BlackSuit"),transform);actor.name="Thành viên nhóm mặc vest "+(i+1);actor.SetActive(false);actors.Add(actor);
            }
            game.sound.Play("arrival_engine",.55f,new Vector3(0,0,-5));
            // A soft street lamp keeps faces and dark clothing readable without lighting the shop interior.
            var lamp=new GameObject("Đèn đường");lamp.transform.SetParent(transform);lamp.transform.position=new Vector3(0,5,-5);var light=lamp.AddComponent<Light>();light.type=LightType.Point;light.range=18;light.intensity=4;light.color=new Color(.72f,.82f,1);
            float elapsed=0;bool doorsHeard=false;
            while(elapsed<14 && !skip){
                // Pause suspends camera, movement and timers together.
                if(game.paused || !game.active){yield return null;continue;}
                elapsed+=Time.deltaTime;
                float drive=Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/4));
                for(int i=0;i<3;i++)cars[i].position=new Vector3(-10+i*9-18*(1-drive),-.1f,-6.5f);
                if(elapsed<4){
                    Caption="Tiếng động cơ ngoài cửa… Có người tới.";
                    Shot(Vector3.Lerp(new Vector3(10,3,-14),new Vector3(8,2.5f,-11),drive),new Vector3(0,1,-5),55);
                } else {
                    if(!doorsHeard){doorsHeard=true;game.sound.Play("door_latch",.45f,new Vector3(0,1,-5));}
                    float walk=Mathf.Clamp01((elapsed-5.6f)/8.4f);
                    float doorAngle=elapsed<6?Mathf.SmoothStep(0,72,Mathf.Clamp01((elapsed-4)/.7f)):Mathf.SmoothStep(72,0,Mathf.Clamp01((elapsed-6)/.8f));
                    foreach(var car in cars)foreach(var door in car.GetComponentsInChildren<Transform>())if(door.name=="Cửa xe chuyển động")door.localRotation=Quaternion.Euler(0,doorAngle,0);
                    Caption=elapsed<11?"KẺ CẦM ĐẦU: Đúng biển số xe của thằng cháu Marcus. Nó vẫn ở trong cửa hàng. Mau vào xử lý hắn!":"NHÓM NGƯỜI MẶC VEST: Lục soát từng phòng. Không để nó thoát!";
                    Shot(elapsed<8?new Vector3(0,2.2f,-15):new Vector3(4,2.3f,-4),new Vector3(-1,1.15f,-3+walk*4),elapsed<8?65:62);
                    for(int i=0;i<10;i++){
                        var a=actors[i];a.SetActive(elapsed>4.65f);float x=(-10+(i/4)*9)+(i%2==0?-1:1);
                        var start=new Vector3(x,0,-4.9f);var end=new Vector3((i%2==0?-.55f:.55f),0,-1.3f-i*.35f);
                        // The two men from the right-hand car walk around Alex's parked motorcycle.
                        var via=new Vector3(6,0,-.7f);var destination=i<2?Vector3.Lerp(start,end,walk):walk<.45f?Vector3.Lerp(start,via,walk/.45f):Vector3.Lerp(via,end,(walk-.45f)/.55f);
                        a.transform.position=elapsed<5.6f?Vector3.Lerp(new Vector3(x,0,-5.65f),start,Mathf.Clamp01((elapsed-4.65f)/.95f)):destination;a.transform.rotation=Quaternion.LookRotation(i<2?end-start:walk<.45f?via-start:end-via);
                        var anim=a.GetComponentInChildren<Animation>();if(anim){anim.cullingType=AnimationCullingType.AlwaysAnimate;}if(anim && !anim.IsPlaying(elapsed>4.65f?"walk":"idle"))anim.CrossFade(elapsed>4.65f?"walk":"idle");
                    }
                    // Cut to the interior before anyone crosses the storefront glass.
                }
                yield return null;
            }
            for(int i=0;i<cars.Count;i++){cars[i].position=new Vector3(-10+i*9,-.1f,-6.5f);foreach(var door in cars[i].GetComponentsInChildren<Transform>())if(door.name=="Cửa xe chuyển động")door.localRotation=Quaternion.identity;}
            game.sound.Stop("arrival_engine");OpenEntrance(false);foreach(var a in actors)Destroy(a);actors.Clear();
            game.view.transform.SetPositionAndRotation(oldPosition,oldRotation);game.view.fieldOfView=oldFov;
            game.MarkStorySeen("arrival");Playing=false;PursuitStarted=true;game.LockCursor(true);
            for(int i=0;i<10;i++)SpawnPursuer(i);
            game.Objective("door06","Chạy tới kho phía sau! Lách qua các kệ và mở Cửa 006 để xuống hầm.");
            game.ui.Subtitle("ALEX","Chúng tới tìm mình… Cửa trước bị chặn rồi. Phải xuống hầm!");
        }
        void Shot(Vector3 position,Vector3 target,float fov){game.view.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position));game.view.fieldOfView=fov;}
        void OpenEntrance(bool open=true){
            foreach(var t in GetComponentsInChildren<Transform>(true))if(t.name=="Locked entrance glass" || t.name=="Entrance handle")t.gameObject.SetActive(!open);
        }
        void SpawnPursuer(int index){
            string route="arrival_"+index;
            Vector3[] points=index==0?new[]{new Vector3(-3,0,5),new Vector3(-3,0,20),new Vector3(-9,0,20.3f),new Vector3(-9,0,24),new Vector3(-9,0,27)}:
                index==1?new[]{new Vector3(3,0,5),new Vector3(3,0,21),new Vector3(0,0,29),new Vector3(11.5f,0,33.3f),new Vector3(-11.5f,0,35.8f),new Vector3(11.5f,0,38.4f),new Vector3(-11.5f,0,41),new Vector3(11.5f,0,43.5f),new Vector3(-11.5f,0,46),new Vector3(0,0,46.7f)}:
                index==2?new[]{new Vector3(-12,0,5),new Vector3(-12,0,19),new Vector3(-3,0,21),new Vector3(0,0,29)}:
                new[]{new Vector3(12,0,5),new Vector3(12,0,20),new Vector3(5,0,24),new Vector3(0,0,29)};
            for(int n=0;n<points.Length;n++){var p=new GameObject("Tuyến lục soát");p.transform.SetParent(transform);p.transform.position=points[n];var m=p.AddComponent<WorldMarker>();m.id="patrol_"+route+"_"+n;m.patrolWait=1;}
            var go=new GameObject("Kẻ truy bắt áo vest");go.transform.SetParent(transform);go.transform.position=new Vector3(index%2==0?-.65f:.65f,0,2+index*.8f);
            var enemy=go.AddComponent<EnemyController>();enemy.campaign=game;enemy.actorPrefab="BlackSuit";enemy.routeId=route;enemy.captureOnContact=true;enemy.speed=4.6f;enemy.Setup();game.enemies.Add(enemy);
        }
    }
}
