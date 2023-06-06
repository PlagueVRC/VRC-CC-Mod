

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MelonLoader;

namespace VRCCCMod.CC.TranscriptData
{
    internal class TextGenerator
    {
        private const int maxLines = 2;
        private float lastTextUpdate = 0.0f;
        private string UnfilteredFullText = "";
        public string FullText { get; private set; } = "";

        public void UpdateText(List<Saying> past_sayings, Saying active_saying, bool IsPriority = false)
        {
            if (Utils.GetTime() - lastTextUpdate < 0.03)
            {
                return;
            }

            lastTextUpdate = Utils.GetTime();

            // Merge past and active to make codepath simpler
            var allSayings = new Saying[past_sayings.Count +
                                        (active_saying != null ? 1 : 0)];
            for (var i = 0; i < past_sayings.Count; i++)
            {
                allSayings[i] = past_sayings[i];
            }

            if (active_saying != null)
            {
                allSayings[past_sayings.Count] = active_saying;
            }

            // Concat allSayings
            var full = "";
            for (var i = 0; i < allSayings.Length; i++)
            {
                var prevAge = 0.0f;

                if (i > 0)
                {
                    prevAge = allSayings[i - 1].timeEnd;
                }

                var diff = Math.Abs(allSayings[i].timeStart - prevAge);

                if (diff > TranscriptSession.sepAge)
                {
                    full += '\n';
                }
                else
                {
                    full += ' ';
                }

                full = full + allSayings[i].fullTxt;
            }

            if (full.Length > 2)
            {
                // Insert line breaks when a line is too long
                var tmp = "";
                var chars_in_current_line = 0;

                foreach (var c in full)
                {
                    tmp = tmp + c;
                    if (c == '\n')
                    {
                        chars_in_current_line = 0;
                        continue;
                    }

                    chars_in_current_line++;

                    if (chars_in_current_line >= 48 && c == ' ')
                    {
                        tmp = tmp + '\n';
                        chars_in_current_line = 0;
                    }
                }

                // Grab only the last ${maxLines} lines
                var lines = tmp.Split('\n');

                full = lines.Where((t, i) => i >= lines.Length - maxLines).Aggregate("", (current, t) => current + t + '\n');
            }

            // Avoid expensive filtering if the text is exactly the same
            if (full.Equals(UnfilteredFullText))
            {
                return;
            }

            UnfilteredFullText = full;

            Task.Run(() =>
            {
                if (Settings.Translate && IsPriority)
                {
                    var Trans = Translator.TranslateNoAsync(UnfilteredFullText, Settings.TranslateFrom, Settings.TranslateTo);

                    MelonLogger.Msg(Trans);

                    FullText = Trans;
                }
                else if (Settings.ProfanityFilterLevel != ProfanityFilter.FilterLevel.NONE)
                {
                    FullText = ProfanityFilter.FilterString(UnfilteredFullText, Settings.ProfanityFilterLevel);
                }
                else
                {
                    FullText = UnfilteredFullText;
                }
            });
        }
    }
}