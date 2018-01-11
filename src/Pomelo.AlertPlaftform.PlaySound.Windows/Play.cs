using System.Threading.Tasks;

namespace Pomelo.AlertPlaftform.PlaySound
{
    public static class Play
    {
        private static TaskCompletionSource<bool> taskCompletionSource;
        
        public static Task SpeechAsync(string filename)
        {
            taskCompletionSource = new TaskCompletionSource<bool>();
            WMPLib.WindowsMediaPlayer player = new WMPLib.WindowsMediaPlayer();
            player.URL = filename;
            player.controls.play();
            player.PlayStateChange += Player_PlayStateChange;
            return taskCompletionSource.Task;
        }

        private static void Player_PlayStateChange(int NewState)
        {
            if (NewState == 1)
                taskCompletionSource.SetResult(true);
        }
    }
}
