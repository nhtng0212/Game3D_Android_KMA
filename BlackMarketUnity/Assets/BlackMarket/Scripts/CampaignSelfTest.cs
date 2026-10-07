using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
namespace BlackMarket {
    // Explicit opt-in, only in the Editor or a development player. Never changes a player's checkpoint.
    [DefaultExecutionOrder(1000)]
    public partial class CampaignSelfTest : MonoBehaviour {
        string ReportPath=>Array.IndexOf(Environment.GetCommandLineArgs(),"--controls-check")>=0?"Documentation/controls-test.txt":"Documentation/runtime-test.txt";
        Campaign g;readonly List<string> log=new List<string>();int failures;float deadline;bool captures;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void StartIfRequested(){if(Debug.isDebugBuild && Array.IndexOf(Environment.GetCommandLineArgs(),"--self-test")>=0)new GameObject("Campaign QA").AddComponent<CampaignSelfTest>();}
        void Check(bool ok,string message){log.Add((ok?"PASS ":"FAIL ")+message);if(!ok)failures++;Debug.Log(log[log.Count-1]);}
        void Use(string id){var p=g.level.GetComponentsInChildren<Interaction>().FirstOrDefault(x=>x.id==id);Check(p!=null,"interaction exists "+id);if(p)p.Use();}
        Action afterPose;
        void LateUpdate(){var callback=afterPose;afterPose=null;callback?.Invoke();}
        IEnumerator AfterPose(Action callback){bool done=false;afterPose=()=>{callback();done=true;};while(!done)yield return null;}
        IEnumerator RenderedFrame(){if(Application.isBatchMode){yield return null;yield return null;}else yield return new WaitForEndOfFrame();}
        IEnumerator Frame(){yield return null;yield return null;}
        IEnumerator ReadDocument(){
            if(g.ui.modal=="computer")g.ui.OpenOrderFile();
            while(g.ui.modal=="story"){
                if(g.ui.ComputerScreen){if(g.ui.TerminalPageReady)g.ui.NextTerminalPage();}
                else {g.ui.ScrollStory(100);if(g.ui.StoryReady)g.ui.ContinueStory();}
                yield return null;
            }
            while(g.opening && g.opening.Playing)yield return null;
        }
        IEnumerator Start(){
            Directory.CreateDirectory("Documentation");File.WriteAllText(ReportPath,"RUNNING — completion required\n");
            Application.runInBackground=true;deadline=Time.realtimeSinceStartup+600;captures=Array.IndexOf(Environment.GetCommandLineArgs(),"--capture")>=0;
            yield return null;g=Campaign.Instance;Check(g!=null,"campaign starts");if(!g){Finish();yield break;}
            Check(g.sound.VariantCount("pistol")==3 && g.sound.VariantCount("rifle")==3,"separate three-variant pistol and AK sound banks");Check(g.ui.modal=="menu","menu on boot");yield return Capture("menu");
            if(Application.isBatchMode){g.ui.ProcessMenuPointer(new Vector2(220,510),true,false);g.ui.ProcessMenuPointer(new Vector2(220,510),false,true);Check(g.ui.TryPointerClick(new Rect(58,485,420,49)),"menu Input System pointer hit test");g.NewGame();}
            else {g.ui.ProcessMenuPointer(new Vector2(220,510),true,false);g.ui.ProcessMenuPointer(new Vector2(220,510),false,true);yield return Frame();Check(g.ui.modal=="story","menu start responds to Input System pointer");if(g.ui.modal!="story")g.NewGame();}Check(g.ui.modal=="story","prologue");yield return ReadDocument();yield return Frame();
            Check(g.state.stage==0 && g.Running,"chapter 1 starts");
            yield return TestControls();
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--controls-check")>=0){Finish();yield break;}
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"--stealth-check")>=0){yield return TestPatrolGap();Finish();yield break;}
            Check(g.player.transform.position.z>0,"spawn inside locked storefront");
            Check(!string.IsNullOrEmpty(g.ObjectivePurpose),"objective explains its purpose");g.ui.OpenJournal();Check(g.ui.modal=="journal" && g.paused,"journal pauses and exposes context");yield return Capture("journal");g.Resume();
            yield return TestShopAndDoors();
            yield return MissionTestSteps.RevealDrawers(g);
            yield return WalkTo(g.level.GetComponentsInChildren<Interaction>().Single(x=>x.id=="keycard").transform.position,"storefront to Marcus office");
            Use("door06");Check(g.state.stage==0,"Door 06 gate");Use("keycard");Check(g.state.keycard,"keycard");Use("computer");yield return ReadDocument();
            yield return Capture("office");Use("door06");yield return MissionTestSteps.PassAirlock(g);yield return ReadDocument();yield return Frame();
            Check(g.state.stage==1 && !g.state.armed,"unarmed stealth");yield return CheckFloor(1,"radio");yield return TestPatrolGap();yield return TestStealth();
            Use("door06");Check(g.state.stage==1,"radio gate");Use("radio");Use("door06");yield return ReadDocument();yield return Frame();
            Check(g.state.stage==2,"biometric descent");yield return CheckFloor(2,"pistol");Use("pistol");Check(g.state.armed,"pistol unlock");g.state.ammo=0;g.state.reserve=12;g.player.Reload();yield return new WaitForSeconds(1.7f);Check(g.state.ammo==8 && g.state.reserve==4,"reload transfer");
            g.player.body.enabled=false;g.player.transform.position=new Vector3(0,.05f,6);g.player.body.enabled=true;
            var dummy=new GameObject("QA target");dummy.transform.SetParent(g.level.transform);dummy.transform.position=g.player.transform.position+Vector3.back*3;var target=dummy.AddComponent<EnemyController>();target.campaign=g;target.Setup();target.enabled=false;g.player.yaw=180;g.player.aiming=true;yield return Frame();g.player.camera.transform.LookAt(dummy.transform.position+Vector3.up*1.2f);g.player.Fire();Check(target.hp==75 && g.state.ammo==7,"hitscan damages visible target and consumes one round");Destroy(dummy);yield return Frame();
            var pose=g.player.visual.GetComponent<WeaponPose>();yield return new WaitForSeconds(.3f);Check(pose.RaisedAmount>.9f,"firing raises gun");yield return new WaitForSeconds(1.3f);Check(pose.RaisedAmount<.1f,"idle lowers gun after firing");yield return Capture("low-ready");
            g.player.touchAim=true;yield return new WaitForSeconds(.3f);Check(pose.RaisedAmount>.9f,"aim raises gun smoothly");yield return Capture("aim-ready");g.player.touchAim=false;


