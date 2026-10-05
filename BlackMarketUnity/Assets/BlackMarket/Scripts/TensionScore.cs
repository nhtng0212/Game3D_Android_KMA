using UnityEngine;
namespace BlackMarket {
    // Original synthesized score, generated once and crossfaded with actual danger.
    // No download, audio thread allocation or third-party music dependency.
    public class TensionScore:MonoBehaviour {
        AudioSource drone,pulse;AudioClip droneClip,pulseClip;Campaign game;
        const int Rate=22050,Seconds=16;
        void Start(){game=GetComponent<Campaign>();droneClip=Make(false);pulseClip=Make(true);drone=Voice(droneClip);pulse=Voice(pulseClip);}
        AudioSource Voice(AudioClip clip){var source=gameObject.AddComponent<AudioSource>();source.clip=clip;source.loop=true;source.volume=0;source.spatialBlend=0;source.Play();return source;}
        AudioClip Make(bool rhythm){
            int count=Rate*Seconds;var samples=new float[count];uint seed=71;
            for(int i=0;i<count;i++){double t=i/(double)Rate;double loop=2*System.Math.PI*t;float value;
                if(rhythm){double beat=t%.5;double envelope=System.Math.Exp(-beat*13);seed=1664525*seed+1013904223;float noise=((seed>>8)/(float)0xffffff)*2-1;value=(float)(System.Math.Sin(2*System.Math.PI*(55*beat+2*(1-System.Math.Exp(-beat*30))))*envelope*.42+noise*System.Math.Exp(-beat*65)*.08);}
                else value=(float)((System.Math.Sin(loop*55)*.2+System.Math.Sin(loop*82.5)*.11+System.Math.Sin(loop*65.375)*.07+System.Math.Sin(loop*110)*.025)*(.7+.3*System.Math.Cos(loop/16)));
                // A tiny fade on the loop seam avoids clicks for the percussive layer.
                float fade=Mathf.Min(1,Mathf.Min(i,count-1-i)/220f);samples[i]=value*fade;
            }
            var clip=AudioClip.Create(rhythm?"North Point / pursuit pulse":"North Point / evidence drone",count,1,Rate,false);clip.SetData(samples,0);return clip;
        }
        void Update(){if(!game || !drone)return;float threat=0;foreach(var e in game.enemies)if(e && e.hp>0)threat=Mathf.Max(threat,e.suspicion);if(game.upload>0)threat=Mathf.Max(threat,.8f);if(game.state.stage==6 && !game.flags.Contains("boss_dead"))threat=Mathf.Max(threat,.4f);
            bool playing=game.active && game.state.hp>0;float duck=game.paused?.3f:1;
            drone.volume=Mathf.MoveTowards(drone.volume,playing?(game.state.stage==0?.12f:.25f)*duck:0,Time.unscaledDeltaTime*.12f);
            pulse.volume=Mathf.MoveTowards(pulse.volume,playing?threat*.32f*duck:0,Time.unscaledDeltaTime*.2f);
        }
        void OnDestroy(){if(droneClip)Destroy(droneClip);if(pulseClip)Destroy(pulseClip);}
    }
}
