using UnityEngine;
namespace BlackMarket {
 public static class EndingNeighborhood {
  public static GameObject Build(Transform parent){
   foreach(var t in parent.GetComponentsInChildren<Transform>())if(t.name=="Dãy nhà bên")Object.Destroy(t.gameObject);
   var root=new GameObject("Khu phố North Point / cảnh kết");root.transform.SetParent(parent,false);
   var wall=Material(new Color(.28f,.31f,.33f));var dark=Material(new Color(.07f,.09f,.11f));var pavement=Material(new Color(.36f,.37f,.38f));var window=Material(new Color(.38f,.49f,.55f));var glow=Material(new Color(.95f,.74f,.38f),true);
   GameObject Box(string name,Vector3 p,Vector3 size,Material m){var o=GameObject.CreatePrimitive(PrimitiveType.Cube);o.name=name;o.transform.SetParent(root.transform,false);o.transform.localPosition=p;o.transform.localScale=size;o.GetComponent<Renderer>().sharedMaterial=m;Object.Destroy(o.GetComponent<Collider>());return o;}
   Box("Đường phố kéo dài",new Vector3(25,-.13f,-12),new Vector3(230,.2f,18),dark);
   Box("Đường bên cửa hàng",new Vector3(32,-.14f,40),new Vector3(12,.2f,115),dark);
   foreach(float z in new[]{-23f,-2f})Box("Vỉa hè",new Vector3(25,0,z),new Vector3(230,.25f,3),pavement);
   for(int x=-85;x<140;x+=8)Box("Vạch đường",new Vector3(x,-.015f,-12),new Vector3(3,.025f,.12f),glow);
   for(int i=0;i<28;i++){
    float x=i<10?-78+i*19:45+(i%2)*22;float z=i<10?-42:18+(i-10)/2*23;float h=7+(i%4)*2.5f;if(i>=16){x=-48+(i%4)*24;z=60+(i-16)/4*24;h=16+(i%3)*4;}if(i>=24){x=-38;z=12+(i-24)*22;h=10+(i%3)*3;}float front=i<10?1:-1;
    Box("Nhà phố "+(i+1),new Vector3(x,h/2,z),new Vector3(15,h,15),wall);Box("Mái nhà",new Vector3(x,h+.2f,z),new Vector3(15.6f,.4f,15.6f),dark);
    Box("Cửa ra vào",new Vector3(x,1.4f,z+front*7.55f),new Vector3(1.8f,2.8f,.12f),dark);Box("Mái hiên cửa hàng",new Vector3(x,3.2f,z+front*8),new Vector3(12,.2f,2),pavement);
    for(int row=0;row<(int)(h/3);row++)for(int col=0;col<4;col++)Box("Cửa sổ nhà phố",new Vector3(x-5.2f+col*3.4f,2+row*3,z+front*7.55f),new Vector3(1.6f,1.6f,.1f),(row+col+i)%4==0?glow:window);
   }
   for(int i=0;i<14;i++){
    float x=-75+i*15;float z=i%2==0?-22:-3;Box("Cột đèn đường",new Vector3(x,3,z),new Vector3(.16f,6,.16f),dark);Box("Tay đèn",new Vector3(x,6,z+(z<-10?1:-1)),new Vector3(.16f,.16f,2),dark);Box("Chụp đèn",new Vector3(x,5.9f,z+(z<-10?2:-2)),new Vector3(.7f,.15f,1.1f),glow);
    var lamp=new GameObject("Ánh sáng đèn đường");lamp.transform.SetParent(root.transform,false);lamp.transform.localPosition=new Vector3(x,5.5f,z);var light=lamp.AddComponent<Light>();light.range=15;light.intensity=3;light.color=new Color(1,.8f,.55f);light.shadows=LightShadows.None;
   }
   var car=Resources.Load<GameObject>("Opening/BlackSedan");if(car)for(int i=0;i<6;i++){var parked=Object.Instantiate(car,root.transform);parked.name="Xe đỗ khu phố";parked.transform.localPosition=new Vector3(-65+i*28,0,-15.5f);parked.transform.localRotation=Quaternion.Euler(0,90,0);foreach(var c in parked.GetComponentsInChildren<Collider>())Object.Destroy(c);}
   return root;
  }
  static Material Material(Color color,bool emission=false){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;if(emission){m.EnableKeyword("_EMISSION");m.SetColor("_EmissionColor",color*1.2f);}return m;}
 }
}
