using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace Pomelo.AlertPlaftform.PlaySound
{
    public static class BaiduTextToSpeech
    {
        private static TaskCompletionSource<bool> taskCompletionSource;

        private static async Task<string> GetAccessTokenAsync()
        {
            using (var http = new HttpClient() { BaseAddress = new Uri("https://openapi.baidu.com") })
            using (var response = await http.GetAsync("/oauth/2.0/token?grant_type=client_credentials&client_id={client-id}&client_secret={client-secret}"))
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

        public static async Task<string> SaveSpeechMp3Async(string text)
        {
            var bytes = await DownloadSpeechAsync(text);
            var name = Guid.NewGuid().ToString().Substring(0, 8) + ".mp3";
            File.WriteAllBytes(name, bytes);
            return name;
        }

        public static Task SpeechAsync(string filename)
        {
            taskCompletionSource = new TaskCompletionSource<bool>();
            WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
            player.URL = filename;
            player.controls.play();
            player.PlayStateChange += Player_PlayStateChange;
            return taskCompletionSource.Task;
        }

        private static void Player_PlayStateChange(int NewState)
        {
            if (NewState == 1)
                taskCompletionSource.SetResult(true);
        }
    }
}
