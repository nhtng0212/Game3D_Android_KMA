using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace BlackMarket.Editor {
 [InitializeOnLoad]public static class ControlPlayShortcut {
  const string Key="BlackMarket.OpenControl";
  static ControlPlayShortcut(){EditorApplication.playModeStateChanged+=s=>{if(s==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Key,false)){SessionState.SetBool(Key,false);EditorApplication.delayCall+=Start;}};}
  [MenuItem("BLACK MARKET/Map 3/Play control center")]
  static void Play(){if(EditorApplication.isPlaying){Start();return;}if(!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");EditorApplication.EnterPlaymode();}
  static void Start(){var g=Campaign.Instance;if(!g){EditorApplication.delayCall+=Start;return;}g.state=new CampaignSave{stage=2,keycard=true,introCompleted=true,armed=true,hasAK=true,hasSniper=true,hasNightVision=true,selectedWeapon=1,hp=100,rifleAmmo=30,rifleReserve=240,sniperAmmo=5,sniperReserve=30,grenades=4};g.LoadWorld(2);}
  [MenuItem("BLACK MARKET/Map 3/Open editable prefab")]
  static void Edit(){AssetDatabase.OpenAsset(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/BlackMarket/Resources/Worlds/UndergroundControl.prefab"));}
 }
}
