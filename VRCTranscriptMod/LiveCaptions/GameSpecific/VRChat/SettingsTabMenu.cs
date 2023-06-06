

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using GoogleTranslateFreeApi;
using HarmonyLib;
using PlagueButtonAPI;
using PlagueButtonAPI.Controls;
using PlagueButtonAPI.Controls.Grouping;
using PlagueButtonAPI.Pages;
using UIExpansionKit.API;
using UnityEngine;
using VRCCCMod.CC.TranscriptData;
using Resources = VRCCCMod.Properties.Resources;

namespace VRCCCMod.CC.GameSpecific.VRChat
{
    internal class SettingsMenuContents
    {
        private static ToggleButton killswitch;
        private static ToggleButton range_transcribe;

        private static ToggleButton filter_words;

        private static Label model_info;
        //static Label update_info;

        private static MenuPage submenu;

        private static ButtonGroup ModelsGroup;

        private static bool was_initialized = false;

        private static List<GameObject> trash = new List<GameObject>();


        public static void Init(MenuPage sub)
        {
            submenu = sub;

            var Group = new ButtonGroup(sub, "Settings");

            killswitch = new ToggleButton(Group, "Killswitch", "Enabling killswitch will instantly disable live captioning and terminate all captioning sessions.", "Disable Killswitch.", KillChanged);
            killswitch.SetToggleState(Settings.Disabled);

            range_transcribe = new ToggleButton(Group, "Range auto-transcribe", "Enable this option if you want close players to be automatically transcribed, without needing to manually enable each player. This may consume a lot of memory.", "Disable", RangeChanged);

            range_transcribe.SetToggleState(Settings.AutoTranscribeWhenInRange);

            filter_words = new ToggleButton(Group, "Swear filter", "Enable swear-word filtering.", "Disable swear-word filtering.",
                state => { Settings.ProfanityFilterLevel = state ? ProfanityFilter.FilterLevel.ALL : ProfanityFilter.FilterLevel.NONE; });

            filter_words.SetToggleState(Settings.ProfanityFilterLevel == ProfanityFilter.FilterLevel.ALL);

            var GroupSize = new ButtonGroup(sub, "Size Editing");

            new SimpleSingleButton(GroupSize, "-", "Decrease subtitle size",
                () => SizeChange(false));

            new SimpleSingleButton(GroupSize, "+", "Increase subtitle size",
                () => SizeChange(true));

            var GroupInfo = new ButtonGroup(sub, "Info");

            model_info = new Label(GroupInfo, "", "Model Info");

            ModelsGroup = new ButtonGroup(submenu, "Models");

            var Trans = new ButtonGroup(submenu, "Translation");

            new ToggleButton(Trans, "Translate", "Enable Translation.", "Disable Translation.",
                state => { Settings.Translate = state; }).SetToggleState(Settings.Translate);

            SimpleSingleButton FromButton = null;
            FromButton = new SimpleSingleButton(Trans, $"Translate From [{Settings.TranslateFrom.FullName}]", "Choose Which Language To Translate From.", () =>
            {
                var Popup = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);

                Popup.AddSimpleButton("Close", Popup.Hide);

                foreach (var lang in typeof(Language).GetProperties(AccessTools.all).Where(o => o.PropertyType == typeof(Language)))
                {
                    Popup.AddSimpleButton(lang.Name, () =>
                    {
                        Settings.TranslateFrom = (Language)lang.GetValue(null);
                        FromButton.SetText($"Translate From [{Settings.TranslateFrom.FullName}]");
                        Popup.Hide();
                    });
                }

                Popup.Show();
            }, true);

            SimpleSingleButton ToButton = null;
            ToButton = new SimpleSingleButton(Trans, $"Translate To [{Settings.TranslateTo.FullName}]", "Choose Which Language To Translate To.", () =>
            {
                var Popup = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.QuickMenu4Columns);

                Popup.AddSimpleButton("Close", Popup.Hide);

                foreach (var lang in typeof(Language).GetProperties(AccessTools.all).Where(o => o.PropertyType == typeof(Language)))
                {
                    Popup.AddSimpleButton(lang.Name, () =>
                    {
                        Settings.TranslateTo = (Language)lang.GetValue(null);
                        ToButton.SetText($"Translate To [{Settings.TranslateTo.FullName}]");
                        Popup.Hide();
                    });
                }

                Popup.Show();
            }, true);

            //update_info = new Label(GroupInfo, "", "Update Info");

            //string UpdateText = UpdateChecker.Check();
            //tooltip.field_Public_String_0 = UpdateText;

            //if(UpdateText.Contains("up-to-date")) {
            //    update_info.TextComponent.text = "Up-to-date";
            //}else if(UpdateText.Contains("out of date")) {
            //    update_info.TextComponent.text = "Out-of-date! Hover for more details.";
            //    GameUtils.LogWarn(UpdateText);
            //} else if(UpdateText.Contains("failed")) {
            //    update_info.TextComponent.text = "Failed to check for updates";
            //    GameUtils.LogWarn(UpdateText);
            //}