            yield return WalkTo(new Vector3(11,0,22.7f),"B2 evidence locker reachable");Use("order");Use("recording");yield return ReadDocument();yield return Frame();
            Check(g.state.stage==2 && g.state.tablet,"recording unlocks tablet without teleporting floors");Use("exit");yield return ReadDocument();yield return Frame();yield return CheckFloor(3,"exit");yield return TestCorridorWindows(3);
            Check(g.state.stage==3 && g.state.tablet,"tablet unlock");g.security.Toggle();g.security.Alarm();g.security.Door();Check(g.flags.Contains("security_done"),"security tutorial");Check(g.security.locked,"physical shutter closes");yield return Capture("tablet");g.security.Door();g.security.Close();
            Use("exit");yield return ReadDocument();yield return Frame();Check(g.state.stage==4,"storage chapter");yield return CheckFloor(4,"exit");yield return TestCorridorWindows(4);g.security.Toggle();g.security.Light();g.security.Door();g.security.Close();Check(g.security.dark,"darkness");Use("exit");yield return ReadDocument();yield return Frame();
            Check(g.state.stage==5,"evidence chapter");yield return CheckFloor(5,"upload");Use("upload");Check(g.enemies.Count==6,"doubled purge encounter");Check(g.enemies.Select(e=>e.actorPrefab).Distinct().Count()==6,"six distinct character models at Uplink");Use("exit");Check(g.state.stage==5,"upload exit gated");foreach(var e in g.enemies)e.enabled=false;g.upload=.05f;yield return new WaitForSeconds(.15f);Use("exit");yield return ReadDocument();yield return Frame();
            Check(g.state.stage==6,"Victor chapter");yield return CheckFloor(6,"override");yield return TestEquipment();var boss=g.enemies.First(x=>x.boss);boss.enabled=false;boss.TakeDamage(100);Check(boss.phase==2 && boss.shielded,"Victor phase 2 revokes security");boss.TakeDamage(25);Check(boss.hp==200,"Victor immune before override");Use("override");Check(!boss.shielded && !g.security.revoked,"local override");boss.TakeDamage(100);Check(boss.phase==3,"Victor phase 3");g.security.Toggle();g.security.Light();Check(boss.stun>0,"EMP");g.security.Close();yield return Capture("boss");boss.TakeDamage(100);Use("final");Check(g.ui.modal=="choice","final choice");g.ui.Ending(false);Check(!g.active && g.ui.modal=="story","destroy ending");yield return Capture("ending-destroy");g.ui.Ending(true);Check(!g.active,"accept ending");g.Continue();yield return Frame();Check(g.state.stage==6 && g.enemies.First(x=>x.boss).hp==300,"retry restores full encounter");g.Damage(1000);Check(g.state.hp==0 && g.ui.modal=="death","death");g.Continue();yield return Frame();Check(g.state.hp>0 && g.Running,"retry after death");g.Pause();Check(g.paused,"pause");g.Resume();Check(g.Running,"resume");Finish();
        }
        IEnumerator TestShopAndDoors(){
            var p=g.player;var original=p.transform.position;p.enabled=false;
            foreach(var spot in new[]{new Vector3(12.8f,.1f,26),new Vector3(12.8f,.1f,30),new Vector3(12.8f,.1f,34),new Vector3(5,.1f,32),new Vector3(-13,.1f,35)}){
                p.body.enabled=false;p.transform.position=spot;p.body.enabled=true;for(int i=0;i<30;i++){p.body.Move(Vector3.down*4*Time.deltaTime);yield return null;}Check(p.transform.position.y>=-.05f && p.body.isGrounded,"shop corner supported "+spot);
            }
            var door=g.level.GetComponentsInChildren<RoomDoor>().First(x=>x.roomName=="VĂN PHÒNG MARCUS");
            p.body.enabled=false;p.transform.position=door.transform.position+Vector3.forward*1.6f;p.body.enabled=true;Physics.SyncTransforms();
            p.FindTarget();Check(p.target && p.target.GetComponent<RoomDoor>()==door,"closed room door reachable with E");
            Check(Physics.Linecast(door.transform.position+new Vector3(0,1,-.5f),door.transform.position+new Vector3(0,1,.5f)),"closed door blocks sight and shots");
            door.Toggle();yield return new WaitForSeconds(1);Check(door.IsOpen && !door.Moving,"room door opens");
            Check(!Physics.Linecast(door.transform.position+new Vector3(0,1,-.5f),door.transform.position+new Vector3(0,1,.5f)),"open doorway clears sightline");
            p.body.enabled=false;p.transform.position=door.transform.position;p.body.enabled=true;Physics.SyncTransforms();door.Toggle();Check(door.IsOpen,"door refuses to close on player");
            p.body.enabled=false;p.transform.position=door.transform.position+Vector3.forward*1.6f;p.body.enabled=true;Physics.SyncTransforms();door.Toggle();yield return new WaitForSeconds(1);Check(!door.IsOpen && !door.Moving,"room door closes again");
            p.body.enabled=false;p.transform.position=original;p.body.enabled=true;p.enabled=true;
        }
        IEnumerator LiveCrouchRoute(Vector3[] points){
            var p=g.player;foreach(var point in points){float end=Time.time+20;while(g.Running && Time.time<end && Vector2.Distance(new Vector2(p.transform.position.x,p.transform.position.z),new Vector2(point.x,point.z))>.18f){var d=point-p.transform.position;d.y=0;p.body.Move((d.normalized*2+Vector3.down*4)*Time.deltaTime);yield return null;}Check(Vector2.Distance(new Vector2(p.transform.position.x,p.transform.position.z),new Vector2(point.x,point.z))<.3f,"live crouch traversal "+point+" / actual="+p.transform.position+" hp="+g.state.hp);}
        }
        IEnumerator TestCorridorWindows(int stage){
            g.LoadWorld(stage);yield return Frame();var points=stage==3?new[]{new Vector3(-2,0,12),new Vector3(0,0,12),new Vector3(0,0,13)}:new[]{new Vector3(-1,0,29),new Vector3(0,0,30),new Vector3(0,0,32)};
            float longest=0,current=0,until=Time.time+38;
            while(Time.time<until){bool clear=!g.enemies.Any(e=>points.Any(v=>e.CanSeePoint(v+Vector3.up*.82f)||e.CanSeePoint(v+Vector3.up*1.12f)));current=clear?current+Time.deltaTime:0;longest=Mathf.Max(longest,current);yield return null;}
            Check(longest>=4,"live NPC corridor opportunity floor "+stage+" / longest="+longest.ToString("F1")+"s (minimum 4s)");g.LoadWorld(stage);yield return Frame();foreach(var e in g.enemies){e.enabled=false;var agent=e.GetComponent<NavMeshAgent>();if(agent.isOnNavMesh)agent.isStopped=true;}
        }
        IEnumerator TestPatrolGap(){
            g.LoadWorld(1);yield return Frame();var p=g.player;p.enabled=false;p.crouch=true;
            p.body.enabled=false;p.transform.position=g.Marker("stair_bottom").position;p.body.enabled=true;
            yield return LiveCrouchRoute(new[]{new Vector3(4.3f,0,3),new Vector3(4.3f,0,5.5f),new Vector3(11.7f,0,5.5f),g.Marker("stealth_wait").position});
            Check(g.state.hp==100 && !g.enemies.Any(e=>e.Armed),"live patrol service approach avoids detection");
            var guard=g.enemies.First(x=>x.routeId=="enemy_b");float timeout=Time.time+30;
            while(Time.time<timeout && !(guard.PatrolRestRemaining>=5 && guard.transform.position.z>26))yield return null;
            Check(guard.PatrolRestRemaining>=5 && guard.transform.position.z>26,"live patrol reaches visible 9-second opportunity");
            p.camera.transform.position=new Vector3(11.9f,1.65f,16.5f);p.camera.transform.LookAt(new Vector3(8,1.3f,28));yield return Capture("stealth-window");
            float health=g.state.hp;bool exposed=false;
            foreach(var point in new[]{new Vector3(11.7f,0,21.5f),g.Marker("stealth_goal").position}){
                float end=Time.time+8;while(g.Running && Time.time<end && Vector2.Distance(new Vector2(p.transform.position.x,p.transform.position.z),new Vector2(point.x,point.z))>.18f){var d=point-p.transform.position;d.y=0;p.body.Move((d.normalized*2+Vector3.down*4)*Time.deltaTime);var watcher=g.enemies.FirstOrDefault(e=>e.CanSee());if(watcher && !exposed)Debug.Log("QA first exposure: "+watcher.routeId+" at "+watcher.transform.position+" looking "+watcher.Flashlight.transform.forward+" player "+p.transform.position+" mode "+watcher.mode+" rest "+watcher.PatrolRestRemaining);exposed|=watcher!=null;yield return null;}
            }
            p.FindTarget();Check(p.target && p.target.id=="radio","live patrol gap reaches radio interaction");Check(!exposed && g.state.hp==health && !g.enemies.Any(e=>e.Armed),"radio approach unseen with all four NPCs active");
            if(p.target && p.target.id=="radio")p.Interact();Check(g.flags.Contains("radio"),"live patrol allows mission action");
            yield return LiveCrouchRoute(new[]{new Vector3(11.7f,0,21.5f),new Vector3(11.7f,0,29.8f),new Vector3(0,0,30)});
            Check(g.state.hp==health && !g.enemies.Any(e=>e.Armed),"live radio distraction permits escape to scanner");
            p.enabled=true;g.state.hp=100;g.LoadWorld(1);yield return Frame();foreach(var e in g.enemies)e.enabled=false;
        }
        IEnumerator TestEquipment(){
            var p=g.player;
            Check(!g.state.hasAK && !g.state.hasNightVision,"equipment starts uncollected at boss checkpoint");
            yield return WalkTo(new Vector3(-9,0,4.7f),"enter AK supply room");p.FindTarget();Check(p.target && p.target.id=="ak","AK reachable through normal interaction");Use("ak");Check(g.state.hasAK && p.UsingAK && p.Ammo==30 && p.Reserve==90,"AK pickup equips separate ammunition");
            var targetObject=new GameObject("AK QA target");targetObject.transform.SetParent(g.level.transform);targetObject.transform.position=new Vector3(0,0,6);var rifleTarget=targetObject.AddComponent<EnemyController>();rifleTarget.campaign=g;rifleTarget.Setup();rifleTarget.enabled=false;
            p.body.enabled=false;p.transform.position=new Vector3(0,.05f,3);p.body.enabled=true;p.yaw=0;yield return Frame();p.camera.transform.LookAt(rifleTarget.transform.position+Vector3.up*1.2f);p.Fire();Check(rifleTarget.hp==78,"AK hitscan deals 22 damage");Destroy(targetObject);int after=p.Ammo;p.Fire();Check(after==29 && p.Ammo==29,"AK consumes ammo and enforces fire interval");yield return new WaitForSeconds(.13f);p.Fire();Check(p.Ammo==28,"AK supports automatic cadence");
            p.Reload();yield return new WaitForSeconds(2.4f);Check(p.Ammo==30 && p.Reserve==88,"AK reload transfers separate reserve");
            p.Ammo=20;p.Reload();p.SwitchWeapon(0);Check(p.reloadTime==0 && !p.UsingAK,"switching cancels reload without mixing ammo");p.SwitchWeapon(1);Check(p.Ammo==20,"switch preserves AK magazine");
            p.touchAim=true;yield return new WaitForSeconds(.4f);yield return Capture("ak-equipped");p.touchAim=false;
            yield return WalkTo(new Vector3(9,0,4.7f),"enter night vision optics room");p.FindTarget();Check(p.target && p.target.id=="nightvision","NVG reachable through normal interaction");Use("nightvision");Check(g.state.hasNightVision && p.nightVision.Active,"night vision pickup activates view");
            g.security.SetDark(true);yield return Frame();yield return Capture("nightvision-on");p.nightVision.Toggle();Check(!p.nightVision.Active,"night vision can switch off");yield return Capture("nightvision-off");p.nightVision.Toggle();
            var checkpoint=File.ReadAllText(g.SavePath);g.Save();g.Continue();yield return Frame();Check(g.state.hasAK && g.state.hasNightVision && g.player.UsingAK && g.player.Ammo==20,"equipment inventory survives save roundtrip");Check(!g.level.GetComponentsInChildren<Interaction>().Any(x=>x.id=="ak" || x.id=="nightvision"),"collected equipment stays hidden after reload");File.WriteAllText(g.SavePath,checkpoint);
            foreach(var e in g.enemies){e.enabled=false;var nav=e.GetComponent<NavMeshAgent>();if(nav.isOnNavMesh)nav.isStopped=true;}
            Check(g.enemies.Count(e=>e.boss)==1 && g.enemies.Single(e=>e.boss).actorPrefab=="Victor","single Victor with exclusive character model");
        }
        IEnumerator TestStealth(){
            foreach(var enemy in g.enemies){enemy.enabled=false;var nav=enemy.GetComponent<NavMeshAgent>();if(nav.isOnNavMesh)nav.isStopped=true;}
            var arena=new GameObject("Stealth test arena");var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.transform.SetParent(arena.transform);floor.transform.position=new Vector3(80,-.2f,0);floor.transform.localScale=new Vector3(30,.4f,30);floor.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("Materials/concrete_floor_02");var surface=arena.AddComponent<Unity.AI.Navigation.NavMeshSurface>();surface.collectObjects=Unity.AI.Navigation.CollectObjects.Children;surface.useGeometry=NavMeshCollectGeometry.PhysicsColliders;surface.BuildNavMesh();
            var p=g.player;var original=p.transform.position;p.enabled=false;p.body.enabled=false;p.transform.position=new Vector3(80,.05f,1);p.body.enabled=true;
            var npc=new GameObject("Stealth QA guard");npc.transform.position=new Vector3(80,0,-5);npc.transform.SetParent(g.level.transform);var guard=npc.AddComponent<EnemyController>();guard.campaign=g;guard.Setup();guard.enabled=false;
            yield return Frame();Physics.SyncTransforms();Check(!guard.Armed && guard.Flashlight.gameObject.activeInHierarchy,"patrol uses flashlight and holsters gun");Check(guard.CanSee(),"player in flashlight cone visible");Check(guard.Flashlight.intensity>=20 && guard.Flashlight.GetComponentInChildren<MeshFilter>(),"bright flashlight with visible cone");p.camera.transform.position=new Vector3(84,2,-8);p.camera.transform.LookAt(new Vector3(80,1,-2));yield return Capture("flashlight-patrol");
            var cover=GameObject.CreatePrimitive(PrimitiveType.Cube);cover.name="QA opaque cover";cover.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("Materials/Cardboard");cover.transform.position=new Vector3(80,1.5f,-1);cover.transform.localScale=new Vector3(3,3,.3f);Physics.SyncTransforms();Check(!guard.CanSee(),"wall blocks flashlight detection");
            cover.transform.position=new Vector3(80,.675f,-.3f);cover.transform.localScale=new Vector3(3,1.35f,.3f);Physics.SyncTransforms();p.crouch=false;Check(guard.CanSee(),"standing head exposed above low cover");p.crouch=true;yield return new WaitForSeconds(.3f);yield return RenderedFrame();Check(!guard.CanSee(),"crouching behind crate breaks visibility");var head=p.visual.GetComponentsInChildren<Transform>().First(x=>x.name=="Bip01 Head");yield return AfterPose(()=>Check(head.position.y-p.transform.position.y<1.35f,"crouch pose head height="+(head.position.y-p.transform.position.y).ToString("F3")+" (must be below 1.35 m)"));yield return Capture("crouch-cover");p.crouch=false;
            Destroy(cover);yield return Frame();guard.enabled=true;float hp=g.state.hp;yield return new WaitForSeconds(.4f);Check(!guard.Armed,"brief exposure builds suspicion before drawing");
            float timeout=0;while(!guard.Armed && timeout<3){timeout+=Time.deltaTime;yield return null;}Check(guard.Armed,"confirmed detection draws gun");Check(g.state.hp==hp,"draw delay prevents instant shot");yield return new WaitForSeconds(1.6f);Check(g.state.hp<hp,"guard fires after detection and draw delay");
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=new Vector3(80,1.5f,-.5f);wall.transform.localScale=new Vector3(40,6,.3f);var obstacle=wall.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.size=Vector3.one;obstacle.carving=true;Physics.SyncTransforms();var last=p.transform.position;hp=g.state.hp;
            p.body.enabled=false;p.transform.position=new Vector3(82,.05f,2);p.body.enabled=true;yield return new WaitForSeconds(.3f);Check(guard.mode==EnemyController.Mode.Search,"lost sight enters search");Check(Vector3.Distance(guard.LastSeen,last)<.2f,"search remembers last sighting without tracking through wall");Check(g.state.hp==hp,"wall blocks incoming shots");
            yield return new WaitForSeconds(12.5f);Check(!guard.Armed,"search timeout returns to flashlight patrol");
            guard.Hear(new Vector3(83,0,-4),10);Check(guard.mode==EnemyController.Mode.Investigate && !guard.Armed,"sound prompts flashlight investigation without instant combat");
            Destroy(npc);Destroy(wall);Destroy(arena);p.body.enabled=false;p.transform.position=original;p.body.enabled=true;p.enabled=true;g.state.hp=100;yield return Frame();
        }
        IEnumerator CheckFloor(int stage,string target){
            Check(g.level.name==StoryData.Worlds[stage],"unique authored floor "+stage+" / "+g.level.name);
            Check(g.player.transform.position.y>2.9f,"arrival on elevated stair landing "+stage);
            Check(g.enemies.Count==(stage==5?0:EncounterData.Counts[stage]),"encounter population floor "+stage);
            Check(g.enemies.Select(e=>e.actorPrefab).Distinct().Count()==g.enemies.Count,"distinct character prefabs within floor "+stage);
            if(stage<6){float health=g.state.hp;bool assist=g.ui.assist;foreach(bool assisted in new[]{false,true}){g.ui.assist=assisted;g.state.hp=100;g.EnemyShot(10);Check(g.state.hp==50,"first shot costs 50 HP floor "+stage+" assist="+assisted);g.EnemyShot(10);Check(g.state.hp==0 && g.ui.modal=="death","second shot kills floor "+stage+" assist="+assisted);g.state.hp=health;g.Resume();}g.ui.assist=assist;}
            foreach(var enemy in g.enemies){enemy.enabled=false;var nav=enemy.GetComponent<NavMeshAgent>();if(nav.isOnNavMesh)nav.isStopped=true;}
            bool wasDark=g.security.dark;g.security.SetDark(true);Check(g.level.GetComponentsInChildren<Light>().Where(l=>l.name.StartsWith("Emergency")).All(l=>l.enabled),"emergency lighting survives blackout floor "+stage);Check(g.enemies.All(e=>e.Flashlight.enabled),"NPC flashlights survive blackout floor "+stage);g.security.SetDark(wasDark);
            yield return Capture("floor-"+stage+"-arrival");
            yield return WalkTo(g.Marker("stair_bottom").position,"descend staircase "+stage);
            Check(g.player.transform.position.y<.5f,"stairs reach floor "+stage);
            var point=g.level.GetComponentsInChildren<Interaction>().First(x=>x.id==target);
            yield return WalkTo(point.transform.position,"floor "+stage+" to "+target);
            yield return Capture("floor-"+stage+"-interior");
            if(stage==1){g.player.body.enabled=false;g.player.transform.position=new Vector3(0,-12,0);g.player.body.enabled=true;g.player.RecoverIfFallen();Check(g.player.transform.position.y>2.9f,"fall recovery restores safe landing");yield return WalkTo(g.Marker("stair_bottom").position,"descend after fall recovery");}
        }
        IEnumerator WalkTo(Vector3 goal,string description){
            foreach(var door in g.level.GetComponentsInChildren<RoomDoor>())door.SetOpenImmediate(true);yield return new WaitForSeconds(.25f);
            var p=g.player;foreach(var e in g.enemies){e.enabled=false;var agent=e.GetComponent<NavMeshAgent>();if(agent.isOnNavMesh)agent.isStopped=true;}p.enabled=false;
            var path=new NavMeshPath();NavMesh.SamplePosition(p.transform.position,out var from,2,NavMesh.AllAreas);NavMesh.SamplePosition(goal,out var to,2,NavMesh.AllAreas);
            bool found=NavMesh.CalculatePath(from.position,to.position,NavMesh.AllAreas,path);Check(found && path.status==NavMeshPathStatus.PathComplete,"navigation "+description);
            foreach(var corner in path.corners){float time=0;while(Vector3.Distance(new Vector3(p.transform.position.x,corner.y,p.transform.position.z),corner)>.3f && time<12){var d=corner-p.transform.position;d.y=0;p.body.Move((d.normalized*5+Vector3.down*9)*Time.deltaTime);time+=Time.deltaTime;yield return null;}}
            Check(Vector2.Distance(new Vector2(p.transform.position.x,p.transform.position.z),new Vector2(goal.x,goal.z))<1.2f,"collision traversal "+description);p.enabled=true;
        }
        IEnumerator Capture(string name){
            if(!captures)yield break;
            yield return AfterPose(()=>{
                string captureDirectory=Application.isBatchMode?"Documentation/EditorPlaytest":"Documentation/Playtest";Directory.CreateDirectory(captureDirectory);
                if(Application.isBatchMode){
                    var cam=g.player.camera;var rt=new RenderTexture(1280,720,24);var previous=RenderTexture.active;var previousTarget=cam.targetTexture;
                    var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);
                    // Batch cameras render before Unity's usual skinning update. Bake the final bones for this snapshot.
                    var skins=g.player.visual.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=>r.enabled).ToArray();
                    var snapshots=new List<GameObject>();var meshes=new List<Mesh>();
                    foreach(var skin in skins){var mesh=new Mesh();skin.BakeMesh(mesh);meshes.Add(mesh);var snapshot=new GameObject("QA skin snapshot");snapshot.transform.SetParent(skin.transform,false);snapshot.AddComponent<MeshFilter>().sharedMesh=mesh;snapshot.AddComponent<MeshRenderer>().sharedMaterials=skin.sharedMaterials;snapshots.Add(snapshot);skin.enabled=false;}
                    try{cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(captureDirectory+"/"+name+".png",texture.EncodeToPNG());}
                    finally{foreach(var skin in skins)skin.enabled=true;foreach(var snapshot in snapshots){snapshot.SetActive(false);Destroy(snapshot);}foreach(var mesh in meshes)Destroy(mesh);cam.targetTexture=previousTarget;RenderTexture.active=previous;rt.Release();Destroy(rt);Destroy(texture);}
                }else ScreenCapture.CaptureScreenshot(Path.GetFullPath("Documentation/Playtest/"+name+".png"));
            });
        }
        void Update(){if(deadline>0 && Time.realtimeSinceStartup>deadline){Check(false,"self-test timed out");Finish();}}
        void Finish(){Directory.CreateDirectory("Documentation");log.Add("RESULT: "+(failures==0?"PASS":"FAIL")+" / "+failures+" failures");File.WriteAllLines(ReportPath,log);enabled=false;
#if UNITY_EDITOR
            UnityEditor.EditorApplication.Exit(failures==0?0:1);
#else
            Application.Quit(failures==0?0:1);
#endif
        }
    }
}
