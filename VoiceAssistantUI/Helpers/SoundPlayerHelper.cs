using System.IO;
using System.Media;

namespace VoiceAssistantUI.Helpers
{
    public static class SoundPlayerHelper
    {
        private static readonly SoundPlayer soundPlayer = new SoundPlayer();

        public static void PlaySound(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            soundPlayer.SoundLocation = path;
            soundPlayer.Play();
        }
    }
}
