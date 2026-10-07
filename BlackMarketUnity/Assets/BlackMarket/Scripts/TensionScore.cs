using UnityEngine;
namespace BlackMarket {
    // Original, seamless synthesized music. Three persistent voices, no per-frame audio allocation.
    public class TensionScore:MonoBehaviour {
        AudioSource drone,pulse,alarm;AudioClip droneClip,pulseClip,alarmClip;Campaign game;float alertHold;GameObject lastLevel;
        public bool PursuitActive {get;private set;}
        public float AlarmVolume=>alarm?alarm.volume:0;
        const int Rate=22050,Seconds=16;
        void Start(){game=GetComponent<Campaign>();droneClip=Make(0);pulseClip=Make(1);alarmClip=Make(2);drone=Voice(droneClip);pulse=Voice(pulseClip);alarm=Voice(alarmClip);}
        AudioSource Voice(AudioClip clip){var source=gameObject.AddComponent<AudioSource>();source.clip=clip;source.loop=true;source.volume=0;source.spatialBlend=0;source.priority=80;source.Play();return source;}
        AudioClip Make(int layer){
            int count=Rate*Seconds;var samples=new float[count];uint seed=71;
            double[] notes={110,130.8125,164.8125,146.8125,110,130.8125,98,123.5};
            for(int i=0;i<count;i++){double t=i/(double)Rate;double loop=2*System.Math.PI*t;float value;
                if(layer==2){
                    // Four urgent alternating bell strikes each second, with a restrained low drum.
                    double beat=t%.25;double frequency=((int)(t*4)%2==0)?880:1174.625;
                    double bell=System.Math.Sin(2*System.Math.PI*frequency*beat)+.3*System.Math.Sin(2*System.Math.PI*frequency*2.76*beat);
                    value=(float)(bell*System.Math.Exp(-beat*19)*.25+System.Math.Sin(2*System.Math.PI*65*(t%.5))*System.Math.Exp(-(t%.5)*24)*.28);
                }else if(layer==1){double beat=t%.5;double envelope=System.Math.Exp(-beat*13);seed=1664525*seed+1013904223;float noise=((seed>>8)/(float)0xffffff)*2-1;value=(float)(System.Math.Sin(2*System.Math.PI*(55*beat+2*(1-System.Math.Exp(-beat*30))))*envelope*.42+noise*System.Math.Exp(-beat*65)*.08);}
                else {
                    double n=t%2;double melody=System.Math.Sin(2*System.Math.PI*notes[(int)(t/2)]*n)*System.Math.Sin(System.Math.PI*n/2)*.16;
                    value=(float)((System.Math.Sin(loop*55)*.18+System.Math.Sin(loop*82.5)*.1+System.Math.Sin(loop*65.375)*.06)*(.7+.3*System.Math.Cos(loop/16))+melody);
                }
                float fade=Mathf.Min(1,Mathf.Min(i,count-1-i)/110f);samples[i]=value*fade;
            }
            var clip=AudioClip.Create(layer==2?"North Point / detection bells":layer==1?"North Point / pursuit pulse":"North Point / evidence theme",count,1,Rate,false);clip.SetData(samples,0);return clip;
        }
        void Update(){
            if(!game || !drone)return;
            if(lastLevel!=game.level || !game.active || game.state.hp<=0){alertHold=0;lastLevel=game.level;}
            float threat=0;bool detected=false;
            foreach(var e in game.enemies)if(e && e.hp>0){threat=Mathf.Max(threat,e.suspicion);detected|=e.Armed && (e.mode==EnemyController.Mode.Attack || e.mode==EnemyController.Mode.Chase || e.mode==EnemyController.Mode.Search);}
            if(game.Running){if(detected)alertHold=2;else alertHold=Mathf.Max(0,alertHold-Time.deltaTime);}
            PursuitActive=game.active && game.state.hp>0 && alertHold>0;
            if(game.upload>0)threat=Mathf.Max(threat,.8f);
            bool playing=game.active && game.state.hp>0;float duck=game.paused?.25f:1;
            drone.volume=Mathf.MoveTowards(drone.volume,playing?.38f*duck:game.ui.modal=="menu"?.2f:0,Time.unscaledDeltaTime*.3f);
            pulse.volume=Mathf.MoveTowards(pulse.volume,playing?(PursuitActive?.55f:threat*.3f)*duck:0,Time.unscaledDeltaTime*.5f);
            alarm.volume=Mathf.MoveTowards(alarm.volume,PursuitActive && !game.paused?.55f:0,Time.unscaledDeltaTime*(PursuitActive?2:.45f));
        }
        void OnDestroy(){if(droneClip)Destroy(droneClip);if(pulseClip)Destroy(pulseClip);if(alarmClip)Destroy(alarmClip);}
    }
}
