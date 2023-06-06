

using MelonLoader;
using UnityEngine;
using VRCCCMod.CC;
using VRCCCMod.CC.GameSpecific;

namespace VRCCCMod
{
    public class VRCCCModMain : MelonMod
    {
        internal static VRCCCModMain Instance { get; private set; }

        #if INTEGRATED_VRCUK
        VRChatUtilityKit.VRChatUtilityKitMod vrcUtility = new VRChatUtilityKit.VRChatUtilityKitMod();
        #endif

        public override void OnApplicationStart()
        {
            Instance = this;

            #if INTEGRATED_VRCUK
            vrcUtility.OnApplicationStart();
            #endif

            VRChatUtilityKit.Utilities.VRCUtils.OnUiManagerInit += OnUiManagerInit;
        }


        private TranscribeWorker worker;

        internal void OnUiManagerInit()
        {
            MelonLogger.Msg("UIMan Init");

            AudioSourceOverrides.Init();
            SubtitleUi.Init();

            GameUtils.Init();

            worker = new TranscribeWorker();
        }

        public override void OnApplicationQuit()
        {
        }

        public override void OnSceneWasUnloaded(int buildIndex, string sceneName)
        {
        }

        private float lastUpdate = 0;

        public override void OnUpdate()
        {
            #if INTEGRATED_VRCUK
            vrcUtility.OnUpdate();
            #endif

            //GenericAudioHooker.Update();

            worker?.Tick();

            if (Time.time - lastUpdate < 1.0f)
            {
                return;
            }

            lastUpdate = Time.time;
        }
    }
}