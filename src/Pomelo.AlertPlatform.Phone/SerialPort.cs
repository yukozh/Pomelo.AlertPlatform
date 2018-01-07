using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO.Ports;

namespace Pomelo.AlertPlatform.Phone
{
    public static class Sim2000SerialPort
    {
        public static async Task Start()
        {
            string[] ports;
            do
            {
                ports = SerialPort.GetPortNames();
            }
            while (ports.Length == 0);

            var port = new SerialPort(ports.First());
            port.BaudRate = 115200;
            port.DataBits = 8;
            port.Parity = Parity.None;
            port.StopBits = StopBits.One;
            port.NewLine = "\n";
            port.Encoding = System.Text.Encoding.ASCII;
            port.Open();
            port.WriteLine("ATE1");
            await Task.Delay(500);
            port.WriteLine("ATV1");
        }
    }
}
