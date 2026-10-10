using UnityEngine;
namespace BlackMarket {
    // Draw after the HUD so the level changes only once the stairway is completely dark.
    public class DescentBlackout:MonoBehaviour {
        public float opacity;
        static readonly System.Collections.Generic.HashSet<DescentBlackout> overlays=new System.Collections.Generic.HashSet<DescentBlackout>();
        public static bool Visible {get{foreach(var overlay in overlays)if(overlay&&overlay.opacity>0)return true;return false;}}
        void OnEnable(){overlays.Add(this);}void OnDisable(){overlays.Remove(this);}

        public void FadeOutAfterLoad(){StartCoroutine(Reveal());}
        System.Collections.IEnumerator Reveal(){opacity=1;yield return null;yield return null;float t=0;while(t<.8f){if(!Campaign.Instance||!Campaign.Instance.paused){t+=Time.unscaledDeltaTime;opacity=1-Mathf.SmoothStep(0,1,t/.8f);}yield return null;}Destroy(this);}

        void OnGUI(){
            if(opacity<=0 || Campaign.Instance && Campaign.Instance.paused)return;
            var matrix=GUI.matrix;var color=GUI.color;int depth=GUI.depth;
            GUI.matrix=Matrix4x4.identity;GUI.depth=-10000;GUI.color=new Color(0,0,0,opacity);
            GUI.DrawTexture(new Rect(0,0,Screen.width,Screen.height),Texture2D.whiteTexture);
            GUI.matrix=matrix;GUI.color=color;GUI.depth=depth;
        }
    }
}
