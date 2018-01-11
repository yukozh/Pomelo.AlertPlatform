using System;
using System.Threading.Tasks;

namespace Pomelo.AlertPlatform.Hardware.SIM2000C
{
    public class CallCommand : Command
    {
        public CallCommand(Port port, string phoneNumber) : base(port)
        {
            this.phoneNumber = phoneNumber;
        }

        public event Func<Task> OnHangUpAsync;
        public event Func<Task> OnConnectedAsync;
        public string Error { get; private set; } = "";
        private string phoneNumber;
        private TaskCompletionSource<bool> tcs;

        public override void Dispose()
        {
            base.Dispose();
            HangUp();
        }

        public void HangUp()
        {
            if (IsDialed)
            {
                BasePort.SerialPort.WriteLine("ATH");
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult(true);
            }
        }

        public bool IsTargetAnsweredTheCall { get; private set; }
        public bool IsDialed { get; private set; }

        public override Task ExecuteAsync()
        {
            BasePort.EnqueueCommand(this).Wait();
            BasePort.BeginCommand(this);
            tcs = new TaskCompletionSource<bool>();
            BasePort.SerialPort.WriteLine("AT+COLP=1");
            Task.Delay(200).Wait();
            BasePort.SerialPort.WriteLine("ATD" + phoneNumber + ";");
            IsDialed = true;
            return tcs.Task;
        }

        public override async Task OnHardwareResponsed(string command)
        {
            if (command == "NO CARRIER")
            {
                if (OnHangUpAsync != null)
                    await OnHangUpAsync();
                if (!tcs.Task.IsCompleted)
                    tcs.SetResult(true);
            }
            else if (command.StartsWith("+COLP:"))
            {
                IsTargetAnsweredTheCall = true;
                if (OnConnectedAsync != null)
                    await OnConnectedAsync();
            }
            else if (command.Contains("ERROR"))
            {
                Error += command + "\r\n";
            }
        }
    }

    public static class CallCommandExtensions
    {
        public static CallCommand CreateCallCommand(this Port self, string phoneNumber) => new CallCommand(self, phoneNumber);
    }
}
