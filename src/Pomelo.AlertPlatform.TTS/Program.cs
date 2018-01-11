using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Pomelo.AlertPlaftform.TTS
{
    public static class TextToSpeech
    {
        public static void Main(string[] args)
        {
            SaveSpeechMp3Async(args[0], args[1]).Wait();
        }

        private static async Task<string> GetAccessTokenAsync()
        {
            using (var http = new HttpClient() { BaseAddress = new Uri("https://openapi.baidu.com") })
            using (var response = await http.GetAsync("/oauth/2.0/token?grant_type=client_credentials&client_id=xxx&client_secret=xxx"))
            {
                var body = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<dynamic>(body);
                return result.access_token;
            }
        }

        private static async Task<byte[]> DownloadSpeechAsync(string text)
        {
            var at = await GetAccessTokenAsync();
            using (var http = new HttpClient() { BaseAddress = new Uri($"https://tsn.baidu.com") })
            using (var response = await http.GetAsync($"/text2audio?tex={HttpUtility.UrlEncode(text)}&lan=zh&cuid=123456&ctp=1&tok={at}"))
            {
                return await response.Content.ReadAsByteArrayAsync();
            }
        }

        private static async Task SaveSpeechMp3Async(string text, string filename)
        {
            var bytes = await DownloadSpeechAsync(text);
            File.WriteAllBytes(filename, bytes);
        }
    }
}
