using UnityEngine;
namespace BlackMarket {
    // The visible beam is clipped by the same collision mask as NPC vision.
    // Update after WeaponPose so the light and mesh originate at the held flashlight.
    [DefaultExecutionOrder(50)]
    public class FlashlightBeam:MonoBehaviour {
        const int Sides=48,Rings=5;Light lamp;Mesh mesh;Vector3[] vertices;Color[] colors;float nextUpdate;
        void Start(){lamp=GetComponent<Light>();var g=new GameObject("Visible flashlight cone");g.transform.SetParent(transform,false);mesh=new Mesh{name="Occluded flashlight beam"};g.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=g.AddComponent<MeshRenderer>();renderer.sharedMaterial=Resources.Load<Material>("Materials/FlashlightBeam");renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            vertices=new Vector3[Sides*Rings];colors=new Color[vertices.Length];var triangles=new int[(Rings-1)*Sides*6];int at=0;
            for(int r=0;r<Rings-1;r++)for(int s=0;s<Sides;s++){int a=r*Sides+s,b=r*Sides+(s+1)%Sides,c=a+Sides,d=b+Sides;triangles[at++]=a;triangles[at++]=c;triangles[at++]=b;triangles[at++]=b;triangles[at++]=c;triangles[at++]=d;}
            mesh.vertices=vertices;mesh.triangles=triangles;
        }
        void LateUpdate(){if(!mesh || Time.time<nextUpdate)return;nextUpdate=Time.time+.07f;float spread=Mathf.Tan(lamp.spotAngle*.5f*Mathf.Deg2Rad);var owner=GetComponentInParent<EnemyController>();
            for(int s=0;s<Sides;s++){float angle=s*Mathf.PI*2/Sides;Vector3 direction=new Vector3(Mathf.Cos(angle)*spread,Mathf.Sin(angle)*spread,1).normalized;float distance=lamp.range;
                foreach(var hit in Physics.RaycastAll(transform.position,transform.TransformDirection(direction),distance,~(1<<2),QueryTriggerInteraction.Ignore))if(hit.collider.GetComponentInParent<EnemyController>()!=owner)distance=Mathf.Min(distance,hit.distance);
                for(int r=0;r<Rings;r++){float f=r/(float)(Rings-1);int index=r*Sides+s;vertices[index]=direction*(.03f+Mathf.Max(0,distance-.07f)*f);colors[index]=new Color(.65f,.8f,1,r==0||r==Rings-1?0:.055f*(1-f*.65f));}
            }mesh.vertices=vertices;mesh.colors=colors;mesh.RecalculateBounds();
        }
        void OnDestroy(){if(mesh)Destroy(mesh);}
    }
}
