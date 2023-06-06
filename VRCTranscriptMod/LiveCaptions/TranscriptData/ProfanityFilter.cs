

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VRCCCMod.CC.TranscriptData {
    static class ProfanityFilter {
        const string BadWordReplacement = "[_]";

        public enum FilterLevel {
            NONE,
            SLURS,
            ALL
        };
        
        private static string FilterWithWordList(string input, Func<string, bool> IsBadWord) {
            string output = "";

            string[] lines = input.Split('\n');
            foreach(string line in lines) {
                string[] words = line.Split(' ');
                foreach(string word in words) {
                    output = output + (IsBadWord(word.ToLower()) ? BadWordReplacement : word) + " ";
                }
                output = output + '\n';
            }

            output = output.TrimEnd('\n', ' ');

            return output;
        }

        public static string FilterString(string input, FilterLevel level) {
            string output = input;
            switch(level) {
                case FilterLevel.ALL:
                    output = FilterWithWordList(output,
                        TranscriptData.profanities.Profanities.IsWordBad);
                    goto case FilterLevel.SLURS;

                case FilterLevel.SLURS:
                    output = FilterWithWordList(output,
                        TranscriptData.profanities.Slurs.IsWordBad);
                    goto case FilterLevel.NONE;

                case FilterLevel.NONE:
                default:
                    break;
            }

            return output;
        }
    }
}
