using UnityEngine;
using UnityEditor;
namespace BlackMarket.Editor {
    public static partial class NorthPointBuilder {
        static void VictorInsignia(GameObject root,Animation animation){
            if(animation.clip)animation.clip.SampleAnimation(animation.gameObject,0);
            Transform chest=null;foreach(var t in root.GetComponentsInChildren<Transform>())if(t.name=="Bip01 Spine2")chest=t;if(!chest)return;
            var previous=parent;parent=root.transform;
            var plate=Box("Victor command insignia",new Vector3(.13f,1.43f,.215f),new Vector3(.24f,.105f,.022f),"Black",false);plate.isStatic=false;plate.transform.SetParent(chest,true);
            for(int i=0;i<3;i++){var stripe=Box("Victor red rank",new Vector3(.06f+i*.067f,1.43f,.233f),new Vector3(.026f,.072f,.012f),"Red",false);stripe.isStatic=false;stripe.transform.SetParent(chest,true);}parent=previous;
        }
        static void MakeEquipment(){
            Plain("Walnut",new Color(.24f,.085f,.025f),0,.28f);Plain("LensGreen",new Color(.08f,.5f,.24f),.35f,.9f,1.2f);
            var previous=parent;var root=new GameObject("AK");parent=root.transform;
            // Recognisable AK silhouette: wooden furniture, long barrel, curved magazine, iron sights.
            Box("Stamped receiver",new Vector3(0,0,.07f),new Vector3(.065f,.075f,.31f),"Steel",false);
            Box("Dust cover",new Vector3(0,.044f,.05f),new Vector3(.062f,.025f,.28f),"Black",false);
            Box("Wooden stock",new Vector3(0,-.025f,-.25f),new Vector3(.055f,.11f,.28f),"Walnut",false);
            Box("Butt plate",new Vector3(0,-.025f,-.397f),new Vector3(.06f,.115f,.014f),"Black",false);
            var grip=Box("Pistol grip",new Vector3(0,-.09f,-.04f),new Vector3(.045f,.14f,.055f),"Walnut",false);grip.transform.localRotation=Quaternion.Euler(-18,0,0);
            Box("Wood foregrip",new Vector3(0,-.002f,.29f),new Vector3(.075f,.07f,.18f),"Walnut",false);
            Tube("Barrel",new Vector3(0,.014f,.47f),.013f,.34f,"Steel");Tube("Gas tube",new Vector3(0,.052f,.35f),.014f,.24f,"Black");Tube("Muzzle",new Vector3(0,.014f,.655f),.019f,.055f,"Black");
            for(int i=0;i<6;i++){var part=Box("Curved magazine segment",new Vector3(0,-.058f-i*.03f,.135f+i*i*.002f),new Vector3(.047f,.04f,.09f),"Black",false);part.transform.localRotation=Quaternion.Euler(-i*6,0,0);}
            for(int i=0;i<5;i++)Box("Foregrip vent",new Vector3(.039f,.005f,.23f+i*.027f),new Vector3(.003f,.025f,.008f),"Black",false);
            Box("Front sight",new Vector3(0,.057f,.57f),new Vector3(.015f,.09f,.025f),"Steel",false);Box("Rear sight",new Vector3(0,.072f,.18f),new Vector3(.04f,.025f,.025f),"Steel",false);
            Box("Charging handle",new Vector3(.053f,.005f,.02f),new Vector3(.06f,.014f,.02f),"Steel",false);
            foreach(Transform t in root.GetComponentsInChildren<Transform>())t.gameObject.isStatic=false;
            PrefabUtility.SaveAsPrefabAsset(root,Root+"Actors/AK.prefab");UnityEngine.Object.DestroyImmediate(root);
            root=new GameObject("NightVisionGoggles");parent=root.transform;
            Box("Goggle bridge",Vector3.zero,new Vector3(.2f,.065f,.065f),"Black",false);
            foreach(float x in new[]{-.065f,.065f}){Tube("Night vision tube",new Vector3(x,0,.06f),.041f,.16f,"Steel");Tube("Optical lens",new Vector3(x,0,.145f),.033f,.01f,"LensGreen");}
            Box("Head mount",new Vector3(0,.055f,-.025f),new Vector3(.07f,.08f,.045f),"Black",false);
            foreach(Transform t in root.GetComponentsInChildren<Transform>())t.gameObject.isStatic=false;
            PrefabUtility.SaveAsPrefabAsset(root,Root+"Actors/NightVisionGoggles.prefab");UnityEngine.Object.DestroyImmediate(root);parent=previous;
        }
        static void Tube(string name,Vector3 p,float radius,float length,string material){var g=GameObject.CreatePrimitive(PrimitiveType.Cylinder);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localRotation=Quaternion.Euler(90,0,0);g.transform.localScale=new Vector3(radius*2,length/2,radius*2);g.GetComponent<Renderer>().sharedMaterial=mats[material];UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());}
        static void EquipmentPickup(string id,string prefab,string title,Vector3 position){
            var point=new GameObject("Pickup / "+id);point.transform.SetParent(parent);point.transform.position=position;var interaction=point.AddComponent<Interaction>();interaction.id=id;interaction.title=title;
            var model=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"Actors/"+prefab+".prefab"),point.transform);model.transform.localPosition=Vector3.zero;model.transform.localRotation=id=="ak"?Quaternion.Euler(0,90,90):Quaternion.identity;
        }
    }
}
