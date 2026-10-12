using System;
using UnityEngine;
namespace BlackMarket {
    // Audio-thread filter on the listener: covers ambient, effects and cinematic sources.
    [RequireComponent(typeof(AudioListener))]
    public sealed class MasterAudioGain : MonoBehaviour {
        public const float Gain = 5f;
        public static float Amplify(float sample){
            float value=sample*Gain;
            float magnitude=Math.Abs(value);
            // Unity's output cannot exceed full scale. Smoothly compress only loud peaks.
            if(magnitude<=.85f)return value;
            return Math.Sign(value)*(.85f+.15f*(1f-(float)Math.Exp(-(magnitude-.85f)/.15f)));
        }
        void OnAudioFilterRead(float[] data,int channels){for(int i=0;i<data.Length;i++)data[i]=Amplify(data[i]);}
    }
}
