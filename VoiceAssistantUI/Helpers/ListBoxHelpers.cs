using System.Linq;
using System.Windows.Controls;

namespace VoiceAssistantUI.Helpers
{
    public static class ListBoxHelpers
    {
        public static void UpdateChoices(ListBox listBox)
        {
            listBox.Items.Clear();
            foreach (var choice in Assistant.Data.Choices.OrderBy(c => c.Name))
            {
                listBox.Items.Add(choice.Name);
            }
        }

        public static void UpdateChoicesWithTooltips(ListBox listBox)
        {
            listBox.Items.Clear();
            foreach (var choice in Assistant.Data.Choices.OrderBy(c => c.Name))
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = choice.Name;
                item.ToolTip = CreateChoiceTooltip(choice);

                //item.IsEnabled = choice.CanBeMoved;

                listBox.Items.Add(item);
            }
        }

        public static void UpdateGrammar(ListBox listBox)
        {
            listBox.Items.Clear();
            foreach (var grammar in Assistant.Data.Grammars.OrderBy(g => g.Name))
            {
                ListBoxItem item = new ListBoxItem();
                item.Content = grammar.Name;
                item.ToolTip = grammar.Description;
                listBox.Items.Add(item);
            }
        }

        public static void UpdateChoiceSentences(ListBox listBox, string currentChoiceName)
        {
            AssistantChoice currentChoice = Assistant.GetChoice(currentChoiceName);
            if (currentChoice is null)
            {
                return;
            }

            listBox.Items.Clear();
            foreach (var value in currentChoice.Sentences.OrderBy(s => s))
            {
                listBox.Items.Add(value);
            }
        }

        public static void UpdateChoiceSentences(ListBox listBox, AssistantChoice currentChoice)
        {
            if (currentChoice is null)
            {
                return;
            }

            listBox.Items.Clear();
            foreach (var value in currentChoice.Sentences)
            {
                listBox.Items.Add(value);
            }
        }
        public static void UpdateGrammarChoices(ListBox grammarChoicesListBox, AssistantGrammar currentGrammar)
        {
            if (currentGrammar is null)
                return;

            grammarChoicesListBox.Items.Clear();
            foreach (var choice in currentGrammar.AssistantChoices)
            {
                ListBoxItem listItem = new ListBoxItem();
                listItem.Content = choice.Name;
                listItem.ToolTip = CreateChoiceTooltip(choice);
                grammarChoicesListBox.Items.Add(listItem);
            }
        }

        private static string CreateChoiceTooltip(AssistantChoice choice)
        {
            string tooltip = string.Empty;

            for (int i = 0; i < choice.Sentences.Count; i++)
            {

                tooltip += $"{choice.Sentences[i]}";
                if (i < choice.Sentences.Count - 1)
                    tooltip += ", ";
            }

            return tooltip;
        }
    }
}
