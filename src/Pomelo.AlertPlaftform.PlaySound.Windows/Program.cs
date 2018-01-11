using System;
using System.IO;
using System.Threading.Tasks;

namespace Pomelo.AlertPlaftform.PlaySound
{
    class Program
    {
        static void Main(string[] args)
        {
            var filename = args[0];
            var times = Convert.ToInt32(args[1]);
            Speech(filename, times).Wait();
        }

        static async Task Speech(string filename, int times)
        {
            while (--times >= 0)
            {
                await Play.SpeechAsync(filename);
            }
            File.Delete(filename);
        }
    }
}
