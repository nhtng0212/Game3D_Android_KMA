using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Linq;
namespace BlackMarket.Editor {
 public static class TransitionFixBuilder {
  const string World="Assets/BlackMarket/Resources/Worlds/NorthPointShop.prefab";
  public static void Build(){
   var path="Assets/BlackMarket/Resources/Materials/DescentDarkness.mat";
   var material=AssetDatabase.LoadAssetAtPath<Material>(path);
   if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Unlit"));AssetDatabase.CreateAsset(material,path);}material.SetColor("_BaseColor",Color.black);
   var root=PrefabUtility.LoadPrefabContents(World);
   try{
    var air=root.GetComponentInChildren<BasementAirlock>();
    var endWall=air.transform.Find("Cuối hành lang xuống hầm");if(endWall)endWall.gameObject.SetActive(false);
    var light=air.transform.Find("Đèn chiếu nghỉ B1");if(light)light.gameObject.SetActive(false);
    if(!air.transform.Find("Lối tối xuống B1")){
     var dark=GameObject.CreatePrimitive(PrimitiveType.Cube);dark.name="Lối tối xuống B1";dark.transform.SetParent(air.transform,false);dark.transform.localPosition=new Vector3(0,-1.25f,12.65f);dark.transform.localScale=new Vector3(3.2f,3.6f,.08f);Object.DestroyImmediate(dark.GetComponent<Collider>());dark.GetComponent<Renderer>().sharedMaterial=material;
    }
    foreach(var item in root.GetComponentsInChildren<Interaction>(true))if(item.id=="frontdoor")item.title="CỬA CHÍNH";
    foreach(var text in root.GetComponentsInChildren<TextMesh>(true))if(text.text.Contains("CỬA TRƯỚC / ĐÃ KHÓA") || text.text.Contains("CỬA CHÍNH ĐÃ BỊ KHÓA"))text.text="CỬA CHÍNH";
    PrefabUtility.SaveAsPrefabAsset(root,World);
   }finally{PrefabUtility.UnloadPrefabContents(root);}
   AssetDatabase.SaveAssets();
   // Refresh the saved environment reference through its existing prefab instance.
   EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPointShop_Environment.unity");EditorSceneManager.SaveOpenScenes();EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");
   Debug.Log("TRANSITION FIX APPLIED");
  }
 }
}
