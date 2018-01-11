using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Pomelo.AlertPlatform.Hardware.SIM2000C;

namespace Pomelo.AlertPlatform.DeviceClient
{
    static class Program
    {
        static Port SIM2000;
        static string DeviceId;
        static string Secret;
        static int AudioId;
        static HttpClient Client = new HttpClient() { BaseAddress = new Uri("http://alert.pomelo.cloud") };

        static void Main(string[] args)
        {
            DeviceId = args[0];
            Secret = args[1];
            AudioId = Convert.ToInt32(args[2]);
            Console.WriteLine("Pomelo 报警设备客户端正在启动，Device ID=" + args[0], "，音频设备ID=" + AudioId);
            SIM2000 = new Port();
            while (true)
            {
                if (!PullTask().Result)
                {
                    Task.Delay(5000).Wait();
                }
            }
        }

        static async Task<bool> PullTask()
        {
            Console.WriteLine("正在拉取任务...");
            try
            {

                using (var response = await Client.PostAsync("/device/gettask", new FormUrlEncodedContent(new Dictionary<string, string> {
                { "device", DeviceId },
                { "secret", Secret }
            })))
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine("目前没有任务");
                        return false;
                    }
                    else
                    {
                        var message = JsonConvert.DeserializeObject<Response<Message>>(await response.Content.ReadAsStringAsync()).data;
                        Console.WriteLine("获得任务 " + message.Id);
                        if (message.Type == MessageType.Sms)
                        {
                            var result = await SendSms(message.To, message.Text);
                            if (result.IsSucceeded)
                            {
                                await UpdateTask(message.Id, MessageStatus.Succeeded, null, 0);
                                return true;
                            }
                            else
                            {
                                await UpdateTask(message.Id, MessageStatus.Failed, result.Error, 0);
                                return true;
                            }
                        }
                        else
                        {
                            GenerateSpeechMp3(message.Id, message.Text);
                            while (message.RetryLeft-- >= 0)
                            {
                                var result = await MakeCall(message.To, message.Id + ".mp3", message.Replay);
                                if (result.IsSucceeded)
                                {
                                    await UpdateTask(message.Id, MessageStatus.Succeeded, null, message.RetryLeft);
                                    return true;
                                }
                                else
                                {
                                    message.Error += result.Error;
                                    await UpdateTask(message.Id, null, result.Error, message.RetryLeft);
                                }
                            }

                            await UpdateTask(message.Id, MessageStatus.Failed, message.Error, 0);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("任务拉取失败：");
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        static async Task UpdateTask(Guid taskId, MessageStatus? status, string error, int? retryLeft)
        {
            var content = new Dictionary<string, string> {
                { "device", DeviceId },
                { "secret", Secret },
                { "message", taskId.ToString() }
            };

            if (!string.IsNullOrEmpty(error))
            {
                content.Add("error", error);
            }

            if (status.HasValue)
            {
                content.Add("status", status.Value.ToString());
            }

            if (retryLeft.HasValue)
            {
                content.Add("retryLeft", retryLeft.Value.ToString());
            }

            using (var res = await Client.PostAsync("/device/updatetask", new FormUrlEncodedContent(content))) { }
        }

        static void GenerateSpeechMp3(Guid taskId, string text)
        {
            var p = Process.Start("Pomelo.AlertPlatform.TTS", $"\"{text}\" {taskId}.mp3");
            p.WaitForExit();
        }

        static async Task<HandleResult> SendSms(string phone, string text)
        {
            Console.WriteLine("正在发送短消息 " + phone);
            using (var sms = SIM2000.CreateSendSmsCommand(
                phone,
                text))
            {
                var sendResult = false;
                sms.OnSendSucceeded += async () => { Console.WriteLine("短信发送成功"); sendResult = true; };
                await sms.ExecuteAsync();
                return new HandleResult { IsSucceeded = sendResult, Error = sms.Error };
            }
        }

        static async Task<HandleResult> MakeCall(string phone, string filename, int replay)
        {
            Console.WriteLine("正在拨号");

            using (var hangup = SIM2000.CreateHangUpCommand())
            {
                await hangup.ExecuteAsync();
            }

            using (var call = SIM2000.CreateCallCommand("18045054321"))
            {
                call.OnConnectedAsync += async () =>
                {
                    Console.WriteLine("用户已接听");
                    var p = Process.Start(
                        RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "Pomelo.AlertPlatform.PlaySound.Windows" : "Pomelo.AlertPlatform.PlaySound.Linux",
                        $"{ filename } { replay } { AudioId }");
                    p.WaitForExit(110000);
                    Console.WriteLine("主动挂断");
                    call.HangUp();
                };
                call.OnHangUpAsync += async () =>
                {
                    Console.WriteLine("对方挂断");
                };

                await call.ExecuteAsync();
                return new HandleResult { IsSucceeded = call.IsTargetAnsweredTheCall, IsUserError = call.IsDialed, Error = call.Error };
            }
        }
    }
}
