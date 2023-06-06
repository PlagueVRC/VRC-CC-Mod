

using System;
using System.Collections.Generic;
using UnityEngine;
using VRCCCMod.CC.GameSpecific;

namespace VRCCCMod {
    /// <summary>
    /// Contains some useful utilities
    /// </summary>
    public static class Utils {
        /// <summary>
        /// Gets the current game time in seconds
        /// </summary>
        /// <returns>Time in seconds</returns>
        public static float GetTime() {
            return Time.time;
        }

        // Taken from VRChatUtilityKit
        /// <summary>
        /// Safely invokes the given delegate with the given args.
        /// </summary>
        /// <param name="delegate">The given delegate</param>
        /// <param name="args">The params of the delegate</param>
        public static void DelegateSafeInvoke(this Delegate @delegate, params object[] args) {
            if(@delegate == null)
                return;

            foreach(Delegate @delegates in @delegate.GetInvocationList()) {
                try {
                    @delegates.DynamicInvoke(args);
                } catch(Exception ex) {
                    GameUtils.LogError("Error while invoking delegate:\n" + ex.ToString());
                }
            }
        }


        private static List<GameObject> trash = new List<GameObject>();

        /// <summary>
        /// Thread-safe method to schedule a GameObject to be deleted
        /// </summary>
        /// <param name="obj">The GameObject to delete</param>
        public static void AddForDeletion(GameObject obj) {
            lock(trash) trash.Add(obj);
        }

        /// <summary>
        /// Method to be called from the main thread to delete any trash pending deletion
        /// </summary>
        public static void DestroyTrash() {
            lock(trash) {
                foreach(GameObject obj in trash) {
                    UnityEngine.Object.Destroy(obj);
                }

                trash.Clear();
            }
        }
    }
}
