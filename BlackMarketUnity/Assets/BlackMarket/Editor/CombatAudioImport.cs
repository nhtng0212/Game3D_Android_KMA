using UnityEditor;
using UnityEngine;
namespace BlackMarket.Editor {
    public class CombatAudioImport:AssetPostprocessor {
        void OnPreprocessAudio(){
            if(!assetPath.Contains("/FieldAudio/") || !assetPath.EndsWith(".wav"))return;
            var importer=(AudioImporter)assetImporter;importer.forceToMono=true;
            var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.sampleRateSetting=AudioSampleRateSetting.OverrideSampleRate;settings.sampleRateOverride=44100;settings.preloadAudioData=true;importer.defaultSampleSettings=settings;
        }
    }
}
