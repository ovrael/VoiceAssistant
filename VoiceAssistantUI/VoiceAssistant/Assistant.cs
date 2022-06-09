using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Media;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace VoiceAssistant
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
        public static double ConfidenceThreshold = 0.7;
        public static string Language = "en-US";
        public static string AssistantName = "Kaladin";
        public static string DataFilePath = @"..\..\..\src\data\data.vad";

        public static List<AssistantChoice> Choices = new List<AssistantChoice>();
        public static List<AssistantGrammar> Grammar = new List<AssistantGrammar>();

        public static ListBox outputListBox;
        private static readonly int outputHistoryLength = 300;

        public static ListBox logsListBox;
        private static readonly int logsHistoryLength = 300;

        public static void InitListBoxes(ListBox output, ListBox logs)
        {
            outputListBox = output;
            logsListBox = logs;
            outputListBox.Items.Clear();
            logsListBox.Items.Clear();
        }

        public static void StartListening()
        {
            CultureInfo cultureInfo = new CultureInfo(Language);


            using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(cultureInfo))
            {
                // Create and load grammar.
                //recognizer.LoadGrammar(new DictationGrammar());

                //AssistantChoice ac = new AssistantChoice("Welcome", new List<string>() { "Hi " + AssistantName, "Okay " + AssistantName, "Hello " + AssistantName }, canBeMoved: false);
                //Choices.Add(ac);
                //AssistantGrammar ag = new AssistantGrammar("Full open app", "Open app by calling assistant name", "Welcome", "open", "apps");
                //Grammar.Add(ag);

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
                while (true)
                {
                }
            }
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

            Application.Current.Dispatcher.Invoke(() =>
            {
                ListBoxItem boxItem = new ListBoxItem();
                boxItem.Content = message;
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

        // Handle the SpeechRecognized event.  
        private static void RecognizedText(object sender, SpeechRecognizedEventArgs e)
        {
            WriteLog($"You said \"{e.Result.Text}\" with {e.Result.Confidence:F2} confidence => runs \"{e.Result.Grammar.Name}\" grammar.");

            int nameIndex = e.Result.Text.IndexOf(AssistantName);
            if (nameIndex < 0)
            {
                return;
            }

            if (e.Result.Confidence < ConfidenceThreshold)
                return;

            string grammarName = e.Result.Grammar.Name;
            switch (grammarName)
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

                //case nameof(AssistantGrammar.ControlPC):
                //    ControlPC(e.Result.Text);
                //    break;

                default:
                    Console.WriteLine($"Grammar {grammarName} is not available yet! (wtf?)");
                    break;
            }
        }

        public static AssistantGrammar GetGrammar(string grammarName)
        {
            return Grammar.Where(g => g.Name == grammarName).FirstOrDefault();
        }

        public static AssistantChoice GetChoice(string choiceName)
        {
            return Choices.Where(c => c.Name == choiceName).FirstOrDefault();
        }

        public static void SaveDataToFile()
        {
            string language = "Language: " + Language;
            string assistantName = "AssistantName: " + AssistantName;
            string choices = "Choices:\n";

            for (int i = 0; i < Choices.Count; i++)
            {
                choices += "\tName: " + Choices[i].Name + "\n";
                choices += "\tSentences: ";

                for (int j = 0; j < Choices[i].Sentences.Count; j++)
                {
                    choices += Choices[i].Sentences[j];

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
            toSave.Add(assistantName);
            toSave.Add(choices);
            toSave.Add(grammars);

            FileManager.SaveToFile(toSave, DataFilePath);
        }

        public static void LoadDataFromFile()
        {
            List<string> data = FileManager.LoadFromFile(DataFilePath);
            AssistantName = data.Where(l => l.Contains("AssistantName")).First().Split(':')[1].Trim(' ');
            Language = data.Where(l => l.Contains("Language")).First().Split(':')[1].Trim(' ');

            int choicesIndex = data.IndexOf(data.Where(l => l.Contains("Choices:")).First());
            int grammarIndex = data.IndexOf(data.Where(l => l.Contains("Grammar:")).First());

            for (int i = choicesIndex + 1; i < grammarIndex; i += 2)
            {
                string name = data[i].Split(':')[1].Trim(' ');
                string[] sentences = data[i + 1].Split(':')[1].Trim(' ').Split(',');

                AssistantChoice assistantChoice = new AssistantChoice(name, sentences.ToList());
                Choices.Add(assistantChoice);
            }

            for (int i = grammarIndex + 1; i < data.Count; i += 3)
            {
                string name = data[i].Split(':')[1].Trim(' ');
                string description = data[i + 1].Split(':')[1].Trim(' ');
                string[] choiceNames = data[i + 2].Split(':')[1].Trim(' ').Split(',');

                AssistantGrammar assistantGrammar = new AssistantGrammar(name, description, choiceNames);
                Grammar.Add(assistantGrammar);
            }
        }

        private static void InstalledApps()
        {
            foreach (string app in Helpers.InstalledApps)
            {
                Console.WriteLine(app);
            }
        }

        private static void OpenApp(string commandText)
        {
            Console.WriteLine($"{commandText}");
        }

        private static void ControlMedia(string commandText)
        {
            Console.WriteLine($"{commandText}");
        }

        private static void ControlPC(string commandText)
        {
            if (commandText.Contains("shutdown"))
            {
                Process.Start("shutdown", "/s /t 0");
            }
        }
    }
}
