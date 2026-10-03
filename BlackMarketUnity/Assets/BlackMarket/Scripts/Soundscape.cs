using UnityEngine;
using System.Collections.Generic;
namespace BlackMarket {
    public class Soundscape : MonoBehaviour {
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        AudioSource[] voices;int cursor;AudioSource ambience,rain;AudioClip[] footsteps;
        void Awake(){
            foreach(var id in new[]{"shot","reload","door","beep","alarm","hit","step","ring","ambient"})clips[id]=Resources.Load<AudioClip>("Audio/"+id);
            var foley=Resources.Load<AudioClip>("Foley/footstep_concrete_000");if(foley)clips["step"]=foley;
            var shot=Resources.Load<AudioClip>("FieldAudio/shot");if(shot)clips["shot"]=shot;var reload=Resources.Load<AudioClip>("FieldAudio/reload");if(reload)clips["reload"]=reload;footsteps=new AudioClip[5];for(int k=0;k<5;k++)footsteps[k]=Resources.Load<AudioClip>("Foley/footstep_concrete_00"+k);
            rain=gameObject.AddComponent<AudioSource>();rain.clip=Resources.Load<AudioClip>("FieldAudio/1");rain.loop=true;rain.volume=0;rain.Play();
            voices=new AudioSource[16];for(int i=0;i<voices.Length;i++){var g=new GameObject("Spatial voice "+i);g.transform.SetParent(transform);voices[i]=g.AddComponent<AudioSource>();voices[i].minDistance=2;voices[i].maxDistance=35;voices[i].rolloffMode=AudioRolloffMode.Logarithmic;}
            ambience=gameObject.AddComponent<AudioSource>();ambience.clip=clips["ambient"];ambience.loop=true;ambience.volume=.09f;ambience.Play();
        }
        void Update(){var g=Campaign.Instance;if(!g || !g.player)return;float target=g.state.stage<2?(g.player.transform.position.z<0?.23f:.035f):0;rain.volume=Mathf.MoveTowards(rain.volume,target,Time.unscaledDeltaTime*.15f);}
        public void Play(string id,float volume,Vector3? position=null){if(!clips.TryGetValue(id,out var clip)||!clip)return;var v=voices[cursor++%voices.Length];v.Stop();v.clip=id=="step" && footsteps!=null?footsteps[Random.Range(0,footsteps.Length)]:clip;v.volume=volume;v.pitch=id=="step"?Random.Range(.85f,1.12f):Random.Range(.97f,1.03f);v.spatialBlend=position.HasValue?1:0;if(position.HasValue)v.transform.position=position.Value;v.Play();}
    }
    public class Effects : MonoBehaviour {
        float life;Light flash;LineRenderer line;
        public static void Shot(Vector3 start,Vector3 end,bool hostile){
            var g=new GameObject("Shot / transient");var fx=g.AddComponent<Effects>();fx.life=.07f;
            fx.line=g.AddComponent<LineRenderer>();fx.line.sharedMaterial=Resources.Load<Material>("Materials/Tracer");fx.line.positionCount=2;fx.line.SetPosition(0,start);fx.line.SetPosition(1,end);fx.line.startWidth=.015f;fx.line.endWidth=.003f;fx.line.startColor=fx.line.endColor=hostile?new Color(1,.4f,.15f):new Color(1,.84f,.5f);
            g.transform.position=start;fx.flash=g.AddComponent<Light>();fx.flash.color=new Color(1,.64f,.3f);fx.flash.intensity=2;fx.flash.range=3;fx.flash.shadows=LightShadows.None;
        }
        public static void Impact(Vector3 position,Vector3 normal){
            var g=new GameObject("Concrete dust / impact");g.transform.position=position+normal*.015f;g.transform.rotation=Quaternion.LookRotation(normal);
            var ps=g.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.loop=false;main.duration=.3f;main.startLifetime=new ParticleSystem.MinMaxCurve(.12f,.35f);main.startSpeed=new ParticleSystem.MinMaxCurve(.6f,2);main.startSize=new ParticleSystem.MinMaxCurve(.012f,.035f);main.startColor=new Color(.7f,.58f,.38f);main.gravityModifier=.5f;main.maxParticles=8;
            var emission=ps.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,7)});var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=50;shape.radius=.025f;ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=Resources.Load<Material>("Materials/Tracer");ps.Play();Destroy(g,.7f);
        }
        void Update(){life-=Time.deltaTime;if(life<=0)Destroy(gameObject);}
    }
}
