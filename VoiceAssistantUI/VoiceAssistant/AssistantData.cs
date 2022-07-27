using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using VoiceAssistantBackend.Commands;

namespace VoiceAssistantUI.VoiceAssistant
{
    public enum WorkingMode
    {
        Debug,
        Release
    }

    public class AssistantData
    {
        [JsonIgnore]
        public WorkingMode WorkingMode { get; set; }

        public double ConfidenceThreshold { get; set; } = 0.55;
        public string Language { get; set; } = "en-US";
        public Dictionary<string, string> ChangeableVariables { get; set; } = new Dictionary<string, string>()
        {
            {"AssistantName", "Kaladin" }
        };

        private string DebugPath = @"..\..\..";
        [JsonIgnore]
        public string FullDataFilePath { get; private set; }
        public string DataFilePath { get; set; } = @"\src\data\data.json";

        [JsonIgnore]
        public string FullTrayIconFilePath { get; private set; }
        public string TrayIconFilePath { get; set; } = @"\src\img\tray.ico";

        public string FoobarExeFilePath { get; set; } = @"C:\Program files (x86)\foobar2000\foobar2000.exe";
        public string MusicDirectoryFilePath { get; set; } = @"D:\Muzyka";

        [JsonIgnore]
        public string FullTimerSoundFilePath { get; private set; }
        public string TimerSoundFilePath { get; set; } = @"\src\sounds\timer.wav";

        public List<AssistantChoice> Choices { get; set; } = new List<AssistantChoice>();
        public List<AssistantGrammar> Grammars { get; set; } = new List<AssistantGrammar>();

        public AssistantData()
        {
            SetWorkingMode();
            SetSystemPaths();

            if (WorkingMode == WorkingMode.Debug)
                SetDebugFilePaths();

            if (WorkingMode == WorkingMode.Release)
                SetReleaseFilePaths();
        }

        public void Init()
        {
            Choices.ForEach(c => c.Init());
            Grammars.ForEach(c => c.Init());
        }

        private void SetWorkingMode()
        {
            var assemblyConfigurationAttribute = typeof(Assistant).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
            var buildConfigurationName = assemblyConfigurationAttribute?.Configuration;

            if (buildConfigurationName is not null)
            {
                if (buildConfigurationName.Contains("Debug"))
                    WorkingMode = WorkingMode.Debug;
                else
                    WorkingMode = WorkingMode.Release;
            }
            else
            {
                WorkingMode = WorkingMode.Debug;
            }
        }

        private void SetSystemPaths()
        {
            FoobarControl.FoobarPath = FoobarExeFilePath;
            FoobarControl.MusicDirectory = MusicDirectoryFilePath;
        }

        private void SetDebugFilePaths()
        {
            FullTrayIconFilePath = DebugPath + TrayIconFilePath;
            FullDataFilePath = DebugPath + DataFilePath;

            FullTimerSoundFilePath = DebugPath + TimerSoundFilePath;
            TimerControl.TimerSoundPath = FullTimerSoundFilePath;
        }

        private void SetReleaseFilePaths()
        {
            string currDirectory = Directory.GetCurrentDirectory();
            FullTrayIconFilePath = currDirectory + TrayIconFilePath;
            FullDataFilePath = currDirectory + DataFilePath;

            FullTimerSoundFilePath = currDirectory + TimerSoundFilePath;
            TimerControl.TimerSoundPath = FullTimerSoundFilePath;
        }
    }
}
