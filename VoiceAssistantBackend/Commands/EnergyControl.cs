using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VoiceAssistantBackend.Commands
{
    public static class EnergyControl
    {
        // LOGOUT
        [DllImport("wtsapi32.dll", SetLastError = true)]
        private static extern bool WTSDisconnectSession(IntPtr hServer, int sessionId, bool bWait);

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern int WTSGetActiveConsoleSessionId();

        private const int WTS_CURRENT_SESSION = -1;
        private static readonly IntPtr WTS_CURRENT_SERVER_HANDLE = IntPtr.Zero;

        // SLEEP / HIBERNATE
        [DllImport("Powrprof.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern bool SetSuspendState(bool hiberate, bool forceCritical, bool disableWakeEvent);

        public static void Shutdown()
        {
            var psi = new ProcessStartInfo("shutdown", "/s /t 0");
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            Process.Start(psi);
        }

        public static void Logout()
        {
            Console.WriteLine("Logging out!");
            if (!WTSDisconnectSession(WTS_CURRENT_SERVER_HANDLE, WTS_CURRENT_SESSION, false))
                throw new Win32Exception();
        }

        public static void Sleep()
        {
            SetSuspendState(false, true, true);
        }

        public static void Hibernate()
        {
            SetSuspendState(true, true, true);
        }
    }
}
