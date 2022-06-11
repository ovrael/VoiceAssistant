using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VoiceAssistantUI
{
    /// <summary>
    /// Logika interakcji dla klasy CreateChoicesWindow.xaml
    /// </summary>
    public partial class CreateChoicesWindow : Window
    {
        private string choiceName = string.Empty;
        private List<string> choiceSentences = new List<string>();

        public CreateChoicesWindow()
        {
            InitializeComponent();
        }

        string ReplaceSpecialVariables(string text)
        {
            string customText = string.Empty;
            bool isSpecialVariable = false;
            string specialVariableName = string.Empty;

            for (int i = 0; i < text.Length; i++)
            {
                if (isSpecialVariable)
                {
                    if (text[i] == '}')
                    {
                        if (VoiceAssistant.Assistant.ChangeableVariables.ContainsKey(specialVariableName))
                        {
                            customText += VoiceAssistant.Assistant.ChangeableVariables[specialVariableName];
                        }
                        else
                        {
                            VoiceAssistant.Assistant.WriteLog($"{specialVariableName} special variable doesn't exist in the program!");
                        }

                        isSpecialVariable = false;
                        specialVariableName = string.Empty;
                        continue;
                    }

                    specialVariableName += text[i];
                }

                if (text[i] == '{')
                {
                    isSpecialVariable = true;
                }

                if (!isSpecialVariable)
                {

                    customText += text[i];
                }
            }

            return customText;
        }

        private void NewChoiceSentenceTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            string choiceSentence = (sender as TextBox).Text;
            if (choiceSentences.Contains(choiceSentence))
                return;

            if (choiceSentence.Length < 1)
                return;

            choiceSentence = ReplaceSpecialVariables(choiceSentence);

            choiceSentences.Add(choiceSentence);

            (sender as TextBox).Text = string.Empty;

            if (choiceSentences.Count > 0)
                enterChoiceValueWatermark.Text = "Enter next choice value";

            UpdateValuesListBox();
        }

        private void UpdateValuesListBox()
        {
            choiceSentences.Sort();
            choicesValueListBox.Items.Clear();
            foreach (var value in choiceSentences)
            {
                choicesValueListBox.Items.Add(value);
            }
        }

        private void NewChoiceTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            TextBox choiceNameTextBox = (TextBox)sender;
            if (choiceNameTextBox.Text.Length < 1)
                return;

            choiceName = choiceNameTextBox.Text;
            choiceNameTextBox.Text = string.Empty;

            enterChoiceNameWatermark.Text = $"Change \"{choiceName}\" name";
            choiceValuesLabel.Content = $"{choiceName} values";
        }

        private string GetChoiceWord()
        {
            if (choicesValueListBox.SelectedIndex < 0)
                return string.Empty;

            return (string)choicesValueListBox.SelectedItem;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            string value = GetChoiceWord();
            string newValue = changedValueTextBox.Text;
            if (value == string.Empty)
                return;

            if (newValue.Length < 1)
                return;

            if (choiceSentences.Contains(newValue))
                return;

            changedValueTextBox.Text = string.Empty;
            choiceSentences.Remove(value);
            choiceSentences.Add(newValue);

            UpdateValuesListBox();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string value = GetChoiceWord();
            if (value == string.Empty)
                return;

            choiceSentences.Remove(value);
            UpdateValuesListBox();
        }

        private void ChoicesValueListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (choicesValueListBox.SelectedIndex >= 0)
                changeChoiceValueWatermark.Text = $"Change \"{GetChoiceWord()}\" value";
            else
                changeChoiceValueWatermark.Text = "Select value";
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (VoiceAssistant.Assistant.Choices.Any(c => c.Name == choiceName))
            {
                MessageBox.Show("Choice with that namy already exists!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            VoiceAssistant.AssistantChoice newChoice = new VoiceAssistant.AssistantChoice(choiceName, choiceSentences);
            VoiceAssistant.Assistant.Choices.Add(newChoice);
            Close();
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Close();
        }

        private void ChangedValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                EditButton_Click(sender, e);
        }

        private void CreateButton_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (choiceName.Length > 0 && choiceSentences.Count > 0)
            {
                createButton.IsEnabled = true;
                createButton.ToolTip = "Choice has to have name and at least one value!";
            }
            else
            {
                createButton.IsEnabled = false;
                createButton.ToolTip = "";
            }
        }
    }
}
