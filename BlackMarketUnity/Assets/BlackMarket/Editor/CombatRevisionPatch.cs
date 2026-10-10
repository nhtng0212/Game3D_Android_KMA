using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Unity.AI.Navigation;
namespace BlackMarket.Editor {
 public static class CombatRevisionPatch {
  public static void Run(){
   const string path="Assets/BlackMarket/Resources/Worlds/UndergroundControl.prefab";var root=PrefabUtility.LoadPrefabContents(path);
   var breaker=root.GetComponentInChildren<LightingBreaker>();breaker.transform.position=new Vector3(-9,1.8f,51.45f);breaker.transform.localScale=new Vector3(1.1f,1.5f,.55f);
   var interactions=root.GetComponentsInChildren<Interaction>(true);interactions.First(i=>i.id=="breaker").transform.position=new Vector3(-9,1.5f,50);
   var exit=interactions.First(i=>i.id=="escape");exit.transform.position=new Vector3(0,1,1.5f);exit.title="CẦU THANG / TRỞ LÊN CỬA HÀNG";
   foreach(var text in root.GetComponentsInChildren<TextMesh>()){if(text.text.Contains("NGUỒN CHIẾU SÁNG"))text.transform.position=new Vector3(-9,2.8f,51.1f);if(text.text.Contains("LỐI BẢO TRÌ")){text.text="LỐI THOÁT: QUAY LẠI CẦU THANG 006";text.transform.position=new Vector3(0,3.5f,1.25f);}}
   var gate=root.transform.Find("Maintenance gate");if(gate)Object.DestroyImmediate(gate.gameObject);
   var nav=root.GetComponent<NavMeshSurface>();if(nav){nav.BuildNavMesh();const string navPath="Assets/BlackMarket/Art/Underground/UndergroundControlNav.asset";var saved=AssetDatabase.LoadAssetAtPath<UnityEngine.AI.NavMeshData>(navPath);if(saved){EditorUtility.CopySerialized(nav.navMeshData,saved);EditorUtility.SetDirty(saved);nav.RemoveData();nav.navMeshData=saved;}else AssetDatabase.CreateAsset(nav.navMeshData,navPath);}PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");
  }
 }
}
