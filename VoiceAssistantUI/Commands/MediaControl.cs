using System;
using System.Runtime.InteropServices;

namespace VoiceAssistantUI.Commands
{
    public static class MediaControl
    {
        public static bool IsAvailable { get; set; } = true;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
        private const int KEYEVENTF_EXTENTEDKEY = 1;
        private const int KEYEVENTF_KEYUP = 0;
        private const int VK_MEDIA_NEXT_TRACK = 0xB0;// code to jump to next track
        private const int VK_MEDIA_PLAY_PAUSE = 0xB3;// code to play or pause a song
        private const int VK_MEDIA_PREV_TRACK = 0xB1;// code to jump to prev track

        public static void PauseMedia()
        {
            // Play or Pause music
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }

        public static void PlayMedia()
        {
            // Play or Pause music
            keybd_event(VK_MEDIA_PLAY_PAUSE, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }

        public static void PreviousMedia()
        {
            // Jump to previous track
            keybd_event(VK_MEDIA_PREV_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }

        public static void NextMedia()
        {
            // Jump to next track
            keybd_event(VK_MEDIA_NEXT_TRACK, 0, KEYEVENTF_EXTENTEDKEY, IntPtr.Zero);
        }
    }
}
