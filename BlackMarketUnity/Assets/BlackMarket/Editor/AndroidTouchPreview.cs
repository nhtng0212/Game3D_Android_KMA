using UnityEditor;
using UnityEngine;
namespace BlackMarket.Editor {
 public static class AndroidTouchPreview {
  const string Menu="BLACK MARKET/Preview Android - Nút cảm ứng";
  [MenuItem(Menu)] static void Toggle(){
   bool enabled=!EditorPrefs.GetBool("BlackMarket.AndroidPreview",true);
   EditorPrefs.SetBool("BlackMarket.AndroidPreview",enabled);
   var game=Campaign.Instance;
   if(game&&game.ui){game.ui.ResetTouchGestures();game.player?.ResetTouch();game.LockCursor(game.Running);}
  }
  [MenuItem(Menu,true)] static bool Validate(){UnityEditor.Menu.SetChecked(Menu,EditorPrefs.GetBool("BlackMarket.AndroidPreview",true));return true;}
 }
}
