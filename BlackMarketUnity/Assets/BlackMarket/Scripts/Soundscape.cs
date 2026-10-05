using UnityEngine;
using System.Collections.Generic;
namespace BlackMarket {
    public class Soundscape : MonoBehaviour {
        readonly Dictionary<string,AudioClip> clips=new Dictionary<string,AudioClip>();
        readonly Dictionary<string,AudioClip[]> variations=new Dictionary<string,AudioClip[]>();
        readonly Dictionary<string,int> lastVariation=new Dictionary<string,int>();
        public int VariantCount(string id)=>variations.TryGetValue(id,out var bank)?bank.Length:0;
        AudioLowPassFilter[] filters;AudioSource[] voices;int cursor;AudioSource ambience,rain;AudioClip[] footsteps;
        void Awake(){
            foreach(var id in new[]{"shot","reload","door","beep","alarm","hit","step","ring","ambient"})clips[id]=Resources.Load<AudioClip>("Audio/"+id);
            var foley=Resources.Load<AudioClip>("Foley/footstep_concrete_000");if(foley)clips["step"]=foley;
            var shot=Resources.Load<AudioClip>("FieldAudio/shot");if(shot)clips["shot"]=shot;var reload=Resources.Load<AudioClip>("FieldAudio/reload");if(reload)clips["reload"]=reload;footsteps=new AudioClip[5];for(int k=0;k<5;k++)footsteps[k]=Resources.Load<AudioClip>("Foley/footstep_concrete_00"+k);
            foreach(var id in new[]{"pistol","rifle"}){var bank=new List<AudioClip>();for(int i=0;i<3;i++){var take=Resources.Load<AudioClip>("FieldAudio/"+id+"_"+i);if(take)bank.Add(take);}if(bank.Count>0){variations[id]=bank.ToArray();clips[id]=bank[0];}else clips[id]=clips["shot"];}
            clips["door_move"]=Resources.Load<AudioClip>("FieldAudio/door_move")??clips["door"];
            clips["door_latch"]=Resources.Load<AudioClip>("Foley/impactMetal_medium_000");
            clips["impact_metal"]=Resources.Load<AudioClip>("Foley/impactMetal_light_001");clips["impact_wood"]=Resources.Load<AudioClip>("Foley/impactWood_light_001");clips["impact_stone"]=Resources.Load<AudioClip>("Foley/impactPlate_light_001");
            rain=gameObject.AddComponent<AudioSource>();rain.clip=Resources.Load<AudioClip>("FieldAudio/1");rain.loop=true;rain.volume=0;rain.Play();
            clips["run"]=clips["step"];clips["draw"]=Resources.Load<AudioClip>("Foley/impactMetal_light_000")??clips["reload"];
            filters=new AudioLowPassFilter[32];voices=new AudioSource[32];for(int i=0;i<voices.Length;i++){var g=new GameObject("Spatial voice "+i);g.transform.SetParent(transform);voices[i]=g.AddComponent<AudioSource>();filters[i]=g.AddComponent<AudioLowPassFilter>();filters[i].cutoffFrequency=22000;voices[i].minDistance=2;voices[i].maxDistance=35;voices[i].rolloffMode=AudioRolloffMode.Logarithmic;}
            ambience=gameObject.AddComponent<AudioSource>();ambience.clip=clips["ambient"];ambience.loop=true;ambience.volume=.09f;ambience.Play();
        }
        void Update(){var g=Campaign.Instance;if(!g || !g.player)return;float target=g.state.stage==0?(g.player.transform.position.z<0?.23f:.035f):0;rain.volume=Mathf.MoveTowards(rain.volume,target,Time.unscaledDeltaTime*.15f);}
        public void Play(string id,float volume,Vector3? position=null){
            if(!clips.TryGetValue(id,out var clip)||!clip)return;
            int slot=-1;for(int i=0;i<voices.Length;i++){int candidate=(cursor+i)%voices.Length;if(!voices[candidate].isPlaying){slot=candidate;break;}}
            if(slot<0){slot=cursor%voices.Length;for(int i=0;i<voices.Length;i++)if(voices[i].priority>voices[slot].priority)slot=i;}
            cursor=(slot+1)%voices.Length;var v=voices[slot];v.Stop();
            if(variations.TryGetValue(id,out var bank)){lastVariation.TryGetValue(id,out int last);int choice=(last+Random.Range(1,bank.Length))%bank.Length;clip=bank[choice];lastVariation[id]=choice;}
            v.clip=(id=="step" || id=="run") && footsteps!=null?footsteps[Random.Range(0,footsteps.Length)]:clip;
            bool firearm=id=="pistol" || id=="rifle";v.priority=firearm?64:128;v.volume=volume;v.pitch=id=="run"?Random.Range(.85f,.98f):id=="step"?Random.Range(.85f,1.12f):Random.Range(.985f,1.015f);
            bool occluded=false;
            if(position.HasValue && Campaign.Instance && Campaign.Instance.player){var from=position.Value+Vector3.up*.1f;var to=Campaign.Instance.player.camera.transform.position;foreach(var hit in Physics.RaycastAll(from,(to-from).normalized,Vector3.Distance(from,to),~(1<<2),QueryTriggerInteraction.Ignore))if(!hit.collider.GetComponentInParent<EnemyController>()){occluded=true;break;}}
            filters[slot].cutoffFrequency=occluded?1800:22000;if(occluded)v.volume*=.55f;v.spatialBlend=position.HasValue?1:0;v.maxDistance=firearm?55:35;if(position.HasValue)v.transform.position=position.Value;v.Play();
        }
        public void Stop(string id){if(!clips.TryGetValue(id,out var clip))return;foreach(var voice in voices)if(voice.clip==clip)voice.Stop();}
        public void Impact(RaycastHit hit){string material=hit.collider.name.ToLowerInvariant();string id=material.Contains("metal")||material.Contains("steel")||material.Contains("door")||material.Contains("locker")?"impact_metal":material.Contains("wood")||material.Contains("carton")?"impact_wood":"impact_stone";Play(id,.1f,hit.point);}

    }
    public class Effects : MonoBehaviour {
        float life;Light flash;LineRenderer line;
        public static void Shot(Vector3 start,Vector3 end,bool hostile){
            var g=new GameObject("Shot / transient");var fx=g.AddComponent<Effects>();fx.life=.07f;
            fx.line=g.AddComponent<LineRenderer>();fx.line.sharedMaterial=Resources.Load<Material>("Materials/Tracer");fx.line.positionCount=2;fx.line.SetPosition(0,start);fx.line.SetPosition(1,end);fx.line.startWidth=.015f;fx.line.endWidth=.003f;fx.line.startColor=fx.line.endColor=hostile?new Color(1,.4f,.15f):new Color(1,.84f,.5f);
            g.transform.position=start;fx.flash=g.AddComponent<Light>();fx.flash.color=new Color(1,.64f,.3f);fx.flash.intensity=28;fx.flash.range=4;fx.flash.shadows=LightShadows.None;
        }
        public static void Impact(Vector3 position,Vector3 normal){
            var g=new GameObject("Concrete dust / impact");g.transform.position=position+normal*.015f;g.transform.rotation=Quaternion.LookRotation(normal);
            var ps=g.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.loop=false;main.duration=.3f;main.startLifetime=new ParticleSystem.MinMaxCurve(.12f,.35f);main.startSpeed=new ParticleSystem.MinMaxCurve(.6f,2);main.startSize=new ParticleSystem.MinMaxCurve(.012f,.035f);main.startColor=new Color(.7f,.58f,.38f);main.gravityModifier=.5f;main.maxParticles=8;
            var emission=ps.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,7)});var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=50;shape.radius=.025f;ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=Resources.Load<Material>("Materials/Tracer");ps.Play();Destroy(g,.7f);
        }
        void Update(){life-=Time.deltaTime;if(flash)flash.intensity=28*Mathf.Clamp01(life/.07f);if(life<=0)Destroy(gameObject);}
    }
}
