using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using System;
using System.Runtime.InteropServices;

namespace VoiceAssistantUI.Commands
{
    public static class AudioControl
    {
        public static bool IsAvailable { get; set; } = true;

        private static readonly CoreAudioDevice playbackDevice = new CoreAudioController().GetDefaultDevice(DeviceType.Playback, Role.Multimedia);

        private const byte VK_VOLUME_MUTE = 0xAD;

        private const UInt32 KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const UInt32 KEYEVENTF_KEYUP = 0x0002;

        static AudioControl()
        {
            IsAvailable = playbackDevice is not null;
        }

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, UInt32 dwFlags, UInt32 dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern Byte MapVirtualKey(UInt32 uCode, UInt32 uMapType);

        public static void VolumeUpByPercent(object percent)
        {
            if (int.TryParse(percent.ToString(), out var result))
                playbackDevice.SetVolumeAsync(playbackDevice.Volume * result / 100.0);
        }

        public static void VolumeUpByValue(object value)
        {
            if (int.TryParse(value.ToString(), out var result))
                playbackDevice.SetVolumeAsync(playbackDevice.Volume + result);
        }

        public static void VolumeDownByPercent(object percent)
        {
            if (int.TryParse(percent.ToString(), out var result))
                playbackDevice.SetVolumeAsync(playbackDevice.Volume * result / 100.0);
        }

        public static void VolumeDownByValue(object value)
        {
            if (int.TryParse(value.ToString(), out var result))
                playbackDevice.SetVolumeAsync(playbackDevice.Volume - result);
        }

        public static void VolumeMute()
        {
            keybd_event(VK_VOLUME_MUTE, MapVirtualKey(VK_VOLUME_MUTE, 0), KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_VOLUME_MUTE, MapVirtualKey(VK_VOLUME_MUTE, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public static void VolumeUnMute()
        {
            VolumeMute();
        }

        public static void VolumeSet(object newVolume)
        {
            if (int.TryParse(newVolume.ToString(), out var result))
                playbackDevice.SetVolumeAsync(result);
        }
    }
}
