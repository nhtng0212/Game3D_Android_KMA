using UnityEngine;
namespace BlackMarket {
 public static class GrenadeVisual {
  public static GameObject Create(Transform parent){var root=new GameObject("Lựu đạn / thân và cần an toàn");root.transform.SetParent(parent,false);Part(root.transform,PrimitiveType.Capsule,"Thân",Vector3.zero,new Vector3(.065f,.052f,.065f));Part(root.transform,PrimitiveType.Cube,"Cần an toàn",new Vector3(.025f,.045f,0),new Vector3(.025f,.07f,.015f));Part(root.transform,PrimitiveType.Cylinder,"Chốt",new Vector3(0,.065f,0),new Vector3(.026f,.014f,.026f));return root;}
  static void Part(Transform parent,PrimitiveType type,string name,Vector3 p,Vector3 size){var o=GameObject.CreatePrimitive(type);o.name=name;Object.Destroy(o.GetComponent<Collider>());o.transform.SetParent(parent,false);o.transform.localPosition=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=Resources.Load<Material>("Materials/Black");}
 }
}
