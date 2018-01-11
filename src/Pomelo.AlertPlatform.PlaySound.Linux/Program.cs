using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace Pomelo.AlertPlatform.PlaySound.Linux
{
    class Program
    {
        static void Main(string[] args)
        {
            var filename = args[0];
            var times = Convert.ToInt32(args[1]);
            var deviceId = Convert.ToInt32(args[2]);
            Speech(filename, times, deviceId);
        }


        static void Speech(string filename, int device = 0)
        {
            var p = Process.Start("mplayer", "-ao alsa:device=hw=" + device + " -af volume=-15 " + filename);
            p.WaitForExit(110000);
        }

        static void Speech(string filename, int times = 3, int device = 0)
        {
            while (--times >= 0)
            {
                Speech(filename, device);
            }
            File.Delete(filename);
        }
    }
}
