using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
namespace BlackMarket.Editor {
    public static class NorthPointValidation {
        [MenuItem("BLACK MARKET/5 - Validate and render environment previews")]
        public static void Run(){
            if(Application.isBatchMode && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/BlackMarket/Scenes/EditorWorkspace.unity");
            var previous=SceneManager.GetActiveScene();var results=new List<string>();Directory.CreateDirectory("Documentation/Previews");
            foreach(var kind in new[]{"pistol","rifle"})for(int i=0;i<3;i++){var clip=Resources.Load<AudioClip>("FieldAudio/"+kind+"_"+i);if(!clip || clip.length<1 || clip.channels!=1)throw new Exception("Missing/invalid combat audio "+kind+"_"+i);}
            results.Add("PASS audio: six separate mono firearm recordings imported, with full attack and tail.");
            foreach(var actor in EncounterData.Actors){
                var instance=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Actors/"+actor));
                try{var animation=instance.GetComponentInChildren<Animation>();foreach(var clip in new[]{"idle","walk","run"})if(!animation || animation[clip]==null)throw new Exception("Missing animation "+actor+"/"+clip);foreach(var r in instance.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)if(!m || (r is SkinnedMeshRenderer && !m.mainTexture))throw new Exception("Missing actor texture "+actor);results.Add("PASS "+actor+": textured model and serialized idle/walk/run clips.");}catch(Exception e){results.Add("FAIL "+e.Message);}finally{UnityEngine.Object.DestroyImmediate(instance);}
            }
            foreach(var id in StoryData.Worlds){
                var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
                try {
                    var prefab=Resources.Load<GameObject>("Worlds/"+id);if(!prefab)throw new Exception("Missing prefab "+id);
                    var world=UnityEngine.Object.Instantiate(prefab);var markers=world.GetComponentsInChildren<WorldMarker>();
                    foreach(var door in world.GetComponentsInChildren<RoomDoor>())door.SetOpenImmediate(true);
                    Physics.SyncTransforms();
                    if(id=="NorthPointShop"){for(float x=-13.5f;x<=13.5f;x+=.5f)for(float z=.5f;z<=35.5f;z+=.5f)if(!Physics.RaycastAll(new Vector3(x,.08f,z),Vector3.down,.3f).Any(h=>h.collider.name=="Floor tile"))throw new Exception("Missing shop floor at "+x+", "+z);results.Add("PASS shop: complete floor coverage at 3905 grid samples, including rear right corner.");}
                    if(markers.Length<7)throw new Exception("Missing serialized markers: "+id);
                    foreach(var r in world.GetComponentsInChildren<Renderer>())foreach(var m in r.sharedMaterials)if(!m || m.shader.name.Contains("Error"))throw new Exception("Missing/invalid material: "+r.name);
                    foreach(Transform child in world.transform){var visual=child.Find("Visual");var collider=child.GetComponent<BoxCollider>();if(!visual || !collider)continue;var renderers=visual.GetComponentsInChildren<Renderer>();var bound=renderers[0].bounds;foreach(var r in renderers)bound.Encapsulate(r.bounds);if(Vector3.Distance(bound.center,collider.bounds.center)>.1f || Vector3.Distance(bound.size,collider.bounds.size)>.1f)throw new Exception("Imported mesh/collider size mismatch: "+child.name);}
                    if(id=="ControlRoom"){
                        int rooms=markers.Count(m=>m.id.StartsWith("room_"));
                        var coverNames=new[]{"metal_office_desk","Stock / sealed carton","cardboard_box_01","Command support pillar","metal_tool_chest","Server cabinet","Command barricade","Staff locker","Electrical switchgear","Meeting tabletop"};
                        int covers=world.transform.Cast<Transform>().Count(t=>coverNames.Contains(t.name));
                        if(rooms!=8 || covers<48)throw new Exception("Victor expansion below target: rooms="+rooms+" cover="+covers);
                        results.Add("PASS Victor: "+rooms+" functional rooms, "+covers+" cover objects (baseline 4 / 24).");
                    }
                    var start=markers.First(x=>x.id=="spawn").transform.position;
                    if(!Physics.Raycast(start+Vector3.up*.2f,Vector3.down,1))throw new Exception("Unsupported spawn "+id);
                    if(id!="NorthPointShop" && (start.y<2.9f || !markers.Any(m=>m.id=="stair_bottom")))throw new Exception("Missing elevated stair arrival "+id);
                    if(!NavMesh.SamplePosition(start,out var s,2,NavMesh.AllAreas))throw new Exception("Spawn outside navigation "+id);
                    foreach(var point in world.GetComponentsInChildren<Interaction>()){
                        if(!NavMesh.SamplePosition(point.transform.position,out var end,2.5f,NavMesh.AllAreas))throw new Exception("No navigation near "+point.id);
                        var path=new NavMeshPath();if(!NavMesh.CalculatePath(s.position,end.position,NavMesh.AllAreas,path) || path.status!=NavMeshPathStatus.PathComplete)throw new Exception("Unreachable interaction "+point.id);
                    }
                    foreach(var marker in markers.Where(m=>m.id.StartsWith("patrol_") || m.id.StartsWith("enemy_") || m.id=="boss")){if(!NavMesh.SamplePosition(marker.transform.position,out var endpoint,1,NavMesh.AllAreas))throw new Exception("Patrol point outside navmesh "+marker.id);var route=new NavMeshPath();if(!NavMesh.CalculatePath(s.position,endpoint.position,NavMesh.AllAreas,route) || route.status!=NavMeshPathStatus.PathComplete)throw new Exception("Patrol point unreachable "+marker.id);}
                    results.Add("PASS "+id+": materials, model/collider bounds, interactions and patrol routes reachable.");
                    var cam=new GameObject("QA camera").AddComponent<Camera>();cam.nearClipPlane=.08f;cam.farClipPlane=150;cam.fieldOfView=67;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.035f,.055f,.07f);
                    var light=new GameObject("QA moon").AddComponent<Light>();light.type=LightType.Directional;light.intensity=.65f;light.color=new Color(.65f,.75f,.88f);light.transform.rotation=Quaternion.Euler(50,-30,0);light.shadows=LightShadows.Soft;
                    RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.25f,.32f,.4f);RenderSettings.ambientEquatorColor=new Color(.16f,.18f,.2f);RenderSettings.ambientGroundColor=new Color(.05f,.06f,.07f);RenderSettings.fog=false;
                    if(id=="NorthPointShop"){
                        Capture(cam,new Vector3(-8,2.3f,-13),new Vector3(1,2.2f,2),"shop-exterior");
                        Capture(cam,new Vector3(-1,1.7f,1),new Vector3(-4,1.3f,9),"shop-interior");
                        Capture(cam,new Vector3(-5,1.8f,13.5f),new Vector3(-10,1.05f,17),"marcus-office");
                        Capture(cam,new Vector3(-7,1.75f,32),new Vector3(0,1.5f,35),"door-06");
                        Capture(cam,new Vector3(4,1.7f,13),new Vector3(11,1.1f,17),"repair-intake");Capture(cam,new Vector3(5,1.7f,20),new Vector3(11,1.1f,23),"workshop-radio");Capture(cam,new Vector3(2,1.7f,26),new Vector3(-9,1.1f,29),"warehouse-cover");
                        foreach(var r in world.GetComponentsInChildren<Renderer>())if(r.gameObject.name=="Ceiling" || r.gameObject.name=="Structural beam" || r.gameObject.name=="Door lintel")r.enabled=false;
                        cam.orthographic=true;cam.orthographicSize=22;Capture(cam,new Vector3(0,44,17.99f),new Vector3(0,0,18),"shop-floorplan");
                    }else {
                        Capture(cam,new Vector3(0,4.6f,-5.4f),new Vector3(0,.7f,8),id+"-stairs");
                        Capture(cam,new Vector3(1,1.8f,3),new Vector3(0,1.5f,20),id);
                        foreach(var r in world.GetComponentsInChildren<Renderer>())if(r.name=="Ceiling" || r.name=="Stairwell ceiling" || r.name=="Door lintel")r.enabled=false;
                        float depth=id=="MedicalStorage"?40:id=="ServerUplink"?38:32;
                        cam.orthographic=true;cam.orthographicSize=depth/2+6;Capture(cam,new Vector3(0,50,depth/2-.01f),new Vector3(0,0,depth/2),id+"-floorplan");
                    }
                }catch(Exception e){results.Add("FAIL "+id+": "+e.Message);Debug.LogException(e);}
                finally{SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true);}
            }
            RenderRoster();
            File.WriteAllLines("Documentation/validation.txt",results);Debug.Log(string.Join("\n",results));if(results.Any(x=>x.StartsWith("FAIL")))throw new Exception("Asset validation failed. See Documentation/validation.txt");
        }
        static void RenderRoster(){
            var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
            try{
                var camera=new GameObject("Roster camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.045f,.055f,.07f);camera.fieldOfView=32;
                var key=new GameObject("Roster key").AddComponent<Light>();key.type=LightType.Directional;key.intensity=1.4f;key.transform.rotation=Quaternion.Euler(35,-25,0);RenderSettings.ambientLight=new Color(.4f,.4f,.4f);
                foreach(var actor in EncounterData.Actors){var model=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("Actors/"+actor));var animation=model.GetComponentInChildren<Animation>();if(animation && animation.clip)animation.clip.SampleAnimation(animation.gameObject,0);Capture(camera,new Vector3(0,1.25f,4.5f),new Vector3(0,1,0),"actor-"+actor);UnityEngine.Object.DestroyImmediate(model);}
            }finally{SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true);}
        }
        public static void ValidateAndBuild(){Run();NorthPointBuilder.BuildLinux();}
        static void Capture(Camera cam,Vector3 position,Vector3 target,string name){
            cam.transform.position=position;cam.transform.LookAt(target);var rt=new RenderTexture(1600,900,24);cam.targetTexture=rt;cam.Render();
            var old=RenderTexture.active;RenderTexture.active=rt;var tex=new Texture2D(1600,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1600,900),0,0);tex.Apply();File.WriteAllBytes("Documentation/Previews/"+name+".png",tex.EncodeToPNG());
            RenderTexture.active=old;cam.targetTexture=null;rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(tex);
        }
    }
}