            Settings.ModelName = "english-light";


            Settings.ModelChanged += () => { UpdateStatusText(); };
        }

        private static void SizeChange(bool increase)
        {
            Settings.TextScale = Settings.TextScale + (increase ? 0.05f : -0.05f);
        }

        private static void RangeChanged(bool to)
        {
            Settings.AutoTranscribeWhenInRange = to;
            Update();
        }

        private static void KillChanged(bool to)
        {
            Settings.Disabled = to;
            Update();
        }

        private static int PopulateModelButtons()
        {
            if (!Directory.Exists(Settings.model_directory))
            {
                return -1;
            }

            var model_dirs = Directory.GetDirectories(Settings.model_directory);

            var currModel = 0;
            for (var y = 1; y < 3; y++)
            {
                for (var x = 0; x < 4; x++)
                {
                    if (currModel >= model_dirs.Length)
                    {
                        break;
                    }

                    var currModelName = model_dirs[currModel].Replace(Settings.model_directory, "");
                    var button = new SimpleSingleButton(ModelsGroup,
                        "Use " + currModelName, "Set active model to " + model_dirs[currModel],
                        () =>
                        {
                            Settings.ModelName = currModelName;
                            Update();
                        }
                    );

                    Object.DontDestroyOnLoad(button.gameObject);

                    trash.Add(button.gameObject);
                    currModel++;
                }
            }

            return model_dirs.Length;
        }

        public static void UpdateStatusText()
        {
            var modelInfoTxt = "";

            if (!Settings.ModelExists())
            {
                modelInfoTxt = "Couldn't find model " + Settings.ModelName;
            }
            else
            {
                if (Settings.Vosk_model == null)
                {
                    if (Settings.Loading)
                    {
                        modelInfoTxt = "Loading model " + Settings.ModelName + "...";
                    }
                    else
                    {
                        modelInfoTxt = "Failed to load model " + Settings.ModelName;
                    }
                }
                else
                {
                    modelInfoTxt = "Using model " + Settings.ModelName;
                }
            }

            model_info.LabelButton.SetText(modelInfoTxt);
        }

        /// <summary>
        ///   Sets the current model to the first one in existence by default
        /// </summary>
        private static void FirstInitialize()
        {
            if (Settings.ModelExists() && Settings.Vosk_model != null)
            {
                return;
            }

            if (!Directory.Exists(Settings.model_directory))
            {
                return;
            }

            var model_dirs = Directory.GetDirectories(Settings.model_directory);

            if (model_dirs.Length == 0)
            {
                return;
            }

            Settings.ModelName = model_dirs[0].Replace(Settings.model_directory, "");
        }

        public static void Update()
        {
            if (!was_initialized)
            {
                FirstInitialize();
                was_initialized = true;
            }

            foreach (var garbage in trash)
            {
                Object.Destroy(garbage);
            }

            killswitch.SetToggleState(Settings.Disabled);
            range_transcribe.SetToggleState(Settings.AutoTranscribeWhenInRange);

            UpdateStatusText();

            var models = PopulateModelButtons();

            var modelInfoTxt = "";
            if (models == -1)
            {
                modelInfoTxt = "Model directory " + Settings.model_directory + " doesn't exist!";
            }
            else if (models == 0)
            {
                modelInfoTxt = "No models found in " + Settings.model_directory + ", please add some.";
            }

            if (modelInfoTxt.Length > 1)
            {
                model_info.LabelButton.SetText(modelInfoTxt);
            }
        }
    }

    internal class SettingsTabMenu
    {
        private static MenuPage menu;
        private static WingSingleButton tabButton;


        public static void Init()
        {
            ButtonAPI.OnInit += () =>
            {
                // Create menu and tab button
                menu = MenuPage.CreatePage("VRCCCettings", "Caption Settings");

                var CCprite = GetSpriteFromResource(Resources.livecaptionicon1);
                tabButton = new WingSingleButton(WingSingleButton.Wing.Left, "Caption Settings", "", OnTabButtonClick, true, CCprite);

                // Populate
                SettingsMenuContents.Init(menu);
            };
        }

        public static void OnTabButtonClick()
        {
            SettingsMenuContents.Update();
            menu.OpenMenu();
        }


        public static Sprite GetSpriteFromResource(Bitmap resource)
        {
            var ms = new MemoryStream();
            resource.Save(ms, resource.RawFormat);

            var tex = new Texture2D(resource.Width, resource.Height);
            Il2CppImageConversionManager.LoadImage(tex, ms.ToArray());

            Sprite sprite;
            {
                var size = new Rect(0.0f, 0.0f, tex.width, tex.height);
                var pivot = new Vector2(0.5f, 0.5f);
                var border = Vector4.zero;
                sprite = Sprite.CreateSprite_Injected(tex, ref size, ref pivot, 100.0f, 0u, SpriteMeshType.Tight, ref border, false);
            }

            return sprite;
        }
    }
}