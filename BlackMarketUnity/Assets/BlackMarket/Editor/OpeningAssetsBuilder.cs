using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace BlackMarket.Editor {
    public static class OpeningAssetsBuilder {
        const string Art="Assets/BlackMarket/Art/Opening/";
        const string Output="Assets/BlackMarket/Resources/Opening/";
        public static void Build(){
            EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");Directory.CreateDirectory(Output);AssetDatabase.Refresh();MakeSuit();MakeCar();PatchShop();TranslateWorlds();AssetDatabase.SaveAssets();Debug.Log("OPENING BUILD COMPLETE");
        }
        public static void Preview(){
            EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPointShop_Environment.unity");
            var root=UnityEngine.Object.FindFirstObjectByType<Unity.AI.Navigation.NavMeshSurface>().gameObject;
            RetailShopBuilder.RenderPreviews(root);
            Directory.CreateDirectory("Documentation/OpeningPreview");File.Copy("Documentation/ShopPreview/05-warehouse.png","Documentation/OpeningPreview/03-warehouse.png",true);
            EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");
        }
        static Material Material(string name,Texture texture,Color tint,float smooth=.2f){
            var path=Art+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,path);}
            m.SetTexture("_BaseMap",texture);m.SetColor("_BaseColor",tint);m.SetFloat("_Smoothness",smooth);return m;
        }
        static void MakeSuit(){
            var root=new GameObject("Người áo vest đen");var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"Rocketbox/Male_Adult_03.fbx"),root.transform);model.name="Rig";
            var rs=model.GetComponentsInChildren<Renderer>();var bounds=rs[0].bounds;foreach(var r in rs)bounds.Encapsulate(r.bounds);
            float scale=1.82f/bounds.size.y;model.transform.localScale*=scale;model.transform.localPosition=-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z)*scale;
            foreach(var r in rs){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++){
                string n=mats[i].name.ToLower();string part=n.Contains("head")?"head":n.Contains("opacity")?"opacity":"body";
                var tex=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Rocketbox/m004_"+part+"_color.tga");
                var m=Material("Vest_"+part,tex,part=="body"?new Color(.38f,.4f,.43f):Color.white);
                var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Rocketbox/m004_"+part+"_normal.tga");if(normal){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");}
                if(part=="opacity"){m.SetFloat("_AlphaClip",1);m.EnableKeyword("_ALPHATEST_ON");m.SetFloat("_Cull",0);}
                mats[i]=m;
            }r.sharedMaterials=mats;}
            var anim=model.GetComponent<Animation>()??model.AddComponent<Animation>();foreach(var id in new[]{"idle","walk","run"}){var clip=Resources.Load<AnimationClip>("Actors/"+id);anim.AddClip(clip,id);anim[id].wrapMode=WrapMode.Loop;if(id=="idle")anim.clip=clip;}anim.playAutomatically=true;anim.cullingType=AnimationCullingType.AlwaysAnimate;
            PrefabUtility.SaveAsPrefabAsset(root,"Assets/BlackMarket/Resources/Actors/BlackSuit.prefab");UnityEngine.Object.DestroyImmediate(root);
        }
        static void MakeCar(){
            var root=new GameObject("Xe đen");var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Art+"Cars/sedan.fbx"),root.transform);
            var rs=model.GetComponentsInChildren<Renderer>();var b=rs[0].bounds;foreach(var r in rs)b.Encapsulate(r.bounds);float scale=4.7f/b.size.z;model.transform.localScale*=scale;model.transform.localPosition=-new Vector3(b.center.x,b.min.y,b.center.z)*scale;
            var texture=AssetDatabase.LoadAssetAtPath<Texture2D>(Art+"Cars/colormap-dark.png");var mat=Material("Xe_den",texture,Color.white,.55f);foreach(var r in rs)r.sharedMaterials=r.sharedMaterials.Select(_=>mat).ToArray();
            // Practical lamps make the convoy legible in the rainy night exterior.
            foreach(float x in new[]{-.7f,.7f}){var light=new GameObject("Đèn pha");light.transform.SetParent(root.transform);light.transform.localPosition=new Vector3(x,.65f,2.15f);var l=light.AddComponent<Light>();l.type=LightType.Spot;l.range=16;l.spotAngle=55;l.intensity=8;l.color=new Color(.85f,.9f,1);}
            var paint=Material("Cửa xe",null,new Color(.025f,.03f,.038f),.55f);var glass=Material("Kính xe",null,new Color(.14f,.2f,.26f),.7f);
            foreach(float z in new[]{.95f,-.45f}){
                var hinge=new GameObject("Cửa xe chuyển động").transform;hinge.SetParent(root.transform);hinge.localPosition=new Vector3(-.92f,.43f,z);
                foreach(bool window in new[]{false,true}){var panel=GameObject.CreatePrimitive(PrimitiveType.Cube);UnityEngine.Object.DestroyImmediate(panel.GetComponent<Collider>());panel.transform.SetParent(hinge);panel.transform.localPosition=new Vector3(0,window?.62f:.22f,-.5f);panel.transform.localScale=new Vector3(.045f,window?.36f:.52f,.96f);panel.GetComponent<Renderer>().sharedMaterial=window?glass:paint;}
            }
            PrefabUtility.SaveAsPrefabAsset(root,Output+"BlackSedan.prefab");UnityEngine.Object.DestroyImmediate(root);
        }
        public static void PatchShop(){
            string path="Assets/BlackMarket/Resources/Worlds/NorthPointShop.prefab";var root=PrefabUtility.LoadPrefabContents(path);
            try{
                AddOpeningLayout(root.transform);
                PrefabUtility.SaveAsPrefabAsset(root,path);
            }finally{PrefabUtility.UnloadPrefabContents(root);}
            RetailShopBuilder.BakeSavedShop();
        }
        public static void AddOpeningLayout(Transform root){
                if(!root.Find("10 - Kho hẹp và sân ngoài")){
                    var group=new GameObject("10 - Kho hẹp và sân ngoài").transform;group.SetParent(root);
                    // Two staggered islands block the direct view; a 1.4m+ route bends right then left.
                    foreach(var p in new[]{new Vector3(-1.5f,0,32.35f),new Vector3(1.5f,0,34.05f),new Vector3(7,0,32),new Vector3(-6,0,33)}){
                        var shelf=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BlackMarket/Prefabs/Shop/Stock rack - ke kho va thung hang.prefab"));shelf.transform.SetParent(group);shelf.transform.localPosition=p;
                    }
                    var road=GameObject.CreatePrimitive(PrimitiveType.Cube);road.name="Mặt đường trước cửa";road.transform.SetParent(group);road.transform.localPosition=new Vector3(0,-.26f,-9);road.transform.localScale=new Vector3(75,.3f,15);road.GetComponent<Renderer>().sharedMaterial=Material("Nhựa đường",null,new Color(.045f,.055f,.065f));
                }
        }
        static void TranslateWorlds(){
            // Patch only visible text/interaction labels; do not regenerate other floors.
            foreach(var file in Directory.GetFiles("Assets/BlackMarket/Resources/Worlds","*.prefab").Concat(Directory.GetFiles("Assets/BlackMarket/Prefabs/Shop","*.prefab"))){
                var root=PrefabUtility.LoadPrefabContents(file);try{
                    foreach(var t in root.GetComponentsInChildren<TextMesh>(true))VietnameseText.TranslateLabel(t);
                    foreach(var i in root.GetComponentsInChildren<Interaction>(true))i.title=VietnameseText.Translate(i.title);
                    foreach(var d in root.GetComponentsInChildren<RoomDoor>(true))d.roomName=VietnameseText.Translate(d.roomName);
                    PrefabUtility.SaveAsPrefabAsset(root,file);
                }finally{PrefabUtility.UnloadPrefabContents(root);}
            }
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BlackMarket/Resources/Worlds/NorthPointShop.prefab"));
            EditorSceneManager.SaveScene(scene,"Assets/BlackMarket/Scenes/NorthPointShop_Environment.unity");
            EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");
        }
    }
}
