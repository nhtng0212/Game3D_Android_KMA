using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace BlackMarket.Editor {
    public class ShopAssetImport : AssetPostprocessor {
        bool Shop => assetPath.StartsWith("Assets/BlackMarket/Art/ShopSources/");
        void OnPreprocessModel() {
            if (!Shop) return;
            var importer = (ModelImporter)assetImporter;
            importer.importAnimation = false;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.isReadable = true;
        }
        void OnPreprocessTexture() {
            if (!Shop) return;
            var importer = (TextureImporter)assetImporter;
            string name = Path.GetFileName(assetPath).ToLowerInvariant();
            bool normal = name.Contains("nor_gl") || name.Contains("normal");
            importer.maxTextureSize = 1024;
            importer.mipmapEnabled = true;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = !(normal || name.Contains("rough") || name.Contains("metal") || name.Contains("mask") || name.Contains("opacity"));
        }
        public static void Inspect() {
            var lines = new System.Collections.Generic.List<string>();
            foreach (var path in Directory.GetFiles("Assets/BlackMarket/Art/ShopSources", "*.fbx", SearchOption.AllDirectories)) {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var go = Object.Instantiate(asset);
                var rs = go.GetComponentsInChildren<Renderer>();
                if (rs.Length == 0) { Object.DestroyImmediate(go); continue; }
                var bounds = rs[0].bounds;
                foreach (var r in rs) bounds.Encapsulate(r.bounds);
                lines.Add(path + " BOUNDS " + bounds + " MATS " + string.Join(",", rs.SelectMany(r => r.sharedMaterials).Where(m => m).Select(m => m.name).Distinct()));
                foreach (var r in rs) lines.Add("  " + r.name + " " + r.bounds);
                Object.DestroyImmediate(go);
            }
            File.WriteAllLines("Documentation/shop-import-inspection.txt", lines);
        }
    }
}
