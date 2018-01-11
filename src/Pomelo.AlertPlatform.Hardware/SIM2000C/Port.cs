using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using RJCP.IO.Ports;

namespace Pomelo.AlertPlatform.Hardware.SIM2000C
{
    public class Port
    {
        public SerialPortStream SerialPort { get; private set; }

        public Command CurrentCommand { get; private set; }

        public Port()
        {
            string[] ports;
            do
            {
                ports = SerialPortStream.GetPortNames();
            }
            while (ports.Length == 0);

            string defaultPort;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                SerialPort = new SerialPortStream(ports.First(x => x.StartsWith("COM")));
            }
            else
            {
                SerialPort = new SerialPortStream(ports.First(x => !x.Contains("ttyAMA0")));
            }
            SetSerialPortProperties();
        }

        public Port(string comName)
        {
            SerialPort = new SerialPortStream(comName);
            SetSerialPortProperties();
        }

        private void SetSerialPortProperties()
        {
            SerialPort.DataReceived += Port_DataReceived;
            SerialPort.BaudRate = 115200;
            SerialPort.DataBits = 8;
            SerialPort.Parity = Parity.None;
            SerialPort.StopBits = StopBits.One;
            SerialPort.Encoding = Encoding.ASCII;
            SerialPort.NewLine = "\r\n";
            Console.WriteLine("正在打开串口：" + SerialPort.PortName);
            SerialPort.Open();
        }

        public Task EnqueueCommand(Command command)
        {
            var tcs = new TaskCompletionSource<bool>();
            queue.Enqueue(tcs);
            if (CurrentCommand == null)
            {
                NextCommand();
            }
            return tcs.Task;
        }

        protected void NextCommand()
        {
            if (queue.Count > 0 && CurrentCommand == null)
            {
                var tcs = queue.Dequeue();
                tcs.SetResult(true);
            }
        }

        public void BeginCommand(Command command)
        {
            this.CurrentCommand = command;
        }

        public void EndCommand()
        {
            CurrentCommand = null;
            NextCommand();
        }

        private Queue<TaskCompletionSource<bool>> queue = new Queue<TaskCompletionSource<bool>>();

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var ReDatas = new byte[SerialPort.BytesToRead];
            SerialPort.Read(ReDatas, 0, ReDatas.Length);
            var data = Encoding.ASCII.GetString(ReDatas);
            data = data.Replace("\r", "\n");
            var splited = data.Split('\n').Where(x => !string.IsNullOrEmpty(x)).Select(x => x.Trim());
            foreach (var x in splited)
            {
                Console.WriteLine(x);
                if (CurrentCommand != null)
                {
                    CurrentCommand.OnHardwareResponsed(x).Wait();
                }
            }
        }
    }
}
