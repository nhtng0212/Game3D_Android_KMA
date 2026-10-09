using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace BlackMarket {
    // Opt-in editor/development smoke test for the rebuilt shop; isolated from player saves.
    public class ShopSelfTest : MonoBehaviour {
        Campaign game;
        readonly List<string> report=new List<string>();
        int failures;
        float deadline;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Launch(){if(Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(),"--shop-check")>=0)new GameObject("Shop QA").AddComponent<ShopSelfTest>();}
        void Check(bool ok,string name){report.Add((ok?"PASS ":"FAIL ")+name);Debug.Log(report.Last());if(!ok)failures++;}
        IEnumerator ReadDocument(){
            MissionTestSteps.SkipIntro(game);
            if(game.ui.modal=="computer")game.ui.OpenOrderFile();
            while(game.ui.modal=="story"){
                if(game.ui.ComputerScreen){if(game.ui.TerminalPageReady)game.ui.NextTerminalPage();}
                else {game.ui.ScrollStory(100);if(game.ui.StoryReady)game.ui.ContinueStory();}
                yield return null;
            }
            while(game.opening && game.opening.Playing)yield return null;
        }
        IEnumerator Start(){
            deadline=Time.realtimeSinceStartup+300;Application.runInBackground=true;
            yield return null;game=Campaign.Instance;
            Check(game!=null,"campaign boot");if(!game){Finish();yield break;}
            game.NewGame();yield return ReadDocument();yield return null;yield return null;
            Check(game.Running && game.state.stage==0,"new game loads replacement shop");
            Check(game.player.transform.position.z>0,"player starts inside locked frontage");
            Check(game.level.GetComponentsInChildren<RoomDoor>().Length==4,"office, warehouse and two separate WC doors");
            var player=game.player;player.enabled=false;
            foreach(var door in game.level.GetComponentsInChildren<RoomDoor>()){
                var at=door.transform.position;
                Check(Physics.Linecast(at+new Vector3(0,1,-.45f),at+new Vector3(0,1,.45f)),"closed door blocks sight: "+door.roomName);
                door.SetOpenImmediate(true);
                Check(!Physics.Linecast(at+new Vector3(0,1,-.45f),at+new Vector3(0,1,.45f)),"open door clears opening: "+door.roomName);
            }
            yield return new WaitForSeconds(.4f);
            foreach(string id in new[]{"wc_men","wc_women","warehouse_visit","office_visit"})yield return Walk(game.Marker(id).position,id);
            yield return Walk(new Vector3(0,0,2),"return to entrance");
            var note=Item("note");yield return Walk(note.transform.position,"checkout note");player.FindTarget();Check(player.target==note,"checkout note reachable by normal E interaction");player.Interact();Check(game.ui.modal=="story","checkout note opens story");yield return ReadDocument();
            Item("door06").Use();Check(game.state.stage==0 && game.ui.modal=="","Door 06 remains gated before terminal");
            yield return MissionTestSteps.RevealDrawers(game);var key=Item("keycard");yield return Walk(key.transform.position,"Marcus keycard");player.FindTarget();Check(player.target==key,"keycard selectable by proximity and line of sight");player.Interact();Check(game.state.keycard,"keycard collected");
            var terminal=Item("computer");yield return Walk(terminal.transform.position,"Marcus terminal");player.FindTarget();Check(player.target==terminal,"terminal selectable by normal E interaction");player.Interact();Check(game.ui.modal=="computer","Order 71 desktop progression");yield return ReadDocument();
            var exit=Item("door06");yield return Walk(exit.transform.position,"warehouse Door 06");player.FindTarget();Check(player.target==exit,"Door 06 selectable at warehouse rear");player.Interact();yield return MissionTestSteps.PassAirlock(game);Check(game.state.stage==1,"Door 06 advances after escape");yield return ReadDocument();yield return null;yield return null;
            Check(game.state.stage==1 && game.Running && game.enemies.Count==4,"shop advances to existing B1 encounter");
            Finish();
        }
        Interaction Item(string id)=>game.level.GetComponentsInChildren<Interaction>().Single(i=>i.id==id);
        IEnumerator Walk(Vector3 goal,string name){
            var p=game.player;var path=new NavMeshPath();
            bool found=NavMesh.SamplePosition(p.transform.position,out var from,2,NavMesh.AllAreas) && NavMesh.SamplePosition(goal,out var to,2,NavMesh.AllAreas) && NavMesh.CalculatePath(from.position,to.position,NavMesh.AllAreas,path) && path.status==NavMeshPathStatus.PathComplete;
            Check(found,"navigation: "+name);if(!found)yield break;
            foreach(var corner in path.corners){float end=Time.time+12;while(Time.time<end && Vector2.Distance(new Vector2(p.transform.position.x,p.transform.position.z),new Vector2(corner.x,corner.z))>.2f){var delta=corner-p.transform.position;delta.y=0;p.body.Move((delta.normalized*6+Vector3.down*8)*Time.deltaTime);yield return null;}}
            Check(Vector2.Distance(new Vector2(p.transform.position.x,p.transform.position.z),new Vector2(goal.x,goal.z))<1.5f,"CharacterController traversal: "+name);
            Check(p.transform.position.y>=-.05f,"floor supports player: "+name);
        }
        void Update(){if(deadline>0 && Time.realtimeSinceStartup>deadline){Check(false,"shop test deadline");Finish();}}
        void Finish(){enabled=false;report.Add("RESULT: "+(failures==0?"PASS":"FAIL")+" / "+failures+" failures");File.WriteAllLines("Documentation/shop-runtime-test.txt",report);
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(failures==0?0:1);
#else
            Application.Quit(failures==0?0:1);
#endif
        }
    }
}
