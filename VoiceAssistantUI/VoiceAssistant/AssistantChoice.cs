using System.Collections.Generic;
using System.Speech.Recognition;

namespace VoiceAssistantUI
{
    public class AssistantChoice
    {
        public string Name { get; set; }
        public Choices Choice { get; private set; }
        public List<string> Sentences { get; private set; } = new List<string>();
        //public List<string> SentencesWithVariables { get; private set; } = new List<string>();
        public bool CanBeEdited { get; private set; } = true;
        public bool CanBeDeleted { get; private set; } = true;
        public bool IsString { get; private set; } = false;
        public bool IsNumber { get; private set; } = false;
        public bool IsSpecial { get; private set; } = false;

        public AssistantChoice(string name, List<string> choicesValues, bool canBeEdited = true, bool canBeDeleted = true, bool isString = false, bool isNumber = false)
        {
            CanBeEdited = canBeEdited;
            CanBeDeleted = canBeDeleted;
            IsString = isString;
            IsNumber = isNumber;

            if (IsString || IsNumber)
                IsSpecial = true;

            Name = name;
            Sentences = choicesValues;

            Choice = BuildChoices(choicesValues.ToArray());
        }

        public AssistantChoice(string name, List<string> choicesValues, string canBeEdited, string canBeDeleted, string isString, string isNumber)
        {
            CanBeEdited = bool.Parse(canBeEdited);
            CanBeDeleted = bool.Parse(canBeDeleted);
            IsString = bool.Parse(isString);
            IsNumber = bool.Parse(isNumber);

            if (IsString || IsNumber)
                IsSpecial = true;

            Name = name;
            Sentences = choicesValues;

            Choice = BuildChoices(choicesValues.ToArray());
        }

        public void AddChoiceSentence(string value)
        {
            if (Sentences.Contains(value))
                return;

            Sentences.Add(value);
            Choice = BuildChoices(Sentences.ToArray());
        }

        public void RemoveChoiceSentence(string value)
        {
            if (!Sentences.Contains(value))
                return;

            Sentences.Remove(value);
            Choice = BuildChoices(Sentences.ToArray());
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
