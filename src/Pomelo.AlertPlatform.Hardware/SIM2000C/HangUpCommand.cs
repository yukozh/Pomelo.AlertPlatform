using System;
using System.Threading.Tasks;

namespace Pomelo.AlertPlatform.Hardware.SIM2000C
{
    public class HangUpCommand : Command
    {
        public HangUpCommand(Port port) : base(port)
        {
        }

        public override async Task ExecuteAsync()
        {
            await BasePort.EnqueueCommand(this);
            BasePort.BeginCommand(this);
            BasePort.SerialPort.WriteLine("ATH");
        }
    }

    public static class HangUpCommandExtensions
    {
        public static HangUpCommand CreateHangUpCommand(this Port self) => new HangUpCommand(self);
    }
}
