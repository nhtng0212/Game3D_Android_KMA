using System;
using UnityEngine;
namespace BlackMarket {
 // Preserve the old QA entry point while testing the current linear opening.
 public class PrologueSelfTest:MonoBehaviour {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
  static void Launch(){
   var args=Environment.GetCommandLineArgs();
   if(Debug.isDebugBuild&&Array.IndexOf(args,"--intro-check")>=0&&Array.IndexOf(args,"--short-call-check")<0)
    new GameObject("Short call QA").AddComponent<ShortCallSelfTest>();
  }
 }
}
