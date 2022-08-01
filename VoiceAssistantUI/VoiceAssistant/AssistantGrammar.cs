using Newtonsoft.Json;
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
        public bool CanBeEdited { get; set; } = true;
        public bool CanBeDeleted { get; set; } = true;

        [JsonIgnore]
        public Grammar Grammar { get; set; }
        //public List<string> ChoiceNames { get; set; }

        [JsonIgnore]
        public List<AssistantChoice> AssistantChoices { get; set; }
        public List<string> AssistantChoicesNames { get; set; }
        [JsonIgnore]
        public List<int> SpecialChoicesIndexes { get; private set; }

        public delegate void Command0Parameters();
        [JsonIgnore]
        public Command0Parameters command0Parameters;

        public delegate void Command1Parameters(object parameter);
        [JsonIgnore]
        public Command1Parameters command1Parameters;

        public delegate void Command2Parameters(object parameter, object parameter2);
        [JsonIgnore]
        public Command2Parameters command2Parameters;

        public void Init()
        {
            AssistantChoices = new List<AssistantChoice>();
            foreach (var choice in AssistantChoicesNames)
            {
                AssistantChoices.Add(Assistant.GetChoice(choice));
            }

            Grammar = GrammarCreator();
            CreateDelegate(CommandName);
        }

        private Grammar GrammarCreator()
        {
            GrammarBuilder grammarBuilder = new GrammarBuilder();
            SpecialChoicesIndexes = new List<int>();
            //GrammarBuilder grammarBuilder = new GrammarBuilder(AssistantChoice.Initiaton);

            int index = -1;
            foreach (var choice in AssistantChoices)
            {
                index++;
                try
                {
                    if (choice.IsSpecial)
                    {
                        SpecialChoicesIndexes.Add(index);

                        if (choice.Name.ToLower() == "$word")
                        {
                            grammarBuilder.AppendDictation();
                        }
                        else
                        {
                            grammarBuilder.Append(choice.Choice);
                        }
                    }
                    else
                    {
                        grammarBuilder.Append(choice.Choice);
                    }
                }
                catch (Exception e)
                {
                    Assistant.WriteLog($"ERROR: {e}", MessageType.Error);
                }
            }

            grammarBuilder.Culture = new System.Globalization.CultureInfo(Assistant.Data.Language);

            return new Grammar(grammarBuilder) { Name = Name };
        }

        private void CreateDelegate(string commandName)
        {
            var command = Helpers.CommandsData.GetCommand(commandName);
            if (command is null)
            {
                Assistant.WriteLog($"Couldn't create delegate. There is no command: {commandName}!", MessageType.Error);
                Console.WriteLine("AssistantGrammar.cs -> CreateDelegate(string commandName) -> command is null");
                return;
            }
            switch (command.GetParameters().Length)
            {
                case 0:
                    command0Parameters = (Command0Parameters)Delegate.CreateDelegate(typeof(Command0Parameters), command);
                    break;

                case 1:
                    command1Parameters = (Command1Parameters)Delegate.CreateDelegate(typeof(Command1Parameters), command);
                    break;

                case 2:
                    command2Parameters = (Command2Parameters)Delegate.CreateDelegate(typeof(Command2Parameters), command);
                    break;

                default:
                    Assistant.WriteLog($"There is no command: {commandName}");
                    break;
            }
        }

        private void PrintCommanNullError(int command)
        {
            Assistant.WriteLog($"Grammar: {Name} command:{CommandName} is null!");

            if (command == 0)
            {
                if (command1Parameters is not null)
                    Assistant.WriteLog($"Comman with 1 parameters is not null");

                if (command2Parameters is not null)
                    Assistant.WriteLog($"Comman with 2 parameters is not null");
            }
            if (command == 1)
            {
                if (command0Parameters is not null)
                    Assistant.WriteLog($"Comman with 0 parameters is not null");

                if (command2Parameters is not null)
                    Assistant.WriteLog($"Comman with 2 parameters is not null");
            }
            if (command == 2)
            {
                if (command0Parameters is not null)
                    Assistant.WriteLog($"Comman with 0 parameters is not null");

                if (command1Parameters is not null)
                    Assistant.WriteLog($"Comman with 1 parameters is not null");
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
                        PrintCommanNullError(parameters.Length);
                    break;

                case 1:
                    if (command1Parameters is not null)
                        command1Parameters.Invoke(parameters[0]);
                    else
                        PrintCommanNullError(parameters.Length);

                    break;

                case 2:
                    if (command2Parameters is not null)
                        command2Parameters.Invoke(parameters[0], parameters[1]);
                    else
                        PrintCommanNullError(parameters.Length);
                    break;

                default:
                    break;
            }
        }

        public AssistantGrammar()
        {

        }

        public AssistantGrammar(string name, string commandName, string description, params string[] choices)
        {
            Name = name;
            CommandName = commandName;
            Description = description;
            AssistantChoicesNames = choices.ToList();
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
