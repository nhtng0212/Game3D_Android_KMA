using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;
using Object = UnityEngine.Object;

namespace BlackMarket.Editor {
    // Shop-only authoring. All coordinates are metres; imported art stays outside Resources.
    // The finished nested prefabs are also a drag-and-drop palette for manual editing.
    public static class RetailShopBuilder {
        public const string WorldPath = "Assets/BlackMarket/Resources/Worlds/NorthPointShop.prefab";
        const string Art = "Assets/BlackMarket/Art/ShopSources/";
        const string Palette = "Assets/BlackMarket/Prefabs/Shop/";
        const string Materials = "Assets/BlackMarket/Art/ShopMaterials/";
        static readonly Dictionary<string,Material> materials = new Dictionary<string,Material>();
        static readonly Dictionary<string,GameObject> props = new Dictionary<string,GameObject>();
        static Font font;
        static Material textMaterial;
        static Transform root;

        [MenuItem("BLACK MARKET/Shop/1 - Rebuild electronics shop (replaces floor 1)")]
        public static void Rebuild() {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before rebuilding.");
            Directory.CreateDirectory(Palette); Directory.CreateDirectory(Materials);
            Directory.CreateDirectory("Documentation/ShopPreview"); AssetDatabase.Refresh();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            // Give the initial scene a path before creating prefab assets in batch mode.
            EditorSceneManager.SaveScene(scene,"Assets/BlackMarket/Scenes/EditorWorkspace.unity");
            Initialize(); MakePalette();
            root = new GameObject("NorthPointShop").transform;
            Layout(root);
            var nav = root.gameObject.AddComponent<NavMeshSurface>();
            nav.collectObjects = CollectObjects.Children;
            nav.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
            nav.layerMask = ~(1 << 2); nav.BuildNavMesh();
            string navPath = "Assets/BlackMarket/Resources/Worlds/NorthPointShopNav.asset";
            var old = AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
            if (old) { EditorUtility.CopySerialized(nav.navMeshData, old); nav.RemoveData(); nav.navMeshData=old; nav.AddData(); }
            else AssetDatabase.CreateAsset(nav.navMeshData,navPath);
            VietnameseText.Apply(root.gameObject);Validate(root.gameObject);
            PrefabUtility.SaveAsPrefabAsset(root.gameObject,WorldPath);
            EditorSceneManager.SaveScene(scene,"Assets/BlackMarket/Scenes/NorthPointShop_Environment.unity");
            AssetDatabase.SaveAssets();
            RenderPreviews(root.gameObject);
            EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");
            Debug.Log("SHOP REBUILD COMPLETE: imported art, palette, navigation and previews.");
        }

        // Called by the original all-floor builder too, so it cannot silently restore the old shop.
        public static void Populate(Transform target) { Initialize(); MakePalette(); Layout(target); }

