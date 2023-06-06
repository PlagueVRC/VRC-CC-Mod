using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using GoogleTranslateFreeApi;
using Newtonsoft.Json;

namespace VRCCCMod.CC.TranscriptData
{
    internal class Translator
    {
        private static GoogleTranslator translator = new ();

        internal static string TranslateNoAsync(string Text, Language From, Language To)
        {
            var Output = "";

            var task = Task.Run(async () =>
            {
                Output = (await translator.TranslateAsync(Text, From, To)).MergedTranslation;
            });

            while (!task.IsCompleted)
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    break;
                }
            }

            return Output;
        }
    }
}