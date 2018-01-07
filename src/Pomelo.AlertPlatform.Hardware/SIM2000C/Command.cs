using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;

namespace Pomelo.AlertPlatform.Hardware.SIM2000C
{
    public abstract class Command : IDisposable
    {
        public Command(Port port)
        {
            this.BasePort = port;
        }

        public Port BasePort { get; private set; }

        public virtual void Dispose()
        {
            End();
        }

        public void End()
        {
            if (BasePort.CurrentCommand == this)
            {
                BasePort.EndCommand();
            }
        }

        public virtual Task ExecuteAsync() { return Task.FromResult(0); }

        public virtual Task OnHardwareResponsed(string command)
        {
            return Task.FromResult(0);
        }
    }
}
