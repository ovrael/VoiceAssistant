using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Speech.Recognition;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Text.Json;

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
        public static double ConfidenceThreshold = 0.55;
        public static string Language = "en-US";
        public static Dictionary<string, string> ChangeableVariables = new Dictionary<string, string>()
        {
            {"AssistantName", "Kaladin" }
        };

        public static string DataFilePath = @"\src\data\data.vad";

        public static List<AssistantChoice> Choices = new List<AssistantChoice>();
        public static List<AssistantGrammar> Grammars = new List<AssistantGrammar>();

        public static ListBox outputListBox;
        private static readonly int outputHistoryLength = 300;

        public static ListBox logsListBox;
        private static readonly int logsHistoryLength = 300;


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
            return Grammars.Where(g => g.Name == grammarName).FirstOrDefault();
        }
        public static AssistantChoice GetChoice(string choiceName)
        {
            return Choices.Where(c => c.Name == choiceName).FirstOrDefault();
        }
        #endregion

        #region File operations
        public static void SaveDataToFile()
        {
            //SaveDataToJsonFile();
            string language = "Language: " + Language;
            string changeableVariables = "ChangeableVariables:";
            foreach (var variableKey in ChangeableVariables.Keys)
            {
                changeableVariables += $"\n\t{variableKey}: {ChangeableVariables[variableKey]}";
            }

            string choices = "Choices:\n";

            for (int i = 0; i < Choices.Count; i++)
            {
                choices += $"\tName: {Choices[i].Name}\n";
                choices += $"\tCanBeEdited: {Choices[i].CanBeEdited}\n";
                choices += $"\tCanBeDeleted: {Choices[i].CanBeDeleted}\n";
                choices += $"\tIsString: {Choices[i].IsString}\n";
                choices += $"\tIsNumber: {Choices[i].IsNumber}\n";
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

            for (int i = 0; i < Grammars.Count; i++)
            {
                grammars += "\tName: " + Grammars[i].Name + "\n";
                grammars += "\tCommandName: " + Grammars[i].CommandName + "\n";
                grammars += "\tDescription: " + Grammars[i].Description + "\n";
                grammars += "\tChoiceNames: ";

                for (int j = 0; j < Grammars[i].AssistantChoices.Count; j++)
                {
                    grammars += Grammars[i].AssistantChoices[j].Name;

                    if (j < Grammars[i].AssistantChoices.Count - 1)
                        grammars += ",";
                }
                if (i < Grammars.Count - 1)
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
                    for (int i = choicesIndex + 1; i < grammarIndex; i += 6)
                    {
                        string name = data[i].Split(':')[1].Trim(' ');
                        string canBeEdited = data[i + 1].Split(':')[1].Trim(' ');
                        string canBeDeleted = data[i + 2].Split(':')[1].Trim(' ');
                        string isString = data[i + 3].Split(':')[1].Trim(' ');
                        string isNumber = data[i + 4].Split(':')[1].Trim(' ');
                        string[] sentences = data[i + 5].Split(':')[1].Trim(' ').Split(',');

                        for (int s = 0; s < sentences.Length; s++)
                        {
                            sentences[s] = ReplaceSpecialVariablesKeysToValues(sentences[s]);
                        }

                        AssistantChoice assistantChoice = new AssistantChoice(name, sentences.ToList(), canBeEdited, canBeDeleted, isString, isNumber);
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
                        Grammars.Add(assistantGrammar);
                    }
                }
            }
            catch (Exception e)
            {
                WriteLog("Can't load grammar!", MessageType.Error);
                WriteLog("ERROR: " + e.ToString(), MessageType.Error);
            }
        }

        public static void SaveDataToJsonFile()
        {
            var choicesJson = JsonSerializer.Serialize(Choices);
            var grammarsJson = JsonSerializer.Serialize(Grammars);
            var changeableVariablesJson = JsonSerializer.Serialize(ChangeableVariables);

            WriteLog(choicesJson);
            WriteLog(grammarsJson);
            WriteLog(changeableVariablesJson);
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
            if (Grammars.Count == 0)
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

                AddDictationChoices();
                foreach (var grammar in Grammars)
                {
                    recognizer.LoadGrammar(grammar.Grammar);
                }

                recognizer.SpeechRecognized += new EventHandler<SpeechRecognizedEventArgs>(RecognizedText);
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
        private static void RecognizedText(object sender, SpeechRecognizedEventArgs e)
        {
            var result = e.Result;
            WriteLog($"You said \"{result.Text}\" with {result.Confidence * 100:F0}% confidence => runs \"{result.Grammar.Name}\" grammar.");

            if (result.Confidence < ConfidenceThreshold)
                return;

            var speakedGrammar = Grammars.Where(g => g.Name == result.Grammar.Name).FirstOrDefault();
            if (speakedGrammar is null)
                return;

            int specialIndexesCount = speakedGrammar.SpecialChoicesIndexes.Count;

            if (specialIndexesCount == 0)
            {
                speakedGrammar.InvokeDelegate();
                return;
            }

            object[] parameters = GetParameters(speakedGrammar, result.Words);

            speakedGrammar.InvokeDelegate(parameters);
        }

        private static bool BuildParameter(AssistantGrammar grammar, int specialIndex, string sentence)
        {
            //for (int i = 0; i < grammar.AssistantChoices[specialIndex].CatchSentences.Count; i++)
            //{
            //    if (grammar.AssistantChoices[specialIndex].CatchSentences[i].StartsWith(sentence))
            //    {
            //        return true;
            //    }
            //}

            //return false;

            return grammar.AssistantChoices[specialIndex].CatchSentences.Any(s => s.Contains(sentence));

            //AssistantChoice choices = null;
            ////var choices = grammar.AssistantChoices.Where(c => c.Sentences.Contains(sentence)).First();
            //for (int i = 0; i < grammar.AssistantChoices.Count; i++)
            //{
            //    for (int j = 0; j < grammar.AssistantChoices[i].CatchSentences.Count; j++)
            //    {
            //        if (grammar.AssistantChoices[i].CatchSentences[j] == sentence)
            //        {
            //            choices = grammar.AssistantChoices[i];
            //            return (choices.IsSpecial, choices.Name);
            //        }
            //    }
            //}


            //return (false, "");
            //foreach (var choices in grammar.AssistantChoices)
            //{
            //    if (choices.IsSpecial)
            //        continue;

            //    if (choices.Sentences.Contains(sentence))
            //        return true;
            //}
            //return false;
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
