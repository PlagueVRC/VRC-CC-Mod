using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;

namespace VRCCCMod.LiveCaptions.GameSpecific.VRChat
{
    public class GenericAudioHooker
    {
        public static event Action<AudioSource, float[], int> OnRawAudio;

        public static void Update()
        {
            var AllAudioSources = UnityEngine.Object.FindObjectsOfType<AudioSource>();

            foreach (var source in AllAudioSources)
            {
                if (source.isPlaying && source.gameObject.active && VRCPlayer.field_Internal_Static_VRCPlayer_0 != null && Vector3.Distance(VRCPlayer.field_Internal_Static_VRCPlayer_0.transform.position, source.transform.position) < 2)
                {
                    var Data = source.GetOutputData(256, 0);

                    ProcessData(source, Data);
                }
            }
        }

        public static Dictionary<AudioSource, float[]> LastData = new Dictionary<AudioSource, float[]>();

        private static void ProcessData(AudioSource __instance, Il2CppStructArray<float> __0)
        {
            if (__instance == null || __0.ToArray().Length <= 0 || __instance.transform.root.GetComponentInChildren<VRCPlayer>() != null || (LastData.ContainsKey(__instance) && LastData[__instance] == __0.ToArray())) // If Is Ded Or Player
                return;

            LastData[__instance] = __0.ToArray();

            int sample_rate = 48000; //default vrchat
            if (__instance.clip != null)
                sample_rate = __instance.clip.frequency;

            OnRawAudio(__instance, __0, sample_rate);
        }
    }
}
