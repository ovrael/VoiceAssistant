using System.Collections.Generic;
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
        private List<string> choiceWords = new List<string>();

        public CreateChoicesWindow()
        {
            InitializeComponent();
        }

        private void NewChoiceWordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            TextBox choiceWordTextBox = (TextBox)sender;
            if (choiceWords.Contains(choiceWordTextBox.Text))
                return;

            if (choiceWordTextBox.Text.Length < 1)
                return;

            choiceWords.Add(choiceWordTextBox.Text);
            choiceWordTextBox.Text = string.Empty;

            if (choiceWords.Count > 0)
                enterChoiceValueWatermark.Text = "Enter next choice value";

            UpdateValuesListBox();
        }

        private void UpdateValuesListBox()
        {
            choiceWords.Sort();
            choicesValueListBox.Items.Clear();
            foreach (var value in choiceWords)
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

            if (choiceWords.Contains(newValue))
                return;

            changedValueTextBox.Text = string.Empty;
            choiceWords.Remove(value);
            choiceWords.Add(newValue);

            UpdateValuesListBox();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string value = GetChoiceWord();
            if (value == string.Empty)
                return;

            choiceWords.Remove(value);
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

            VoiceAssistant.AssistantChoice newChoice = new VoiceAssistant.AssistantChoice(choiceName, choiceWords);
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
            if (choiceName.Length > 0 && choiceWords.Count > 0)
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
