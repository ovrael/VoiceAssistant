using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Recognition;

namespace VoiceAssistantUI
{
    public class AssistantGrammar
    {
        public string Name { get; set; }
        public string CommandName { get; set; }
        public string Description { get; set; }
        public Grammar Grammar { get; set; }
        //public List<string> ChoiceNames { get; set; }
        public List<AssistantChoice> AssistantChoices { get; set; }

        public delegate void Command0Parameters();
        public Command0Parameters command0Parameters;

        public delegate void Command1Parameters(object parameter);
        public Command1Parameters command1Parameters;

        //public static readonly Grammar InstalledApps = InstalledAppsBuilder();
        //public static readonly Grammar OpenApp = OpenAppBuilder();
        //public static readonly Grammar ControlMedia = ControlMediaBuilder();
        //public static readonly Grammar ControlPC = ControlPCBuilder();

        private static Grammar InstalledAppsBuilder()
        {
            //Grammar grammar = GrammarCreator(AssistantChoice.Show, AssistantChoice.Installed, AssistantChoice.Apps);

            return null;
        }

        //private Grammar OpenAppBuilder()
        //{
        //    Grammar grammar = GrammarCreator("Open", "InstalledApps");

        //    return grammar;
        //}

        //private Grammar ControlMediaBuilder()
        //{
        //    Grammar grammar = GrammarCreator("MediaControl", "MediaType");

        //    return grammar;
        //}

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

        private Grammar GrammarCreator()
        {
            GrammarBuilder grammarBuilder = new GrammarBuilder();
            //GrammarBuilder grammarBuilder = new GrammarBuilder(AssistantChoice.Initiaton);

            foreach (var choice in AssistantChoices)
            {
                try
                {
                    if (choice.Name.ToLower() == "number")
                    {
                        Choices numbers = Helpers.CreateNumberChoices(min: 0, max: 100);
                        grammarBuilder.Append(numbers);
                    }
                    else
                    {
                        //var propValue = typeof(AssistantChoice).GetField(choice, BindingFlags.Public | BindingFlags.Static).GetValue(null);
                        grammarBuilder.Append(choice.Choice);
                    }

                }
                catch (Exception e)
                {
                    Assistant.WriteLog($"ERROR: {e}", MessageType.Error);
                }
            }

            grammarBuilder.Culture = new System.Globalization.CultureInfo(Assistant.Language);

            return new Grammar(grammarBuilder) { Name = Name };
        }

        private void CreateDelegate(string commandName)
        {
            var command = VoiceAssistantBackend.Commands.Misc.GetCommand(commandName);
            switch (command.GetParameters().Length)
            {
                case 0:
                    if (command is null)
                    {
                        Assistant.WriteLog($"There is no command: {commandName}!", MessageType.Error);
                        return;
                    }

                    command0Parameters = (Command0Parameters)Delegate.CreateDelegate(typeof(Command0Parameters), command);
                    break;

                case 1:
                    if (command is null)
                    {
                        Assistant.WriteLog($"There is no command: {commandName}!", MessageType.Error);
                        return;
                    }

                    command1Parameters = (Command1Parameters)Delegate.CreateDelegate(typeof(Command1Parameters), command);
                    break;

                default:
                    Assistant.WriteLog($"There is no command: {commandName}");
                    break;
            }
        }

        public void InvokeDelegate(params object[] parameters)
        {
            switch (parameters.Length)
            {
                case 0:
                    if (command0Parameters is not null)
                        command0Parameters.Invoke();
                    else
                        Assistant.WriteLog("Command is null!");
                    break;

                case 1:
                    if (command1Parameters is not null)
                        command1Parameters.Invoke(parameters[0]);
                    else
                        Assistant.WriteLog("Command is null!");
                    break;

                default:
                    break;
            }
        }

        public AssistantGrammar(string name, string commandName, string description, params string[] choices)
        {
            Name = name;
            CommandName = commandName;
            Description = description;
            AssistantChoices = new List<AssistantChoice>();
            foreach (var choice in choices)
            {
                AssistantChoices.Add(Assistant.GetChoice(choice));
            }
            //ChoiceNames = choices.ToList();
            CreateDelegate(commandName);

            Grammar = GrammarCreator();
        }
    }
}
