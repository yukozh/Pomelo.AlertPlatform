using System.IO;
using System.Threading.Tasks;

namespace Pomelo.AlertPlaftform.PlaySound
{
    class Program
    {
        static void Main(string[] args)
        {
            var speechText = args[0];
            Speech(speechText).Wait();
        }

        static async Task Speech(string text)
        {
            var name = await BaiduTextToSpeech.SaveSpeechMp3Async(text);
            await BaiduTextToSpeech.SpeechAsync(name);
            await BaiduTextToSpeech.SpeechAsync(name);
            await BaiduTextToSpeech.SpeechAsync(name);
            File.Delete(name);
        }
    }
}
