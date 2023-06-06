

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using MelonLoader;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;
using VRCCCMod.CC.Abstract;
using VRCCCMod.CC.GameSpecific.VRChat;
using VRCCCMod.CC.VoskSpecific;
using VRChatUtilityKit.Utilities;

namespace VRCCCMod.CC.GameSpecific
{
    internal static class VRCPlayerUtils
    {
        public static string GetUID(VRCPlayer ply)
        {
            return ply.prop_String_3;
        }

        public static string GetDisplayName(VRCPlayer ply)
        {
            return ply.prop_String_1;
        }
    }

    public class VRCPlayerAudioSource : IAudioSource
    {
        private VRCPlayer _ply;
        private Transform head_transform;

        public VRCPlayerAudioSource(VRCPlayer ply)
        {
            _ply = ply;
        }

        public string GetFriendlyName()
        {
            return VRCPlayerUtils.GetDisplayName(_ply);
        }

        public Vector3 GetPosition()
        {
            if (head_transform == null)
            {
                head_transform = _ply.gameObject.transform.Find("AnimationController/HeadAndHandIK/HeadEffector");
            }

            return head_transform.position;
        }

        public string GetUID()
        {
            return VRCPlayerUtils.GetUID(_ply);
        }

        public bool IsImportant()
        {
            return GameUtils.GetProvider().IsUidImportant(GetUID());
        }
    }

    internal class GenericAudioSource : IAudioSource
    {
        private AudioSource _ply;
        private Transform head_transform;

        public GenericAudioSource(AudioSource ply)
        {
            _ply = ply;
        }

        public string GetFriendlyName()
        {
            return _ply.transform.name;
        }

        public Vector3 GetPosition()
        {
            if (head_transform == null)
            {
                head_transform = _ply.transform;
            }

            return head_transform.position;
        }

        public string GetUID()
        {
            return _ply.name;
        }

        public bool IsImportant()
        {
            return GameUtils.GetProvider().IsUidImportant(GetUID());
        }
    }

    internal class VRChatGameProvider : IGameProvider
    {
        private Dictionary<string, GenericAudioSource> GenToAudioSource = new Dictionary<string, GenericAudioSource>();

        private Dictionary<string, VRCPlayerAudioSource> plyToAudioSource = new Dictionary<string, VRCPlayerAudioSource>();

        public VRChatGameProvider()
        {
            var folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\";
            VoskUtil.EnsureDependencies(folder);

            if (!USpeakHooker.Init())
            {
                MelonLogger.Error("Failed To Hook To USpeak!");
            }

            USpeakHooker.OnRawAudio += OnRawAudioReceivedFromUSpeak;

            //GenericAudioHooker.OnRawAudio += OnRawAudioReceivedFromGeneric;

            NetworkEvents.OnRoomLeft += () => AllAudioSourcesRemoved?.DelegateSafeInvoke();

            NetworkEvents.OnPlayerJoined += ply =>
            {
                AudioSourceAdded?.DelegateSafeInvoke(
                    PlayerToAudioSource(ply.gameObject.GetComponent<VRCPlayer>())
                );
            };

            NetworkEvents.OnPlayerLeft += ply =>
            {
                var src = PlayerToAudioSource(ply.gameObject.GetComponent<VRCPlayer>());
                AudioSourceRemoved?.DelegateSafeInvoke(src);

                plyToAudioSource.Remove(src.GetUID());
            };

            TranscriptPlayerUi.Init();

            Settings.Init();
            SettingsTabMenu.Init();
        }

        public event Action<IAudioSource, float[], int, int> AudioEmitted;
        public event Action<IAudioSource> AudioSourceAdded;
        public event Action<IAudioSource> AudioSourceRemoved;
        public event Action AllAudioSourcesRemoved;

        public Vector3 GetLocalHeadPosition()
        {
            if (Networking.LocalPlayer == null)
            {
                return Vector3.zero;
            }

            return Networking.LocalPlayer.GetBonePosition(HumanBodyBones.Head);
        }

        public bool IsUidImportant(string uid)
        {
            return APIUser.IsFriendsWith(uid);
        }

        private IAudioSource PlayerToAudioSource(VRCPlayer ply)
        {
            var uid = VRCPlayerUtils.GetUID(ply);
            if (!plyToAudioSource.ContainsKey(uid) || plyToAudioSource[uid] == null)
            {
                plyToAudioSource[uid] = new VRCPlayerAudioSource(ply);
            }

            return plyToAudioSource[uid];
        }

        private IAudioSource GenericToAudioSource(AudioSource ply)
        {
            var uid = ply.name;
            if (!GenToAudioSource.ContainsKey(uid) || GenToAudioSource[uid] == null)
            {
                GenToAudioSource[uid] = new GenericAudioSource(ply);
            }

            return GenToAudioSource[uid];
        }

        private void OnRawAudioReceivedFromUSpeak(VRCPlayer ply, float[] samples, int sample_rate)
        {
            AudioEmitted?.DelegateSafeInvoke(PlayerToAudioSource(ply), samples, samples.Length, sample_rate);
        }

        private void OnRawAudioReceivedFromGeneric(AudioSource aud, float[] samples, int sample_rate)
        {
            AudioEmitted?.DelegateSafeInvoke(GenericToAudioSource(aud), samples, samples.Length, sample_rate);
        }
    }

    public static class GameUtils
    {
        private static VRChatGameProvider provider;

        public static void Init()
        {
            provider = new VRChatGameProvider();
        }

        public static IGameProvider GetProvider()
        {
            return provider;
        }

        public static void Log(string s)
        {
            MelonLogger.Msg(s);
        }

        public static void LogWarn(string s)
        {
            MelonLogger.Warning(s);
        }

        public static void LogError(string s)
        {
            MelonLogger.Error(s);
        }

        public static void LogDebug(string s)
        {
            #if DEBUG
            Log(s);
            #endif
        }

        public static IVoiceRecognizer GetVoiceRecognizer()
        {
            if (Settings.Vosk_model == null)
            {
                return null;
            }

            return new VoskVoiceRecognizer(Settings.Vosk_model);
        }

        public static Transform GetSubtitleUiParent()
        {
            return GameObject.Find("UserInterface").transform;
        }

        public static string GetPathForModels()
        {
            var folder = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + @"\Models\";

            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            return folder;
        }
    }
}