using System.Speech.Recognition;

namespace VoiceAssistant
{
    public class AssistantChoice
    {
        public string Name { get; set; }
        public Choices Choice { get; private set; }
        public List<string> Words { get; private set; } = new List<string>();

        //public static readonly Choices Initiaton = CreateInitiaton();
        //public static readonly Choices Show = BuildChoices("show", "print", "display");
        //public static readonly Choices Apps = BuildChoices("applications", "apps", "programs");
        //public static readonly Choices Installed = BuildChoices("available", "installed", "all");
        //public static readonly Choices InstalledApps = CreateInstalledApps();
        //public static readonly Choices Open = BuildChoices("open", "run");
        //public static readonly Choices MediaControl = BuildChoices("play", "pause", "stop");
        //public static readonly Choices MediaType = BuildChoices("music", "video", "media");
        //public static readonly Choices PC_Control = BuildChoices("shutdown", "reboot", "restart");

        public AssistantChoice(string name, List<string> choicesValues)
        {
            Name = name;
            Words = choicesValues;

            Choice = BuildChoices(choicesValues.ToArray());
        }

        public void AddChoicesValue(string value)
        {
            if (Words.Contains(value))
                return;

            Words.Add(value);
            Choice = BuildChoices(Words.ToArray());
        }

        public void RemoveChoicesValue(string value)
        {
            if (!Words.Contains(value))
                return;

            Words.Remove(value);
            Choice = BuildChoices(Words.ToArray());
        }


        //private static Choices CreateInitiaton()
        //{
        //    Choices choices = new Choices();

        //    choices.Add("Hi " + Assistant.AssistantName);
        //    choices.Add("Ok " + Assistant.AssistantName);
        //    choices.Add("Hey " + Assistant.AssistantName);
        //    choices.Add("Okay " + Assistant.AssistantName);
        //    choices.Add("Hello " + Assistant.AssistantName);

        //    return choices;
        //}

        //private static Choices CreateInstalledApps()
        //{
        //    Choices choices = new Choices();

        //    foreach (var app in Helpers.InstalledApps)
        //    {
        //        string[] words = app.Split(' ');
        //        string appPart = string.Empty;

        //        for (int i = 0; i < words.Length; i++)
        //        {
        //            if (words[i] != string.Empty)
        //            {
        //                if (i > 0)
        //                    appPart += ' ';
        //                appPart += words[i];
        //                choices.Add(appPart);
        //            }
        //        }
        //    }

        //    return choices;
        //}

        private Choices BuildChoices(params string[] choiceCollection)
        {
            Choices choices = new Choices();

            foreach (string choice in choiceCollection)
            {
                choices.Add(choice);
            }


            return choices;
        }
    }
}
