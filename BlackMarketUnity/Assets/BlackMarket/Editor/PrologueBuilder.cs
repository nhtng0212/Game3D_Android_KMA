using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
namespace BlackMarket.Editor {
    public static class PrologueBuilder {
        const string Art="Assets/BlackMarket/Art/Prologue/";
        const string Out="Assets/BlackMarket/Resources/Opening/";
        static Material Mat(string n,Color c,bool glow=false){var path=Art+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}m.color=c;if(glow){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",c*2);}return m;}
        static GameObject Box(Transform p,string n,Vector3 pos,Vector3 scale,Material mat){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=n;g.transform.SetParent(p,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;return g;}
        static void Marker(Transform p,string n,Vector3 pos){var g=new GameObject(n);g.transform.SetParent(p,false);g.transform.localPosition=pos;}
        [MenuItem("BLACK MARKET/Build bedroom and motorcycle only")]
        public static void Build(){Directory.CreateDirectory(Art);Directory.CreateDirectory(Out);AssetDatabase.Refresh();Bike();Bedroom();AssetDatabase.SaveAssets();Debug.Log("PROLOGUE PREFABS BUILT");}
        static void Bike(){
            string path=Art+"Motorcycle/Fonk-Bike.obj";var importer=(ModelImporter)AssetImporter.GetAtPath(path);importer.materialImportMode=ModelImporterMaterialImportMode.None;importer.SaveAndReimport();
            var root=new GameObject("Xe máy của Alex");var model=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path),root.transform);model.name="Mẫu xe Duhgless (CC0)";
            // Unity mirrors OBJ X; turn its front wheel towards prefab +Z.
            model.transform.localRotation=Quaternion.Euler(0,-90,0);var rs=model.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);model.transform.localScale=Vector3.one*(2.25f/b.size.z);
            b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);model.transform.localPosition=new Vector3(-b.center.x,-b.min.y,-b.center.z);
            foreach(var r in rs){string texture=r.name.Contains("Front")?"Front-Wheeel":r.name.Contains("Back")?"Rear-Tire":r.name.Contains("Fork")?"Forks":"Fonk-Frame";var m=Mat("Bike_"+texture,Color.white);m.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Motorcycle/"+texture+".png"));m.SetFloat("_Smoothness",.25f);r.sharedMaterial=m;}
            foreach(var wheel in rs.Where(r=>r.name=="FrontWheel" || r.name=="BackWheel")){
                var pivot=new GameObject(wheel.name=="FrontWheel"?"Bánh xe trước":"Bánh xe sau");pivot.transform.SetParent(root.transform,false);pivot.transform.position=wheel.bounds.center;wheel.transform.SetParent(pivot.transform,true);
            }
            Marker(root.transform,"Seat",new Vector3(0,.75f,-.35f));Marker(root.transform,"Left grip",new Vector3(-.345f,1.28f,.02f));Marker(root.transform,"Right grip",new Vector3(.345f,1.28f,.02f));
            var black=Mat("PhoneBlack",new Color(.025f,.03f,.035f));Box(root.transform,"Biển số xe Alex",new Vector3(0,.42f,-1.02f),new Vector3(.23f,.16f,.025f),Mat("Plate",new Color(.88f,.86f,.73f)));
            var plate=new GameObject("Biển số 71 A1 / 006.21");plate.transform.SetParent(root.transform,false);plate.transform.localPosition=new Vector3(0,.42f,-1.038f);plate.transform.localRotation=Quaternion.Euler(0,180,0);var text=plate.AddComponent<TextMesh>();text.text="71-A1\n006.21";text.fontSize=48;text.characterSize=.018f;text.anchor=TextAnchor.MiddleCenter;text.alignment=TextAlignment.Center;text.color=Color.black;plate.AddComponent<WorldTextDepth>();
            PrefabUtility.SaveAsPrefabAsset(root,Out+"AlexMotorcycle.prefab");Object.DestroyImmediate(root);
        }
        static void Bedroom(){
            var root=new GameObject("Phòng ngủ Alex — mở đầu");var t=root.transform;
            var wall=Mat("BedroomWall",new Color(.24f,.32f,.37f));var wood=Mat("BedroomWood",new Color(.22f,.12f,.065f));var dark=Mat("PhoneBlack",new Color(.025f,.03f,.035f));var fabric=Mat("Bedding",new Color(.20f,.34f,.42f));var white=Mat("Linen",new Color(.83f,.8f,.71f));var screen=Mat("MonitorLight",new Color(.1f,.6f,.7f),true);
            Box(t,"Sàn phòng ngủ",new Vector3(0,-.12f,0),new Vector3(6,.2f,6),wood);
            Box(t,"Trần phòng ngủ",new Vector3(0,3.05f,0),new Vector3(6,.12f,6),wall);
            Box(t,"Tường sau",new Vector3(0,1.5f,3),new Vector3(6,3,.15f),wall);Box(t,"Tường trước",new Vector3(0,1.5f,-3),new Vector3(6,3,.15f),wall);Box(t,"Tường trái",new Vector3(-3,1.5f,0),new Vector3(.15f,3,6),wall);
            Box(t,"Tường cửa sổ dưới",new Vector3(3,.5f,0),new Vector3(.15f,1,6),wall);Box(t,"Tường cửa sổ trên",new Vector3(3,2.8f,0),new Vector3(.15f,.4f,6),wall);
            Box(t,"Tường cạnh cửa sổ A",new Vector3(3,1.8f,-1.9f),new Vector3(.15f,1.6f,2.2f),wall);Box(t,"Tường cạnh cửa sổ B",new Vector3(3,1.8f,2.6f),new Vector3(.15f,1.6f,.8f),wall);
            Box(t,"Khung cửa sổ đêm",new Vector3(3,1.85f,.7f),new Vector3(.12f,1.6f,2.8f),dark);Box(t,"Kính cửa sổ xanh",new Vector3(2.92f,1.85f,.7f),new Vector3(.025f,1.42f,2.6f),Mat("NightWindow",new Color(.04f,.1f,.2f),true));
            Box(t,"Đố cửa sổ",new Vector3(2.9f,1.85f,.7f),new Vector3(.06f,1.5f,.07f),white);Box(t,"Thanh ngang cửa sổ",new Vector3(2.9f,1.85f,.7f),new Vector3(.06f,.06f,2.7f),white);
            Box(t,"Cửa phòng",new Vector3(1.95f,1.05f,2.89f),new Vector3(.9f,2.1f,.12f),wood);Box(t,"Tay nắm cửa",new Vector3(2.27f,1,2.8f),new Vector3(.12f,.035f,.08f),white);
            Box(t,"Khung giường",new Vector3(-1.85f,.22f,-.35f),new Vector3(1.55f,.4f,2.25f),wood);Box(t,"Nệm",new Vector3(-1.85f,.48f,-.35f),new Vector3(1.5f,.24f,2.2f),white);Box(t,"Chăn",new Vector3(-1.85f,.63f,-.7f),new Vector3(1.51f,.08f,1.5f),fabric);Box(t,"Gối",new Vector3(-1.85f,.67f,.45f),new Vector3(.95f,.16f,.42f),white);
            Box(t,"Bàn làm việc",new Vector3(.1f,.77f,1.65f),new Vector3(1.8f,.1f,.8f),wood);foreach(float x in new[]{-.7f,.9f})foreach(float z in new[]{1.35f,1.95f})Box(t,"Chân bàn",new Vector3(x,.36f,z),new Vector3(.08f,.72f,.08f),dark);
            Box(t,"Màn hình máy tính",new Vector3(.1f,1.18f,1.85f),new Vector3(.88f,.5f,.06f),dark);Box(t,"Trò chơi đang chạy",new Vector3(.1f,1.18f,1.813f),new Vector3(.79f,.41f,.015f),screen);Box(t,"Chân màn hình",new Vector3(.1f,.9f,1.88f),new Vector3(.09f,.25f,.08f),dark);
            for(int i=0;i<7;i++)Box(t,"Cảnh trò chơi trên màn hình",new Vector3(-.22f+i*.10f,1.08f+(i%3)*.055f,1.8f),new Vector3(.075f,.09f+(i%2)*.07f,.007f),i%2==0?dark:white);
            Box(t,"Bàn phím",new Vector3(.1f,.836f,1.44f),new Vector3(.48f,.028f,.17f),dark);Box(t,"Chuột",new Vector3(.5f,.85f,1.44f),new Vector3(.06f,.035f,.11f),white);Box(t,"Thùng máy",new Vector3(.72f,.31f,1.8f),new Vector3(.27f,.6f,.48f),dark);
            Box(t,"Đệm ghế",new Vector3(.1f,.45f,.8f),new Vector3(.48f,.1f,.46f),dark);Box(t,"Lưng ghế",new Vector3(.1f,.8f,.59f),new Vector3(.48f,.65f,.075f),fabric);foreach(float x in new[]{-.08f,.28f})foreach(float z in new[]{.64f,.96f})Box(t,"Chân ghế",new Vector3(x,.21f,z),new Vector3(.045f,.42f,.045f),dark);
            var shelf=new GameObject("Giá sách sát tường");shelf.transform.SetParent(t,false);shelf.transform.localPosition=new Vector3(-1.7f,0,2.7f);
            foreach(float x in new[]{-.7f,.7f})Box(shelf.transform,"Thành giá",new Vector3(x,1.1f,0),new Vector3(.06f,2.2f,.4f),wood);
            for(int y=0;y<5;y++){Box(shelf.transform,"Đợt sách",new Vector3(0,.12f+y*.48f,0),new Vector3(1.45f,.055f,.42f),wood);for(int i=0;i<12;i++)Box(shelf.transform,"Sách",new Vector3(-.6f+i*.105f,.30f+y*.48f,0),new Vector3(.075f,.28f+(i%3)*.035f,.26f),i%3==0?fabric:i%3==1?white:wall);}
            Marker(t,"Actor seat",new Vector3(.1f,0,.8f));
            var lamp=new GameObject("Đèn phòng ấm");lamp.transform.SetParent(t,false);lamp.transform.localPosition=new Vector3(0,2.6f,0);var l=lamp.AddComponent<Light>();l.type=LightType.Point;l.range=10;l.intensity=3;l.color=new Color(1,.8f,.6f);
            var monitor=new GameObject("Ánh sáng màn hình");monitor.transform.SetParent(t,false);monitor.transform.localPosition=new Vector3(.1f,1.4f,1.55f);l=monitor.AddComponent<Light>();l.range=2;l.intensity=.8f;l.color=Color.cyan;
            PrefabUtility.SaveAsPrefabAsset(root,Out+"AlexBedroom.prefab");Object.DestroyImmediate(root);
        }
    }
}
