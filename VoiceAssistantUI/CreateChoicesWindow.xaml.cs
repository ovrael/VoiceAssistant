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
        private List<string> choiceValues = new List<string>();

        public CreateChoicesWindow()
        {
            InitializeComponent();
        }

        private void newChoiceValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            TextBox choiceValueTB = (TextBox)sender;
            if (choiceValues.Contains(choiceValueTB.Text))
                return;

            choiceValues.Add(choiceValueTB.Text);
            choiceValueTB.Text = string.Empty;

            if (choiceValues.Count > 0)
                enterChoiceValueWatermark.Text = "Enter next choice value";

            UpdateValuesListBox();
        }

        private void UpdateValuesListBox()
        {
            choiceValues.Sort();
            choicesValueListBox.Items.Clear();
            foreach (var value in choiceValues)
            {
                choicesValueListBox.Items.Add(value);
            }
        }

        private void newChoiceTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Return)
                return;

            TextBox choiceValueTB = (TextBox)sender;
            if (choiceValueTB.Text.Length < 1)
                return;

            choiceName = choiceValueTB.Text;
            choiceValueTB.Text = string.Empty;

            enterChoiceNameWatermark.Text = $"Change \"{choiceName}\" name";
            choiceValuesLabel.Content = $"{choiceName} values";
        }

        private string GetChoiceValue()
        {
            if (choicesValueListBox.SelectedIndex < 0)
                return string.Empty;

            return (string)choicesValueListBox.SelectedItem;
        }

        private void editButton_Click(object sender, RoutedEventArgs e)
        {
            string value = GetChoiceValue();
            string newValue = changedValueTextBox.Text;
            if (value == string.Empty)
                return;

            if (newValue.Length < 1)
                return;

            if (choiceValues.Contains(newValue))
                return;

            changedValueTextBox.Text = string.Empty;
            choiceValues.Remove(value);
            choiceValues.Add(newValue);

            UpdateValuesListBox();
        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {
            string value = GetChoiceValue();
            if (value == string.Empty)
                return;

            choiceValues.Remove(value);
            UpdateValuesListBox();
        }

        private void choicesValueListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (choicesValueListBox.SelectedIndex >= 0)
                changeChoiceValueWatermark.Text = $"Change \"{GetChoiceValue()}\" value";
            else
                changeChoiceValueWatermark.Text = "Select value";
        }

        private void createButton_Click(object sender, RoutedEventArgs e)
        {
            if (VoiceAssistant.Assistant.Choices.Any(c => c.Name == choiceName))
            {
                MessageBox.Show("Choice with that namy already exists!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            VoiceAssistant.AssistantChoice newChoice = new VoiceAssistant.AssistantChoice(choiceName, choiceValues);
            VoiceAssistant.Assistant.Choices.Add(newChoice);
            Close();
        }

        private void abortButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Close();
        }

        private void changedValueTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                editButton_Click(sender, e);
        }

        private void createButton_LayoutUpdated(object sender, System.EventArgs e)
        {
            if (choiceName.Length > 0 && choiceValues.Count > 0)
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
