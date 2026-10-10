using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace BlackMarket {
    public class NightVision:MonoBehaviour {
        public bool Active {get;private set;}
        Campaign game;Volume volume;VolumeProfile profile;GameObject goggles;Transform head;
        public void Setup(Campaign owner){game=owner;var node=new GameObject("Player-only night vision");node.transform.SetParent(transform,false);node.layer=2;volume=node.AddComponent<Volume>();volume.isGlobal=true;volume.priority=100;volume.weight=0;
            profile=ScriptableObject.CreateInstance<VolumeProfile>();var color=profile.Add<ColorAdjustments>();color.postExposure.Override(3f);color.saturation.Override(-10);color.colorFilter.Override(new Color(.15f,1,.2f));var tone=profile.Add<Tonemapping>();tone.mode.Override(TonemappingMode.Neutral);var grain=profile.Add<FilmGrain>();grain.intensity.Override(.25f);var vignette=profile.Add<Vignette>();vignette.intensity.Override(.3f);volume.sharedProfile=profile;
            GetComponent<UniversalAdditionalCameraData>().volumeLayerMask=1|(1<<2);
            foreach(var t in game.player.visual.GetComponentsInChildren<Transform>())if(t.name=="Bip01 Head")head=t;
            if(head){goggles=Instantiate(Resources.Load<GameObject>("Actors/NightVisionGoggles"));foreach(var t in goggles.GetComponentsInChildren<Transform>())t.gameObject.layer=2;goggles.SetActive(false);}
        }
        public void Toggle(){if(!game.Running)return;if(!game.state.hasNightVision){game.ui.Toast("Lấy kính nhìn đêm cùng bộ lựu đạn và đạn dự trữ ở kho vũ khí tầng quân giới.");return;}Active=!Active;game.sound.Play("beep",.18f);}
        void LateUpdate(){if(!game || !volume)return;bool enabled=!(game.underground && game.underground.Playing) && Active && game.state.hasNightVision && !game.security.opened;volume.weight=enabled?1:0;if(goggles){goggles.SetActive(enabled);if(enabled && head)goggles.transform.SetPositionAndRotation(head.position+game.player.visual.forward*.12f+Vector3.up*.08f,game.player.visual.rotation);}}
        void OnDestroy(){if(goggles)Destroy(goggles);if(profile){foreach(var c in profile.components)Destroy(c);Destroy(profile);}}
    }
}
