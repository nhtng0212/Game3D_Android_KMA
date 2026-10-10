using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace BlackMarket.Editor {
 [InitializeOnLoad]
 public static class ArmoryPlayShortcut {
  const string Key="BlackMarket.OpenArmory";
  static ArmoryPlayShortcut(){EditorApplication.playModeStateChanged+=Changed;}
  [MenuItem("BLACK MARKET/Map 2/Play from weapon room chapter")]
  static void Play(){if(EditorApplication.isPlaying){Start();return;}if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");EditorApplication.EnterPlaymode();}
  static void Changed(PlayModeStateChange state){if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false)){SessionState.SetBool(Key,false);EditorApplication.delayCall+=Start;}}
  static void Start(){var g=Campaign.Instance;if(!g){EditorApplication.delayCall+=Start;return;}g.state=new CampaignSave{stage=1,keycard=true,introCompleted=true};g.LoadWorld(1);}
  [MenuItem("BLACK MARKET/Map 2/Open editable prefab")]
  static void Edit(){AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BlackMarket/Resources/Worlds/UndergroundArmory.prefab"));}
 }
}
