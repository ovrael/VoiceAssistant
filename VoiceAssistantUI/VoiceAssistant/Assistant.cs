using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VoiceAssistantUI
{
    public enum MessageType
    {
        Normal,
        Success,
        Warning,
        Error
    }

    public static class Assistant
    {
        // Create an in-process speech recognizer for the en-US locale.
        public static bool IsListening = true;
        public static double ConfidenceThreshold = 0.7;
        public static string Language = "en-US";
        public static Dictionary<string, string> ChangeableVariables = new Dictionary<string, string>()
        {
            {"AssistantName", "Kaladin" }
        };

        public static string DataFilePath = @"..\..\..\src\data\data.vad";

        public static List<AssistantChoice> Choices = new List<AssistantChoice>();
        public static List<AssistantGrammar> Grammar = new List<AssistantGrammar>();

        public static ListBox outputListBox;
        private static readonly int outputHistoryLength = 300;

        public static ListBox logsListBox;
        private static readonly int logsHistoryLength = 300;


        static Assistant()
        {

        }

        #region Consoles
        public static void InitConsoles(ListBox output, ListBox logs)
        {
            outputListBox = output;
            logsListBox = logs;
            outputListBox.Items.Clear();
            logsListBox.Items.Clear();
        }
        private static SolidColorBrush PickBrush(MessageType msgType)
        {
            return msgType switch
            {
                MessageType.Normal => Brushes.White,
                MessageType.Success => Brushes.Green,
                MessageType.Warning => Brushes.Orange,
                MessageType.Error => Brushes.Red,
                _ => Brushes.White,
            };
        }
        public static void WriteLog(string message, MessageType type = MessageType.Normal)
        {
            if (logsListBox.Items.Count >= logsHistoryLength)
                logsListBox.Items.RemoveAt(0);

            string time = DateTime.Now.ToString("HH:mm:ss");

            Application.Current.Dispatcher.Invoke(() =>
            {
                ListBoxItem boxItem = new ListBoxItem();
                boxItem.Content = $"[{time}] {message}";
                boxItem.Foreground = PickBrush(type);
                logsListBox.Items.Add(boxItem);
            });
        }
        public static void WriteOutput(string message, MessageType type = MessageType.Normal)
        {
            if (outputListBox.Items.Count >= outputHistoryLength)
                outputListBox.Items.RemoveAt(0);

            Application.Current.Dispatcher.Invoke(() =>
            {
                ListBoxItem boxItem = new ListBoxItem();
                boxItem.Content = message;
                boxItem.Foreground = PickBrush(type);
                outputListBox.Items.Add(boxItem);
            });
        }
        #endregion

        #region Misc
        public static string ReplaceSpecialVariablesKeysToValues(string text)
        {
            if (!text.Contains('{') || !text.Contains('}'))
                return text;

            string customText = string.Empty;
            bool isSpecialVariable = false;
            string specialVariableName = string.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                if (isSpecialVariable)
                {
                    if (text[i] == '}')
                    {
                        if (ChangeableVariables.ContainsKey(specialVariableName))
                        {
                            customText += ChangeableVariables[specialVariableName];
                        }
                        else
                        {
                            WriteLog($"{specialVariableName} special variable doesn't exist in the program!");
                        }

                        isSpecialVariable = false;
                        specialVariableName = string.Empty;
                        continue;
                    }

                    specialVariableName += text[i];
                }

                if (text[i] == '{')
                {
                    isSpecialVariable = true;
                }

                if (!isSpecialVariable)
                {

                    customText += text[i];
                }
            }

            return customText;
        }
        public static string ReplaceSpecialVariablesValuesToKeys(string text)
        {
            foreach (var specialVariable in ChangeableVariables)
            {
                if (!text.Contains(specialVariable.Value))
                    continue;

                int startIndex = text.IndexOf(specialVariable.Value);
                int endIndex = text.IndexOf(' ', startIndex);
                endIndex = endIndex < 0 ? text.Length : endIndex;

                string leftPart = text.Substring(0, startIndex);
                string rightPart = text.Substring(endIndex);

                text = leftPart + $"{{{specialVariable.Key}}}" + rightPart;
            }

            return text;
        }

        public static AssistantGrammar GetGrammar(string grammarName)
        {
            return Grammar.Where(g => g.Name == grammarName).FirstOrDefault();
        }
        public static AssistantChoice GetChoice(string choiceName)
        {
            return Choices.Where(c => c.Name == choiceName).FirstOrDefault();
        }
        #endregion

        #region File operations
        public static void SaveDataToFile()
        {
            string language = "Language: " + Language;
            string changeableVariables = "ChangeableVariables:";
            foreach (var variableKey in ChangeableVariables.Keys)
            {
                changeableVariables += $"\n\t{variableKey}: {ChangeableVariables[variableKey]}";
            }

            string choices = "Choices:\n";

            for (int i = 0; i < Choices.Count; i++)
            {
                choices += "\tName: " + Choices[i].Name + "\n";
                choices += "\tSentences: ";

                for (int j = 0; j < Choices[i].Sentences.Count; j++)
                {
                    choices += ReplaceSpecialVariablesValuesToKeys(Choices[i].Sentences[j]);

                    if (j < Choices[i].Sentences.Count - 1)
                        choices += ",";
                }

                if (i < Choices.Count - 1)
                    choices += "\n";
            }

            string grammars = "Grammar:\n";

            for (int i = 0; i < Grammar.Count; i++)
            {
                grammars += "\tName: " + Grammar[i].Name + "\n";
                grammars += "\tCommandName: " + Grammar[i].CommandName + "\n";
                grammars += "\tDescription: " + Grammar[i].Description + "\n";
                grammars += "\tChoiceNames: ";

                for (int j = 0; j < Grammar[i].ChoiceNames.Count; j++)
                {
                    grammars += Grammar[i].ChoiceNames[j];

                    if (j < Grammar[i].ChoiceNames.Count - 1)
                        grammars += ",";
                }
                if (i < Grammar.Count - 1)
                    grammars += "\n";
            }

            List<string> toSave = new List<string>();
            toSave.Add(language);
            toSave.Add(changeableVariables);
            toSave.Add(choices);
            toSave.Add(grammars);

            FileManager.SaveToFile(toSave, DataFilePath);
        }
        public static void LoadDataFromFile()
        {
            ChangeableVariables = new Dictionary<string, string>();

            List<string> data = FileManager.LoadFromFile(DataFilePath);
            Language = data.Where(l => l.Contains("Language")).First().Split(':')[1].Trim(' ');

            int variablesIndex = data.IndexOf(data.Where(l => l.Contains("ChangeableVariables:")).First());
            int choicesIndex = data.IndexOf(data.Where(l => l.Contains("Choices:")).First());
            int grammarIndex = data.IndexOf(data.Where(l => l.Contains("Grammar:")).First());

            try
            {
                if (data[variablesIndex + 1].Length > 0)
                {
                    for (int i = variablesIndex + 1; i < choicesIndex; i++)
                    {
                        string[] variableData = data[i].Split(':');
                        ChangeableVariables.Add(variableData[0].Trim(' ').TrimStart('\t'), variableData[1].Trim(' '));
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog("Can't load changeable variable!", MessageType.Error);
                WriteLog("ERROR: " + e.ToString(), MessageType.Error);
            }

            try
            {
                if (data[choicesIndex + 1].Length > 0)
                {
                    for (int i = choicesIndex + 1; i < grammarIndex; i += 2)
                    {
                        string name = data[i].Split(':')[1].Trim(' ');
                        string[] sentences = data[i + 1].Split(':')[1].Trim(' ').Split(',');

                        for (int s = 0; s < sentences.Length; s++)
                        {
                            sentences[s] = ReplaceSpecialVariablesKeysToValues(sentences[s]);
                        }

                        AssistantChoice assistantChoice = new AssistantChoice(name, sentences.ToList());
                        Choices.Add(assistantChoice);
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog("Can't load choice!", MessageType.Error);
                WriteLog("ERROR: " + e.ToString(), MessageType.Error);
            }

            try
            {
                if (data[grammarIndex + 1].Length > 0)
                {
                    for (int i = grammarIndex + 1; i < data.Count; i += 4)
                    {
                        string name = data[i].Split(':')[1].Trim(' ');
                        string commandName = data[i + 1].Split(':')[1].Trim(' ');
                        string description = data[i + 2].Split(':')[1].Trim(' ');
                        string[] choiceNames = data[i + 3].Split(':')[1].Trim(' ').Split(',');

                        AssistantGrammar assistantGrammar = new AssistantGrammar(name, commandName, description, choiceNames);
                        Grammar.Add(assistantGrammar);
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog("Can't load grammar!", MessageType.Error);
                WriteLog("ERROR: " + e.ToString(), MessageType.Error);
            }
        }
        #endregion

        #region Speech recognition

        private static void AddDictationChoices()
        {
            if (!Choices.Exists(c => c.Name == "number" || c.Name == "Number"))
            {
                AssistantChoice numberChoice = new AssistantChoice("number", new List<string>() { "Tries to", "recognize", "number" }, false, false, true);
                Choices.Add(numberChoice);
            }
        }

        // Handle the SpeechRecognized event.
        public static async Task StartListening()
        {
            if (Grammar.Count == 0)
            {
                WriteLog("At least 1 grammar must exist to work!", MessageType.Warning);
                return;
            }

            IsListening = true;
            CultureInfo cultureInfo = new CultureInfo(Language);
            SpeechRecognitionEngine recognizer = null;
            try
            {
                if (SpeechRecognitionEngine.InstalledRecognizers() is null)
                {
                    WriteLog("There is no installed speech recognizers in Windows!", MessageType.Error);
                    return;
                }

                recognizer = new SpeechRecognitionEngine(cultureInfo);

                recognizer.UnloadAllGrammars();
                // Create and load grammar.
                //recognizer.LoadGrammar(new DictationGrammar());

                AddDictationChoices();
                foreach (var grammar in Grammar)
                {
                    recognizer.LoadGrammar(grammar.Grammar);
                }

                // Add a handler for the speech recognized event.  
                recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(RecognizedText);

                // Configure input to the speech recognizer.  
                recognizer.SetInputToDefaultAudioDevice();

                // Start asynchronous, continuous speech recognition.  
                recognizer.RecognizeAsync(RecognizeMode.Multiple);


                // Keep the console window open.  
                while (IsListening)
                {
                }
            }
            catch (Exception e)
            {
                WriteLog($"Error: {e.Message}", MessageType.Error);
                WriteLog($"More info: {e.ToString}", MessageType.Error);
            }
            finally
            {
                if (recognizer is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                //StartListening();
            }
        }
        private static void RecognizedText(object sender, SpeechRecognizedEventArgs e)
        {
            WriteLog($"You said \"{e.Result.Text}\" with {e.Result.Confidence * 100:F0}% confidence => runs \"{e.Result.Grammar.Name}\" grammar.");

            //int nameIndex = e.Result.Text.IndexOf(AssistantName);
            //if (nameIndex < 0)
            //{
            //    return;
            //}

            if (e.Result.Confidence < ConfidenceThreshold)
                return;

            switch (e.Result.Grammar.Name)
            {
                //case nameof(AssistantGrammar.InstalledApps):
                //    InstalledApps();
                //    break;

                //case nameof(AssistantGrammar.OpenApp):
                //    OpenApp(e.Result.Text);
                //    break;

                //case nameof(AssistantGrammar.ControlMedia):
                //    ControlMedia(e.Result.Text);
                //    break;

                case "shutdown":
                    VoiceAssistantBackend.Commands.EnergyControl.Shutdown();
                    break;

                case "volumeUpPercent":
                    VoiceAssistantBackend.Commands.AudioControl.VolumeUpByPercent(0);
                    break;

                case "volumeUpValue":
                    VoiceAssistantBackend.Commands.AudioControl.VolumeUpByValue(0);
                    break;

                case "volumeDownPercent":
                    VoiceAssistantBackend.Commands.AudioControl.VolumeDownByPercent(0);
                    break;

                case "volumeDownValue":
                    VoiceAssistantBackend.Commands.AudioControl.VolumeDownByValue(0);
                    break;

                case "volumeMute":
                    VoiceAssistantBackend.Commands.AudioControl.VolumeMute();
                    break;

                case "volumeSet":
                    VoiceAssistantBackend.Commands.AudioControl.VolumeSet(50);
                    break;

                default:
                    Console.WriteLine($"Grammar \"{e.Result.Grammar.Name}\" is not available yet!");
                    break;
            }
        }
        #endregion
    }
}
