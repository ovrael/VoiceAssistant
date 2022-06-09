using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using VoiceAssistant;

namespace VoiceAssistantUI
{
    public static class ListBoxHelpers
    {
        public static void UpdateChoices(ListBox listBox)
        {
            listBox.Items.Clear();
            foreach (var choice in Assistant.Choices)
            {
                listBox.Items.Add(choice.Name);
            }
        }

        public static void UpdateChoicesWithTooltips(ListBox listBox)
        {
            listBox.Items.Clear();
            foreach (var choice in Assistant.Choices)
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = choice.Name;
                item.ToolTip = CreateChoiceTooltip(choice.Name);
                listBox.Items.Add(item);
            }
        }

        public static void UpdateGrammar(ListBox listBox)
        {
            listBox.Items.Clear();
            foreach (var choice in Assistant.Grammar)
            {
                listBox.Items.Add(choice.Name);
            }
        }

        public static void UpdateChoiceWords(ListBox listBox, string currentChoiceName)
        {
            AssistantChoice currentChoice = Assistant.GetChoice(currentChoiceName);
            if (currentChoice is null)
            {
                return;
            }

            listBox.Items.Clear();
            foreach (var value in currentChoice.Words)
            {
                listBox.Items.Add(value);
            }
        }

        public static void UpdateChoiceWords(ListBox listBox, AssistantChoice currentChoice)
        {
            if (currentChoice is null)
            {
                return;
            }

            listBox.Items.Clear();
            foreach (var value in currentChoice.Words)
            {
                listBox.Items.Add(value);
            }
        }
        public static void UpdateGrammarChoices(ListBox grammarChoicesListBox, AssistantGrammar currentGrammar)
        {
            if (currentGrammar is null)
                return;

            grammarChoicesListBox.Items.Clear();
            foreach (var choice in currentGrammar.ChoiceNames)
            {
                ListBoxItem listItem = new ListBoxItem();
                listItem.Content = choice;
                listItem.ToolTip = CreateChoiceTooltip(choice);
                grammarChoicesListBox.Items.Add(listItem);
            }
        }

        private static string CreateChoiceTooltip(string choiceName)
        {
            var choice = Assistant.GetChoice(choiceName);

            string tooltip = string.Empty;

            for (int i = 0; i < choice.Words.Count; i++)
            {

                tooltip += $"{ choice.Words[i]}";
                if (i < choice.Words.Count - 1)
                    tooltip += ", ";
            }

            return tooltip;
        }

    }
}
