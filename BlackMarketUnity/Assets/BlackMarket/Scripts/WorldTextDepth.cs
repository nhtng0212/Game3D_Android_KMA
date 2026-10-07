using UnityEngine;
namespace BlackMarket {
 // Share a depth-tested shader; each label supplies its own font atlas.
 [ExecuteAlways,RequireComponent(typeof(TextMesh))]
 public class WorldTextDepth:MonoBehaviour {
  TextMesh text;MaterialPropertyBlock properties;
  void OnEnable(){text=GetComponent<TextMesh>();properties=new MaterialPropertyBlock();Font.textureRebuilt+=Refresh;Refresh(text.font);}
  void Refresh(Font font){if(!text||!text.font||font!=text.font)return;var material=Resources.Load<Material>("Materials/WorldText");if(!material)return;var renderer=GetComponent<Renderer>();renderer.sharedMaterial=material;properties.SetTexture("_MainTex",font.material.mainTexture);renderer.SetPropertyBlock(properties);}
  void OnDisable(){Font.textureRebuilt-=Refresh;}
 }
}
