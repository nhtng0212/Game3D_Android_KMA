using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace BlackMarket {
 public class DemoControlsSelfTest:MonoBehaviour {
  List<string> results=new List<string>();int failures;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void StartTest(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"--demo-check")>=0)new GameObject("Demo controls QA").AddComponent<DemoControlsSelfTest>();}
  void Check(bool value,string label){results.Add((value?"PASS ":"FAIL ")+label);if(!value)failures++;}
  IEnumerator Start(){yield return null;var g=Campaign.Instance;
   for(int i=0;i<10;i++){DemoCheckpoints.Jump(g,i);yield return null;Check(g.demoMode&&g.state.stage==(i<3?0:i<7?1:2)&&g.player&&g.level,"demo checkpoint "+i);}
   DemoCheckpoints.Jump(g,4);yield return null;var ui=g.ui;var p=g.player;
   ui.BeginTouchFrame();var center=ui.TouchRect("move").center;
   ui.ProcessTouch(11,center,Vector2.zero,true,false);ui.ProcessTouch(11,center+new Vector2(0,-55),Vector2.zero,false,false);
   float yaw=p.yaw;ui.ProcessTouch(12,new Vector2(820,350),new Vector2(12,0),true,false);ui.ProcessTouch(13,ui.TouchRect("fire").center,Vector2.zero,true,false);
   Check(p.touchMove.y>0&&p.touchFire&&p.yaw>yaw,"three concurrent fingers move look fire");
   ui.ProcessTouch(14,ui.TouchRect("aim").center,Vector2.zero,true,false);ui.ProcessTouch(14,ui.TouchRect("aim").center,Vector2.zero,false,true);ui.BeginTouchFrame();Check(p.touchAim,"aim remains on after release");
   ui.ProcessTouch(11,center,Vector2.zero,false,true);ui.ProcessTouch(12,new Vector2(820,350),Vector2.zero,false,true);ui.ProcessTouch(13,ui.TouchRect("fire").center,Vector2.zero,false,true);ui.BeginTouchFrame();Check(!p.touchFire&&p.touchMove==Vector2.zero,"released fingers do not stick");
   p.ResetTouch();p.crouch=false;p.slowWalk=false;Check(p.MovementSpeed==6.5f,"default movement is running");
   ui.BeginTouchFrame();ui.ProcessTouch(20,ui.TouchRect("slow").center,Vector2.zero,true,false);ui.ProcessTouch(20,ui.TouchRect("slow").center,Vector2.zero,false,true);Check(p.slowWalk&&p.MovementSpeed==3.2f,"slow toggle remains after release");
   ui.ProcessTouch(21,ui.TouchRect("slow").center,Vector2.zero,true,false);ui.ProcessTouch(21,ui.TouchRect("slow").center,Vector2.zero,false,true);Check(!p.slowWalk&&p.MovementSpeed==6.5f,"second slow tap restores running");p.crouch=true;Check(p.MovementSpeed==2,"crouch keeps its speed");
   g.Menu();Check(!g.demoMode,"main menu exits demo save mode");File.WriteAllLines("Documentation/demo-controls-test.txt",results);
#if UNITY_EDITOR
   UnityEditor.EditorApplication.Exit(failures==0?0:1);
#endif
  }
 }
}
