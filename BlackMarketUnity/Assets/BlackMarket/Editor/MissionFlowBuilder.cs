using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace BlackMarket.Editor {
 public static class MissionFlowBuilder {
  const string Path="Assets/BlackMarket/Resources/Worlds/NorthPointShop.prefab";
  static Material Mat(string name,Color color){string p="Assets/BlackMarket/Resources/Materials/Mission_"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(p);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,p);}m.SetColor("_BaseColor",color);if(name=="Scan"){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*2);}return m;}
  static Transform Node(Transform parent,string name,Vector3 pos){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=pos;return t;}
  static Transform Box(Transform parent,string name,Vector3 pos,Vector3 size,Material mat,bool solid=true){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());return g.transform;}
  static void Label(Transform parent,string value,Vector3 pos,float size=.12f){var t=Node(parent,value,pos);t.localRotation=Quaternion.identity;var m=t.gameObject.AddComponent<TextMesh>();m.text=value;m.font=Resources.Load<Font>("Fonts/Bold");m.GetComponent<Renderer>().sharedMaterial=m.font.material;m.characterSize=size*.22f;m.alignment=TextAlignment.Center;m.fontSize=48;m.anchor=TextAnchor.MiddleCenter;}
  static Interaction Item(Transform parent,string id,string title,Vector3 pos){var t=Node(parent,title,pos);var i=t.gameObject.AddComponent<Interaction>();i.id=id;i.title=title;return i;}
  static void Marker(Transform parent,string id,Vector3 pos){Node(parent,id,pos).gameObject.AddComponent<WorldMarker>().id=id;}
  public static void AddMissionLayout(Transform root){
   Mat("Scan",new Color(.1f,.75f,.8f));
   foreach(var label in root.GetComponentsInChildren<TextMesh>(true))if(label.text=="06 / LỐI XUỐNG HẦM")label.text="006 / LỐI XUỐNG HẦM";
   if(root.Find("11 - Thẻ đỏ và Cửa 006")){foreach(var label in root.Find("11 - Thẻ đỏ và Cửa 006").GetComponentsInChildren<TextMesh>(true)){label.transform.localRotation=Quaternion.identity;label.characterSize=label.text.StartsWith("TỦ")?.07f*.22f:label.text.StartsWith("CỬA")?.12f*.22f:label.text.StartsWith("ĐỨNG")?.08f*.22f:.065f*.22f;label.alignment=TextAlignment.Center;}return;}
   foreach(var t in root.GetComponentsInChildren<Transform>(true).Where(t=>t.name=="Rear exterior"||t.name=="rollershutter_door"||t.name=="Carter scanner"||t.name=="Marcus keycard").ToArray())if(t)Object.DestroyImmediate(t.gameObject);
   foreach(var i in root.GetComponentsInChildren<Interaction>(true).Where(i=>i.id=="keycard").ToArray())Object.DestroyImmediate(i.gameObject);
   var g=Node(root,"11 - Thẻ đỏ và Cửa 006",Vector3.zero);var metal=Mat("Metal",new Color(.22f,.27f,.29f));var wood=Mat("Wood",new Color(.36f,.23f,.13f));var red=Mat("Red",new Color(.85f,.035f,.025f));var wall=Mat("Wall",new Color(.58f,.61f,.59f));var cyan=Mat("Scan",new Color(.1f,.75f,.8f));
   var cabinet=Node(g,"Tủ cá nhân Marcus",new Vector3(-6.5f,0,26));
   Box(cabinet,"Vách trái",new Vector3(-.63f,.86f,0),new Vector3(.09f,1.72f,.85f),wood);Box(cabinet,"Vách phải",new Vector3(.63f,.86f,0),new Vector3(.09f,1.72f,.85f),wood);Box(cabinet,"Lưng tủ",new Vector3(0,.86f,.4f),new Vector3(1.3f,1.72f,.08f),wood);Box(cabinet,"Mặt tủ",new Vector3(0,1.73f,0),new Vector3(1.4f,.1f,.9f),wood);
   Label(cabinet,"TỦ CÁ NHÂN / MARCUS",new Vector3(0,1.95f,-.4f),.07f);
   for(int n=1;n<=4;n++){
    var i=Item(cabinet,"drawer_"+n,"MỞ NGĂN KÉO "+n,new Vector3(0,.23f+(n-1)*.4f,-.3f));var d=i.gameObject.AddComponent<OfficeDrawer>();d.number=n;d.tray=Node(i.transform,"Ngăn kéo trượt",Vector3.zero);
    Box(d.tray,"Đáy",new Vector3(0,-.12f,.15f),new Vector3(1.14f,.04f,.7f),wood,false);Box(d.tray,"Mặt ngăn",new Vector3(0,0,-.12f),new Vector3(1.16f,.35f,.06f),wood,false);Box(d.tray,"Tay nắm",new Vector3(0,.02f,-.18f),new Vector3(.32f,.045f,.05f),metal,false);Label(d.tray,n.ToString(),new Vector3(.46f,.03f,-.155f),.065f);
    if(n==3){var card=Item(d.tray,"keycard","NHẶT THẺ ĐỎ",new Vector3(.1f,-.07f,.12f));Box(card.transform,"Thẻ đỏ",Vector3.zero,new Vector3(.25f,.015f,.16f),red,false);Box(card.transform,"Chip",new Vector3(-.065f,.012f,0),new Vector3(.04f,.008f,.04f),metal,false);d.redCard=card.gameObject;card.gameObject.SetActive(false);}
   }
   Marker(g,"drawers",new Vector3(-6.5f,.9f,24.9f));
   Box(g,"Tường sau trái",new Vector3(-7.8f,1.7f,36),new Vector3(12.4f,3.4f,.24f),wall);Box(g,"Tường sau phải",new Vector3(7.8f,1.7f,36),new Vector3(12.4f,3.4f,.24f),wall);
   var air=Node(g,"Cửa 006 - Khoang xác thực",new Vector3(0,0,35.8f));var a=air.gameObject.AddComponent<BasementAirlock>();
   Box(air,"Sàn khoang",new Vector3(0,-.1f,2.4f),new Vector3(3.4f,.2f,5.2f),metal);foreach(float x in new[]{-1.6f,1.6f})Box(air,"Vách khoang",new Vector3(x,1.7f,2.4f),new Vector3(.2f,3.4f,5.2f),wall);Box(air,"Trần khoang",new Vector3(0,3.4f,2.4f),new Vector3(3.4f,.15f,5.2f),wall);Box(air,"Tường cuối",new Vector3(0,1.7f,5),new Vector3(3.4f,3.4f,.2f),wall);
   a.outerLeaf=Gate(air,"Cửa ngoài - thẻ đỏ",0,metal);a.innerLeaf=Gate(air,"Cửa trong - khuôn mặt",3.4f,metal);
   Label(air,"CỬA 006 / THẺ ĐỎ",new Vector3(0,3.05f,-.16f),.12f);Label(air,"ĐỨNG VÀO DẤU CHÂN\nQUÉT KHUÔN MẶT",new Vector3(0,2.5f,3.22f),.08f);
   a.indicator=Box(air,"Máy quét mặt",new Vector3(.8f,1.5f,3.2f),new Vector3(.32f,.4f,.12f),cyan,false).GetComponent<Renderer>();Item(air,"face_scan","QUÉT KHUÔN MẶT",new Vector3(.6f,1.3f,2.65f));
   foreach(float x in new[]{-.18f,.18f})Box(air,"Dấu chân",new Vector3(x,.012f,1.9f),new Vector3(.13f,.012f,.32f),cyan,false);
   a.scanBeam=Box(air,"Vệt quét",new Vector3(0,1.6f,2),new Vector3(.65f,.018f,.025f),cyan,false);a.scanBeam.gameObject.SetActive(false);Marker(air,"basement_exit",new Vector3(0,1,4.45f));
   var light=Node(air,"Đèn khoang",new Vector3(0,2.7f,2)).gameObject.AddComponent<Light>();light.range=6;light.intensity=3;
   foreach(var i in root.GetComponentsInChildren<Interaction>())if(i.id=="door06")i.title="CỬA 006 / DÙNG THẺ ĐỎ";
  }
  static Transform Gate(Transform p,string name,float z,Material m){var t=Box(p,name,new Vector3(0,1.35f,z),new Vector3(3,2.7f,.16f),m);t.gameObject.AddComponent<NavMeshModifier>().ignoreFromBuild=true;var o=t.gameObject.AddComponent<NavMeshObstacle>();o.shape=NavMeshObstacleShape.Box;o.size=Vector3.one;o.carving=true;o.enabled=false;return t;}
  public static void Verify(){Build();PlayVerification.Run();}
  [MenuItem("BLACK MARKET/Shop/Apply red card and face scanner")]
  public static void Build(){EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");var root=PrefabUtility.LoadPrefabContents(Path);try{AddMissionLayout(root.transform);PrefabUtility.SaveAsPrefabAsset(root,Path);}finally{PrefabUtility.UnloadPrefabContents(root);}RetailShopBuilder.BakeSavedShop();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Path));EditorSceneManager.SaveScene(scene,"Assets/BlackMarket/Scenes/NorthPointShop_Environment.unity");EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");AssetDatabase.SaveAssets();}
 }
}
