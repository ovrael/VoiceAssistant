using System.Media;
using System.Timers;
using Timer = System.Timers.Timer;

namespace VoiceAssistantBackend.Commands
{
    public static class TimerControl
    {
        public static bool IsAvailable { get; set; } = true;

        private static Timer timer;
        private static Timer soundTimer;
        private static SoundPlayer soundPlayer;

        public static string TimerSoundPath { get; set; } = @"\src\sounds\timer.wav";

        public static void SetTimerSeconds(object seconds)
        {
            if (!int.TryParse(seconds.ToString(), out int correctSeconds))
            {
                return;
            }
            int miliseconds = correctSeconds * 1000;

            soundPlayer = new SoundPlayer(TimerSoundPath);
            timer = new Timer(miliseconds);
            timer.Elapsed += TimerEnd;
            timer.Enabled = true;
            timer.AutoReset = false;
        }

        public static void SetTimerMinutes(object minutes)
        {
            if (!int.TryParse(minutes.ToString(), out int correctMinutes))
            {
                return;
            }
            SetTimerSeconds(correctMinutes * 60);
        }

        public static void StopTimer()
        {
            soundPlayer.Stop();
            if (soundTimer is not null)
                soundTimer.Dispose();

            if (timer is not null)
                timer.Dispose();

            if (soundPlayer is not null)
                soundPlayer.Dispose();
        }

        private static void TimerEnd(object source, ElapsedEventArgs e)
        {
            PlaySound(null, null);

            soundTimer = new Timer(4000);
            soundTimer.Elapsed += PlaySound;
            soundTimer.Enabled = true;
            soundTimer.AutoReset = true;
        }

        private static void PlaySound(object source, ElapsedEventArgs e)
        {
            soundPlayer.Play();
        }
    }
}
