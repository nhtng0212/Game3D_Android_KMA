using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace BlackMarket {
 [DefaultExecutionOrder(1000)]
 public class PrologueSelfTest:MonoBehaviour {
  Campaign g;List<string> report=new List<string>();int failures;float deadline;bool ending;Action afterPose;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Launch(){if(Debug.isDebugBuild&&Array.IndexOf(Environment.GetCommandLineArgs(),"--intro-check")>=0)new GameObject("Prologue QA").AddComponent<PrologueSelfTest>();}
  void Check(bool value,string text){report.Add((value?"PASS ":"FAIL ")+text);Debug.Log(report.Last());if(!value)failures++;}
  void LateUpdate(){var a=afterPose;afterPose=null;a?.Invoke();}
  IEnumerator PoseCheck(Action a){bool done=false;afterPose=()=>{a();done=true;};while(!done)yield return null;}
  IEnumerator Shot(string name){yield return null;yield return null;if(Application.isBatchMode){yield return PoseCheck(()=>{var rt=new RenderTexture(1280,720,24);g.view.targetTexture=rt;g.view.Render();RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes("Documentation/IntroPreview/"+name+".png",tex.EncodeToPNG());RenderTexture.active=null;g.view.targetTexture=null;Destroy(rt);Destroy(tex);});}else ScreenCapture.CaptureScreenshot("Documentation/IntroPreview/"+name+".png");yield return null;yield return null;}
  IEnumerator Start(){
   Application.runInBackground=true;if(UnityEngine.InputSystem.Keyboard.current!=null)UnityEngine.InputSystem.InputSystem.DisableDevice(UnityEngine.InputSystem.Keyboard.current);if(UnityEngine.InputSystem.Mouse.current!=null)UnityEngine.InputSystem.InputSystem.DisableDevice(UnityEngine.InputSystem.Mouse.current);deadline=Time.realtimeSinceStartup+240;Directory.CreateDirectory("Documentation/IntroPreview");File.WriteAllText(Application.isBatchMode?"Documentation/intro-runtime-test.txt":"Documentation/intro-ui-test.txt","RUNNING\n");yield return null;g=Campaign.Instance;File.Delete(g.SavePath+".reading");
   var documentStyle=new GUIStyle{font=Resources.Load<Font>("Fonts/Regular"),fontSize=23,wordWrap=true};
   foreach(var file in new[]{StoryData.Terminal,StoryData.BaseFile,StoryData.FinalFile})Check(file.Split(new[]{"\n\n"},StringSplitOptions.RemoveEmptyEntries).All(page=>documentStyle.CalcHeight(new GUIContent(page+"▌"),1030)<=275),"all document pages fit computer text area");
   var phoneStyle=new GUIStyle{font=Resources.Load<Font>("Fonts/Regular"),fontSize=22,wordWrap=true};Check(PhoneDialogue.Nodes.Values.All(node=>phoneStyle.CalcHeight(new GUIContent(node.text),1100)<=90),"all phone dialogue fits subtitle area");
   var reached=new HashSet<string>();Action<string> visit=null;visit=id=>{if(id=="end"||!reached.Add(id))return;Check(PhoneDialogue.Nodes.ContainsKey(id),"dialogue node exists: "+id);if(!PhoneDialogue.Nodes.ContainsKey(id))return;foreach(var c in PhoneDialogue.Nodes[id].choices)visit(c.next);};visit("start");Check(reached.Count==PhoneDialogue.Nodes.Count,"all dialogue nodes reachable");Check(PhoneDialogue.Nodes["start"].choices.Length==3,"three real conversation branches");
   var state=new CampaignSave();foreach(string e in new[]{"card","hunters","promise"})PhoneDialogue.Apply(state,e);Check(state.askedAboutCard&&state.askedAboutHunters&&state.promisedMarcus,"branch effects independent");
   g.NewGame();Check(g.prologue.Playing&&!g.Running&&!g.prologue.CanSkip,"first viewing locks controls and skip");g.prologue.Skip();Check(g.prologue.Playing,"premature skip rejected");yield return new WaitForSeconds(1);yield return Shot("01-bedroom");
   while(g.prologue.Node==null)yield return null;var intro=g.prologue;Check(intro.NodeId=="start"&&!intro.Ready,"phone opens first node with gated options");intro.Choose(0);Check(intro.NodeId=="start","early choice ignored");
   g.Pause();float read=intro.ReadTime;Vector3 camera=g.view.transform.position;yield return new WaitForSecondsRealtime(.4f);Check(intro.ReadTime==read&&g.view.transform.position==camera,"pause freezes dialogue and camera");g.Resume();Check(!g.Running&&Cursor.lockState!=CursorLockMode.Locked,"resume keeps cinematic control lock and pointer");
   while(intro.ReadTime<intro.Node.text.Length/42f)yield return null;Check(!intro.Ready,"options remain locked when text ends");yield return new WaitForSeconds(2.6f);Check(!intro.Ready,"three-second reading delay enforced");while(!intro.Ready)yield return null;yield return Shot("02-phone-choices");
   // Real choice click on the IMGUI surface when a visible Game view exists.
   if(!Application.isBatchMode){g.ui.ProcessMenuPointer(new Vector2(220,600),true,false);g.ui.ProcessMenuPointer(new Vector2(220,600),false,true);yield return null;yield return null;Check(intro.NodeId=="danger","UI click selects branch");}else intro.Choose(0);
   while(intro.Node!=null){while(!intro.Ready)yield return null;int index=intro.NodeId=="danger"?1:0;intro.Choose(index);yield return null;}
   Check(g.state.askedAboutCard&&g.state.askedAboutHunters&&g.state.promisedMarcus,"chosen path recorded on campaign state");
   while(g.ui.modal=="story"){g.ui.ScrollStory(100);if(g.ui.StoryReady)g.ui.ContinueStory();yield return null;}
   Check(intro.Phase=="ride"&&!g.Running,"news leads to motorcycle cinematic");yield return new WaitForSeconds(4);yield return PoseCheck(()=>{var actor=g.prologue.GetComponentInChildren<IntroActorPose>();var bones=actor.GetComponentsInChildren<Transform>();Check(Vector3.Distance(bones.First(t=>t.name=="Bip01 L Hand").position,actor.leftGrip.position)<.04f,"left hand grips motorcycle handlebar");Check(Vector3.Distance(bones.First(t=>t.name=="Bip01 R Hand").position,actor.rightGrip.position)<.04f,"right hand grips motorcycle handlebar");});yield return Shot("03-motorcycle-arrival");
   var bike=g.level.transform.Find("Xe máy Alex đã đỗ");Check(bike.InverseTransformPoint(bike.Find("Bánh xe trước").position).z>bike.InverseTransformPoint(bike.Find("Bánh xe sau").position).z,"motorcycle front wheel faces direction of travel");g.Pause();var pos=bike.position;yield return new WaitForSecondsRealtime(.3f);Check(pos==bike.position,"pause freezes motorcycle");g.Resume();
   while(intro.Phase!="dismount")yield return null;yield return new WaitForSeconds(.8f);yield return Shot("04-dismount");while(intro.Phase!="walk")yield return null;Check(!g.Running,"automatic walk locks movement");yield return new WaitForSeconds(2);yield return Shot("05-walk-to-shop");while(intro.Playing)yield return null;
   Check(g.Running&&g.state.stage==0&&g.objectiveId=="drawers","control returns to card search");Check(g.state.introCompleted&&g.HasSeenStory("alex-intro"),"intro completion and replay entitlement saved");Check(bike.gameObject.activeSelf&&Vector3.Distance(bike.position,PrologueSequence.ParkPosition)<.01f,"parked bike remains for villain recognition");Check(g.journal.Count>=4,"dialogue hints added to journal");
   g.Continue();yield return null;Check(!g.prologue&&g.Running&&g.state.promisedMarcus&&g.state.askedAboutCard,"continue restores choices without replaying completed intro");
   g.NewGame();Check(g.prologue.CanSkip,"new game can skip previously completed intro");g.prologue.Skip();yield return null;Check(g.Running&&g.level.GetComponentsInChildren<Transform>().Count(t=>t.name=="Xe máy Alex đã đỗ")==1,"skip leaves one bike and restores control");Check(!g.state.promisedMarcus&&!g.state.askedAboutCard,"new game clears previous choices");
   var p=g.player;p.crouch=true;yield return new WaitForSeconds(.4f);yield return PoseCheck(()=>{foreach(string side in new[]{"L","R"}){var bones=p.visual.GetComponentsInChildren<Transform>();var shoulder=bones.First(t=>t.name=="Bip01 "+side+" UpperArm");var elbow=bones.First(t=>t.name=="Bip01 "+side+" Forearm");var hand=bones.First(t=>t.name=="Bip01 "+side+" Hand");float angle=Vector3.Angle(shoulder.position-elbow.position,hand.position-elbow.position);Check(angle>35&&angle<145,"crouch elbow bent "+side+" "+angle.ToString("F1"));Check(hand.position.y>p.transform.position.y+.75f,"crouch hand raised "+side);}});
   p.enabled=false;g.view.transform.position=p.transform.position+new Vector3(2.4f,1.5f,2);g.view.transform.LookAt(p.transform.position+Vector3.up*.8f);yield return Shot("06-crouch");p.enabled=true;
   g.state.armed=true;g.state.hasAK=true;p.SwitchWeapon(1);yield return new WaitForSeconds(.4f);Check(p.visual.GetComponent<WeaponPose>().enabled,"armed crouch preserves weapon posing");g.MarkStorySeen("arrival");g.opening.Begin();yield return new WaitForSeconds(7);Check(g.opening.Caption.Contains("biển số xe"),"villains identify Alex from parked motorcycle");yield return Shot("07-vest-group-bike");g.opening.Skip();yield return null;yield return null;Check(g.opening.PursuitStarted&&g.enemies.Count==10,"convoy still starts pursuit after new intro");g.LoadWorld(1);yield return null;Check(g.Running&&!g.prologue&&g.state.stage==1,"stage transition clears intro");Finish();
  }
  void Update(){if(!ending&&deadline>0&&Time.realtimeSinceStartup>deadline){Check(false,"test deadline exceeded");Finish();}}
  void Finish(){ending=true;report.Add("RESULT "+failures+" failures");File.WriteAllLines(Application.isBatchMode?"Documentation/intro-runtime-test.txt":"Documentation/intro-ui-test.txt",report);
#if UNITY_EDITOR
   UnityEditor.EditorApplication.Exit(failures==0?0:1);
#else
   Application.Quit(failures==0?0:1);
#endif
  }
 }
}
