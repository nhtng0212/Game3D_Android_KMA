using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
namespace BlackMarket {
    public class OpeningSelfTest : MonoBehaviour {
        bool computerCaptured;Campaign game;readonly List<string> report=new List<string>();int failures;float deadline;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch(){if(Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(),"--opening-check")>=0)new GameObject("Kiểm tra mở đầu").AddComponent<OpeningSelfTest>();}
        void Check(bool value,string label){report.Add((value?"PASS ":"FAIL ")+label);Debug.Log(report.Last());if(!value)failures++;}
        IEnumerator Read(){while(game.ui.modal=="story"){
            if(game.ui.ComputerScreen){if(!computerCaptured && game.ui.TerminalPage==1 && game.ui.TerminalPageReady){computerCaptured=true;ScreenCapture.CaptureScreenshot("Documentation/OpeningPreview/00-computer.png");yield return null;yield return null;if(Array.IndexOf(Environment.GetCommandLineArgs(),"--opening-ui-only")>=0){Check(game.ui.ComputerScreen && game.ui.TerminalPageReady,"computer page ready for visual verification");Finish();yield break;}}if(game.ui.TerminalPageReady)game.ui.NextTerminalPage();}
            else {game.ui.ScrollStory(100);if(game.ui.StoryReady)game.ui.ContinueStory();}yield return null;
        }}
        IEnumerator Start(){
            Directory.CreateDirectory("Documentation/OpeningPreview");Application.runInBackground=true;deadline=Time.realtimeSinceStartup+330;yield return null;game=Campaign.Instance;
            Check(game!=null,"campaign loads");if(!game){Finish();yield break;}
            if(!Application.isBatchMode){game.ui.modal="help";yield return null;yield return null;ScreenCapture.CaptureScreenshot("Documentation/OpeningPreview/00-help.png");yield return null;yield return null;}
            File.Delete(game.SavePath+".reading");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--opening-ui-only")>=0){game.LoadWorld(0);game.state.keycard=true;Item("computer").Use();game.ui.OpenOrderFile();yield return Read();yield break;}
            game.NewGame();Check(game.prologue.Playing && !game.Running,"interactive prologue locks controls");MissionTestSteps.SkipIntro(game);yield return Read();
            Check(game.Running && game.state.stage==0,"prologue returns control");
            Check(game.HasSeenStory("alex-intro"),"reading completion persists separately");
            foreach(var d in game.level.GetComponentsInChildren<RoomDoor>())d.SetOpenImmediate(true);
            Teleport(new Vector3(-9,0,26));game.state.keycard=true;
            var terminal=Item("computer");terminal.Use();game.ui.OpenOrderFile();game.ui.NextTerminalPage();Check(game.ui.TerminalPage==0,"first terminal page cannot advance before text finishes");game.ui.ContinueStory();Check(game.ui.modal=="story" && !game.opening.Playing,"terminal must finish before convoy");yield return Read();
            Check(game.opening.Playing && !game.Running,"cutscene locks movement");game.opening.Skip();yield return null;Check(game.opening.Playing,"first arrival cannot skip");
            var point=game.player.transform.position;yield return new WaitForSeconds(5);
            Capture("01-convoy");game.Pause();var camera=game.view.transform.position;yield return new WaitForSecondsRealtime(.3f);Check(game.view.transform.position==camera,"pause freezes cinematic");game.Resume();
            Check(game.player.transform.position==point,"player remains in office during cinematic");
            yield return new WaitForSeconds(4);Capture("02-black-suits");
            while(game.opening.Playing)yield return null;
            Check(game.enemies.Count==4 && game.enemies.All(e=>e.captureOnContact && e.actorPrefab=="BlackSuit"),"four civilian suited pursuers");
            Check(game.Running && game.objectiveId=="door06","camera returns and escape objective starts");
            Check(game.HasSeenStory("arrival"),"arrival seen flag recorded");
            yield return Walk(Item("door06").transform.position);
            Check(game.state.hp>0,"office to basement escape is survivable");yield return null;yield return null;Capture("03-warehouse-escape");
            Item("door06").Use();yield return MissionTestSteps.PassAirlock(game);yield return null;Check(game.state.stage==1 && game.enemies.Count==4,"basement transition preserves B1");
            game.NewGame();Check(game.prologue.CanSkip,"completed prologue skippable on reset");game.prologue.Skip();yield return null;
            game.state.keycard=true;Item("computer").Use();game.ui.OpenOrderFile();Check(game.ui.StoryReady,"completed terminal skippable on reset");game.ui.SkipReadStory();game.opening.Skip();yield return null;yield return null;
            Check(game.opening.PursuitStarted && game.enemies.Count==4,"skipping cinematic preserves encounter");
            var e=game.enemies[0];var agent=e.GetComponent<NavMeshAgent>();Teleport(new Vector3(0,0,5));agent.Warp(new Vector3(0,0,4));
            foreach(var a in e.GetComponentsInChildren<Animation>())a.transform.rotation=Quaternion.identity;
            yield return new WaitForSecondsRealtime(4);Check(game.state.hp==0 && game.ui.modal=="death","detected player caught at contact dies");
            game.Continue();yield return null;Check(game.Running && game.state.hp>0 && !game.opening.PursuitStarted && game.enemies.Count==0,"death checkpoint resets encounter cleanly");
            Check(game.HasSeenStory("terminal071") && game.HasSeenStory("arrival"),"death reset keeps skip entitlement");
            game.state.keycard=true;Item("computer").Use();game.ui.OpenOrderFile();game.ui.SkipReadStory();game.opening.Skip();yield return null;yield return null;
            Teleport(new Vector3(-9,0,26));float until=Time.time+45;while(game.Running && Time.time<until)yield return null;
            Check(game.state.hp==0,"ignoring escape in office lets patrol find and capture player");
            if(game.state.hp>0)foreach(var enemy in game.enemies){var nav=enemy.GetComponent<NavMeshAgent>();Debug.Log("PURSUIT DIAGNOSTIC "+enemy.routeId+" "+enemy.transform.position+" "+enemy.mode+" "+nav.pathStatus+" destination "+nav.destination);}

            for(int stage=1;stage<=6;stage++){
                game.LoadWorld(stage);yield return null;yield return null;
                Check(game.level && game.level.name==StoryData.Worlds[stage] && game.player,"translated floor loads: "+stage);
                Check(!game.level.GetComponentsInChildren<TextMesh>().Any(t=>System.Text.RegularExpressions.Regex.IsMatch(t.text,@"\b(STAIRS|DOWN|ENCRYPTED|READY|WORKSHOP|KEEPER|ARMORY|UPLINK|CHECKOUT)\b")),"no untranslated direction labels: "+stage);
            }
            Finish();
        }
        Interaction Item(string id)=>game.level.GetComponentsInChildren<Interaction>().Single(i=>i.id==id);
        void Teleport(Vector3 p){game.player.body.enabled=false;game.player.transform.position=p;game.player.body.enabled=true;Physics.SyncTransforms();}
        IEnumerator Walk(Vector3 goal){
            foreach(var d in game.level.GetComponentsInChildren<RoomDoor>())d.SetOpenImmediate(true);yield return new WaitForSeconds(.3f);
            var p=game.player;var path=new NavMeshPath();bool ok=NavMesh.SamplePosition(p.transform.position,out var start,2,NavMesh.AllAreas)&&NavMesh.SamplePosition(goal,out var end,2,NavMesh.AllAreas)&&NavMesh.CalculatePath(start.position,end.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete;
            Check(ok,"narrow warehouse route exists");if(!ok)yield break;p.enabled=false;
            foreach(var corner in path.corners){float stop=Time.time+8;while(game.Running && Time.time<stop && Vector2.Distance(new Vector2(p.transform.position.x,p.transform.position.z),new Vector2(corner.x,corner.z))>.2f){Vector3 d=corner-p.transform.position;d.y=0;p.body.Move(Vector3.ClampMagnitude(d.normalized*5*Time.deltaTime,d.magnitude)+Vector3.down*8*Time.deltaTime);yield return null;}}
            Check(Vector3.Distance(p.transform.position,new Vector3(goal.x,p.transform.position.y,goal.z))<1.5f,"controller fits between stocked shelves");p.enabled=true;
        }
        void Capture(string name){Directory.CreateDirectory("Documentation/OpeningPreview");var cam=game.view;var rt=new RenderTexture(1280,720,24);cam.targetTexture=rt;cam.Render();var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1280,720,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1280,720),0,0);tex.Apply();File.WriteAllBytes("Documentation/OpeningPreview/"+name+".png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=old;Destroy(rt);Destroy(tex);}
        void Update(){if(deadline>0 && Time.realtimeSinceStartup>deadline){Check(false,"deadline");Finish();}}
        void Finish(){enabled=false;report.Add("RESULT: "+(failures==0?"PASS":"FAIL")+" / "+failures+" failures");File.WriteAllLines(Array.IndexOf(Environment.GetCommandLineArgs(),"--opening-ui-only")>=0?"Documentation/opening-ui-test.txt":"Documentation/opening-runtime-test.txt",report);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(failures==0?0:1);
#else
            Application.Quit(failures==0?0:1);
#endif
        }
    }
}
