

using System;
using System.Collections.Generic;
using VRCCCMod.CC.Abstract;
using VRCCCMod.CC.GameSpecific;

namespace VRCCCMod.CC
{
    /// <summary>
    ///   Holds a whitelist of audio sources for transcribing.
    /// </summary>
    public class AudioSourceOverrides
    {
        /// <summary>
        ///   The dictionary which stores the overrides
        /// </summary>
        private static Dictionary<string, bool> overrides = new Dictionary<string, bool>();

        /// <summary>
        ///   Emitted when a UID is removed from the whitelist
        /// </summary>
        public static event Action<string> OnRemovedFromWhitelist;

        /// <summary>
        ///   Emitted when a UID is added to the whitelist
        /// </summary>
        public static event Action<string> OnAddedToWhitelist;

        /// <summary>
        ///   Static initializer
        /// </summary>
        public static void Init()
        {
            // TODO: Load/save from file
        }

        /// <summary>
        ///   This method changesthe override state of a UID
        /// </summary>
        /// <param name="uid">UID to affect (from IAudioSource.GetUID())</param>
        /// <param name="is_whitelisted">Whether to whitelist the audio source or not</param>
        public static void SetOverride(string uid, bool is_whitelisted)
        {
            overrides[uid] = is_whitelisted;

            if (is_whitelisted)
            {
                OnAddedToWhitelist?.DelegateSafeInvoke(uid);
            }
            else
            {
                OnRemovedFromWhitelist?.DelegateSafeInvoke(uid);
            }

            // TODO: save config
        }

        /// <summary>
        ///   Check whether or not an audio source is whitelisted
        /// </summary>
        /// <param name="uid">The Audio Source to check</param>
        /// <returns>Whether or not the UID has transcription whitelisted</returns>
        public static bool IsWhitelisted(IAudioSource src)
        {
            if (!overrides.ContainsKey(src.GetUID()))
            {
                return src.IsImportant();
            }

            return overrides[src.GetUID()];
        }

        /// <summary>
        ///   Check whether or not an audio source UID is whitelisted
        /// </summary>
        /// <param name="uid">The Audio Source UID to check</param>
        /// <returns>Whether or not the UID has transcription whitelisted</returns>
        public static bool IsWhitelisted(string uid)
        {
            if (!overrides.ContainsKey(uid))
            {
                return GameUtils.GetProvider().IsUidImportant(uid);
            }

            return overrides[uid];
        }
    }
}