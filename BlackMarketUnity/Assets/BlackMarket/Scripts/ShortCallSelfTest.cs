using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
namespace BlackMarket {
 public class ShortCallSelfTest:MonoBehaviour {
  Campaign g;int failures;float deadline;List<string> report=new List<string>();
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Launch(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"--short-call-check")>=0)new GameObject("Short call QA").AddComponent<ShortCallSelfTest>();}
  void Check(bool value,string text){report.Add((value?"PASS ":"FAIL ")+text);Debug.Log(report.Last());if(!value)failures++;}
  IEnumerator Start(){deadline=Time.realtimeSinceStartup+180;Application.runInBackground=true;if(Keyboard.current!=null)InputSystem.DisableDevice(Keyboard.current);if(Mouse.current!=null)InputSystem.DisableDevice(Mouse.current);yield return null;g=Campaign.Instance;File.Delete(g.SavePath+".reading");g.NewGame();
   Check(PhoneDialogue.Nodes.Count==2&&PhoneDialogue.Nodes.Values.All(n=>n.choices.Length==0),"exactly two Marcus lines and no dialogue choices");Check(g.prologue.Playing&&!g.Running,"bedroom phone cinematic locks movement");
   while(g.prologue.Node==null)yield return null;var intro=g.prologue;Check(intro.NodeId=="start","first Marcus line appears");g.Pause();float t=intro.ReadTime;yield return new WaitForSecondsRealtime(.4f);Check(intro.ReadTime==t,"pause freezes automatic call timing");g.Resume();
   while(intro.NodeId=="start")yield return null;Check(intro.NodeId=="warning","second line follows automatically without clicking");while(intro.Phase=="dialogue")yield return null;Check(intro.Phase=="disconnected","impact and disconnection follow second line");Check(g.sound.GetComponentsInChildren<AudioSource>().Any(a=>a.clip&&a.clip.name.Contains("impactWood")&&a.isPlaying),"impact audio actually plays");
   while(g.ui.modal!="story")yield return null;Check(intro.Phase=="news","three days later story follows disconnection");while(g.ui.modal=="story"){if(g.ui.StoryReady)g.ui.ContinueStory();yield return null;}
   Check(intro.Phase=="ride","continuing news starts preserved motorcycle arrival");while(intro.Playing)yield return null;
   Check(g.Running&&g.objectiveId=="computer"&&g.state.introCompleted,"arrival returns control to USB search");Check(g.journal.Any(s=>s.Contains("North Point")&&s.Contains("006")),"journal retains both short call clues");Check(g.HasSeenStory("alex-intro"),"completed intro can be skipped on replay");g.NewGame();g.prologue.Skip();yield return null;Check(g.Running&&g.state.introCompleted,"replay skip still works");yield return StoryFlow();Finish();
  }
  IEnumerator StoryFlow(){
   var style=new GUIStyle{font=Resources.Load<Font>("Fonts/Regular"),fontSize=23,wordWrap=true};
   foreach(var content in new[]{StoryData.Terminal,StoryData.FinalFile})foreach(var page in content.Split(new[]{"\n\n"},StringSplitOptions.RemoveEmptyEntries))Check(style.CalcHeight(new GUIContent(page+"▌"),1030)<=275,"document page fits: "+page.Split('\n')[0]);
   Check(!StoryData.Terminal.Contains("Victor")&&!StoryData.Terminal.Contains("ĐÃ CHẾT"),"opening conceals culprit and murder");
   var computer=g.level.GetComponentsInChildren<Interaction>().First(i=>i.id=="computer");computer.Use();Check(g.objectiveId=="drawers"&&g.ui.modal=="story","locked computer starts USB search");g.MarkStorySeen("locked-workstation-v3");g.ui.SkipReadStory();
   var drawer=g.level.GetComponentsInChildren<OfficeDrawer>().First(d=>d.redCard);drawer.Open();while(!drawer.Revealed)yield return null;drawer.redCard.GetComponent<Interaction>().Use();Check(g.state.keycard&&g.objectiveId=="computer","drawer USB returns player to computer");computer.Use();Check(g.ui.modal=="computer","USB unlocks desktop");g.ui.OpenOrderFile();Check(g.ui.ComputerScreen&&!g.opening.Playing,"file precedes convoy");g.MarkStorySeen("terminal071-v3");g.ui.SkipReadStory();Check(g.opening.Playing,"closing order starts convoy");g.MarkStorySeen("arrival");g.opening.Skip();while(g.opening.Playing)yield return null;Check(g.objectiveId=="door06","convoy assigns escape objective");
   g.state=new CampaignSave{stage=2,checkpoint=4,keycard=true,codeRecovered=true,hasAK=true,armed=true,hasNightVision=true};g.LoadWorld(2);yield return null;g.underground.ReadFinal();Check(g.ui.ComputerScreen&&!g.state.evidenceCopied,"final evidence awaits reading and send action");g.MarkStorySeen("final-file-v3");g.ui.SkipReadStory();Check(g.state.evidenceCopied&&g.ui.modal=="story"&&g.objectiveId=="escape","send confirms evidence and shows warning");g.MarkStorySeen("evidence-sent-v3");g.ui.SkipReadStory();Check(g.Running&&g.underground.EscapeRemaining>0,"warning returns control for escape");
  }
  void Update(){if(deadline>0&&Time.realtimeSinceStartup>deadline){Check(false,"timeout");Finish();}}
  void Finish(){deadline=0;report.Add("RESULT "+failures+" failures");File.WriteAllLines("Documentation/short-call-test.txt",report);
#if UNITY_EDITOR
   UnityEditor.EditorApplication.Exit(failures==0?0:1);
#endif
  }
 }
}
