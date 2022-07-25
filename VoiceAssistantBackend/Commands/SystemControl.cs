using System.Media;
using System.Timers;
using Timer = System.Timers.Timer;

namespace VoiceAssistantBackend.Commands
{
    public static class TimerControl
    {
        private static Timer timer;
        private static Timer soundTimer;
        public static string TimerSoundPath { get; set; } = @"";

        public static void SetTimerSeconds(object seconds)
        {
            if (!int.TryParse(seconds.ToString(), out int correctSeconds))
            {
                return;
            }
            int miliseconds = correctSeconds * 1000;

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
            if (soundTimer is not null)
                soundTimer.Dispose();

            if (timer is not null)
                timer.Dispose();
        }

        private static void TimerEnd(object source, ElapsedEventArgs e)
        {
            SoundPlayer snd = new SoundPlayer(@"D:\timer.wav");
            snd.Play();

            soundTimer = new Timer(4000);
            soundTimer.Elapsed += PlaySound;
            soundTimer.Enabled = true;
            soundTimer.AutoReset = true;
        }

        private static void PlaySound(object source, ElapsedEventArgs e)
        {
            SoundPlayer snd = new SoundPlayer(@"D:\timer.wav");
            snd.Play();
        }
    }
}
