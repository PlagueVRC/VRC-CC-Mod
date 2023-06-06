

using System.Collections.Generic;
using System.Linq;
using PlagueButtonAPI;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using VRC;
using VRC.UI.Elements.Menus;

namespace VRCCCMod.CC.GameSpecific.VRChat
{
    internal class TranscriptPlayerUi
    {
        private const string name = "Captions [High Priority]";
        private const string tooltip = "Toggles live captioning of the user's voice with high priority.";
        private static bool toggleState = false;

        private static string currently_active_uid = "";

        private static GameObject ButtonObj;

        internal static Player GetCurrentlySelectedPlayer()
        {
            if (GameObject.Find("UserInterface")?.GetComponentInChildren<SelectedUserMenuQM>() == null)
            {
                return null;
            }

            return GetPlayerFromIDInLobby(GameObject.Find("UserInterface")?.GetComponentInChildren<SelectedUserMenuQM>()?.field_Private_IUser_0?.prop_String_0);
        }

        internal static Player GetPlayerFromIDInLobby(string id)
        {
            var all_player = GetAllPlayers();

            foreach (var player in all_player)
            {
                if (player != null && player.prop_APIUser_0 != null)
                {
                    if (id.Contains(player.prop_APIUser_0.id))
                    {
                        return player;
                    }
                }
            }

            return null;
        }

        internal static List<Player> GetAllPlayers()
        {
            return PlayerManager.field_Private_Static_PlayerManager_0?.field_Private_List_1_Player_0?.ToArray()?.ToList();
        }

        public static void Init()
        {
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.UserQuickMenu).AddSimpleButton("Enable " + name, () =>
            {
                if (currently_active_uid != null)
                {
                    toggleState = !toggleState;

                    if (ButtonObj != null)
                    {
                        if (toggleState)
                        {
                            ButtonObj.GetComponentInChildren<Text>().text = "Disable " + name;
                        }
                        else
                        {
                            ButtonObj.GetComponentInChildren<Text>().text = "Enable " + name;
                        }
                    }

                    AudioSourceOverrides.SetOverride(currently_active_uid, toggleState);
                }
            }, obj =>
            {
                ButtonObj = obj;

                var Handler = obj.AddComponent<ObjectHandler>();

                Handler.OnUpdateEachSecond += (obj2, Enabled) =>
                {
                    currently_active_uid = VRCPlayerUtils.GetUID(GetCurrentlySelectedPlayer()?._vrcplayer);

                    if (currently_active_uid != null)
                    {
                        toggleState = AudioSourceOverrides.IsWhitelisted(currently_active_uid);

                        if (toggleState)
                        {
                            ButtonObj.GetComponentInChildren<Text>().text = "Disable " + name;
                        }
                        else
                        {
                            ButtonObj.GetComponentInChildren<Text>().text = "Enable " + name;
                        }
                    }
                    else
                    {
                        ButtonObj.GetComponentInChildren<Text>().text = "No Player Found!";
                    }
                };
            });
        }
    }
}