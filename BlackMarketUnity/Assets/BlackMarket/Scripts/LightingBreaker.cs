using UnityEngine;
namespace BlackMarket {
 public class LightingBreaker:MonoBehaviour {public float hp=60;public void Hit(float damage){var g=Campaign.Instance;if(!g||!g.underground||!g.underground.Available("breaker"))return;hp-=damage;if(hp<=0){BlastEffects.Explode(transform.position,1);g.underground.BreakLights();}}}

}
