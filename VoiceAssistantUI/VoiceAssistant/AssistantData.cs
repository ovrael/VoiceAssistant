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

    public enum AssistantFile
    {
        Data,
        TrayIcon,
        MusicPlayer,
        MusicDirectory,
        CallSound,
        UncallSound,
        SuccessSound,
        FailSound,
        TimerSound
    }

    public class AssistantData
    {
        [JsonIgnore]
        public WorkingMode WorkingMode { get; set; }

        public double ConfidenceThreshold { get; set; } = 0.55;
        public string Language { get; set; } = "en-US";
        public Dictionary<string, string> ChangeableVariables { get; set; } = new Dictionary<string, string>()
        {
            {"AssistantName", "Kaladin" },
            {"City", "Katowice" }
        };

        private readonly string DebugPath = @"..\..\..";

        [JsonIgnore]
        public Dictionary<AssistantFile, string> FullFilePaths { get; set; } = new Dictionary<AssistantFile, string>();

        public Dictionary<AssistantFile, string> FilePaths { get; set; } = new Dictionary<AssistantFile, string>()
        {
            {AssistantFile.Data, @"\src\data\data.json" },
            {AssistantFile.TrayIcon, @"\src\img\tray.ico" },
            {AssistantFile.MusicPlayer, @"C:\Program files (x86)\foobar2000\foobar2000.exe" },
            {AssistantFile.MusicDirectory, @"D:\Muzyka" },
            {AssistantFile.CallSound, @"\src\sounds\assistantCall.wav" },
            {AssistantFile.UncallSound, @"\src\sounds\assistantUncall.wav" },
            {AssistantFile.SuccessSound, @"\src\sounds\successCommand.wav" },
            {AssistantFile.FailSound, @"\src\sounds\failedCommand.wav" },
            {AssistantFile.TimerSound, @"\src\sounds\timer.wav" }
        };

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

            TimerControl.TimerSoundPath = FullFilePaths[AssistantFile.TimerSound];
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
            FoobarControl.FoobarPath = FilePaths[AssistantFile.MusicPlayer];
            FoobarControl.MusicDirectory = FilePaths[AssistantFile.MusicDirectory];
        }

        private void SetDebugFilePaths()
        {
            foreach (var filePath in FilePaths)
            {
                if (filePath.Value.Contains("src"))
                    FullFilePaths.Add(filePath.Key, DebugPath + filePath.Value);
            }
        }

        private void SetReleaseFilePaths()
        {
            string currDirectory = Directory.GetCurrentDirectory();

            foreach (var filePath in FilePaths)
            {
                if (filePath.Value.Contains("src"))
                    FullFilePaths.Add(filePath.Key, currDirectory + filePath.Value);
            }
        }
    }
}
