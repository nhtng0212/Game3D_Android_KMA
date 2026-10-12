using System;
using System.IO;
using UnityEngine;
namespace BlackMarket {
    // Persist device exceptions even if the graphics/UI cannot render them.
    public sealed class AndroidStartupDiagnostics : MonoBehaviour {
        static readonly object gate=new object();
        string path;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install(){if(Application.platform!=RuntimePlatform.Android)return;var go=new GameObject("Android startup diagnostics");DontDestroyOnLoad(go);go.AddComponent<AndroidStartupDiagnostics>();}
        void Awake(){
            path=Path.Combine(Application.persistentDataPath,"android-startup.log");
            try{File.WriteAllText(path,$"{DateTime.UtcNow:O}\nUnity {Application.unityVersion}\n{SystemInfo.deviceModel} / {SystemInfo.operatingSystem}\nGPU {SystemInfo.graphicsDeviceName} / {SystemInfo.graphicsDeviceType}\n");}catch(Exception){}
            Application.logMessageReceivedThreaded+=Record;
            Invoke(nameof(Report),5);
        }
        void Record(string message,string stack,LogType type){
            if(type!=LogType.Exception&&type!=LogType.Error&&type!=LogType.Assert)return;
            lock(gate){try{File.AppendAllText(path,type+": "+message+"\n"+stack+"\n");}catch(Exception){}}
        }
        void Report(){var g=Campaign.Instance;string line=$"Startup: campaign={g!=null}, level={(g&&g.level)}, player={(g&&g.player)}, UI={(g&&g.ui?g.ui.modal:"missing")}, screen={Screen.width}x{Screen.height}, safe={Screen.safeArea}\n";lock(gate){try{File.AppendAllText(path,line);}catch(Exception){}}Debug.Log(line);}
        void OnDestroy(){Application.logMessageReceivedThreaded-=Record;}
    }
}
