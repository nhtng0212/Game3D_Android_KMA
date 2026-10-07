using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
using Object=UnityEngine.Object;
namespace BlackMarket.Editor {
 public static class ShopRevisionBuilder {
  const string World="Assets/BlackMarket/Resources/Worlds/NorthPointShop.prefab", Art="Assets/BlackMarket/Art/Revision/";
  static Material Mat(string n,Color c,float smooth=.2f){string path=Art+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.SetColor("_BaseColor",c);m.SetFloat("_Smoothness",smooth);return m;}
  static Transform Node(Transform p,string n,Vector3 v=default){var t=new GameObject(n).transform;t.SetParent(p,false);t.localPosition=v;return t;}
  static Transform Box(Transform p,string n,Vector3 v,Vector3 size,Material mat,bool solid=true){var t=GameObject.CreatePrimitive(PrimitiveType.Cube).transform;t.name=n;t.SetParent(p,false);t.localPosition=v;t.localScale=size;t.GetComponent<Renderer>().sharedMaterial=mat;if(!solid)Object.DestroyImmediate(t.GetComponent<Collider>());return t;}
  static void Text(Transform p,string s,Vector3 v,float size=.023f){var t=Node(p,s,v);var m=t.gameObject.AddComponent<TextMesh>();m.font=Resources.Load<Font>("Fonts/Bold");m.fontSize=64;m.characterSize=size;m.text=s;m.anchor=TextAnchor.MiddleCenter;m.alignment=TextAlignment.Center;m.GetComponent<Renderer>().sharedMaterial=m.font.material;}
  static Bounds Bounds(GameObject g){var rs=g.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);return b;}
  static void Fit(GameObject g,float height){var b=Bounds(g);float f=height/b.size.y;g.transform.localScale*=f;g.transform.localPosition-=new Vector3(b.center.x,b.min.y,b.center.z)*f;}
  static void Actor(){
   var root=new GameObject("Alex - chàng trai trẻ");var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"Rocketbox/Male_Adult_08.fbx"),root.transform);model.name="Rig";Fit(model,1.78f);
   foreach(var r in model.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++){string name=mats[i].name.ToLower(),part=name.Contains("head")?"head":name.Contains("opacity")?"opacity":"body";var mat=Mat("AlexYoung_"+part,Color.white);mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Rocketbox/m014_"+part+"_color.tga"));var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Rocketbox/m014_"+part+"_normal.tga");if(normal){mat.SetTexture("_BumpMap",normal);mat.EnableKeyword("_NORMALMAP");}if(part=="opacity"){mat.SetFloat("_AlphaClip",1);mat.SetFloat("_Cull",0);mat.EnableKeyword("_ALPHATEST_ON");}mats[i]=mat;}r.sharedMaterials=mats;}
   var animation=model.GetComponent<Animation>()??model.AddComponent<Animation>();foreach(string id in new[]{"idle","walk","run"}){var c=Resources.Load<AnimationClip>("Actors/"+id);animation.AddClip(c,id);animation[id].wrapMode=WrapMode.Loop;if(id=="idle")animation.clip=c;}animation.playAutomatically=true;animation.cullingType=AnimationCullingType.AlwaysAnimate;
   PrefabUtility.SaveAsPrefabAsset(root,"Assets/BlackMarket/Resources/Actors/Alex.prefab");Object.DestroyImmediate(root);
  }
  static void Car(){
   var root=new GameObject("Sedan đen - MrJaneLAB");var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"Sedan/Large-Sedan-Black.fbx"),root.transform);model.name="Sedan thân và bánh";var b=Bounds(model);if(b.size.x>b.size.z)model.transform.localRotation=Quaternion.Euler(0,90,0);model.transform.Rotate(0,180,0);b=Bounds(model);float f=5f/b.size.z;model.transform.localScale*=f;model.transform.localPosition-=new Vector3(b.center.x,b.min.y,b.center.z)*f;
   foreach(var r in model.GetComponentsInChildren<Renderer>()){var ms=r.sharedMaterials;for(int i=0;i<ms.Length;i++){var old=ms[i];string n=old.name.ToLower();Color c=old.HasProperty("_Color")?old.color:Color.gray;float smooth=.4f;if(n.Contains("black")||n.Contains("body")){c=new Color(.025f,.035f,.045f);smooth=.8f;}if(n.Contains("glass")||n.Contains("window")){c=new Color(.12f,.22f,.29f);smooth=.9f;}var m=Mat("Sedan_"+old.name,c,smooth);if(n.Contains("metal")||n.Contains("rim"))m.SetFloat("_Metallic",.65f);ms[i]=m;Debug.Log("SEDAN MATERIAL "+old.name+" "+c);}r.sharedMaterials=ms;}
   var plate=Mat("Plate",new Color(.85f,.86f,.8f));Box(root.transform,"Biển số",new Vector3(0,.47f,2.49f),new Vector3(.52f,.12f,.018f),plate,false);
   foreach(float x in new[]{-.73f,.73f}){var l=Node(root.transform,"Đèn pha",new Vector3(x,.68f,2.35f)).gameObject.AddComponent<Light>();l.type=LightType.Spot;l.range=19;l.spotAngle=60;l.intensity=9;}
   PrefabUtility.SaveAsPrefabAsset(root,"Assets/BlackMarket/Resources/Opening/BlackSedan.prefab");Object.DestroyImmediate(root);
  }
  static void Office(Transform root,Transform group,Material wood,Material steel){
   var template=root.GetComponentsInChildren<OfficeDrawer>(true).First().transform.parent;var card=root.GetComponentsInChildren<Interaction>(true).Single(i=>i.id=="keycard").gameObject;card.transform.SetParent(group);foreach(var d in template.GetComponentsInChildren<OfficeDrawer>(true))d.redCard=null;
   var positions=new[]{new Vector3(-5.6f,0,23.7f),new Vector3(-5.6f,0,25.7f),new Vector3(-5.6f,0,27.7f),new Vector3(-13.4f,0,23.7f),new Vector3(-13.4f,0,25.7f),new Vector3(-13.4f,0,27.7f)};
   for(int c=0;c<6;c++){
    var cabinet=Object.Instantiate(template.gameObject,group).transform;cabinet.name="Tủ sát tường "+(c+1);cabinet.localPosition=positions[c];cabinet.localRotation=Quaternion.Euler(0,c<3?90:270,0);
    foreach(var d in cabinet.GetComponentsInChildren<OfficeDrawer>(true)){int num=c*4+d.number;d.number=num;var i=d.GetComponent<Interaction>();i.id="drawer_"+num;i.title="MỞ NGĂN TỦ "+num;d.redCard=null;foreach(var label in d.GetComponentsInChildren<TextMesh>())label.text=num.ToString();if(num==18){d.redCard=card;card.transform.SetParent(d.tray,false);card.transform.localPosition=new Vector3(.1f,-.07f,.12f);card.SetActive(false);}}
   }Object.DestroyImmediate(template.gameObject);
   var office=root.Cast<Transform>().First(t=>t.name.StartsWith("04 -"));foreach(var t in office.Cast<Transform>().ToArray())if(t.name.Contains("steel_frame_shelves")||t.name.Contains("Stockroom steel rack")||t.name.Contains("metal_tool_chest"))Object.DestroyImmediate(t.gameObject);
   foreach(float x in new[]{-12.2f,-10.3f,-8.4f,-6.5f})Bookcase(group,new Vector3(x,0,29.55f),0,wood,steel);
   foreach(float x in new[]{-12.4f,-6.5f})Bookcase(group,new Vector3(x,0,22.45f),180,wood,steel);
   root.GetComponentsInChildren<WorldMarker>().Single(m=>m.id=="drawers").transform.position=new Vector3(-9,.9f,24);
  }
  static void Bookcase(Transform p,Vector3 pos,float yaw,Material wood,Material steel){var t=Node(p,"Giá sách sát tường",pos);t.localRotation=Quaternion.Euler(0,yaw,0);Box(t,"Lưng giá",new Vector3(0,1.25f,.35f),new Vector3(1.65f,2.5f,.08f),wood);foreach(float x in new[]{-.8f,.8f})Box(t,"Vách giá",new Vector3(x,1.25f,0),new Vector3(.08f,2.5f,.75f),wood);for(int row=0;row<5;row++){float y=.12f+row*.49f;Box(t,"Đợt sách",new Vector3(0,y,0),new Vector3(1.65f,.06f,.75f),wood);for(int k=0;k<9;k++){var m=Mat("Book_"+(k%4),new[]{new Color(.25f,.36f,.4f),new Color(.45f,.2f,.13f),new Color(.6f,.53f,.32f),new Color(.17f,.24f,.18f)}[k%4]);Box(t,"Hồ sơ và sách",new Vector3(-.65f+k*.16f,y+.19f,-.09f),new Vector3(.12f,.32f,.42f),m,false);}}}
  static void Warehouse(Transform root,Transform group,Material wall,Material steel){
   var warehouse=root.Cast<Transform>().First(t=>t.name.StartsWith("07 -"));
   foreach(var t in warehouse.Cast<Transform>().ToArray())if(!t.name.Contains("Warehouse front")&&!t.GetComponent<RoomDoor>()&&!t.GetComponent<Interaction>()&&!t.GetComponent<WorldMarker>()&&!t.name.Contains("KHO")&&!t.name.Contains("STOCKROOM"))Object.DestroyImmediate(t.gameObject);
   var extra=root.Find("10 - Kho hẹp và sân ngoài");foreach(var t in extra.Cast<Transform>().ToArray())if(t.name!="Mặt đường trước cửa")Object.DestroyImmediate(t.gameObject);
   var mission=root.Find("11 - Thẻ đỏ và Cửa 006");foreach(string name in new[]{"Tường sau trái","Tường sau phải"}){var t=mission.Find(name);t.localPosition+=Vector3.forward*12;}
   var air=root.GetComponentInChildren<BasementAirlock>();air.transform.localPosition+=Vector3.forward*12;
   var door=root.GetComponentsInChildren<Interaction>().Single(i=>i.id=="door06");door.transform.position=new Vector3(0,1,46.7f);
   Box(group,"Floor tile",new Vector3(0,-.15f,42),new Vector3(28,.3f,12),wall);foreach(float x in new[]{-14f,14f})Box(group,"Tường kho mở rộng",new Vector3(x,1.75f,42),new Vector3(.24f,3.5f,12),wall);Box(group,"Trần kho mở rộng",new Vector3(0,3.75f,42),new Vector3(28,.2f,12),wall);
   var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BlackMarket/Prefabs/Shop/Stock rack - ke kho va thung hang.prefab");
   for(int row=0;row<6;row++)for(int col=0;col<6;col++){
    var shelf=(GameObject)PrefabUtility.InstantiatePrefab(source);shelf.transform.SetParent(group,false);var b=Bounds(shelf);shelf.transform.localScale=new Vector3(3.7f/b.size.x,2.35f/b.size.y,1.05f/b.size.z);shelf.name="Giá kho "+(row*6+col+1).ToString("00");shelf.transform.localPosition=new Vector3((row%2==0?-11.15f:-7.35f)+col*3.7f,0,32+row*2.55f);
   }
   for(int row=0;row<6;row++){var l=Node(group,"Đèn kho",new Vector3(row%2==0?10:-10,3.25f,32+row*2.55f)).gameObject.AddComponent<Light>();l.range=9;l.intensity=2.5f;Text(group,"LỐI QUA KỆ "+(row+1),new Vector3(row%2==0?11.5f:-11.5f,2.6f,32+row*2.55f),.018f);}
   // Small freight piles sit against side walls, leaving the end aisles clear.
   var cardboard=Mat("Carton",new Color(.47f,.32f,.17f));for(int k=0;k<10;k++){float x=k%2==0?-13.55f:13.55f,z=31+k*1.55f;Box(group,"Thùng hàng sát tường",new Vector3(x,.4f,z),new Vector3(.55f,.8f,.7f),cardboard);}
   var back=air.transform.Find("Tường cuối");Object.DestroyImmediate(back.gameObject);
   foreach(float x in new[]{-1.6f,1.6f})Box(air.transform,"Vách cầu thang",new Vector3(x,-.25f,9),new Vector3(.2f,7.5f,9),wall);
   Box(air.transform,"Chiếu nghỉ cầu thang",new Vector3(0,-.1f,5.5f),new Vector3(3.2f,.2f,2),steel);
   for(int n=0;n<20;n++){float y=-.15f*(n+1);Box(air.transform,"Bậc thang "+n,new Vector3(0,y-.15f,6.3f+n*.3f),new Vector3(3,.3f,.3f),steel);}
   Box(air.transform,"Chiếu nghỉ B1",new Vector3(0,-3.1f,12.65f),new Vector3(3.2f,.2f,1.4f),steel);Box(air.transform,"Cuối hành lang xuống hầm",new Vector3(0,-1.3f,13.4f),new Vector3(3.4f,4,.2f),wall);
   air.descentPath=new[]{Node(air.transform,"Đường chạy - ra cửa",new Vector3(0,0,4.4f)),Node(air.transform,"Đường chạy - đầu thang",new Vector3(0,0,6)),Node(air.transform,"Đường chạy - chân thang",new Vector3(0,-3,12.3f))};
   var light=Node(air.transform,"Đèn cầu thang",new Vector3(0,1.8f,8)).gameObject.AddComponent<Light>();light.range=12;light.intensity=4;
  }
  static void Exterior(Transform p,Material wall,Material steel){
   var asphalt=Mat("Asphalt",new Color(.06f,.075f,.09f));Box(p,"Nền phố liên tục",new Vector3(0,-.35f,-35),new Vector3(240,.2f,60),asphalt);
   var concrete=Mat("Pavement",new Color(.33f,.35f,.34f));foreach(float z in new[]{-2.5f,-17f})Box(p,"Vỉa hè",new Vector3(0,-.15f,z),new Vector3(150,.25f,4),concrete);
   for(int i=-4;i<=4;i++){float x=i*13;var facade=Node(p,"Nhà phố đối diện "+i,new Vector3(x,0,-25));float h=7+(i+4)%3*3;Box(facade,"Khối nhà",new Vector3(0,h/2,0),new Vector3(12,h,12),Mat("Facade_"+(i+4)%3,new Color(.19f+(i+4)%3*.05f,.23f,.25f)));for(int floor=0;floor<3;floor++)for(int win=0;win<4;win++)Box(facade,"Cửa sổ",new Vector3(-4.5f+win*3,2+floor*2.6f,6.02f),new Vector3(1.1f,1.4f,.04f),steel,false);Text(facade,i%2==0?"CỬA HÀNG / ĐÃ ĐÓNG":"NHÀ KHO / GIAO NHẬN",new Vector3(0,3.2f,6.05f),.025f);}
   foreach(float x in new[]{-23f,23f})for(int z=6;z<68;z+=14)Box(p,"Dãy nhà bên",new Vector3(x,5,z),new Vector3(15,10,13),wall);
   for(int x=-30;x<=30;x+=15){Box(p,"Cột đèn",new Vector3(x,2.8f,-14),new Vector3(.13f,5.6f,.13f),steel);var l=Node(p,"Đèn đường",new Vector3(x,5.45f,-14)).gameObject.AddComponent<Light>();l.color=new Color(1,.78f,.5f);l.range=14;l.intensity=4;Box(p,"Bồn cây",new Vector3(x+4,.3f,-15.5f),new Vector3(2,.6f,1),wall);for(int k=0;k<4;k++)Box(p,"Tán cây",new Vector3(x+3.4f+k*.4f,.85f,-15.5f),new Vector3(.7f,.8f,.75f),Mat("Leaves",new Color(.07f,.16f,.1f)),false);}
   for(int x=-60;x<=60;x+=6)Box(p,"Vạch đường",new Vector3(x,-.225f,-10),new Vector3(2.8f,.01f,.12f),Mat("RoadMark",new Color(.7f,.67f,.52f)),false);
  }
  public static void Build(){
   EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/BlackMarket/Resources/Materials/WorldText.mat"))AssetDatabase.CreateAsset(new Material(Shader.Find("BlackMarket/WorldText")),"Assets/BlackMarket/Resources/Materials/WorldText.mat");Actor();Car();var root=PrefabUtility.LoadPrefabContents(World);
   try{if(!root.transform.Find("12 - Mở rộng cửa hàng")){var group=Node(root.transform,"12 - Mở rộng cửa hàng");var wood=Mat("OfficeWood",new Color(.3f,.21f,.14f));var steel=Mat("Steel",new Color(.23f,.28f,.3f));var wall=Mat("Wall",new Color(.45f,.49f,.48f));Office(root.transform,Node(group,"Văn phòng - tủ và giá sách"),wood,steel);Warehouse(root.transform,Node(group,"Kho 28 x 18 - 36 giá hàng"),wall,steel);Exterior(Node(group,"Bối cảnh phố"),wall,steel);}foreach(var t in root.transform.Cast<Transform>().First(t=>t.name.StartsWith("04 -")).Cast<Transform>().ToArray())if(t.name.Contains("Stockroom steel rack"))Object.DestroyImmediate(t.gameObject);
   foreach(var shelf in root.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Giá kho "))){int row=(int.Parse(shelf.name.Substring(7))-1)/6;var v=shelf.localPosition;v.z=32+row*2.55f;shelf.localPosition=v;}
   var warehouseGroup=root.transform.Find("12 - Mở rộng cửa hàng/Kho 28 x 18 - 36 giá hàng");if(!warehouseGroup.Find("Chặn cuối dãy 0"))for(int row=0;row<6;row++)Box(warehouseGroup,"Chặn cuối dãy "+row,new Vector3(row%2==0?-13.4f:13.7f,1.15f,32+row*2.55f),new Vector3(row%2==0?1.2f:.6f,2.3f,1.05f),Mat("Steel",new Color(.23f,.28f,.3f)));
   foreach(var label in root.GetComponentsInChildren<TextMesh>(true)){if(!label.GetComponent<WorldTextDepth>())label.gameObject.AddComponent<WorldTextDepth>();if(label.transform.parent && label.transform.parent.name.StartsWith("Nhà phố đối diện"))label.transform.localRotation=Quaternion.Euler(0,180,0);if(label.text.StartsWith("MARCUS CARTER"))label.transform.localPosition=new Vector3(-10,2.85f,29.86f);}
   var revision=root.transform.Find("12 - Mở rộng cửa hàng");foreach(var t in revision.GetComponentsInChildren<Transform>().Where(t=>(t.name=="Cột đèn"||t.name=="Đèn đường")&&Mathf.Abs(t.localPosition.x)<.01f)){var v=t.localPosition;v.x=3.5f;t.localPosition=v;}
   var airlock=root.GetComponentInChildren<BasementAirlock>().transform;var endWall=airlock.Find("Cuối hành lang xuống hầm");endWall.localPosition=new Vector3(0,0,13.4f);endWall.localScale=new Vector3(3.4f,7,.2f);
   if(!airlock.Find("Trần cầu thang")){var stairMat=Mat("Steel",new Color(.23f,.28f,.3f));Box(airlock,"Trần cầu thang",new Vector3(0,3.5f,9),new Vector3(3.4f,.2f,9),stairMat);foreach(float x in new[]{-1.3f,1.3f})Box(airlock,"Tay vịn cầu thang",new Vector3(x,-.45f,9.15f),new Vector3(.07f,.07f,6.7f),stairMat).localRotation=Quaternion.Euler(26.565f,0,0);for(int n=0;n<20;n++)Box(airlock,"Mép bậc thang",new Vector3(0,-.15f*(n+1)+.008f,6.155f+n*.3f),new Vector3(2.9f,.014f,.035f),Mat("StairEdge",new Color(.67f,.67f,.57f)),false);var l=Node(airlock,"Đèn chiếu nghỉ B1",new Vector3(0,-.4f,12)).gameObject.AddComponent<Light>();l.range=6;l.intensity=3;}
   var ground=root.GetComponentsInChildren<Transform>().First(t=>t.name=="Nền phố liên tục");ground.localPosition=new Vector3(0,-.35f,-35);ground.localScale=new Vector3(240,.2f,60);PrefabUtility.SaveAsPrefabAsset(root,World);}finally{PrefabUtility.UnloadPrefabContents(root);}
   RetailShopBuilder.BakeSavedShop();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(World));EditorSceneManager.SaveScene(scene,"Assets/BlackMarket/Scenes/NorthPointShop_Environment.unity");EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");AssetDatabase.SaveAssets();Debug.Log("SHOP REVISION BUILT");
  }
  public static void Diagnose(){
   EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");var g=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(World));foreach(var d in g.GetComponentsInChildren<RoomDoor>())d.SetOpenImmediate(true);g.GetComponent<NavMeshSurface>().BuildNavMesh();
   var lines=new System.Collections.Generic.List<string>();foreach(var t in g.transform.Cast<Transform>().First(t=>t.name.StartsWith("07 -")).Cast<Transform>())lines.Add("WAREHOUSE "+t.name+" "+t.position);
   foreach(var t in g.transform.Cast<Transform>().First(t=>t.name.StartsWith("04 -")).Cast<Transform>())lines.Add("OFFICE "+t.name+" "+t.position);
   foreach(var d in g.GetComponentsInChildren<OfficeDrawer>()){UnityEngine.AI.NavMesh.SamplePosition(d.transform.position,out var near,5,-1);lines.Add("DRAWER "+d.number+" "+d.transform.position+" nav="+near.position+" distance="+near.distance);}
   foreach(var t in g.GetComponentsInChildren<Transform>().Where(t=>t.name.StartsWith("Giá kho")))lines.Add(t.name+" "+Bounds(t.gameObject));
   UnityEngine.AI.NavMesh.SamplePosition(new Vector3(0,0,29),out var start,2,-1);
   for(float z=29;z<49;z+=.5f){string line=z+" ";for(float x=-13.5f;x<14;x+=.5f){if(!UnityEngine.AI.NavMesh.SamplePosition(new Vector3(x,0,z),out var end,.2f,-1))line+="#";else{var path=new UnityEngine.AI.NavMeshPath();UnityEngine.AI.NavMesh.CalculatePath(start.position,end.position,-1,path);line+=path.status==UnityEngine.AI.NavMeshPathStatus.PathComplete?".":"X";}}lines.Add(line);}File.WriteAllLines("/tmp/revision-nav.txt",lines);
  }
  public static void Preview(){
   Directory.CreateDirectory("Documentation/RevisionPreview");EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPointShop_Environment.unity");RenderSettings.skybox=null;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.54f,.6f,.65f);RenderSettings.ambientEquatorColor=new Color(.37f,.4f,.41f);RenderSettings.ambientGroundColor=new Color(.19f,.19f,.18f);RenderSettings.fog=false;
   var cam=new GameObject("Revision preview").AddComponent<Camera>();cam.nearClipPlane=.05f;cam.farClipPlane=200;cam.fieldOfView=65;cam.backgroundColor=new Color(.045f,.065f,.08f);cam.clearFlags=CameraClearFlags.SolidColor;cam.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
   var actor=Object.Instantiate(Resources.Load<GameObject>("Actors/Alex"));actor.transform.position=new Vector3(0,0,-3);actor.transform.rotation=Quaternion.Euler(0,180,0);Capture(cam,new Vector3(1.4f,1.65f,-5.5f),new Vector3(0,1.15f,-3),"09-young-alex");Object.DestroyImmediate(actor);
   var car=Object.Instantiate(Resources.Load<GameObject>("Opening/BlackSedan"));car.transform.position=new Vector3(0,0,-7);Capture(cam,new Vector3(5,2.8f,-2),new Vector3(0,.7f,-7),"10-sedan");Object.DestroyImmediate(car);
   Capture(cam,new Vector3(10,3,-14),new Vector3(0,1,-5),"11-street");Capture(cam,new Vector3(-9,3,22.5f),new Vector3(-9.5f,1.2f,27),"12-office-layout");Capture(cam,new Vector3(11.5f,3,31),new Vector3(-9,1.1f,42),"13-warehouse-layout");
   var world=Object.FindFirstObjectByType<NavMeshSurface>().transform;world.Find("09 - CEILING - hide with eye for editing").gameObject.SetActive(false);world.Find("12 - Mở rộng cửa hàng/Kho 28 x 18 - 36 giá hàng/Trần kho mở rộng").gameObject.SetActive(false);cam.orthographic=true;cam.orthographicSize=16;Capture(cam,new Vector3(0,35,39),new Vector3(0,0,39.01f),"14-warehouse-plan");EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");
  }
  static void Capture(Camera cam,Vector3 position,Vector3 target,string name){cam.transform.SetPositionAndRotation(position,Quaternion.LookRotation(target-position));var rt=new RenderTexture(1440,1000,24);var old=RenderTexture.active;cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var tex=new Texture2D(1440,1000,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1440,1000),0,0);tex.Apply();File.WriteAllBytes("Documentation/RevisionPreview/"+name+".png",tex.EncodeToPNG());cam.targetTexture=null;RenderTexture.active=old;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);}
  public static void Verify(){Build();PlayVerification.Run();}
 }
}