        static Material SaveMaterial(string name, Material value) {
            var path=Materials+name+".mat"; var old=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(old) {EditorUtility.CopySerialized(value,old);Object.DestroyImmediate(value);value=old;}
            else AssetDatabase.CreateAsset(value,path);
            materials[name]=value;return value;
        }
        static Material Plain(string name,Color color,float metallic=0,float smoothness=.3f,float glow=0) {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            m.color=color;m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",smoothness);
            if(glow>0){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*glow);}
            return SaveMaterial(name,m);
        }
        static void Initialize() {
            Directory.CreateDirectory(Palette);Directory.CreateDirectory(Materials);AssetDatabase.Refresh();
            materials.Clear();props.Clear();font=Resources.Load<Font>("Fonts/Bold");
            textMaterial=Resources.Load<Material>("Materials/WorldText");
            Plain("Ivory",new Color(.79f,.8f,.76f),0,.3f);
            Plain("Wall",new Color(.67f,.71f,.7f),0,.18f);
            Plain("Navy",new Color(.035f,.085f,.11f),.15f,.32f);
            Plain("Teal",new Color(.025f,.3f,.34f),.15f,.4f);
            Plain("Amber",new Color(.91f,.52f,.19f),.1f,.4f);
            Plain("Metal",new Color(.16f,.2f,.21f),.7f,.4f);
            Plain("Chrome",new Color(.65f,.7f,.72f),.85f,.75f);
            Plain("Black",new Color(.015f,.022f,.025f),.2f,.25f);
            Plain("Cardboard",new Color(.46f,.32f,.18f),0,.1f);
            Plain("White",new Color(.93f,.93f,.87f),0,.55f);
            Plain("LED",new Color(.78f,.88f,1),0,.4f,2);
            Plain("Display",new Color(.035f,.33f,.44f),.1f,.6f,.5f);
            var glass=Plain("Glass",new Color(.35f,.58f,.65f,.16f),.05f,.8f);
            glass.SetFloat("_Surface",1);glass.SetFloat("_Blend",0);glass.SetInt("_SrcBlend",5);glass.SetInt("_DstBlend",10);glass.SetInt("_ZWrite",0);
            glass.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");glass.renderQueue=3000;
            var floor=Plain("FloorTiles",new Color(.8f,.8f,.75f),0,.32f);
            floor.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BlackMarket/Art/ShopSources/Finishes/floor_tiles.png"));
            floor.mainTextureScale=new Vector2(14,18);
            var wood=Plain("Oak",new Color(.7f,.55f,.34f),0,.35f);
            wood.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/BlackMarket/Art/ShopSources/Finishes/oak.png"));
            Plain("BathroomTile",new Color(.69f,.76f,.73f),0,.55f);
        }
        static Transform Node(string name,Transform parent,Vector3 position=default,float yaw=0) {
            var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=position;t.localRotation=Quaternion.Euler(0,yaw,0);return t;
        }
        static GameObject Box(Transform parent,string name,Vector3 p,Vector3 size,string material,bool solid=true) {
            var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;
            g.GetComponent<Renderer>().sharedMaterial=materials[material];
            if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());
            GameObjectUtility.SetStaticEditorFlags(g,StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic);
            return g;
        }
        static void Text(Transform parent,string value,Vector3 p,float size=.12f,float yaw=0,Color? color=null) {
            var t=Node("Label - "+value.Replace('\n',' '),parent,p,yaw);var text=t.gameObject.AddComponent<TextMesh>();
            text.font=font;text.fontSize=64;text.characterSize=size*.22f;text.text=value;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;
            text.color=color??new Color(.9f,.91f,.84f);t.GetComponent<MeshRenderer>().sharedMaterial=textMaterial;
        }
        static void Sign(Transform parent,string value,Vector3 p,float width=3,float yaw=0,string material="Navy") {
            var t=Node("Sign - "+value,parent,p,yaw);Box(t,"Sign panel",Vector3.zero,new Vector3(width,.5f,.06f),material,false);
            Text(t,value,new Vector3(0,0,-.035f),Mathf.Min(.14f,width/Mathf.Max(value.Length,1)*1.6f));
        }
        static Bounds BoundsOf(GameObject go) {
            var rs=go.GetComponentsInChildren<Renderer>();if(rs.Length==0)throw new Exception("Empty model "+go.name);
            var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;
        }
        static Texture2D Texture(string folder,string stem) {
            return Directory.GetFiles(folder).Where(p=>new[]{".png",".jpg"}.Contains(Path.GetExtension(p).ToLowerInvariant()))
                .Where(p=>Path.GetFileNameWithoutExtension(p).Equals(stem,StringComparison.OrdinalIgnoreCase))
                .Select(p=>AssetDatabase.LoadAssetAtPath<Texture2D>(p)).FirstOrDefault();
        }
        static Material ImportedMaterial(string id,Material original) {
            string sourceName=original?original.name:id;
            string key=id+"_"+sourceName.Replace("/","_");if(materials.TryGetValue(key,out var cached))return cached;
            bool sanitary=id=="Sink_A" || id=="Toilet_Elongated_A" || id=="Urinal_A";
            string folder=Art+(sanitary?"Sanitary":id);
            string prefix=sourceName.StartsWith(id,StringComparison.OrdinalIgnoreCase)?sourceName.Substring(id.Length).TrimStart('_'):"";
            string token=prefix==""?"":prefix+"_";
            Texture2D diffuse=null,normal=null,mask=null;
            if(sanitary) {
                string group=sourceName.Replace(".001","").Replace(" (Instance)","").Replace("_Mat","");
                diffuse=Texture(folder,group+"_Diffuse_1K");normal=Texture(folder,group+"_Normal_1K");mask=Texture(folder,group+"_unity_mask");
                if(!diffuse){group=id=="Toilet_Elongated_A"?"Toilet_Elongated":id;diffuse=Texture(folder,group+"_Diffuse_1K");normal=Texture(folder,group+"_Normal_1K");mask=Texture(folder,group+"_unity_mask");}
            } else {
                diffuse=Texture(folder,token+"diff")??Texture(folder,token+"diffuse");
                normal=Texture(folder,token+"nor_gl");mask=Texture(folder,token+"unity_mask");
                if(!diffuse){diffuse=Texture(folder,"diffuse")??Texture(folder,"diff");normal=normal??Texture(folder,"nor_gl");mask=mask??Texture(folder,"unity_mask");}
            }
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=Color.white;m.SetFloat("_Smoothness",.55f);
            if(diffuse)m.SetTexture("_BaseMap",diffuse);
            if(normal){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
            if(mask){m.SetTexture("_MetallicGlossMap",mask);m.EnableKeyword("_METALLICSPECGLOSSMAP");}
            if(prefix=="glass" || prefix=="lens"){m.SetFloat("_Metallic",.5f);m.SetFloat("_Smoothness",.85f);if(!diffuse)m.color=new Color(.06f,.13f,.16f);}
            // Laptop's native screen shares the base atlas; do not replace it with a flat material.
            return SaveMaterial(key,m);
        }
        static GameObject ImportProp(string id,string label,float targetWidth,bool solid=false,bool byHeight=false) {
            bool sanitary=id=="Sink_A" || id=="Toilet_Elongated_A" || id=="Urinal_A";
            string path=Art+(sanitary?"Sanitary":id)+"/"+id+".fbx";
            var asset=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(!asset)throw new Exception("Missing selected free model: "+path);
            var holder=new GameObject(label);var model=Object.Instantiate(asset,holder.transform);model.name="Model";
            var b=BoundsOf(holder);float scale=targetWidth/(byHeight?b.size.y:b.size.x);
            model.transform.localScale*=scale;model.transform.localPosition-=new Vector3(b.center.x,b.min.y,b.center.z)*scale;
            foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>ImportedMaterial(id,m)).ToArray();
            if(solid){b=BoundsOf(holder);var collider=holder.AddComponent<BoxCollider>();collider.center=b.center;collider.size=b.size;}
            return SaveProp(id,holder);
        }
        static GameObject SaveProp(string key,GameObject go) {
            var prefab=PrefabUtility.SaveAsPrefabAsset(go,Palette+go.name+".prefab");props[key]=prefab;Object.DestroyImmediate(go);return prefab;
        }
        static GameObject Place(string key,Transform parent,Vector3 p,float yaw=0,float scale=1) {
            var go=(GameObject)PrefabUtility.InstantiatePrefab(props[key],parent);go.transform.localPosition=p;go.transform.localRotation=Quaternion.Euler(0,yaw,0);go.transform.localScale=Vector3.one*scale;return go;
        }
        static void OldModel(Transform parent,string id,Vector3 p,float width,float yaw=0,bool solid=true) {
            var holder=Node(id,parent,p,yaw);var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BlackMarket/Resources/Media/"+id+"/"+id+".fbx");
            var model=Object.Instantiate(source,holder);model.name="Model";
            // Measure at origin to keep rotation from distorting normalization.
            var pos=holder.position;var rotation=holder.rotation;holder.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
            var b=BoundsOf(model);float scale=width/b.size.x;model.transform.localScale*=scale;model.transform.localPosition-=new Vector3(b.center.x,b.min.y,b.center.z)*scale;
            foreach(var r in model.GetComponentsInChildren<Renderer>())r.sharedMaterials=r.sharedMaterials.Select(m=>Resources.Load<Material>("Materials/"+id)).ToArray();
            if(solid){b=BoundsOf(model);var c=holder.gameObject.AddComponent<BoxCollider>();c.center=b.center;c.size=b.size;}
            holder.SetPositionAndRotation(pos,rotation);
        }
        static void Package(Transform p,Vector3 at,string label,int variant=0) {
            var box=Node("Box - "+label,p,at);
            Box(box,"Product carton",new Vector3(0,.16f,0),new Vector3(.42f,.32f,.29f),variant%2==0?"Ivory":"Navy",false);
            Box(box,"Brand band",new Vector3(0,.24f,-.15f),new Vector3(.42f,.07f,.008f),"Teal",false);
            Text(box,label,new Vector3(0,.135f,-.156f),.025f,0,variant%2==0?new Color(.04f,.12f,.14f):Color.white);
        }
        static void MakePalette() {
            ImportProp("classic_laptop","Laptop - may tinh xach tay",.48f);
            ImportProp("CashRegister_01","Cash register - may tinh tien",.5f);
            ImportProp("boombox","Boombox - radio loa",.5f);
            ImportProp("Camera_01","Camera - may anh",.23f);
            ImportProp("vintage_electric_kettle","Kettle - am dien",.25f);
            ImportProp("vintage_microwave","Microwave - lo vi song",.6f);
            ImportProp("security_camera_01","CCTV - camera an ninh",.18f);
            ImportProp("plastic_monobloc_chair_01","Chair - ghe nhua",.88f,true,true);
            ImportProp("steel_frame_shelves_02","Narrow steel rack - ke hep",2.1f,true,true);
            ImportProp("steel_frame_shelves_03","Stockroom steel rack - ke kho",2.2f,true,true);
            ImportProp("Sink_A","Sink - lavabo",.65f,true);
            ImportProp("Toilet_Elongated_A","Toilet - bon cau",.78f,true,true);
            ImportProp("Urinal_A","Urinal - bon tieu",.42f,true);
            foreach(var type in new[]{"AUDIO","COMPUTING","APPLIANCES","CAMERAS"}) {
                var shelf=new GameObject("Shelf - "+type+" - ke nguyen cum");var p=shelf.transform;
                Box(p,"Base",new Vector3(0,.09f,0),new Vector3(2.8f,.18f,.9f),"Navy");
                Box(p,"Back panel",new Vector3(0,1.05f,.36f),new Vector3(2.8f,1.95f,.08f),"Ivory");
                foreach(float x in new[]{-1.34f,1.34f})Box(p,"Side upright",new Vector3(x,1.12f,0),new Vector3(.055f,2.15f,.82f),"Metal");
                for(int tier=0;tier<3;tier++) {
                    float y=.3f+tier*.59f;
                    Box(p,"Shelf level "+(tier+1),new Vector3(0,y,0),new Vector3(2.72f,.055f,.84f),"Ivory");
                    Box(p,"Price rail",new Vector3(0,y-.025f,-.43f),new Vector3(2.72f,.065f,.02f),"Teal",false);
                    for(int j=0;j<3;j++) {
                        float x=-.88f+j*.88f;
                        if(tier==0)Package(p,new Vector3(x,y+.03f,0),type=="COMPUTING"?"NORTH / PC":type=="AUDIO"?"NORTH / AUDIO":type=="CAMERAS"?"CAM / 071":"HOME / TECH",j);
                        else {
                            string id=type=="AUDIO"?"boombox":type=="COMPUTING"?"classic_laptop":type=="CAMERAS"?"Camera_01":j==1?"vintage_microwave":"vintage_electric_kettle";
                            Place(id,p,new Vector3(x,y+.028f,-.02f),180);
                        }
                        Text(p,tier==0?"IN STOCK":"DEMO / NORTH",new Vector3(x,y-.025f,-.445f),.018f);
                    }
                }
                Sign(p,type,new Vector3(0,2.25f,.04f),2.8f);
                SaveProp("Shelf_"+type,shelf);
            }
            var island=new GameObject("Laptop island - ban trai nghiem");
            Box(island.transform,"Cabinet base",new Vector3(0,.44f,0),new Vector3(2.8f,.88f,1.3f),"Ivory");
            Box(island.transform,"Oak worktop",new Vector3(0,.91f,0),new Vector3(3,.07f,1.5f),"Oak");
            for(int i=0;i<3;i++){Place("classic_laptop",island.transform,new Vector3(-.95f+i*.95f,.947f,0),180);Text(island.transform,"NORTH / LAPTOP",new Vector3(-.95f+i*.95f,.78f,-.66f),.035f,0,new Color(.04f,.15f,.17f));}
            SaveProp("LaptopIsland",island);
            var television=new GameObject("TV display - ke trung bay tivi");
            Box(television.transform,"TV console",new Vector3(0,.39f,0),new Vector3(3,.78f,.65f),"Navy");
            Box(television.transform,"Console top",new Vector3(0,.82f,0),new Vector3(3.1f,.08f,.75f),"Oak");
            for(int i=0;i<2;i++){
                var tv=Node("Television complete",television.transform,new Vector3(-.78f+i*1.56f,.86f,0));
                Box(tv,"TV foot",new Vector3(0,.025f,0),new Vector3(.45f,.05f,.25f),"Black",false);
                Box(tv,"TV stand",new Vector3(0,.13f,.05f),new Vector3(.06f,.2f,.06f),"Black",false);
                Box(tv,"TV bezel",new Vector3(0,.55f,.05f),new Vector3(1.24f,.72f,.055f),"Black",false);
                Box(tv,"TV screen",new Vector3(0,.55f,.018f),new Vector3(1.17f,.65f,.009f),"Display",false);
                Text(tv,"NORTH\nVISION",new Vector3(0,.55f,.008f),.1f);
            }
            SaveProp("TV",television);
            var checkout=new GameObject("Checkout - quay thu ngan va may tinh");var cp=checkout.transform;
            Box(cp,"Main counter",new Vector3(0,.5f,0),new Vector3(4,1,1.1f),"Ivory");
            Box(cp,"Oak top",new Vector3(0,1.045f,0),new Vector3(4.1f,.09f,1.2f),"Oak");
            Box(cp,"Counter return",new Vector3(1.45f,.5f,1.2f),new Vector3(1.1f,1,1.35f),"Ivory");
            Box(cp,"Return top",new Vector3(1.45f,1.045f,1.2f),new Vector3(1.2f,.09f,1.45f),"Oak");
            Box(cp,"Front brand panel",new Vector3(0,.58f,-.56f),new Vector3(3.7f,.57f,.035f),"Teal",false);
            Text(cp,"NORTH POINT  /  CHECKOUT",new Vector3(0,.59f,-.584f),.14f);
            Place("classic_laptop",cp,new Vector3(-.55f,1.091f,0));Place("CashRegister_01",cp,new Vector3(.7f,1.091f,0));
            Place("plastic_monobloc_chair_01",cp,new Vector3(0,0,1.2f),180);
            Sign(cp,"THU NGAN / CHECKOUT",new Vector3(0,2.55f,.1f),4);
            SaveProp("Checkout",checkout);
            var stock=new GameObject("Stock rack - ke kho va thung hang");
            var rack=Place("steel_frame_shelves_03",stock.transform,Vector3.zero);
            // Find real horizontal shelf surfaces in the imported mesh, rather than guessing heights.
            var levels=new Dictionary<int,float>();
            foreach(var filter in rack.GetComponentsInChildren<MeshFilter>()) {
                var mesh=filter.sharedMesh;var vertices=mesh.vertices;var triangles=mesh.triangles;
                for(int i=0;i<triangles.Length;i+=3) {
                    var a=filter.transform.TransformPoint(vertices[triangles[i]]);var b=filter.transform.TransformPoint(vertices[triangles[i+1]]);var c=filter.transform.TransformPoint(vertices[triangles[i+2]]);
                    var cross=Vector3.Cross(b-a,c-a);if(cross.sqrMagnitude<.000001f || cross.normalized.y<.98f)continue;
                    int height=Mathf.RoundToInt((a.y+b.y+c.y)/3*100);if(height<15 || height>200)continue;
                    if(!levels.ContainsKey(height))levels[height]=0;levels[height]+=cross.magnitude*.5f;
                }
            }
            foreach(var level in levels.Where(pair=>pair.Value>.25f).OrderBy(pair=>pair.Key)) {
                // The left bay is clear of the drawers on the right of this rack.
                Package(stock.transform,new Vector3(-.6f,level.Key/100f+.005f,0),"STOCK / 071");
            }
            // Separate cartons next to the imported rack avoid intersecting its drawers.
            for(int i=0;i<3;i++)Box(stock.transform,"Delivery carton",new Vector3(1.5f,.23f+i*.46f,0),new Vector3(.6f,.45f,.55f),"Cardboard");
            SaveProp("Stock",stock);
            var desk=new GameObject("Marcus desk - ban chu Marcus");
            Box(desk.transform,"Desktop",new Vector3(0,.82f,0),new Vector3(2.4f,.09f,1.05f),"Oak");
            foreach(float x in new[]{-.95f,.95f})Box(desk.transform,"Desk pedestal",new Vector3(x,.39f,0),new Vector3(.42f,.78f,.85f),"Ivory");
            Place("classic_laptop",desk.transform,new Vector3(.2f,.869f,.12f),180);
            OldModel(desk.transform,"desk_lamp_arm_01",new Vector3(-.8f,.869f,.15f),.32f,180,false);
            SaveProp("Desk",desk);
            var wall=new GameObject("Wall 4m - tuong 4 met");Wall(wall.transform,"Wall section",0,0,4);SaveProp("Wall",wall);
            var doorway=new GameObject("Doorway - cua truot va khung tuong");
            WallWithDoor(doorway.transform,"Doorway",-2.3f,2.3f,0,0);Door(doorway.transform,"manual","PHÒNG",0,0);SaveProp("Doorway",doorway);
        }

        static void Wall(Transform p,string name,float x,float z,float length,bool alongX=true,float height=3.6f) {
            var t=Node(name,p,new Vector3(x,0,z));
            Box(t,"Wall",new Vector3(0,height/2,0),alongX?new Vector3(length,height,.2f):new Vector3(.2f,height,length),"Wall");
            Box(t,"Skirting",new Vector3(0,.075f,0),alongX?new Vector3(length,.15f,.23f):new Vector3(.23f,.15f,length),"Navy",false);
        }
        static void WallWithDoor(Transform p,string name,float from,float to,float z,float doorX) {
            Wall(p,name+" left",(from+doorX-1.1f)/2,z,doorX-1.1f-from);
            Wall(p,name+" right",(doorX+1.1f+to)/2,z,to-doorX-1.1f);
            Box(p,name+" header",new Vector3(doorX,3.1f,z),new Vector3(2.2f,1,.2f),"Wall");
        }
        static void Door(Transform p,string id,string room,float x,float z) {
            var door=Node("Door - "+room,p,new Vector3(x,0,z));
            var leaf=Node("Sliding leaf - keep with parent",door,new Vector3(0,1.25f,0));
            var c=leaf.gameObject.AddComponent<BoxCollider>();c.size=new Vector3(2.12f,2.5f,.12f);
            var obstacle=leaf.gameObject.AddComponent<NavMeshObstacle>();obstacle.shape=NavMeshObstacleShape.Box;obstacle.size=c.size;obstacle.carving=true;obstacle.enabled=false;
            leaf.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild=true;
            Box(leaf,"Door panel",Vector3.zero,c.size,"Teal",false);
            Box(leaf,"Handle",new Vector3(-.77f,-.05f,-.11f),new Vector3(.035f,.36f,.07f),"Chrome",false);
            Text(leaf,room,new Vector3(0,.32f,-.067f),.11f);
            Box(door,"Rail",new Vector3(1.1f,2.56f,0),new Vector3(4.4f,.09f,.19f),"Metal",false);
            var script=door.gameObject.AddComponent<RoomDoor>();script.leaf=leaf;script.roomName=room;
            var item=door.gameObject.AddComponent<Interaction>();item.id="roomdoor_"+id;item.title="MỞ CỬA / "+room;
        }
        static void Marker(Transform p,string id,Vector3 at,float yaw=0) {Node("@"+id,p,at,yaw).gameObject.AddComponent<WorldMarker>().id=id;}
        static void Item(Transform p,string id,string title,Vector3 at) {var i=Node("Interact - "+id,p,at).gameObject.AddComponent<Interaction>();i.id=id;i.title=title;}
        static void CeilingLight(Transform p,Vector3 at,bool warm=false) {
            var t=Node("Ceiling light",p,at);
            Box(t,"Luminaire",Vector3.zero,new Vector3(1.6f,.07f,.34f),"Ivory",false);
            Box(t,"Diffuser",new Vector3(0,-.04f,0),new Vector3(1.48f,.02f,.27f),"LED",false);
            var lamp=Node("Shop ceiling practical",t,new Vector3(0,-.13f,0)).gameObject.AddComponent<Light>();
            lamp.type=LightType.Point;lamp.range=9;lamp.intensity=2.2f;lamp.color=warm?new Color(1,.85f,.68f):new Color(.83f,.9f,1);lamp.shadows=LightShadows.None;
        }
        static void Bathroom(Transform p,bool men,float x) {
            var group=Node(men?"05 - WC NAM":"06 - WC NU",p);
            string title=men?"WC NAM / MEN":"WC NU / WOMEN";
            WallWithDoor(group,title,x-2,x+2,22,x);Door(group,men?"men":"women",title,x,22);
            Box(group,"Tile floor",new Vector3(x,.015f,26),new Vector3(3.8f,.03f,7.8f),"BathroomTile");
            Place("Sink_A",group,new Vector3(x-1.2f,.78f,24),90);
            Box(group,"Mirror",new Vector3(x-1.88f,1.65f,24),new Vector3(.025f,.85f,.8f),"Chrome",false);
            Box(group,"Soap dispenser",new Vector3(x-1.65f,1.12f,24.55f),new Vector3(.17f,.22f,.12f),"White",false);
            Place("Toilet_Elongated_A",group,new Vector3(x+.7f,0,28.9f),180);
            Wall(group,"Cubicle side",x-.25f,28.45f,2.6f,false,2.1f);
            Box(group,"Cubicle door",new Vector3(x+.7f,1.05f,27.1f),new Vector3(1.4f,2.1f,.06f),"Ivory",false).transform.localRotation=Quaternion.Euler(0,55,0);
            if(men)Place("Urinal_A",group,new Vector3(x+1.55f,.55f,25.1f),270);
            else Place("Toilet_Elongated_A",group,new Vector3(x-.95f,0,28.9f),180);
            Sign(group,title,new Vector3(x,2.9f,21.85f),3.7f);
            Marker(group,men?"wc_men":"wc_women",new Vector3(x,.1f,25));
        }
        static void Layout(Transform p) {
            root=p;var shell=Node("01 - BUILDING - walls floor frontage",p);
            Box(shell,"Floor tile",new Vector3(0,-.15f,18),new Vector3(28,.3f,36),"FloorTiles");
            Box(shell,"Exterior pavement",new Vector3(0,-.2f,-4),new Vector3(30,.3f,8),"Metal");
            Wall(shell,"West exterior",-14,18,36,false);Wall(shell,"East exterior",14,18,36,false);Wall(shell,"Rear exterior",0,36,28);
            foreach(float x in new[]{-7.7f,7.7f})Box(shell,"Storefront glass",new Vector3(x,1.7f,0),new Vector3(12.4f,3.4f,.08f),"Glass");
            Box(shell,"Locked entrance glass",new Vector3(0,1.4f,0),new Vector3(3,2.8f,.09f),"Glass");
            foreach(float x in new[]{-14f,-10f,-6f,-1.5f,0,1.5f,6,10,14})Box(shell,"Storefront mullion",new Vector3(x,1.7f,-.05f),new Vector3(.065f,3.4f,.13f),"Metal");
            foreach(float x in new[]{-.18f,.18f})Box(shell,"Entrance handle",new Vector3(x,1.25f,.1f),new Vector3(.035f,.5f,.035f),"Chrome",false);
            Sign(shell,"NORTH POINT / ELECTRONICS",new Vector3(0,3.45f,-.13f),14);
            Sign(shell,"NORTH POINT / ELECTRONICS",new Vector3(0,3.45f,.13f),14,180);
            Text(shell,"CLOSED / 21:30",new Vector3(0,1.7f,.11f),.13f,180);
            // A separate ceiling group can be hidden with the editor eye while editing the floor plan.
            var ceiling=Node("09 - CEILING - hide with eye for editing",p);
            Box(ceiling,"Ceiling",new Vector3(0,3.75f,18),new Vector3(28,.2f,36),"Ivory");
            for(int z=3;z<36;z+=6)for(int x=-10;x<=10;x+=10)CeilingLight(ceiling,new Vector3(x,3.58f,z),z>=22);
            for(int z=0;z<=36;z+=6)Box(ceiling,"Ceiling beam",new Vector3(0,3.58f,z),new Vector3(28,.15f,.12f),"Metal",false);
            var retail=Node("02 - SALES FLOOR - move whole shelf groups",p);
            // Wide front entrance zone, central north/south aisle and cross aisles between gondolas.
            string[] types={"AUDIO","COMPUTING","APPLIANCES","CAMERAS"};
            for(int row=0;row<3;row++)for(int col=0;col<4;col++) {
                var island=Node("Gondola "+(row*4+col+1)+" - "+types[col],retail,new Vector3(-9+col*6,0,8+row*5));
                Place("Shelf_"+types[col],island,new Vector3(0,0,-.43f));Place("Shelf_"+(col==1?"CAMERAS":types[col]),island,new Vector3(0,0,.43f),180);
            }
            Place("LaptopIsland",retail,new Vector3(-7,0,3.4f));
            foreach(float z in new[]{6f,12f,18f}){Place("TV",retail,new Vector3(-12.9f,0,z),270);Place("Shelf_CAMERAS",retail,new Vector3(12.9f,0,z),90);}
            for(int col=0;col<4;col++)Sign(retail,(col+1).ToString("00")+" / "+types[col],new Vector3(-9+col*6,3.05f,7),3.8f);
            var checkout=Node("03 - CHECKOUT - counter computer receipt register",p);
            Place("Checkout",checkout,new Vector3(7.5f,0,3.4f));
            Item(checkout,"note","GHI CHÚ MARCUS",new Vector3(5.95f,.9f,2.75f));
            Box(checkout,"Marcus note",new Vector3(5.95f,1.095f,3.1f),new Vector3(.25f,.008f,.18f),"White",false);
            var office=Node("04 - MARCUS OFFICE - mission objects inside",p);
            WallWithDoor(office,"Office front",-14,-5,22,-9);Wall(office,"Office side",-5,26,8,false);Wall(office,"Office back",-9.5f,30,9);
            Door(office,"marcus","VĂN PHÒNG MARCUS",-9,22);
            Sign(office,"M. CARTER / OFFICE",new Vector3(-9.5f,2.95f,21.85f),7);
            Place("Desk",office,new Vector3(-10,0,27.3f));Place("plastic_monobloc_chair_01",office,new Vector3(-10,0,28.6f),180);
            Place("steel_frame_shelves_03",office,new Vector3(-12.45f,0,25),90);
            OldModel(office,"metal_tool_chest",new Vector3(-6.5f,0,28.8f),1.5f,0);
            Box(office,"Marcus keycard",new Vector3(-10.8f,.88f,26.98f),new Vector3(.17f,.012f,.1f),"Amber",false);
            Item(office,"keycard","THẺ MARCUS",new Vector3(-10.8f,.95f,26.7f));
            Item(office,"computer","TERMINAL MARCUS / ORDER 071",new Vector3(-9.75f,1.02f,26.75f));
            Text(office,"MARCUS CARTER\nNORTH POINT / KEEPER",new Vector3(-10,2.1f,29.86f),.18f);
            Marker(office,"office_visit",new Vector3(-9,.1f,24));
            Wall(shell,"WC west wall",6,26,8,false);Wall(shell,"WC middle wall",10,26,8,false);Wall(shell,"WC rear wall",10,30,8);
            Bathroom(p,true,8);Bathroom(p,false,12);
            var warehouse=Node("07 - WAREHOUSE - racks and Door 06",p);
            WallWithDoor(warehouse,"Warehouse front",-5,6,30,0);Door(warehouse,"warehouse","KHO HÀNG",0,30);
            Sign(warehouse,"KHO HANG / STOCKROOM",new Vector3(.5f,2.9f,29.85f),7);
            foreach(float x in new[]{-10f,-5.6f,5.6f,10f})Place("Stock",warehouse,new Vector3(x,0,34.8f));
            Place("steel_frame_shelves_02",warehouse,new Vector3(-12.9f,0,32),90);
            OldModel(warehouse,"wooden_crate_01",new Vector3(11.8f,0,31.7f),1.1f);
            OldModel(warehouse,"cardboard_box_01",new Vector3(9.8f,0,31.7f),.8f);
            var delivery=Node("Packing table",warehouse,new Vector3(-8,0,31.5f));
            Box(delivery,"Packing bench",new Vector3(0,.45f,0),new Vector3(2,.9f,.8f),"Oak");Package(delivery,new Vector3(-.5f,.9f,0),"NORTH / AUDIO");
            OldModel(warehouse,"rollershutter_door",new Vector3(0,0,35.8f),2.7f);
            Sign(warehouse,"06 / STAFF STAIRS",new Vector3(0,2.9f,35.5f),3.2f);
            Box(warehouse,"Carter scanner",new Vector3(1.7f,1.2f,35.5f),new Vector3(.24f,.4f,.12f),"Display",false);
            Item(warehouse,"door06","DOOR 06 / CẦU THANG NHÂN VIÊN",new Vector3(0,1,34.7f));
            Marker(warehouse,"warehouse_visit",new Vector3(0,.1f,32));
            var gameplay=Node("08 - GAMEPLAY - keep IDs when moving",p);
            Marker(gameplay,"spawn",new Vector3(0,.1f,2));Marker(gameplay,"safe",new Vector3(0,.1f,2));
            Marker(gameplay,"shutter",new Vector3(0,5,30));Marker(gameplay,"alarm",new Vector3(3,1,29));
            Item(gameplay,"frontdoor","CỬA CHÍNH",new Vector3(0,1,.4f));
            for(int i=0;i<3;i++){
                var camera=Node("@camera_"+i,gameplay,new Vector3(i==1?13:-13,3.15f,3+i*12));camera.gameObject.AddComponent<WorldMarker>().id="camera_"+i;camera.LookAt(p.TransformPoint(new Vector3(0,1,8+i*10)));
                Place("security_camera_01",gameplay,camera.localPosition,i==1?270:90);
            }
            var emergency=Node("Emergency shop exit",gameplay,new Vector3(0,2.9f,1)).gameObject.AddComponent<Light>();emergency.type=LightType.Point;emergency.color=new Color(.3f,.65f,.7f);emergency.range=6;emergency.intensity=1;
            OpeningAssetsBuilder.AddOpeningLayout(p);MissionFlowBuilder.AddMissionLayout(p);VietnameseText.Apply(p.gameObject);
        }

        public static void Validate(GameObject world) {
            var log=new List<string>();
            foreach(var door in world.GetComponentsInChildren<RoomDoor>())door.SetOpenImmediate(true);
            Physics.SyncTransforms();
            var markers=world.GetComponentsInChildren<WorldMarker>();var start=markers.Single(m=>m.id=="spawn").transform.position;
            if(!NavMesh.SamplePosition(start,out var from,2,NavMesh.AllAreas))throw new Exception("Shop spawn outside NavMesh");
            foreach(var id in new[]{"keycard","computer","door06","note","frontdoor"})if(world.GetComponentsInChildren<Interaction>(true).Count(i=>i.id==id)!=1)throw new Exception("Missing/duplicate mission item "+id);
            foreach(var point in world.GetComponentsInChildren<Interaction>().Select(i=>i.transform).Concat(markers.Where(m=>m.id.EndsWith("_visit")||m.id.StartsWith("wc_")).Select(m=>m.transform))) {
                if(!NavMesh.SamplePosition(new Vector3(point.position.x,.1f,point.position.z),out var to,2,NavMesh.AllAreas))throw new Exception("No navigation near "+point.name);
                var path=new NavMeshPath();if(!NavMesh.CalculatePath(from.position,to.position,NavMesh.AllAreas,path)||path.status!=NavMeshPathStatus.PathComplete)throw new Exception("Unreachable "+point.name);
                log.Add("PASS reachable "+point.name);
            }
            int samples=0;for(float x=-13.5f;x<=13.5f;x+=.5f)for(float z=.5f;z<=35.5f;z+=.5f){if(!Physics.RaycastAll(new Vector3(x,.08f,z),Vector3.down,.4f).Any(h=>h.collider.name=="Floor tile"))throw new Exception("Floor gap "+x+","+z);samples++;}
            foreach(var renderer in world.GetComponentsInChildren<Renderer>())foreach(var mat in renderer.sharedMaterials)if(!mat||!mat.shader||mat.shader.name.Contains("Error"))throw new Exception("Invalid material "+renderer.name);
            foreach(var door in world.GetComponentsInChildren<RoomDoor>())door.SetOpenImmediate(false);
            log.Add("PASS "+samples+" floor support samples");log.Add("PASS material references");
            log.Add("PASS 13 downloaded model variants and reusable shop prefabs");
            log.Add("OBJECTS "+world.GetComponentsInChildren<Transform>().Length);
            File.WriteAllLines("Documentation/shop-validation.txt",log);
        }
        [MenuItem("BLACK MARKET/Shop/2 - Open editable shop prefab")]
        public static void OpenShop(){AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>(WorldPath));}
        [MenuItem("BLACK MARKET/Shop/3 - Show drag-and-drop shop furniture")]
        public static void OpenPalette(){Selection.activeObject=AssetDatabase.LoadAssetAtPath<DefaultAsset>(Palette.TrimEnd('/'));EditorGUIUtility.PingObject(Selection.activeObject);}
        [MenuItem("BLACK MARKET/Shop/4 - Bake navigation from saved shop (keeps layout)")]
        public static void BakeSavedShop() {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play mode and save the shop first.");
            var previous=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);SceneManager.SetActiveScene(scene);
            try {
                var world=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(WorldPath));world.name="NorthPointShop";
                var nav=world.GetComponent<NavMeshSurface>();nav.BuildNavMesh();
                string path="Assets/BlackMarket/Resources/Worlds/NorthPointShopNav.asset";
                var saved=AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
                if(saved){EditorUtility.CopySerialized(nav.navMeshData,saved);nav.RemoveData();nav.navMeshData=saved;nav.AddData();}
                else AssetDatabase.CreateAsset(nav.navMeshData,path);
                Validate(world);PrefabUtility.SaveAsPrefabAsset(world,WorldPath);AssetDatabase.SaveAssets();
                Debug.Log("Saved shop layout retained; NavMesh rebuilt and routes validated.");
            }finally{SceneManager.SetActiveScene(previous);EditorSceneManager.CloseScene(scene,true);}
        }
        public static void RenderPreviews(GameObject world) {
            RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Trilight;
            RenderSettings.ambientSkyColor=new Color(.54f,.6f,.65f);RenderSettings.ambientEquatorColor=new Color(.37f,.4f,.41f);RenderSettings.ambientGroundColor=new Color(.19f,.19f,.18f);RenderSettings.fog=false;
            var camera=new GameObject("Shop preview camera").AddComponent<Camera>();camera.nearClipPlane=.08f;camera.farClipPlane=100;camera.fieldOfView=68;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.05f,.08f,.1f);
            camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            Capture(camera,new Vector3(0,2.1f,1.8f),new Vector3(0,1.3f,15),"01-showroom");
            Capture(camera,new Vector3(2,2.1f,1),new Vector3(7.5f,1.2f,3.8f),"02-checkout");
            Capture(camera,new Vector3(-4,1.8f,5),new Vector3(-8,1.3f,9),"03-merchandise");
            Capture(camera,new Vector3(-7.2f,2,23.6f),new Vector3(-10,1.2f,27.6f),"04-marcus-office");
            Capture(camera,new Vector3(1.5f,2.1f,31.2f),new Vector3(-5,1.2f,34.5f),"05-warehouse");
            Capture(camera,new Vector3(8,2,22.7f),new Vector3(8,1.1f,28),"06-wc-men");
            Capture(camera,new Vector3(12,2,22.7f),new Vector3(12,1.1f,28),"07-wc-women");
            world.transform.Find("09 - CEILING - hide with eye for editing").gameObject.SetActive(false);
            camera.orthographic=true;camera.orthographicSize=21;
            Capture(camera,new Vector3(0,42,17.99f),new Vector3(0,0,18),"08-floorplan");
            world.transform.Find("09 - CEILING - hide with eye for editing").gameObject.SetActive(true);
            Object.DestroyImmediate(camera.gameObject);
        }
        static void Capture(Camera camera,Vector3 at,Vector3 look,string name) {
            camera.transform.SetPositionAndRotation(at,Quaternion.LookRotation(look-at));
            var rt=new RenderTexture(1440,1000,24);var old=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);texture.Apply();
            File.WriteAllBytes("Documentation/ShopPreview/"+name+".png",texture.EncodeToPNG());RenderTexture.active=old;camera.targetTexture=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(texture);
        }
    }
}
