using System.Diagnostics;

namespace VoiceAssistantBackend.Commands
{
    public static class BrowserControl
    {
        public static void OpenBrowser()
        {
            Process.Start("explorer.exe", "http://google.pl");
        }
    }
}
