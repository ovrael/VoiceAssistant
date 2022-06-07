using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Speech.Recognition;
using System.Text;
using System.Threading.Tasks;

namespace VoiceAssistant
{
    public static class AssistantGrammar
    {
        public static readonly Grammar InstalledApps = InstalledAppsBuilder();
        public static readonly Grammar OpenApp = OpenAppBuilder();
        public static readonly Grammar ControlMedia = ControlMediaBuilder();
        public static readonly Grammar ControlPC = ControlPCBuilder();

        private static Grammar InstalledAppsBuilder()
        {
            Grammar grammar = GrammarCreator(AssistantChoices.Show, AssistantChoices.Installed, AssistantChoices.Apps);

            return grammar;
        }


        private static Grammar OpenAppBuilder()
        {
            Grammar grammar = GrammarCreator("Open", "InstalledApps");

            return grammar;
        }

        private static Grammar ControlMediaBuilder()
        {
            Grammar grammar = GrammarCreator("MediaControl", "MediaType");

            return grammar;
        }

        private static Grammar ControlPCBuilder()
        {
            Grammar grammar = GrammarCreator(nameof(AssistantChoices.PC_Control));

            return grammar;
        }

        private static Grammar GrammarCreator(params Choices[] choices)
        {
            string caller = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
            caller = caller.Substring(0, caller.LastIndexOf("Builder"));
            GrammarBuilder grammarBuilder = new GrammarBuilder(AssistantChoices.Initiaton);

            foreach (var choice in choices)
            {
                grammarBuilder.Append(choice);
            }

            grammarBuilder.Culture = new System.Globalization.CultureInfo(Assistant.language);
            return new Grammar(grammarBuilder) { Name = caller };
        }

        private static Grammar GrammarCreator(params string[] choices)
        {
            string caller = new System.Diagnostics.StackTrace().GetFrame(1).GetMethod().Name;
            caller = caller.Substring(0, caller.LastIndexOf("Builder"));

            GrammarBuilder grammarBuilder = new GrammarBuilder(AssistantChoices.Initiaton);

            foreach (var choice in choices)
            {
                try
                {
                    var propValue = typeof(AssistantChoices).GetField(choice, BindingFlags.Public | BindingFlags.Static).GetValue(null);
                    grammarBuilder.Append((Choices)propValue);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"ERROR: {e}");
                    Console.ReadKey();
                }
            }

            grammarBuilder.Culture = new System.Globalization.CultureInfo(Assistant.language);

            return new Grammar(grammarBuilder) { Name = caller };
        }
    }
}
