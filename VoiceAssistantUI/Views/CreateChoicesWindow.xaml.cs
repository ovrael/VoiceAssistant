﻿using System.Collections.Generic;
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
        private readonly List<string> choiceSentences = new List<string>();
        private readonly List<string> catchChoiceSentences = new List<string>();

        public CreateChoicesWindow()
        {
            InitializeComponent();
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

            choiceSentence = choiceSentence.Split(' ')[0];
            string catchChoiceSentence = Assistant.ReplaceSpecialVariablesKeysToValues(choiceSentence);

            if (catchChoiceSentence.Length < 1)
                return;

            choiceSentences.Add(choiceSentence);
            catchChoiceSentences.Add(catchChoiceSentence);

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

        private string GetChoiceSentence()
        {
            if (choicesValueListBox.SelectedIndex < 0)
                return string.Empty;

            return (string)choicesValueListBox.SelectedItem;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            string currentSentence = GetChoiceSentence();
            string editedSentence = changedValueTextBox.Text;
            if (currentSentence == string.Empty)
                return;

            if (editedSentence.Length < 1)
                return;

            if (choiceSentences.Contains(editedSentence))
                return;

            string catchEditedSentence = Assistant.ReplaceSpecialVariablesKeysToValues(editedSentence);

            if (catchEditedSentence.Length < 1)
                return;

            changedValueTextBox.Text = string.Empty;

            int index = choiceSentences.IndexOf(currentSentence);
            choiceSentences.RemoveAt(index);
            catchChoiceSentences.RemoveAt(index);

            if (editedSentence.Length > 0)
            {
                choiceSentences.Add(editedSentence);
                catchChoiceSentences.Add(catchEditedSentence);
            }

            UpdateValuesListBox();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            string value = GetChoiceSentence();
            if (value == string.Empty)
                return;

            int index = choiceSentences.IndexOf(value);
            choiceSentences.RemoveAt(index);
            catchChoiceSentences.RemoveAt(index);
            UpdateValuesListBox();
        }

        private void ChoicesValueListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (choicesValueListBox.SelectedIndex >= 0)
                changeChoiceValueWatermark.Text = $"Change \"{GetChoiceSentence()}\" value";
            else
                changeChoiceValueWatermark.Text = "Select value";
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (Assistant.Data.Choices.Any(c => c.Name == choiceName))
            {
                MessageBox.Show("Choice with that namy already exists!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AssistantChoice newChoice = new AssistantChoice(choiceName, choiceSentences);
            newChoice.IsSpecial = isSpecialCheckBox.IsChecked == null ? false : (bool)isSpecialCheckBox.IsChecked;
            newChoice.SetCatchSentences(catchChoiceSentences);
            Assistant.Data.Choices.Add(newChoice);
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
