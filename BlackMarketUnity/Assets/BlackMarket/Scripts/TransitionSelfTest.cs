using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace BlackMarket {
 public class TransitionSelfTest:MonoBehaviour {
  Campaign g;readonly List<string> report=new List<string>();int failures;float deadline;bool done;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Launch(){if(Debug.isDebugBuild&&Array.IndexOf(Environment.GetCommandLineArgs(),"--transition-check")>=0)new GameObject("Transition QA").AddComponent<TransitionSelfTest>();}
  void Check(bool ok,string message){report.Add((ok?"PASS ":"FAIL ")+message);Debug.Log(report.Last());if(!ok)failures++;}
  IEnumerator Shot(string name){yield return null;yield return null;ScreenCapture.CaptureScreenshot("Documentation/TransitionPreview/"+name+".png");yield return null;yield return null;}
  IEnumerator Start(){
   deadline=Time.realtimeSinceStartup+150;Application.runInBackground=true;Directory.CreateDirectory("Documentation/TransitionPreview");yield return null;g=Campaign.Instance;
   g.NewGame();var intro=g.prologue;intro.StopAllCoroutines();intro.StartCoroutine((IEnumerator)typeof(PrologueSequence).GetMethod("Arrival",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(intro,null));
   while(intro.Phase!="walk")yield return null;bool outside=true;while(intro.Playing){var actor=g.transform.Find("Alex — diễn viên hoạt cảnh");if(actor)outside&=actor.position.z<=-1.29f;yield return null;}
   Check(outside,"Alex cuts before reaching storefront glass");Check(g.Running&&g.player.transform.position.z>0,"cut returns control with Alex inside");Check(g.level.GetComponentsInChildren<Interaction>(true).Single(x=>x.id=="frontdoor").title=="CỬA CHÍNH","front door prompt no longer says locked");yield return Shot("01-alex-inside");
   g.opening.Begin();bool groupOutside=true;float time=0;while(g.opening.Playing){foreach(var t in g.level.GetComponentsInChildren<Transform>())if(t.name.StartsWith("Thành viên nhóm mặc vest"))groupOutside&=t.position.z<-.65f;if(time<11&&time+Time.deltaTime>=11)yield return Shot("02-group-at-door");time+=Time.deltaTime;yield return null;}
   Check(groupOutside,"suited group stays outside until camera cut");Check(g.enemies.Count==4&&g.opening.PursuitStarted,"cut creates four pursuers inside");
   g.state.keycard=true;g.flags.Add("computer");var air=g.airlock;g.player.body.enabled=false;g.player.transform.position=air.transform.position-Vector3.forward;g.player.body.enabled=true;air.UnlockOuter();while(air.OuterMoving)yield return null;
   g.player.body.enabled=false;g.player.transform.position=air.transform.TransformPoint(new Vector3(0,0,1.9f));g.player.body.enabled=true;air.StartScan();while(!air.AutoDescending)yield return null;
   Check(!air.transform.Find("Cuối hành lang xuống hầm").gameObject.activeSelf,"end wall removed from descent shot");var portal=air.transform.Find("Lối tối xuống B1");Check(portal && !portal.GetComponent<Collider>(),"black passage exists without a blocking collider");
   while(air.transform.InverseTransformPoint(g.player.transform.position).z<10.6f)yield return null;Check(!g.Running,"descent remains automatic");yield return Shot("03-dark-stair-exit");var fade=air.GetComponent<DescentBlackout>();while(fade.opacity<.2f)yield return null;
   g.Pause();float opacity=fade.opacity;Vector3 position=g.player.transform.position;yield return new WaitForSecondsRealtime(.3f);Check(fade.opacity==opacity&&position==g.player.transform.position,"pause freezes descent and blackout");g.Resume();
   while(fade.opacity<1)yield return null;Check(g.state.stage==0&&!g.Running,"screen becomes black before loading map 2");yield return Shot("04-black-cut");var pixels=new Texture2D(2,2);pixels.LoadImage(File.ReadAllBytes("Documentation/TransitionPreview/04-black-cut.png"));Check(pixels.GetPixels32().All(p=>p.r<4&&p.g<4&&p.b<4),"black cut covers HUD as well as world");Destroy(pixels);while(g.state.stage==0)yield return null;yield return null;Check(g.Running&&g.state.stage==1,"map 2 restores control after black cut");Check(!g.GetComponentInChildren<DescentBlackout>(),"blackout does not persist on map 2");yield return Shot("05-map2");Finish();
  }
  void Update(){if(!done&&deadline>0&&Time.realtimeSinceStartup>deadline){Check(false,"timeout");Finish();}}
  void Finish(){done=true;report.Add("RESULT "+failures+" failures");File.WriteAllLines("Documentation/transition-test.txt",report);
#if UNITY_EDITOR
   UnityEditor.EditorApplication.Exit(failures==0?0:1);
#else
   Application.Quit(failures==0?0:1);
#endif
  }
 }
}
