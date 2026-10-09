using System.Collections;
using UnityEngine;
namespace BlackMarket {
 // Shared setup for opt-in regression suites. MissionFlowSelfTest exercises real walking and targeting.
 public static class MissionTestSteps {
  public static void SkipIntro(Campaign g){if(g.prologue && g.prologue.Playing){g.MarkStorySeen("alex-intro");g.prologue.Skip();}}
 public static IEnumerator RevealDrawers(Campaign g){foreach(var d in g.level.GetComponentsInChildren<OfficeDrawer>()){d.Open();yield return new WaitForSeconds(.7f);}}
  public static IEnumerator PassAirlock(Campaign g){
   var air=g.airlock;while(air.OuterMoving)yield return null;
   g.player.body.enabled=false;g.player.transform.position=air.transform.TransformPoint(new Vector3(0,0,1.9f));g.player.body.enabled=true;air.StartScan();
   while(g.state.stage==0)yield return null;
  }
 }
}
