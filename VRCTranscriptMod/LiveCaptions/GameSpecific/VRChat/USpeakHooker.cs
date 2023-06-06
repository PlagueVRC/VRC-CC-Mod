

using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using MelonLoader;
using UnhollowerBaseLib;

namespace VRCCCMod.CC.GameSpecific.VRChat
{
    internal class USpeakHooker
    {
        public static event Action<VRCPlayer, float[], int> OnRawAudio;

        public static bool Init()
        {
            MethodInfo decompressedAudioReceiver = null;

            var debug_info = "";

            foreach (var info in typeof(USpeaker).GetMethods().Where(
                         mi => mi.GetParameters().Length == 4
                     ))
            {
                debug_info += string.Format("Method: {1} {0}({2} params)",
                    info.Name,
                    info.ReturnType,
                    info.GetParameters().Length.ToString()
                );

                var ints = 0;
                var floats = 0;
                foreach (var inf in info.GetParameters())
                {
                    debug_info += string.Format(" - Param {0}", inf.ParameterType) + "\n";
                    if (inf.ParameterType.ToString().Contains("Single"))
                    {
                        floats++;
                    }

                    if (inf.ParameterType.ToString().Contains("Int32"))
                    {
                        ints++;
                    }
                }

                if (ints == 2 && floats == 2)
                {
                    decompressedAudioReceiver = info;
                }
            }

            if (decompressedAudioReceiver == null)
            {
                MelonLogger.Error("Couldn't find decompressedAudioReceiver!");
                MelonLogger.Error(debug_info);
                return false;
            }

            VRCCCModMain.Instance.HarmonyInstance.Patch(decompressedAudioReceiver, postfix:
                new HarmonyMethod(typeof(USpeakHooker).GetMethod(nameof(onDecompressedAudio), BindingFlags.NonPublic | BindingFlags.Static)));

            return true;
        }

        private static void onDecompressedAudio(USpeaker __instance, Il2CppStructArray<float> param_1, float param_2, int param_3, int param_4)
        {
            var tgt_ply = __instance.field_Private_VRCPlayer_0;
            if (tgt_ply == null)
            {
                return;
            }

            var sample_rate = 48000; //default vrchat
            if (__instance.field_Private_AudioClip_0 != null)
            {
                sample_rate = __instance.field_Private_AudioClip_0.frequency;
            }

            OnRawAudio(tgt_ply, param_1, sample_rate);
        }
    }
}