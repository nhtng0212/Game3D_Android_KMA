using UnityEngine;
using UnityEditor;
namespace BlackMarket.Editor {
    // Import only the two selected MIT Rocketbox avatars and their native animation rigs.
    public class RocketboxImport : AssetPostprocessor {
        bool IsRocket=>assetPath.Contains("/Rocketbox/");
        void OnPreprocessModel(){if(!IsRocket)return;var i=(ModelImporter)assetImporter;i.animationType=ModelImporterAnimationType.Legacy;i.importAnimation=true;i.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;i.isReadable=true;}
        void OnPreprocessTexture(){if(!IsRocket)return;var i=(TextureImporter)assetImporter;i.maxTextureSize=1024;if(assetPath.Contains("normal"))i.textureType=TextureImporterType.NormalMap;}
        void OnPostprocessModel(GameObject g){if(!IsRocket)return;foreach(var t in g.GetComponentsInChildren<Transform>(true))if(t.name.ToLower().Contains("poly"))t.gameObject.SetActive(t.name.ToLower().Contains("hipoly"));}
    }
}
