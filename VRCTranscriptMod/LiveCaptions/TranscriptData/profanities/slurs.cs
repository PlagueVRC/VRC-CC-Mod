

/*
 *
 * 
 * 
 * 
 * 
 *
 * 
 * 
 * !!! WARNING !!!
 * 
 * This file contains offensive words.
 * 
 * This is used for the profanity filter to remove them
 * from transcribed text, if filtering is enabled.
 * 
 * Please don't continue if you're not prepared to see
 * racist slurs, profanities and other bad words.
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */












using System.Collections.Generic;

namespace VRCCCMod.CC.TranscriptData.profanities {
    public static class Slurs {
        public static readonly string[] words = {
            "nigger",
            "nigga",
            "nig",
            "coon",
            "chink",
            "kkk",
            "triple k",
            "wigger",
            "white boy",
            "whitey",
            "nazi",
            "hitler",
            "fag",
            "faggot",
            "whore",
            "retard",
            "retarded",
            "slut",
            "sluttish"
        };

        private static Dictionary<string, bool> BadWordDict = null;
        private static void Init() {
            BadWordDict = new Dictionary<string, bool>();
            foreach(string word in words) {
                BadWordDict[word.ToLower()] = true;
                BadWordDict[word.ToLower() + "s"] = true;
                BadWordDict[word.ToLower() + "es"] = true;
                BadWordDict[word.ToLower() + "ing"] = true;
            }
        }

        public static bool IsWordBad(string word) {
            if(BadWordDict == null) Init();

            return BadWordDict.ContainsKey(word);
        }
    }
}
