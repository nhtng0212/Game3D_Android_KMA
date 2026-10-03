using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
namespace BlackMarket {
    // Explicit opt-in, only in a development player. Never changes a player's checkpoint.
    public class CampaignSelfTest : MonoBehaviour {
        Campaign g;readonly List<string> log=new List<string>();int failures;float deadline;bool captures;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartIfRequested(){if(Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(),"--self-test")>=0)new GameObject("Campaign QA").AddComponent<CampaignSelfTest>();}
        void Check(bool ok,string message){log.Add((ok?"PASS ":"FAIL ")+message);if(!ok)failures++;Debug.Log(log[log.Count-1]);}
        void Use(string id){var p=g.level.GetComponentsInChildren<Interaction>().FirstOrDefault(x=>x.id==id);Check(p!=null,"interaction exists "+id);if(p)p.Use();}
        IEnumerator Frame(){yield return null;yield return null;}
        IEnumerator Start(){
            Application.runInBackground=true;deadline=Time.realtimeSinceStartup+150;captures=Array.IndexOf(Environment.GetCommandLineArgs(),"--capture")>=0;
            yield return null;g=Campaign.Instance;Check(g!=null,"campaign starts");if(!g){Finish();yield break;}
            Check(g.ui.modal=="menu","menu on boot");yield return Capture("menu");
            g.NewGame();Check(g.ui.modal=="story","prologue");g.ui.ContinueStory();yield return Frame();
            Check(g.state.stage==0 && g.Running,"chapter 1 starts");
            yield return WalkTo(new Vector3(-6.75f,0,15.4f),"parking to Marcus office");
            Use("door06");Check(g.state.stage==0,"Door 06 gate");Use("keycard");Check(g.state.keycard,"keycard");Use("computer");g.ui.ContinueStory();
            yield return Capture("office");Use("door06");g.ui.ContinueStory();yield return Frame();
            Check(g.state.stage==1 && !g.state.armed,"unarmed stealth");
            Use("door06");Check(g.state.stage==1,"radio gate");Use("radio");Use("door06");g.ui.ContinueStory();yield return Frame();
            Check(g.state.stage==2,"biometric descent");Use("pistol");Check(g.state.armed,"pistol unlock");g.state.ammo=0;g.state.reserve=12;g.player.Reload();yield return new WaitForSeconds(1.7f);Check(g.state.ammo==8 && g.state.reserve==4,"reload transfer");
            var dummy=new GameObject("QA target");dummy.transform.SetParent(g.level.transform);dummy.transform.position=new Vector3(0,0,8);var target=dummy.AddComponent<EnemyController>();target.campaign=g;target.Setup();target.enabled=false;g.player.camera.transform.LookAt(dummy.transform.position+Vector3.up*1.2f);g.player.Fire();Check(target.hp==75 && g.state.ammo==7,"hitscan damages visible target and consumes one round");Destroy(dummy);yield return Frame();

            yield return WalkTo(new Vector3(6,0,20.5f),"bunker room B reachable");Use("order");Use("recording");g.ui.ContinueStory();yield return Frame();
            Check(g.state.stage==3 && g.state.tablet,"tablet unlock");g.security.Toggle();g.security.Alarm();g.security.Door();Check(g.flags.Contains("security_done"),"security tutorial");Check(g.security.locked,"physical shutter closes");yield return Capture("tablet");g.security.Door();g.security.Close();
            Use("exit");yield return Frame();Check(g.state.stage==4,"storage chapter");g.security.Toggle();g.security.Light();g.security.Door();g.security.Close();Check(g.security.dark,"darkness");Use("exit");yield return Frame();
            Check(g.state.stage==5,"evidence chapter");Use("upload");Check(g.enemies.Count==3,"bounded purge encounter");Use("exit");Check(g.state.stage==5,"upload exit gated");foreach(var e in g.enemies)e.enabled=false;g.upload=.05f;yield return new WaitForSeconds(.15f);Use("exit");yield return Frame();
            Check(g.state.stage==6,"Victor chapter");var boss=g.enemies.First(x=>x.boss);boss.enabled=false;boss.TakeDamage(100);Check(boss.phase==2 && boss.shielded,"Victor phase 2 revokes security");boss.TakeDamage(25);Check(boss.hp==200,"Victor immune before override");Use("override");Check(!boss.shielded && !g.security.revoked,"local override");boss.TakeDamage(100);Check(boss.phase==3,"Victor phase 3");g.security.Toggle();g.security.Light();Check(boss.stun>0,"EMP");g.security.Close();yield return Capture("boss");boss.TakeDamage(100);Use("final");Check(g.ui.modal=="choice","final choice");g.ui.Ending(false);Check(!g.active && g.ui.modal=="story","destroy ending");yield return Capture("ending-destroy");g.ui.Ending(true);Check(!g.active,"accept ending");g.Continue();yield return Frame();Check(g.state.stage==6 && g.enemies.First(x=>x.boss).hp==300,"retry restores full encounter");g.Damage(1000);Check(g.state.hp==0 && g.ui.modal=="death","death");g.Continue();yield return Frame();Check(g.state.hp>0 && g.Running,"retry after death");g.Pause();Check(g.paused,"pause");g.Resume();Check(g.Running,"resume");Finish();
        }
        IEnumerator WalkTo(Vector3 goal,string description){
            var p=g.player;foreach(var e in g.enemies)e.enabled=false;p.enabled=false;
            var path=new NavMeshPath();NavMesh.SamplePosition(p.transform.position,out var from,2,NavMesh.AllAreas);NavMesh.SamplePosition(goal,out var to,2,NavMesh.AllAreas);
            bool found=NavMesh.CalculatePath(from.position,to.position,NavMesh.AllAreas,path);Check(found && path.status==NavMeshPathStatus.PathComplete,"navigation "+description);
            foreach(var corner in path.corners){float time=0;while(Vector3.Distance(new Vector3(p.transform.position.x,corner.y,p.transform.position.z),corner)>.3f && time<12){var d=corner-p.transform.position;d.y=0;p.body.Move((d.normalized*5+Vector3.down*2)*Time.deltaTime);time+=Time.deltaTime;yield return null;}}
            Check(Vector2.Distance(new Vector2(p.transform.position.x,p.transform.position.z),new Vector2(goal.x,goal.z))<1.2f,"collision traversal "+description);p.enabled=true;
        }
        IEnumerator Capture(string name){if(!captures)yield break;yield return new WaitForEndOfFrame();Directory.CreateDirectory("Documentation/Playtest");ScreenCapture.CaptureScreenshot(Path.GetFullPath("Documentation/Playtest/"+name+".png"));yield return null;}
        void Update(){if(deadline>0 && Time.realtimeSinceStartup>deadline){Check(false,"self-test timed out");Finish();}}
        void Finish(){Directory.CreateDirectory("Documentation");log.Add("RESULT: "+(failures==0?"PASS":"FAIL")+" / "+failures+" failures");File.WriteAllLines("Documentation/runtime-test.txt",log);Application.Quit(failures==0?0:1);enabled=false;}
    }
}
