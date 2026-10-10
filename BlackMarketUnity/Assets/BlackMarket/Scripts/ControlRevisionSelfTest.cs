using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
namespace BlackMarket {
 public class ControlRevisionSelfTest:MonoBehaviour {
  Campaign g;int failures;readonly List<string> results=new List<string>();float deadline;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Launch(){if((Array.IndexOf(Environment.GetCommandLineArgs(),"--control-revision-check")>=0||Array.IndexOf(Environment.GetCommandLineArgs(),"--ending-review-check")>=0))new GameObject("Control QA").AddComponent<ControlRevisionSelfTest>();}
  void Check(bool ok,string text){results.Add((ok?"PASS ":"FAIL ")+text);Debug.Log(results.Last());if(!ok)failures++;}
  IEnumerator Start(){Application.runInBackground=true;deadline=Time.realtimeSinceStartup+300;if(Keyboard.current!=null)InputSystem.DisableDevice(Keyboard.current);if(Mouse.current!=null)InputSystem.DisableDevice(Mouse.current);yield return null;
   g=Campaign.Instance;if(Array.IndexOf(Environment.GetCommandLineArgs(),"--ending-review-check")>=0){yield return EndingReview();Finish();yield break;}g.state=new CampaignSave{stage=2,keycard=true,introCompleted=true,armed=true,hasAK=true,hasSniper=true,hasNightVision=true,selectedWeapon=1,hp=50,rifleAmmo=30,rifleReserve=210};g.LoadWorld(2);yield return null;
   var all=g.level.GetComponentsInChildren<Transform>();Check(all.Count(t=>t.name=="Bàn điều phối vòng cung")==21,"twenty one arc workstations");Check(all.Count(t=>t.parent==g.level.transform&&t.name.StartsWith("Desktop computer"))==22,"twenty two desktop computers including command desk");Check(!all.Any(t=>t.name.Contains("Vách phòng trực")||t.name.Contains("Trạm sơ cứu")||t.name.Contains("Tủ kính đêm")||t.name.Contains("Laptop")),"no waiting room or laptops");Check(all.Count(t=>t.name=="Thùng tài liệu niêm phong")==60,"sixty archive boxes on additional storage racks");Check(all.Count(t=>t.name=="Bậc thang B3")==16,"actual descent stairs at entry");
   foreach(var id in new[]{"breaker","control_entry","final_terminal","escape"}){var item=g.level.GetComponentsInChildren<Interaction>().First(i=>i.id==id);NavMesh.SamplePosition(item.transform.position,out var end,3,NavMesh.AllAreas);var path=new NavMeshPath();Check(NavMesh.CalculatePath(g.Marker("spawn").position,end.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"reachable "+id);}
   while(g.underground.Playing)yield return null;Check(g.player.transform.position.z>42,"Alex arrives at central screen automatically");Check(g.underground.State==UndergroundCampaign.Encounter.FinalBattle&&g.underground.Available("breaker"),"Victor arrival immediately enables breaker objective");Check(g.enemies.Any(e=>e.boss&&e.maxHp==1350),"Victor has triple final battle health");var cabinet=g.level.GetComponentInChildren<LightingBreaker>();Check(Mathf.Abs(cabinet.transform.position.z-51.45f)<.01f,"breaker mounted against rear wall");
   var cam=new GameObject("Review camera").AddComponent<Camera>();cam.transform.SetPositionAndRotation(new Vector3(0,4.5f,9),Quaternion.LookRotation(new Vector3(0,-2.9f,30)));g.view.enabled=false;yield return null;Capture(cam,"Documentation/control-room-preview.png");yield return null;yield return null;cam.enabled=false;g.view.enabled=true;
   var boss=g.enemies.First(e=>e.boss);boss.GetComponent<NavMeshAgent>().Warp(new Vector3(0,0,30));g.level.GetComponentInChildren<LightingBreaker>().Hit(100);Check(g.underground.Dark,"shooting breaker extinguishes lights");g.player.nightVision.Toggle();Check(g.player.nightVision.Active,"night vision toggles with map 2 equipment");
   float until=Time.time+22;bool grenade=false;float closest=100;while(Time.time<until){g.state.hp=100;grenade|=g.level.GetComponentsInChildren<HostileGrenade>().Length>0;closest=Mathf.Min(closest,Vector3.Distance(boss.transform.position,g.player.transform.position));yield return null;}Check(grenade,"Victor throws recurring warning grenades");Check(closest>7,"Victor keeps combat distance instead of rushing into player");
   float before=g.state.hp;for(int i=0;i<30;i++)g.EnemyShot(8);Check(before-g.state.hp<=6,"map 3 shares anti burst damage limit");
   float endBattle=Time.time+30;while(g.underground.State==UndergroundCampaign.Encounter.FinalBattle&&Time.time<endBattle){g.state.hp=100;foreach(var e in g.enemies.ToArray())if(e&&e.hp>0)e.TakeDamage(1000);yield return null;}
   Check(g.underground.State==UndergroundCampaign.Encounter.Decrypt&&g.underground.Available("final_terminal"),"battle can finish and unlock final story terminal");
   g.underground.Interact(g.level.GetComponentsInChildren<Interaction>().First(i=>i.id=="final_terminal"));
   Check(g.ui.modal=="decoder"&&g.state.codeRecovered,"USB terminal recovers the ten digit code");
   g.underground.ReadFinal();Check(g.ui.ComputerScreen,"final evidence opens on computer screen");
   yield return new WaitForSecondsRealtime(1);Check(g.underground.State==UndergroundCampaign.Encounter.Decrypt&&g.underground.EscapeRemaining==0,"destruction countdown does not run during evidence reading");
   while(g.ui.modal=="story"){if(g.ui.TerminalPageReady)g.ui.NextTerminalPage();yield return null;}
   Check(g.state.evidenceCopied&&g.underground.State==UndergroundCampaign.Encounter.Escape&&g.underground.EscapeRemaining>89,"reading all pages copies evidence and starts ninety second escape");
   var escape=g.level.GetComponentsInChildren<Interaction>().First(i=>i.id=="escape");Check(g.underground.Available("escape"),"maintenance exit unlocks after evidence transmission");
   Check(g.Running&&escape.transform.position.z<3,"player controls return route to entry staircase");
   g.player.body.enabled=false;g.player.transform.position=new Vector3(0,0,2);g.player.body.enabled=true;yield return null;yield return null;Check(g.underground.Playing&&!g.Running,"approaching entry staircase automatically starts ending");
   yield return new WaitForSeconds(1);Check(DescentBlackout.Visible,"ending transition fades gradually to black");
   yield return new WaitForSeconds(2);var neighborhood=GameObject.Find("Khu phố North Point / cảnh kết");Check(neighborhood&&neighborhood.GetComponentsInChildren<Light>().Count(l=>l.name=="Ánh sáng đèn đường")==14,"ending exterior has neighborhood and fourteen streetlights");
   Capture(g.view,"Documentation/ending-neighborhood-preview.png");
   while(g.underground.Playing&&g.underground.Caption!="NORTH POINT / QUY TRÌNH TIÊU HỦY")yield return null;yield return null;Capture(g.view,"Documentation/ending-explosion-neighborhood.png");
   while(g.underground.Playing)yield return null;
   Check(g.state.campaignComplete&&g.state.checkpoint==5&&g.ui.modal=="story","motorcycle departure and explosion reach saved canonical ending");
   yield return null;Check(!GameObject.Find("North Point / hoạt cảnh kết"),"ending cleans up exterior set");Finish();
  }
  IEnumerator EndingReview(){
   g.state=new CampaignSave{stage=2,checkpoint=4,keycard=true,codeRecovered=true,armed=true,hasAK=true,hasNightVision=true,hp=100};g.LoadWorld(2);yield return null;
   g.MarkStorySeen("final-file-v2");g.underground.ReadFinal();while(g.ui.modal=="story"){if(g.ui.TerminalPageReady)g.ui.NextTerminalPage();yield return null;}
   Check(g.Running,"reading returns player control before escape");g.player.body.enabled=false;g.player.transform.position=new Vector3(0,0,2);g.player.body.enabled=true;yield return null;yield return null;yield return new WaitForSeconds(1);Check(DescentBlackout.Visible,"exit uses gradual fade");
   while(g.underground.Playing&&g.underground.Caption!="NORTH POINT / QUY TRÌNH TIÊU HỦY")yield return null;yield return null;
   var neighborhood=GameObject.Find("Khu phố North Point / cảnh kết");Check(neighborhood&&neighborhood.GetComponentsInChildren<Transform>().Count(t=>t.name.StartsWith("Nhà phố "))==28,"twenty eight detailed neighborhood buildings");Check(neighborhood&&neighborhood.GetComponentsInChildren<Light>().Count(l=>l.name=="Ánh sáng đèn đường")==14,"fourteen streetlights");
   Capture(g.view,"Documentation/ending-explosion-neighborhood.png");while(g.underground.Playing)yield return null;Check(g.state.campaignComplete,"revised ending completes");
  }
  public static void Capture(Camera camera,string path){var old=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var img=new Texture2D(1280,720,TextureFormat.RGB24,false);img.ReadPixels(new Rect(0,0,1280,720),0,0);img.Apply();File.WriteAllBytes(path,img.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=active;Destroy(img);Destroy(rt);}
  void Update(){if(deadline>0&&Time.realtimeSinceStartup>deadline){Check(false,"timeout");Finish();}}
  void Finish(){deadline=0;results.Add("RESULT "+failures+" failures");File.WriteAllLines(Array.IndexOf(Environment.GetCommandLineArgs(),"--ending-review-check")>=0?"Documentation/ending-review-test.txt":"Documentation/control-revision-test.txt",results);
#if UNITY_EDITOR
   UnityEditor.EditorApplication.Exit(failures==0?0:1);
#endif
  }
 }
}
