using UnityEditor;
using UnityEditor.SceneManagement;
namespace BlackMarket.Editor {
    // Run with --self-test (and optionally --controls-check --capture) in a graphics-capable editor.
    // No player build is produced. The opt-in self-test exits this editor when it finishes.
    public static class PlayVerification {
        public static void Run(){
            EditorSceneManager.OpenScene("Assets/BlackMarket/Scenes/NorthPoint.unity");
            var gameView=System.Type.GetType("UnityEditor.GameView,UnityEditor");
            if(!UnityEngine.Application.isBatchMode && gameView!=null)EditorWindow.GetWindow(gameView).Focus();
            EditorApplication.delayCall+=()=>EditorApplication.EnterPlaymode();
        }
    }
}
