using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using System.Runtime.InteropServices;

namespace VoiceAssistantBackend.Commands
{
    public static class AudioControl
    {
        private static readonly CoreAudioDevice playbackDevice = new CoreAudioController().GetDefaultDevice(deviceType: DeviceType.Playback, Role.Multimedia);

        private const byte VK_VOLUME_MUTE = 0xAD;

        private const UInt32 KEYEVENTF_EXTENDEDKEY = 0x0001;
        private const UInt32 KEYEVENTF_KEYUP = 0x0002;

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, UInt32 dwFlags, UInt32 dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern Byte MapVirtualKey(UInt32 uCode, UInt32 uMapType);

        public static void VolumeUpByPercent(int percent)
        {
            playbackDevice.Volume += playbackDevice.Volume * percent / 100d;
        }

        public static void VolumeUpByValue(int value)
        {
            playbackDevice.Volume += value;
        }

        public static void VolumeDownByPercent(int percent)
        {
            playbackDevice.Volume -= playbackDevice.Volume * percent / 100d;
        }

        public static void VolumeDownByValue(int value)
        {
            playbackDevice.Volume -= value;
        }

        public static void VolumeMute()
        {
            keybd_event(VK_VOLUME_MUTE, MapVirtualKey(VK_VOLUME_MUTE, 0), KEYEVENTF_EXTENDEDKEY, 0);
            keybd_event(VK_VOLUME_MUTE, MapVirtualKey(VK_VOLUME_MUTE, 0), KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        public static void VolumeSet(int newVolume)
        {
            playbackDevice.Volume = newVolume;
        }
    }
}
