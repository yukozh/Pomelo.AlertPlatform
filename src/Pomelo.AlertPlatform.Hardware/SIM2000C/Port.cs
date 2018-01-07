using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.IO.Ports;

namespace Pomelo.AlertPlatform.Hardware.SIM2000C
{
    public class Port
    {
        public SerialPort SerialPort { get; private set; }

        public Command CurrentCommand { get; private set; }

        public Port()
        {
            string[] ports;
            do
            {
                ports = SerialPort.GetPortNames();
            }
            while (ports.Length == 0);

            SerialPort = new SerialPort(ports.First());
            SetSerialPortProperties();
        }

        public Port(string comName)
        {
            SerialPort = new SerialPort(comName);
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
