using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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

        public string DataFilePath { get; set; } = @"\src\data\data.json";
        public string TrayIconFilePath { get; set; } = @"\src\img\tray.ico";
        public string FoobarExeFilePath { get; set; } = @"C:\Program files (x86)\foobar2000\foobar2000.exe";
        public string MusicDirectoryFilePath { get; set; } = @"D:\Muzyka";
        public string TimerSoundFilePath { get; set; } = @"\src\sounds\timer.wavs";

        public List<AssistantChoice> Choices { get; set; } = new List<AssistantChoice>();
        public List<AssistantGrammar> Grammars { get; set; } = new List<AssistantGrammar>();

        public AssistantData()
        {

        }

        public void Init()
        {
            Choices.ForEach(c => c.Init());
            Grammars.ForEach(c => c.Init());

            SetWorkingMode();

            SetSystemPaths();

            if (Assistant.Data.WorkingMode == WorkingMode.Debug)
                SetDebugFilePaths();

            if (Assistant.Data.WorkingMode == WorkingMode.Release)
                SetReleaseFilePaths();
        }

        private void SetWorkingMode()
        {
            var assemblyConfigurationAttribute = typeof(Assistant).Assembly.GetCustomAttribute<AssemblyConfigurationAttribute>();
            var buildConfigurationName = assemblyConfigurationAttribute?.Configuration;

            if (buildConfigurationName is not null)
            {
                if (buildConfigurationName.Contains("Debug"))
                    Assistant.Data.WorkingMode = WorkingMode.Debug;
                else
                    Assistant.Data.WorkingMode = WorkingMode.Release;
            }
            else
            {
                Assistant.Data.WorkingMode = WorkingMode.Debug;
            }
        }

        private void SetSystemPaths()
        {
            FoobarControl.FoobarPath = FoobarExeFilePath;
            FoobarControl.MusicDirectory = MusicDirectoryFilePath;
        }

        private void SetDebugFilePaths()
        {
            TrayIconFilePath = @"..\..\.." + TrayIconFilePath;
            DataFilePath = @"..\..\.." + DataFilePath;

            TimerSoundFilePath = @"..\..\.." + TimerSoundFilePath;
            TimerControl.TimerSoundPath = TimerSoundFilePath;
        }

        private void SetReleaseFilePaths()
        {
            string currDirectory = Directory.GetCurrentDirectory();
            TrayIconFilePath = currDirectory + TrayIconFilePath;
            DataFilePath = currDirectory + DataFilePath;

            TimerSoundFilePath = currDirectory + TimerSoundFilePath;
            TimerControl.TimerSoundPath = TimerSoundFilePath;
        }
    }
}
