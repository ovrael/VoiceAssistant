using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistantBackend.Commands
{
    public static class ProgramsControl
    {
        public static void OpenDefaultBrowser()
        {
            string target = "blank";
            //string target = "http://www.google.com";
            System.Diagnostics.Process.Start("explorer", target);
        }
    }
}
