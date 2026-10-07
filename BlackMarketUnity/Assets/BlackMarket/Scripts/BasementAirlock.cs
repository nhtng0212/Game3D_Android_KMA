using System.Collections;
using UnityEngine;
using UnityEngine.AI;
namespace BlackMarket {
    public class BasementAirlock : MonoBehaviour {
        public Transform outerLeaf,innerLeaf,scanBeam;
        public Renderer indicator;
        public Transform[] descentPath;
        public bool AutoDescending {get;private set;}
        public bool OuterUnlocked { get; private set; }
        public bool OuterMoving { get; private set; }
        public bool Scanning { get; private set; }
        public bool FaceVerified { get; private set; }
        public float ScanProgress { get; private set; }
        public string Status { get; private set; }
        Vector3 outerClosed,innerClosed;
        Campaign game=>Campaign.Instance;
        void Awake(){outerClosed=outerLeaf.localPosition;innerClosed=innerLeaf.localPosition;foreach(var obstacle in GetComponentsInChildren<NavMeshObstacle>())obstacle.enabled=true;scanBeam.gameObject.SetActive(false);}
        public void UnlockOuter(){
            if(!game.Running || OuterMoving || OuterUnlocked)return;
            if(!game.state.keycard){game.ui.Toast("Cửa 006 cần thẻ đỏ của Marcus.");return;}
            if(!game.flags.Contains("computer") || !game.opening.PursuitStarted){game.ui.Toast("Trước hết hãy mở tệp Order 71 trên máy tính của Marcus.");return;}
            StartCoroutine(OpenOuter());
        }
        IEnumerator OpenOuter(){
            OuterUnlocked=true;OuterMoving=true;game.sound.Play("door_move",.5f,outerLeaf.position);
            game.Objective("face_scan","Qua Cửa 006, đứng trên dấu chân trước lớp cửa trong để quét khuôn mặt.");
            while(Vector3.Distance(outerLeaf.localPosition,outerClosed+Vector3.up*2.85f)>.01f){if(game.Running)outerLeaf.localPosition=Vector3.MoveTowards(outerLeaf.localPosition,outerClosed+Vector3.up*2.85f,Time.deltaTime*2.5f);yield return null;}
            OuterMoving=false;game.ui.Subtitle("HỆ THỐNG","Thẻ đỏ hợp lệ. Lớp cửa trong yêu cầu xác thực khuôn mặt.");
        }
        public void StartScan(){
            if(!game.Running || !OuterUnlocked || OuterMoving || Scanning || FaceVerified)return;
            var local=transform.InverseTransformPoint(game.player.transform.position);
            if(Mathf.Abs(local.x)>1.05f || local.z<1.05f || local.z>2.8f){game.ui.Toast("Bước vào khoang, đứng trên dấu chân trước máy quét mặt.");return;}
            StartCoroutine(Scan());
        }
        IEnumerator Scan(){
            Scanning=true;ScanProgress=0;Status="ĐANG KHÓA LỚP CỬA NGOÀI";game.player.ResetTouch();game.LockCursor(false);
            var player=game.player;var cam=game.view;
            player.crouch=false;player.body.enabled=false;
            var animation=player.visual.GetComponentInChildren<Animation>();if(animation)animation.CrossFade("idle",.15f);
            // Keep the lens inside the vestibule, even if E was pressed close to the inner gate.
            var stand=transform.TransformPoint(new Vector3(0,0,1.65f));
            player.transform.position=stand;
            player.visual.rotation=transform.rotation;
            var cameraData=cam.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();bool post=cameraData.renderPostProcessing;cameraData.renderPostProcessing=false;
            var lampObject=new GameObject("Đèn chiếu khuôn mặt");lampObject.transform.SetParent(cam.transform,false);var lamp=lampObject.AddComponent<Light>();lamp.type=LightType.Point;lamp.range=4;lamp.intensity=2.5f;
            SetScanCamera(cam);scanBeam.gameObject.SetActive(true);
            while(Vector3.Distance(outerLeaf.localPosition,outerClosed)>.01f){if(!game.paused)outerLeaf.localPosition=Vector3.MoveTowards(outerLeaf.localPosition,outerClosed,Time.deltaTime*3);yield return null;}
            game.sound.Play("door_latch",.35f,outerLeaf.position);Status="ĐANG QUÉT KHUÔN MẶT";
            while(ScanProgress<1){
                if(game.paused){yield return null;continue;}
                ScanProgress=Mathf.Min(1,ScanProgress+Time.deltaTime/3.2f);SetScanCamera(cam);
                scanBeam.position=player.transform.position+transform.forward*.28f+Vector3.up*(1.38f+Mathf.PingPong(ScanProgress*1.3f,.45f));yield return null;
            }
            FaceVerified=true;Status="ĐÃ XÁC NHẬN: ALEX CARTER";game.sound.Play("beep",.4f);scanBeam.gameObject.SetActive(false);
            var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",new Color(.15f,.85f,.45f));block.SetColor("_EmissionColor",new Color(.15f,.85f,.45f)*2);indicator.SetPropertyBlock(block);
            while(Vector3.Distance(innerLeaf.localPosition,innerClosed+Vector3.up*2.85f)>.01f){if(!game.paused)innerLeaf.localPosition=Vector3.MoveTowards(innerLeaf.localPosition,innerClosed+Vector3.up*2.85f,Time.deltaTime*2);yield return null;}
            Destroy(lampObject);cameraData.renderPostProcessing=post;AutoDescending=true;Status="ALEX ĐANG CHẠY XUỐNG TẦNG HẦM";
            game.Objective("basement_exit","Đã xác thực. Alex đang tự chạy xuống cầu thang tới tầng hầm B1.");
            if(animation)animation.CrossFade("run",.2f);
            foreach(var waypoint in descentPath){
                while(Vector3.Distance(player.transform.position,waypoint.position)>.015f){
                    if(game.paused){yield return null;continue;}
                    var direction=waypoint.position-player.transform.position;var flat=Vector3.ProjectOnPlane(direction,Vector3.up);if(flat.sqrMagnitude>.001f)player.visual.rotation=Quaternion.Slerp(player.visual.rotation,Quaternion.LookRotation(flat),Time.deltaTime*10);
                    player.transform.position=Vector3.MoveTowards(player.transform.position,waypoint.position,Time.deltaTime*3.2f);
                    var target=player.transform.position+Vector3.up*1.35f;var eye=player.transform.position-transform.forward*2.1f+transform.right*.6f+Vector3.up*2.1f;
                    cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(target-eye));cam.fieldOfView=58;yield return null;
                }
            }
            game.Advance();
        }
        void SetScanCamera(Camera cam){
            var face=game.player.transform.position+Vector3.up*1.62f;
            var eye=transform.TransformPoint(new Vector3(.15f,1.7f,2.85f));
            cam.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(face-eye));cam.fieldOfView=42;cam.nearClipPlane=.05f;
        }
    }
}
