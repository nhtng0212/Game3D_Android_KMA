using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace BlackMarket.Editor {
 public static class CampaignAssetReview {
  public static void Run(){
   var report=new List<string>();int failures=0,renderers=0,meshes=0;
   foreach(var folder in new[]{"Worlds","Actors","Opening"})foreach(var prefab in Resources.LoadAll<GameObject>(folder)){
    int before=failures;
    foreach(var t in prefab.GetComponentsInChildren<Transform>(true))if(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject)>0){report.Add("FAIL missing script: "+prefab.name+" / "+t.name);failures++;}
    foreach(var renderer in prefab.GetComponentsInChildren<Renderer>(true)){renderers++;foreach(var material in renderer.sharedMaterials)if(!material||!material.shader||material.shader.name=="Hidden/InternalErrorShader"){report.Add("FAIL missing material or shader: "+prefab.name+" / "+renderer.name);failures++;}}
    foreach(var mesh in prefab.GetComponentsInChildren<MeshFilter>(true)){meshes++;if(!mesh.sharedMesh){report.Add("FAIL missing mesh: "+prefab.name+" / "+mesh.name);failures++;}}
    foreach(var mesh in prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true)){meshes++;if(!mesh.sharedMesh){report.Add("FAIL missing skinned mesh: "+prefab.name+" / "+mesh.name);failures++;}}
    if(before==failures)report.Add("PASS prefab references: "+folder+"/"+prefab.name);
   }
   report.Add("Checked "+renderers+" renderers and "+meshes+" meshes. This does not judge animation quality or every camera angle.");report.Add("RESULT "+failures+" failures");File.WriteAllLines("Documentation/campaign-asset-review.txt",report);Debug.Log(report[report.Count-1]);EditorApplication.Exit(failures==0?0:1);
  }
 }
}
