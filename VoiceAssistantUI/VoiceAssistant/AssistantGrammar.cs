using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant
{
    public class AssistantGrammar
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Grammar Grammar { get; set; }
        public List<string> ChoiceNames { get; set; }

        //public static readonly Grammar InstalledApps = InstalledAppsBuilder();
        //public static readonly Grammar OpenApp = OpenAppBuilder();
        //public static readonly Grammar ControlMedia = ControlMediaBuilder();
        //public static readonly Grammar ControlPC = ControlPCBuilder();

        private static Grammar InstalledAppsBuilder()
        {
            //Grammar grammar = GrammarCreator(AssistantChoice.Show, AssistantChoice.Installed, AssistantChoice.Apps);

            return null;
        }

        private Grammar OpenAppBuilder()
        {
            Grammar grammar = GrammarCreator("Open", "InstalledApps");

            return grammar;
        }

        private Grammar ControlMediaBuilder()
        {
            Grammar grammar = GrammarCreator("MediaControl", "MediaType");

            return grammar;
        }

        private Grammar ControlPCBuilder()
        {
            //Grammar grammar = GrammarCreator(nameof(AssistantChoice.PC_Control));

            return null;
        }

        //private static Grammar GrammarCreator(params Choices[] choices)
        //{
        //    string caller = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
        //    caller = caller.Substring(0, caller.LastIndexOf("Builder"));
        //    GrammarBuilder grammarBuilder = new GrammarBuilder();
        //    //GrammarBuilder grammarBuilder = new GrammarBuilder(AssistantChoice.Initiaton);

        //    foreach (var choice in choices)
        //    {
        //        grammarBuilder.Append(choice);
        //    }

        //    grammarBuilder.Culture = new System.Globalization.CultureInfo(Assistant.language);
        //    return new Grammar(grammarBuilder) { Name = caller };
        //}

        private Grammar GrammarCreator(params string[] choices)
        {
            GrammarBuilder grammarBuilder = new GrammarBuilder();
            //GrammarBuilder grammarBuilder = new GrammarBuilder(AssistantChoice.Initiaton);

            foreach (var choice in choices)
            {
                try
                {
                    //var propValue = typeof(AssistantChoice).GetField(choice, BindingFlags.Public | BindingFlags.Static).GetValue(null);
                    var choiceToAdd = Assistant.Choices.Where(c => c.Name == choice).First().Choice;
                    grammarBuilder.Append(choiceToAdd);
                }
                catch (Exception e)
                {
                    Assistant.WriteLog($"ERROR: {e}", MessageType.Error);
                }
            }

            grammarBuilder.Culture = new System.Globalization.CultureInfo(Assistant.Language);

            return new Grammar(grammarBuilder) { Name = this.Name };
        }

        public AssistantGrammar(string name, string description, params string[] choices)
        {
            Name = name;
            Description = description;
            ChoiceNames = choices.ToList();

            Grammar = GrammarCreator(choices);
        }
    }
}
