using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pomelo.AlertPlatform.Hardware.SIM2000C
{
    public class SendSmsCommand : Command
    {
        public SendSmsCommand(Port port, string phoneNumber, string text) : base(port)
        {
            computedPhoneNumber = string.Join("", phoneNumber.Select(x => "003" + x));
            unicodeText = String2Unicode(text);
        }

        public event Func<Task> OnSendSucceeded;
        public event Func<string, Task> OnSendFailed;

        private string computedPhoneNumber;
        private string unicodeText;
        private TaskCompletionSource<bool> tcs;

        public override Task ExecuteAsync()
        {
            BasePort.EnqueueCommand(this).Wait();
            BasePort.BeginCommand(this);
            tcs = new TaskCompletionSource<bool>();
            BasePort.SerialPort.WriteLine("AT+CMGF=1");
            Task.Delay(200).Wait();
            BasePort.SerialPort.WriteLine("AT+CSCS=\"UCS2\"");
            Task.Delay(200).Wait();
            BasePort.SerialPort.WriteLine($"AT+CMGS=\"{computedPhoneNumber}\"");
            return tcs.Task;
        }

        public override void Dispose()
        {
            if (!tcs.Task.IsCompleted)
                tcs.SetResult(true);

            base.Dispose();
        }

        private string String2Unicode(string text)
        {
            var bytes = Encoding.Unicode.GetBytes(text);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("{0}{1}", bytes[i + 1].ToString("X").PadLeft(2, '0'), bytes[i].ToString("X").PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }

        public override async Task OnHardwareResponsed(string command)
        {
            if (command.StartsWith(">"))
            {
                BasePort.SerialPort.WriteLine(unicodeText);
                BasePort.SerialPort.Write(new[] { (char)0x1a }, 0, 1);
            }
            else if (command.Contains("+CMGS:"))
            {
                if (OnSendSucceeded != null)
                {
                    OnSendSucceeded().Wait();
                    if (!tcs.Task.IsCompleted)
                        tcs.SetResult(true);
                }
            }
            else if (command.Contains("ERROR"))
            {
                OnSendFailed(command).Wait();
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult(true);
            }
        }
    }

    public static class SendSmsCommandExtensions
    {
        public static SendSmsCommand CreateSendSmsCommand(this Port self, string phoneNumber, string text) => new SendSmsCommand(self, phoneNumber, text);
    }
}
