using System.Diagnostics;
using System.Globalization;
using System.Speech.Recognition;

namespace VoiceAssistant
{
    public static class Assistant
    {
        // Create an in-process speech recognizer for the en-US locale.
        public static readonly string language = "en-US";
        public static readonly string AssistantName = "Kaladin";

        public static List<AssistantChoices> AssistantChoices = new List<AssistantChoices>();

        public static void StartListening()
        {
            CultureInfo cultureInfo = new CultureInfo(language);

            using (SpeechRecognitionEngine recognizer = new SpeechRecognitionEngine(cultureInfo))
            {
                // Create and load grammar.
                //recognizer.LoadGrammar(new DictationGrammar());

                recognizer.LoadGrammar(AssistantGrammar.InstalledApps);
                recognizer.LoadGrammar(AssistantGrammar.OpenApp);
                recognizer.LoadGrammar(AssistantGrammar.ControlMedia);
                recognizer.LoadGrammar(AssistantGrammar.ControlPC);

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

        // Handle the SpeechRecognized event.  
        private static void RecognizedText(object sender, SpeechRecognizedEventArgs e)
        {
            Console.WriteLine($"You said: {e.Result.Text} grammar: {e.Result.Grammar.Name}");

            int nameIndex = e.Result.Text.IndexOf(AssistantName);
            if (nameIndex < 0)
            {
                return;
            }

            if (e.Result.Confidence < 0.75)
                return;

            string grammarName = e.Result.Grammar.Name;
            switch (grammarName)
            {
                case nameof(AssistantGrammar.InstalledApps):
                    InstalledApps();
                    break;

                case nameof(AssistantGrammar.OpenApp):
                    OpenApp(e.Result.Text);
                    break;

                case nameof(AssistantGrammar.ControlMedia):
                    ControlMedia(e.Result.Text);
                    break;

                case nameof(AssistantGrammar.ControlPC):
                    ControlPC(e.Result.Text);
                    break;

                default:
                    Console.WriteLine($"Grammar {grammarName} is not available yet! (wtf?)");
                    break;
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
