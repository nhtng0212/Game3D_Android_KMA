using UnityEngine;
namespace BlackMarket {
    public class ExteriorRain:MonoBehaviour {
        Material material;
        void Start(){
            var g=new GameObject("Rain / exterior only");g.transform.SetParent(transform);g.transform.position=new Vector3(0,8,-12);g.transform.rotation=Quaternion.Euler(90,0,0);
            var ps=g.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=ps.main;main.startLifetime=.8f;main.startSpeed=11;main.startSize=.012f;main.maxParticles=900;main.startColor=new Color(.55f,.68f,.8f,.25f);main.simulationSpace=ParticleSystemSimulationSpace.World;
            var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(24,24,.1f);var emission=ps.emission;emission.rateOverTime=650;
            var r=ps.GetComponent<ParticleSystemRenderer>();r.renderMode=ParticleSystemRenderMode.Stretch;r.lengthScale=9;r.velocityScale=.025f;
            material=Resources.Load<Material>("Materials/Rain");r.sharedMaterial=material;ps.Play();
        }
        
    }
}
