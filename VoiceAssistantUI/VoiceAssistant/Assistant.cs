//using System.Text.Json;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VoiceAssistantUI.Helpers;
using VoiceAssistantUI.VoiceAssistant;

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
        private enum Sounds
        {
            Call,
            Uncall,
            Success,
            Fail
        }

        public static AssistantData Data = new AssistantData();

        // Create an in-process speech recognizer for the en-US locale.
        public static bool IsListening = true;
        private static bool calledAssistant = false;
        private static Timer calledAssistantTimer = null;

        public static ListBox outputListBox;
        private static readonly int outputHistoryLength = 300;

        public static ListBox logsListBox;
        private static readonly int logsHistoryLength = 300;

        private static readonly int uncallAssistantTimeSeconds = 10;

        static Assistant()
        {
            //AssistantChoice ac = new AssistantChoice("string var", new List<string>() { "" }, false, false);
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
            if (logsListBox is null)
                return;

            if (logsListBox.Items.Count >= logsHistoryLength)
                logsListBox.Items.RemoveAt(0);

            string time = DateTime.Now.ToString("HH:mm:ss");

            try
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ListBoxItem boxItem = new ListBoxItem();
                    boxItem.Content = $"[{time}] {message}";
                    boxItem.Foreground = PickBrush(type);
                    logsListBox.Items.Add(boxItem);

                    //logsListBox.
                    var scrollViewer = logsListBox.Template.FindName("Scroller", logsListBox);
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString);
            }
        }
        public static void WriteOutput(string message, MessageType type = MessageType.Normal)
        {
            if (logsListBox is null)
                return;

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
                        if (Data.ChangeableVariables.ContainsKey(specialVariableName))
                        {
                            customText += Data.ChangeableVariables[specialVariableName];
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
            foreach (var specialVariable in Data.ChangeableVariables)
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
            return Data.Grammars.Where(g => g.Name == grammarName).FirstOrDefault();
        }
        public static AssistantChoice GetChoice(string choiceName)
        {
            return Data.Choices.Where(c => c.Name == choiceName).FirstOrDefault();
        }
        #endregion

        #region File operations
        public static void SaveDataToFile()
        {
            var dataJson = JsonConvert.SerializeObject(Data);
            FileManager.SaveToFile(dataJson, Data.FullFilePaths[AssistantFile.Data]);

        }
        public static void LoadDataFromFile()
        {
            string dataJson = FileManager.LoadAllText(Data.FullFilePaths[AssistantFile.Data]);

            Data = JsonConvert.DeserializeObject<AssistantData>(dataJson);
            Data.Init();
        }

        #endregion

        #region Speech recognition

        private static void AddDictationChoices()
        {
            if (!Data.Choices.Exists(c => c.Name.ToLower() == "$number"))
            {
                AssistantChoice numberChoice = new AssistantChoice("number", new List<string>() { "Tries to recognize number" }, false, false, true);
                Data.Choices.Add(numberChoice);
            }

            if (!Data.Choices.Exists(c => c.Name.ToLower() == "$artist"))
            {
                AssistantChoice artistChoices = new AssistantChoice("$artist", new List<string>() { "Gets artists from music directory" }, false, false, true);
                Data.Choices.Add(artistChoices);
            }

            if (!Data.Choices.Exists(c => c.Name.ToLower() == "$songtitle"))
            {
                AssistantChoice songChoices = new AssistantChoice("$songtitle", new List<string>() { "Gets songs from music directory" }, false, false, true);
                Data.Choices.Add(songChoices);
            }
        }

        // Handle the SpeechRecognized event.
        public static async Task StartListening()
        {
            if (Data.Grammars.Count == 0)
            {
                WriteLog("At least 1 grammar must exist to work!", MessageType.Warning);
                return;
            }

            IsListening = true;
            CultureInfo cultureInfo = new CultureInfo(Data.Language);
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

                AddDictationChoices();
                foreach (var grammar in Data.Grammars)
                {
                    recognizer.LoadGrammar(grammar.Grammar);
                }


                recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(RecognizedText);
                recognizer.SpeechRecognitionRejected += new EventHandler<SpeechRecognitionRejectedEventArgs>(RejectedText);
                recognizer.SetInputToDefaultAudioDevice();
                recognizer.RecognizeAsync(RecognizeMode.Multiple);

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
            }
        }

        private static void RejectedText(object sender, SpeechRecognitionRejectedEventArgs e)
        {
            //if (calledAssistant)
            //    PlaySound(Data.FullFilePaths[AssistantFile.FailSound]);
            if (e.Result.Grammar is not null)
                RunAssistant(e.Result);
        }

        private static void RecognizedText(object sender, SpeechRecognizedEventArgs e)
        {
            RunAssistant(e.Result);
        }

        private static void RunAssistant(RecognitionResult result)
        {

            WriteLog($"RECOGNIZED: \"{result.Text}\" with {result.Confidence * 100:F0}% confidence => runs \"{result.Grammar.Name}\" grammar. CalledAssistant:{calledAssistant}", MessageType.Success);
            if (result.Confidence < Data.ConfidenceThreshold)
            {
                if (calledAssistant)
                    SoundPlayerHelper.PlaySound(Data.FullFilePaths[AssistantFile.FailSound]);

                return;
            }

            var speakedGrammar = Data.Grammars.Where(g => g.Name == result.Grammar.Name).FirstOrDefault();
            if (speakedGrammar is null)
                return;


            if (speakedGrammar.Name == "Call Assistant")
            {
                calledAssistant = true;
                SoundPlayerHelper.PlaySound(Data.FullFilePaths[AssistantFile.CallSound]);

                calledAssistantTimer = new Timer(uncallAssistantTimeSeconds * 1000);
                calledAssistantTimer.Elapsed += DisableCall;
                calledAssistantTimer.Enabled = true;
                calledAssistantTimer.AutoReset = false;
                return;
            }

            if (!calledAssistant)
                return;

            calledAssistantTimer.Dispose();

            int specialIndexesCount = speakedGrammar.SpecialChoicesIndexes.Count;
            SoundPlayerHelper.PlaySound(Data.FullFilePaths[AssistantFile.SuccessSound]);

            if (specialIndexesCount == 0)
            {
                speakedGrammar.InvokeDelegate();
            }
            else
            {
                object[] parameters = GetParameters(speakedGrammar, result.Words);
                speakedGrammar.InvokeDelegate(parameters);
            }

            calledAssistant = false;
        }

        private static void DisableCall(object source, ElapsedEventArgs e)
        {
            SoundPlayerHelper.PlaySound(Data.FullFilePaths[AssistantFile.UncallSound]);
            calledAssistant = false;
            (source as Timer).Enabled = false;
        }

        private static bool BuildParameter(AssistantGrammar grammar, int specialIndex, string sentence)
        {
            return grammar.AssistantChoices[specialIndex].CatchSentences.Any(s => s.Contains(sentence));
        }

        private static object[] GetParameters(AssistantGrammar grammar, IEnumerable<RecognizedWordUnit> text)
        {
            string[] parameters = new string[grammar.SpecialChoicesIndexes.Count];
            int offset = 0;

            for (int i = 0; i < grammar.SpecialChoicesIndexes.Count; i++)
            {
                string sentence = text.ElementAt(grammar.SpecialChoicesIndexes[i] + offset).Text;

                for (int j = grammar.SpecialChoicesIndexes[i] + offset; j < text.Count(); j++)
                {
                    if (BuildParameter(grammar, grammar.SpecialChoicesIndexes[i], sentence))
                    {
                        offset++;

                        if (grammar.SpecialChoicesIndexes[i] + offset == text.Count())
                            break;

                        sentence += ' ' + text.ElementAt(grammar.SpecialChoicesIndexes[i] + offset).Text;
                    }
                    else
                    {
                        offset--;
                        sentence = sentence.Substring(0, sentence.LastIndexOf(' '));
                        break;
                    }
                }

                parameters[i] = sentence;
            }

            return parameters;
        }
        #endregion
    }
}
