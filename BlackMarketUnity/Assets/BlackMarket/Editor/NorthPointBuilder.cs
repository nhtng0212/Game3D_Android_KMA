using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
namespace BlackMarket.Editor {
    public static partial class NorthPointBuilder {
        const string Root="Assets/BlackMarket/Resources/";
        static Transform parent;static Font signFont;
        static Dictionary<string,Material> mats=new Dictionary<string,Material>();
        static string[] modelIds={"metal_office_desk","vintage_radio_transceiver","metal_tool_chest","rollershutter_door","wooden_crate_01","metal_stool_01","Shelf_01","desk_lamp_arm_01","street_lamp_01","Television_01","television_02","cardboard_box_01"};
        [MenuItem("BLACK MARKET/1 - Prepare North Point project")]
        public static void Prepare(){
            Directory.CreateDirectory(Root+"Materials");Directory.CreateDirectory(Root+"Worlds");Directory.CreateDirectory(Root+"Actors");Directory.CreateDirectory("Assets/BlackMarket/Scenes");Directory.CreateDirectory("Documentation");
            AssetDatabase.Refresh();
            if(Application.isBatchMode && string.IsNullOrEmpty(SceneManager.GetActiveScene().path))EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Assets/BlackMarket/Scenes/EditorWorkspace.unity");
            ImportSettings();MakeMaterials();MakeActors();
            for(int stage=0;stage<StoryData.Worlds.Length;stage++)BuildWorld(StoryData.Worlds[stage],stage);BuildEntry();
            PlayerSettings.companyName="KMA";PlayerSettings.productName="BLACK MARKET — North Point";
            PlayerSettings.SetApplicationIdentifier(UnityEditor.Build.NamedBuildTarget.Android,"vn.kma.blackmarket.unity");
            PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;PlayerSettings.colorSpace=ColorSpace.Linear;
            AssetDatabase.SaveAssets();
            File.WriteAllText("Documentation/editor-ready.txt","North Point generated successfully at "+DateTime.UtcNow.ToString("O")+"\nOpen Assets/BlackMarket/Scenes/NorthPoint.unity and press Play.\nGodot files were not modified.");
            Debug.Log("BLACK MARKET ready: Assets/BlackMarket/Scenes/NorthPoint.unity");
        }
        static void ImportSettings(){
            foreach(string path in Directory.GetFiles(Root+"Media","*",SearchOption.AllDirectories)){
                if(AssetImporter.GetAtPath(path) is TextureImporter ti){bool normal=Path.GetFileName(path).Contains("nor_gl");bool linear=normal||path.Contains("unity_mask")||path.Contains("rough")||path.Contains("metal.")||path.Contains("ao.");bool change=ti.maxTextureSize!=1024||ti.textureType!=(normal?TextureImporterType.NormalMap:TextureImporterType.Default)||ti.sRGBTexture==linear;
                    if(change){ti.maxTextureSize=1024;ti.textureType=normal?TextureImporterType.NormalMap:TextureImporterType.Default;ti.sRGBTexture=!linear;ti.mipmapEnabled=true;ti.textureCompression=TextureImporterCompression.Compressed;ti.SaveAndReimport();}}
                if(AssetImporter.GetAtPath(path) is ModelImporter mi && mi.materialImportMode!=ModelImporterMaterialImportMode.None){mi.materialImportMode=ModelImporterMaterialImportMode.None;mi.SaveAndReimport();}
            }
            foreach(string path in Directory.GetFiles(Root+"LegacyCharacters","*.fbx"))if(AssetImporter.GetAtPath(path) is ModelImporter mi){if(mi.animationType!=ModelImporterAnimationType.Legacy){mi.animationType=ModelImporterAnimationType.Legacy;mi.importAnimation=true;mi.SaveAndReimport();}}
        }
        static Texture2D Tex(string id,string name){foreach(var ext in new[]{".jpg",".png"}){var t=AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Media/"+id+"/"+name+ext);if(t)return t;}return null;}
        static Material SaveMat(string name,Material mat){string path=Root+"Materials/"+name+".mat";var existing=AssetDatabase.LoadAssetAtPath<Material>(path);if(existing){EditorUtility.CopySerialized(mat,existing);UnityEngine.Object.DestroyImmediate(mat);mat=existing;}else AssetDatabase.CreateAsset(mat,path);mats[name]=mat;return mat;}
        static Material Plain(string name,Color color,float metal=0,float smooth=.3f,float emission=0){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);if(emission>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*emission);m.globalIlluminationFlags=MaterialGlobalIlluminationFlags.RealtimeEmissive;}return SaveMat(name,m);}
        static Material Pbr(string id,string prefix=""){
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.SetColor("_BaseColor",Color.white);
            m.SetTexture("_BaseMap",Tex(id,prefix+"diffuse")??Tex(id,prefix+"diff"));
            var normal=Tex(id,prefix+"nor_gl");if(normal){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
            var mask=Tex(id,prefix+"unity_mask");if(mask){m.SetTexture("_MetallicGlossMap",mask);m.EnableKeyword("_METALLICSPECGLOSSMAP");m.SetFloat("_Smoothness",.75f);}
            var ao=Tex(id,prefix+"ao");if(ao){m.SetTexture("_OcclusionMap",ao);m.EnableKeyword("_OCCLUSIONMAP");}
            return SaveMat(id+(prefix==""?"":"_accessories"),m);
        }
        static void MakeMaterials(){
            var rain=new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));rain.SetFloat("_Surface",1);rain.SetFloat("_SrcBlend",5);rain.SetFloat("_DstBlend",10);rain.SetFloat("_ZWrite",0);rain.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");rain.renderQueue=3000;SaveMat("Rain",rain);
            mats.Clear();SaveMat("FlashlightBeam",new Material(Shader.Find("BlackMarket/FlashlightBeam")));foreach(var id in modelIds)Pbr(id);Pbr("vintage_radio_transceiver","accessories_");
            foreach(var id in new[]{"painted_plaster_wall","concrete_floor_02","asphalt_02","brick_wall_001","metal_plate"})Pbr(id);
            Plain("Cardboard",new Color(.48f,.29f,.13f),0,.1f);Plain("PackingTape",new Color(.5f,.36f,.19f),0,.2f);Plain("ShippingLabel",new Color(.75f,.73f,.66f),0,.05f);
            Plain("Steel",new Color(.12f,.145f,.15f),.8f,.42f);Plain("Black",new Color(.018f,.024f,.029f),.2f,.4f);
            Plain("Glass",new Color(.035f,.075f,.09f),.75f,.95f);Plain("Paper",new Color(.76f,.73f,.63f));
            Plain("Amber",new Color(.9f,.4f,.1f),.2f,.3f,2);Plain("WhiteLight",new Color(.7f,.84f,.88f),0,.4f,3);
            Plain("Red",new Color(.6f,.025f,.012f),.2f,.3f,2);Plain("Screen",new Color(.025f,.15f,.12f),0,.6f,1);
            Plain("Tracer",new Color(1,.7f,.3f),0,.2f,4);Plain("AlexSkin",Color.white);mats["AlexSkin"].SetTexture("_BaseMap",Resources.Load<Texture2D>("LegacyCharacters/alex"));
            Plain("OperatorSkin",Color.white);mats["OperatorSkin"].SetTexture("_BaseMap",Resources.Load<Texture2D>("LegacyCharacters/operator"));
        }
        static void MakeActors(){
            for(int actorIndex=0;actorIndex<EncounterData.Actors.Length;actorIndex++){
                string name=EncounterData.Actors[actorIndex],id=EncounterData.Models[actorIndex];
                var root=new GameObject(name);var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Rocketbox/"+id+".fbx");var model=UnityEngine.Object.Instantiate(source,root.transform);model.name="Rig";
                var renderers=model.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
                float scale=(name=="Victor"?1.92f:1.78f)/b.size.y;model.transform.localScale*=scale;model.transform.localPosition=(model.transform.localPosition-new Vector3(b.center.x,b.min.y,b.center.z))*scale;
                foreach(var r in renderers){var materials=r.sharedMaterials;for(int i=0;i<materials.Length;i++){
                    var src=materials[i];string key=id+"_"+(src?src.name:"body");
                    var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.SetColor("_BaseColor",Color.white);mat.SetFloat("_Smoothness",.23f);
                    var tex=src?src.mainTexture:null;if(tex){mat.SetTexture("_BaseMap",tex);string normalPath=AssetDatabase.GetAssetPath(tex).Replace("_color","_normal");var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);if(normal){mat.SetTexture("_BumpMap",normal);mat.EnableKeyword("_NORMALMAP");}}
                    if(src && src.name.ToLower().Contains("opacity")){mat.SetFloat("_AlphaClip",1);mat.EnableKeyword("_ALPHATEST_ON");mat.SetFloat("_Cutoff",.4f);mat.SetFloat("_Cull",0);}
                    materials[i]=SaveMat(key,mat);Debug.Log("ACTOR MATERIAL "+key+" texture="+(tex?tex.name:"NONE"));
                }r.sharedMaterials=materials;}
                var anim=model.GetComponent<Animation>()??model.AddComponent<Animation>();
                foreach(var pair in new[]{new[]{"idle","m_idle_neutral_01.max"},new[]{"run","m_run_neutral_01.max"},new[]{"walk","m_walk_neutral_01.max"}}){
                    var clip=AssetDatabase.LoadAllAssetsAtPath(Root+"Rocketbox/"+pair[1]+".fbx").OfType<AnimationClip>().FirstOrDefault(c=>!c.name.StartsWith("__"));
                    if(!clip)throw new Exception("Missing native animation "+pair[1]);
                    var copy=UnityEngine.Object.Instantiate(clip);copy.name=pair[0];copy.legacy=true;copy.wrapMode=WrapMode.Loop;
                    // CharacterController supplies displacement: remove root translation from native motion capture.
                    foreach(var binding in AnimationUtility.GetCurveBindings(copy))if(binding.propertyName.StartsWith("m_LocalPosition") && (binding.path=="" || !binding.path.Contains("/"))){var curve=AnimationUtility.GetEditorCurve(copy,binding);float initial=curve.Evaluate(0);AnimationUtility.SetEditorCurve(copy,binding,AnimationCurve.Constant(0,copy.length,initial));}
                    string cp=Root+"Actors/"+pair[0]+".anim";var saved=AssetDatabase.LoadAssetAtPath<AnimationClip>(cp);if(!saved){AssetDatabase.CreateAsset(copy,cp);saved=copy;}else{EditorUtility.CopySerialized(copy,saved);UnityEngine.Object.DestroyImmediate(copy);}
                    anim.AddClip(saved,pair[0]);anim[pair[0]].wrapMode=WrapMode.Loop;if(pair[0]=="idle")anim.clip=saved;
                }
                if(name=="Victor")VictorInsignia(root,anim);
                anim.playAutomatically=true;PrefabUtility.SaveAsPrefabAsset(root,Root+"Actors/"+name+".prefab");UnityEngine.Object.DestroyImmediate(root);
            }
            MakeWeapon();MakeEquipment();
        }
        static void MakeWeapon(){
            var root=new GameObject("Pistol");var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Weapons/Frame.fbx"),root.transform);
            var rs=model.GetComponentsInChildren<Renderer>();if(rs.Length==0)throw new Exception("Weapon mesh missing");var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);float scale=.25f/Mathf.Max(b.size.x,b.size.y,b.size.z);model.transform.localScale*=scale;model.transform.localPosition=(model.transform.localPosition-b.center)*scale;model.transform.localPosition=Quaternion.Euler(0,90,0)*model.transform.localPosition;model.transform.localRotation=Quaternion.Euler(0,90,0)*model.transform.localRotation;
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"Weapons/Pistol_1_Albedo.png"));m.SetFloat("_Metallic",.7f);m.SetFloat("_Smoothness",.5f);m=SaveMat("Pistol",m);foreach(var r in rs)r.sharedMaterial=m;
            PrefabUtility.SaveAsPrefabAsset(root,Root+"Actors/Pistol.prefab");UnityEngine.Object.DestroyImmediate(root);
        }
        static GameObject Box(string name,Vector3 p,Vector3 size,string mat,bool solid=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mats[mat];if(new[]{"painted_plaster_wall","brick_wall_001","concrete_floor_02","asphalt_02","metal_plate"}.Contains(mat) || mat.StartsWith("FloorSurface") || mat.StartsWith("WallSurface"))MetricUV(g,size);if(!solid)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());g.isStatic=true;return g;}
        static void MetricUV(GameObject g,Vector3 dimensions){
            Directory.CreateDirectory(Root+"Meshes");
            string key=dimensions.x.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+"_"+dimensions.y.ToString("F3",System.Globalization.CultureInfo.InvariantCulture)+"_"+dimensions.z.ToString("F3",System.Globalization.CultureInfo.InvariantCulture);
            string path=Root+"Meshes/Box_"+key+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(!mesh){mesh=UnityEngine.Object.Instantiate(g.GetComponent<MeshFilter>().sharedMesh);mesh.name="Metric surface "+key;var uv=mesh.uv;var n=mesh.normals;var v=mesh.vertices;for(int i=0;i<uv.Length;i++){Vector3 p=Vector3.Scale(v[i],dimensions);uv[i]=Mathf.Abs(n[i].y)>.5f?new Vector2(p.x,p.z)/2:Mathf.Abs(n[i].x)>.5f?new Vector2(p.z,p.y)/2:new Vector2(p.x,p.y)/2;}mesh.uv=uv;mesh.RecalculateTangents();AssetDatabase.CreateAsset(mesh,path);}g.GetComponent<MeshFilter>().sharedMesh=mesh;
        }
        static GameObject Model(string id,Vector3 p,float width,float yaw=0,bool solid=true){
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Media/"+id+"/"+id+".fbx");var root=new GameObject(id);root.transform.SetParent(parent);root.transform.position=Vector3.zero;
            var g=UnityEngine.Object.Instantiate(source,root.transform);g.name="Visual";var renderers=g.GetComponentsInChildren<Renderer>();var b=renderers[0].bounds;foreach(var r in renderers)b.Encapsulate(r.bounds);
            float height=id=="metal_office_desk"?.85f:id=="metal_stool_01"?.6f:id=="Shelf_01"?2.4f:0;float scale=height>0?height/Mathf.Max(.01f,b.size.y):width/Mathf.Max(.01f,b.size.x);g.transform.localScale*=scale;g.transform.localPosition=(g.transform.localPosition-new Vector3(b.center.x,b.min.y,b.center.z))*scale;
            foreach(var r in renderers){r.sharedMaterials=Enumerable.Repeat(mats[id],Mathf.Max(1,r.sharedMaterials.Length)).ToArray();r.gameObject.isStatic=true;}
            if(solid){var c=root.AddComponent<BoxCollider>();c.center=new Vector3(0,b.size.y*scale/2,0);c.size=b.size*scale;}
            root.transform.position=p;root.transform.localRotation=Quaternion.Euler(0,yaw+(id=="metal_office_desk"?180:0),0);root.isStatic=true;return root;
        }
        static void Sign(string value,Vector3 pos,float size=0.1f,Color? color=null,float yaw=0){var g=new GameObject(value.Replace('\n',' '));g.transform.SetParent(parent);g.transform.position=pos;g.transform.rotation=Quaternion.Euler(0,yaw,0);var t=g.AddComponent<TextMesh>();t.text=value;t.font=signFont;t.fontSize=64;t.characterSize=size*.52f;t.anchor=TextAnchor.MiddleCenter;t.alignment=TextAlignment.Center;t.color=color??new Color(.83f,.83f,.73f);var material=new Material(Shader.Find("BlackMarket/WorldText"));material.mainTexture=signFont.material.mainTexture;string path=Root+"Materials/WorldText.mat";var saved=AssetDatabase.LoadAssetAtPath<Material>(path);if(!saved){AssetDatabase.CreateAsset(material,path);saved=material;}else UnityEngine.Object.DestroyImmediate(material);g.GetComponent<MeshRenderer>().sharedMaterial=saved;}
        static Transform Mark(string id,Vector3 p,float yaw=0){var g=new GameObject("@"+id);g.transform.SetParent(parent);g.transform.position=p;g.transform.rotation=Quaternion.Euler(0,yaw,0);g.AddComponent<WorldMarker>().id=id;return g.transform;}
        static void Item(string id,string title,Vector3 p){var g=new GameObject("Interact / "+id);g.transform.SetParent(parent);g.transform.position=p;var i=g.AddComponent<Interaction>();i.id=id;i.title=title;}
        static void Light(string name,Vector3 p,Color color,float power,float range,LightType type=LightType.Point){var g=new GameObject(name);g.transform.SetParent(parent);g.transform.position=p;g.transform.rotation=Quaternion.Euler(90,0,0);var l=g.AddComponent<Light>();l.type=type;l.color=color;l.intensity=power;l.range=range;l.shadows=type==LightType.Spot?LightShadows.Soft:LightShadows.None;l.renderMode=LightRenderMode.Auto;if(type==LightType.Spot){l.spotAngle=105;l.innerSpotAngle=75;}}
        static void Fixture(Vector3 p,bool warm=false){Box("Fluorescent housing",p,new Vector3(1.6f,.08f,.3f),"Steel",false);Box("Diffuser",p-Vector3.up*.055f,new Vector3(1.45f,.025f,.2f),warm?"Amber":"WhiteLight",false);Light("Ceiling practical",p-Vector3.up*.15f,warm?new Color(1,.72f,.4f):new Color(.64f,.81f,1),warm?2.8f:3.8f,10,LightType.Spot);}
        static void Floor(float width,float depth,float z,string material){for(float x=-width/2;x<width/2-.01f;x+=4)for(float zz=z;zz<z+depth-.01f;zz+=4)Box("Floor tile",new Vector3(x+Mathf.Min(4,width/2-x)/2,-.15f,zz+Mathf.Min(4,z+depth-zz)/2),new Vector3(Mathf.Min(4,width/2-x),.3f,Mathf.Min(4,z+depth-zz)),material);}
        static void CrossWall(float z,float width,float opening=3,string material="painted_plaster_wall"){
            float side=(width-opening)/2;Box("Partition west",new Vector3(-(opening/2+side/2),1.8f,z),new Vector3(side,3.6f,.22f),material);Box("Partition east",new Vector3(opening/2+side/2,1.8f,z),new Vector3(side,3.6f,.22f),material);Box("Lintel",new Vector3(0,3.15f,z),new Vector3(opening,.9f,.25f),material);
            foreach(float x in new[]{-opening/2,opening/2})Box("Door jamb",new Vector3(x,1.4f,z),new Vector3(.09f,2.8f,.32f),"Steel");
        }
        static void Monitor(Vector3 p,string caption,float yaw=0){
            if(p.y<1.6f)p.y=1.24f;Box("Monitor neck",p-new Vector3(0,.28f,0),new Vector3(.09f,.16f,.06f),"Steel",false);
            Box("Monitor stand",p-new Vector3(0,.35f,0),new Vector3(.36f,.08f,.3f),"Steel",false);Box("Terminal",p,new Vector3(.8f,.48f,.09f),"Black",false);Box("CRT display",p-new Vector3(0,0,.053f),new Vector3(.71f,.38f,.015f),"Screen",false);Sign(caption,p-new Vector3(0,0,.065f),.025f,new Color(.5f,.9f,.68f),yaw);
        }
        static void Supply(Vector3 p,string id){Box("Medical supply",p+Vector3.up*.25f,new Vector3(.65f,.5f,.45f),"Steel");Box("Medical stripe",p+new Vector3(0,.32f,-.23f),new Vector3(.42f,.08f,.02f),"Paper",false);Box("Medical cross",p+new Vector3(0,.32f,-.245f),new Vector3(.08f,.25f,.02f),"Paper",false);Item(id,"TIẾP TẾ / MEDICAL",p+Vector3.forward*-.65f);}
        static void Rack(Vector3 p){Box("Server cabinet",p+Vector3.up*1.15f,new Vector3(1.1f,2.3f,.8f),"Steel");for(int i=0;i<10;i++){Box("Server blade",p+new Vector3(0,.22f+i*.2f,-.41f),new Vector3(.95f,.16f,.025f),"Black",false);Box("Status LED",p+new Vector3(.35f,.22f+i*.2f,-.43f),new Vector3(.018f,.02f,.01f),"Screen",false);}}
        static void BuildWorld(string name,int type){
            var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
            var root=new GameObject(name);parent=root.transform;signFont=Resources.Load<Font>("Fonts/Bold");
            CampaignFloor(type);
            var nav=root.AddComponent<NavMeshSurface>();nav.collectObjects=CollectObjects.Children;nav.useGeometry=NavMeshCollectGeometry.PhysicsColliders;nav.layerMask=~(1<<2);nav.BuildNavMesh();
            string navPath=Root+"Worlds/"+name+"Nav.asset";var old=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);if(old){EditorUtility.CopySerialized(nav.navMeshData,old);nav.RemoveData();nav.navMeshData=old;nav.AddData();}else AssetDatabase.CreateAsset(nav.navMeshData,navPath);
            PrefabUtility.SaveAsPrefabAsset(root,Root+"Worlds/"+name+".prefab");
            scene.name=name;EditorSceneManager.SaveScene(scene,"Assets/BlackMarket/Scenes/"+name+"_Environment.unity");
            SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true);
        }
        static void Bunker(bool final){
            float width=24,depth=44;Floor(width,depth,0,"concrete_floor_02");
            Box("West retaining wall",new Vector3(-12,1.9f,22),new Vector3(.4f,3.8f,44),"painted_plaster_wall");Box("East retaining wall",new Vector3(12,1.9f,22),new Vector3(.4f,3.8f,44),"painted_plaster_wall");
            Box("Rear retaining wall",new Vector3(0,1.9f,44),new Vector3(width,3.8f,.4f),"painted_plaster_wall");CrossWall(0,width,3);Box("Ceiling",new Vector3(0,4,22),new Vector3(width,.2f,44),"metal_plate");
            CrossWall(14,width,3.4f);CrossWall(28,width,3.4f);
            for(int z=3;z<44;z+=6){Fixture(new Vector3(-5,3.7f,z));Fixture(new Vector3(5,3.7f,z),true);Box("Ceiling truss",new Vector3(0,3.7f,z),new Vector3(24,.25f,.2f),"Steel",false);foreach(float x in new[]{-11.2f,11.2f})Box("Utility duct",new Vector3(x,3.2f,z),new Vector3(.55f,.4f,6),"metal_plate",false);}
            for(int z=2;z<43;z+=2){Box("Safety route",new Vector3(-1.4f,.012f,z),new Vector3(.06f,.015f,1.5f),"Paper",false);Box("Safety route",new Vector3(1.4f,.012f,z),new Vector3(.06f,.015f,1.5f),"Paper",false);}
            Sign(final?"CONTROL ROOM / KEEPER ACCESS":"−03 / NORTH POINT OPERATIONS",new Vector3(-6,2.6f,13.8f),.1f);
            Sign(final?"LOCAL OVERRIDE":"ORDER ROOM / 071",new Vector3(6,2.6f,27.8f),.1f);
            Sign(final?"THE MARKET":"SERVER / FOR_ALEX",new Vector3(0,2.8f,43.7f),.16f);
            for(int i=0;i<4;i++){Model("wooden_crate_01",new Vector3(-6,0,5+i*9),1.9f);Model("metal_tool_chest",new Vector3(7,0,7+i*8),2);}
            for(int i=0;i<4;i++)Rack(new Vector3(-10,0,30+i*3));
            Model("metal_office_desk",new Vector3(-7,0,10),2.8f);var pistol=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Actors/Pistol.prefab"),parent);pistol.transform.position=new Vector3(-7,.9f,10);pistol.transform.rotation=Quaternion.Euler(0,0,90);Item("pistol","PISTOL / ARMORY",new Vector3(-7,1,9.2f));
            Model("metal_tool_chest",new Vector3(6,0,22),2);Sign("071",new Vector3(6,1.5f,21.5f),.12f);Item("order","LOCKER 071",new Vector3(6,1,21));
            Model("metal_office_desk",new Vector3(-5,0,39),2.6f);Monitor(new Vector3(-5,1.25f,39),"FOR_ALEX\nENCRYPTED");Item("recording","FOR_ALEX / DATA DRIVE",new Vector3(-5,1,38.2f));
            Model("metal_office_desk",new Vector3(5,0,19),2.3f);Monitor(new Vector3(5,1.25f,19),final?"LOCAL OVERRIDE":"ORDER #071\nUPLINK READY");Item(final?"override":"upload",final?"KHÔI PHỤC KEEPER":"UPLINK / ORDER 071",new Vector3(5,1,18.2f));
            if(final){for(int i=0;i<5;i++)Monitor(new Vector3(-6+i*3,2,43.5f),"KEEPER NETWORK\n[ ONLINE ]");Item("final","SUCCESSOR TERMINAL",new Vector3(0,1,41.5f));}
            else {Model("rollershutter_door",new Vector3(0,0,43.7f),3);Item("exit","CHUYỂN KHU",new Vector3(0,1,42.5f));}
            Supply(new Vector3(10,0,12),"supply_a");Supply(new Vector3(-8,0,26),"supply_b");
            // Bake with the shutter open; NavMeshObstacle carving updates at runtime.
            var shutter=Box("Security shutter B",new Vector3(0,5.1f,28),new Vector3(3.35f,3,.2f),"metal_plate");shutter.isStatic=false;
            var obstacle=shutter.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.size=Vector3.one;obstacle.carving=true;
            // Marker is a parent, so runtime movement carries the collider and visual together.
            var marker=Mark("shutter",new Vector3(0,0,28));shutter.transform.SetParent(marker,true);shutter.transform.localPosition=new Vector3(0,1.5f,0);marker.position+=Vector3.up*3.5f;
            Light("Emergency B",new Vector3(0,3.3f,27),new Color(1,.25f,.05f),.8f,5);
            Mark("spawn",new Vector3(0,.1f,3));Mark("enemy_a",new Vector3(-3,.1f,18),180);Mark("enemy_b",new Vector3(4,.1f,32),180);Mark("enemy_c",new Vector3(-3,.1f,38),180);Mark("boss",new Vector3(0,.1f,35),180);Mark("alarm",new Vector3(-8,1,35));
            for(int i=0;i<3;i++){var c=Mark("camera_"+i,new Vector3(-10,3.2f,4+i*14));c.LookAt(new Vector3(1,1,10+i*13));Box("CCTV camera",c.position,new Vector3(.3f,.2f,.45f),"Paper",false);}
        }
        static void BuildEntry(){
            var previous=SceneManager.GetActiveScene();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
            new GameObject("BLACK MARKET / Campaign").AddComponent<Campaign>();
            RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.32f,.39f,.46f);RenderSettings.ambientEquatorColor=new Color(.22f,.25f,.28f);RenderSettings.ambientGroundColor=new Color(.055f,.065f,.07f);RenderSettings.ambientIntensity=.8f;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogDensity=.012f;RenderSettings.fogColor=new Color(.045f,.065f,.08f);
            var moon=new GameObject("Moonlight");moon.transform.rotation=Quaternion.Euler(55,-35,0);var sun=moon.AddComponent<Light>();sun.type=LightType.Directional;sun.color=new Color(.6f,.72f,.89f);sun.intensity=.5f;sun.shadows=LightShadows.Soft;
            var volume=new GameObject("Film look / restrained bloom").AddComponent<Volume>();volume.isGlobal=true;
            var profile=ScriptableObject.CreateInstance<VolumeProfile>();profile.Add<Tonemapping>().mode.Override(TonemappingMode.ACES);var bloom=profile.Add<Bloom>();bloom.intensity.Override(.18f);bloom.threshold.Override(1.2f);
            var vignette=profile.Add<Vignette>();vignette.intensity.Override(.22f);vignette.smoothness.Override(.5f);var color=profile.Add<ColorAdjustments>();color.postExposure.Override(.7f);color.saturation.Override(-12);color.contrast.Override(12);
            string path=Root+"NorthPointLook.asset";var old=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);if(old){AssetDatabase.DeleteAsset(path);}AssetDatabase.CreateAsset(profile,path);foreach(var c in profile.components)AssetDatabase.AddObjectToAsset(c,profile);volume.sharedProfile=profile;
            EditorSceneManager.SaveScene(scene,"Assets/BlackMarket/Scenes/NorthPoint.unity");SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/BlackMarket/Scenes/NorthPoint.unity",true)};
        }
        [MenuItem("BLACK MARKET/2 - Open game scene")]
        public static void Open(){if(EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");}
        public static void RebuildAndValidate(){Prepare();NorthPointValidation.ValidateAndBuild();}
        [MenuItem("BLACK MARKET/3 - Build Linux preview")]
        public static void BuildLinux(){Directory.CreateDirectory("Builds/Linux");PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneLinux64,false);PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneLinux64,new[]{GraphicsDeviceType.OpenGLCore});var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/BlackMarket/Scenes/NorthPoint.unity"},locationPathName="Builds/Linux/BlackMarket.x86_64",target=BuildTarget.StandaloneLinux64,options=BuildOptions.Development});File.WriteAllText("Documentation/build-result.txt",report.summary.result.ToString()+" / "+report.summary.totalErrors+" errors");}
        [MenuItem("BLACK MARKET/4 - Build Android preview")]
        public static void BuildAndroid(){Directory.CreateDirectory("Builds/Android");var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/BlackMarket/Scenes/NorthPoint.unity"},locationPathName="Builds/Android/BlackMarket.apk",target=BuildTarget.Android,options=BuildOptions.Development});Debug.Log("Android: "+report.summary.result);}
    }
    [InitializeOnLoad] public static class NorthPointEditorJobs {
        static double next;
        static NorthPointEditorJobs(){EditorApplication.update+=Update;}
        static void Update(){
            if(EditorApplication.timeSinceStartup<next || EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode)return;
            next=EditorApplication.timeSinceStartup+2;
            string path="Library/BlackMarket-job.txt";if(!File.Exists(path))return;
            string job=File.ReadAllText(path).Trim();File.Delete(path);
            try {if(job=="prepare")NorthPointBuilder.Prepare();else if(job=="build-linux")NorthPointBuilder.BuildLinux();else if(job=="validate")NorthPointValidation.Run();}
            catch(Exception e){Directory.CreateDirectory("Documentation");File.WriteAllText("Documentation/editor-error.txt",e.ToString());Debug.LogException(e);}
        }
    }
}
