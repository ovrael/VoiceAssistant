﻿using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VoiceAssistantUI
{
    /// <summary>
    /// Logika interakcji dla klasy CreateGrammar.xaml
    /// </summary>
    public partial class CreateEditGrammarWindow : Window
    {
        private readonly char mode = 'c'; // c - create, e - edit
        private readonly List<ListBoxItem> chosenChoicesList = new List<ListBoxItem>();
        private readonly AssistantGrammar grammarForEdit = null;

        public CreateEditGrammarWindow()
        {
            InitializeComponent();
            mode = 'c';
            createEditButton.IsEnabled = false;
            ListBoxHelpers.UpdateChoicesWithTooltips(availableChoicesListBox);

            LoadCommandsToComboBox();

            Style itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PreviewMouseLeftButtonDown)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ChosenChoicesListBox_Drop)));
            chosenChoicesListBox.ItemContainerStyle = itemContainerStyle;
        }

        public CreateEditGrammarWindow(AssistantGrammar grammar)
        {
            InitializeComponent();
            grammarForEdit = grammar;
            mode = 'e';
            enterGrammarNameWatermark.Text = "Change name";
            grammarNameLabel.Content = grammar.Name;
            createEditButton.Content = "Edit";
            descriptionCheckBox.Content = "Edit description?";
            createEditButton.Background = new SolidColorBrush(Color.FromArgb(127, 251, 206, 48));

            foreach (var choiceName in grammar.ChoiceNames)
            {
                ListBoxItem boxItem = new ListBoxItem();
                boxItem.Content = choiceName;
                chosenChoicesList.Add(boxItem);
            }

            LoadCommandsToComboBox();

            ListBoxHelpers.UpdateChoicesWithTooltips(availableChoicesListBox);
            ListBoxHelpers.UpdateGrammarChoices(chosenChoicesListBox, grammar);

            Style itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PreviewMouseLeftButtonDown)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ChosenChoicesListBox_Drop)));
            chosenChoicesListBox.ItemContainerStyle = itemContainerStyle;
        }

        private void LoadCommandsToComboBox()
        {
            string[] availableCommands = VoiceAssistantBackend.Commands.Misc.GetAvailableCommands();

            commandsComboBox.Items.Clear();

            foreach (var feature in availableCommands)
            {
                commandsComboBox.Items.Add(feature);
            }

            commandsComboBox.SelectedItem = null;
            commandsComboBox.SelectedValue = "Select command";
        }

        private void UpdateChosenChoices()
        {
            chosenChoicesListBox.Items.Clear();
            foreach (var item in chosenChoicesList)
            {
                chosenChoicesListBox.Items.Add(item);
            }
        }

        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void ChosenChoicesListBox_Drop(object sender, DragEventArgs e)
        {
            var myObject = sender as ListBox;
            if (myObject != null)
            {
                return;
            }

            ListBoxItem droppedData = e.Data.GetData(typeof(ListBoxItem)) as ListBoxItem;
            ListBoxItem target = (ListBoxItem)sender;

            int removedIdx = chosenChoicesListBox.Items.IndexOf(droppedData);
            int targetIdx = chosenChoicesListBox.Items.IndexOf(target);

            if (removedIdx < 0 || removedIdx > chosenChoicesListBox.Items.Count)
                return;

            if (targetIdx < 0 || targetIdx > chosenChoicesListBox.Items.Count)
                return;

            chosenChoicesList.RemoveAt(removedIdx);
            chosenChoicesList.Insert(targetIdx, droppedData);
            UpdateChosenChoices();
        }

        private void AbortButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure?", "Exit", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                Close();
        }

        private ListBoxItem CreateListBoxItem(string choicesName)
        {
            ListBoxItem newItem = new ListBoxItem();
            newItem.Content = choicesName;

            return newItem;
        }

        private ListBoxItem CopyListBoxItem(ListBoxItem listBoxItem)
        {
            ListBoxItem copy = new ListBoxItem();
            copy.Content = listBoxItem.Content;

            return copy;
        }

        private void AvailableChoicesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (availableChoicesListBox.SelectedIndex < 0)
                return;

            ListBoxItem choiceNameListBoxItem = (ListBoxItem)availableChoicesListBox.Items[availableChoicesListBox.SelectedIndex];
            if (choiceNameListBoxItem is null)
                return;

            var addedItem = CopyListBoxItem(choiceNameListBoxItem);
            chosenChoicesList.Add(addedItem);
            UpdateChosenChoices();
            ChangeAcceptButtonEnabling();
        }

        private void ChosenChoicesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (chosenChoicesListBox.SelectedIndex < 0)
                return;

            ListBoxItem choiceNameListBoxItem = (ListBoxItem)chosenChoicesListBox.Items[chosenChoicesListBox.SelectedIndex];
            if (choiceNameListBoxItem is null)
                return;

            //chosenChoicesListBox.Items.Remove(choiceNameListBoxItem);
            chosenChoicesList.Remove(choiceNameListBoxItem);
            UpdateChosenChoices();
            ChangeAcceptButtonEnabling();
        }

        private void addChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (availableChoicesListBox.SelectedIndex < 0)
                return;

            var originalItem = (ListBoxItem)availableChoicesListBox.Items[availableChoicesListBox.SelectedIndex];

            chosenChoicesList.Add(CopyListBoxItem(originalItem));
            UpdateChosenChoices();
            ChangeAcceptButtonEnabling();
        }

        private void removeChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (chosenChoicesListBox.SelectedIndex < 0)
                return;

            chosenChoicesList.Remove((ListBoxItem)chosenChoicesListBox.Items[chosenChoicesListBox.SelectedIndex]);
            UpdateChosenChoices();
            ChangeAcceptButtonEnabling();
        }

        private void moveUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (chosenChoicesListBox.SelectedIndex <= 0)
                return;

            ListBoxItem movedItem = (ListBoxItem)chosenChoicesListBox.Items[chosenChoicesListBox.SelectedIndex];
            chosenChoicesList.Remove(movedItem);
            chosenChoicesList.Insert(chosenChoicesListBox.SelectedIndex - 1, movedItem);

            UpdateChosenChoices();
            movedItem.IsSelected = true;
        }

        private void moveDownButton_Click(object sender, RoutedEventArgs e)
        {
            int idx = chosenChoicesListBox.SelectedIndex;
            if (idx < 0 || idx >= chosenChoicesListBox.Items.Count - 1)
                return;

            ListBoxItem movedItem = (ListBoxItem)chosenChoicesListBox.Items[chosenChoicesListBox.SelectedIndex];
            chosenChoicesList.Remove(movedItem);
            chosenChoicesList.Insert(chosenChoicesListBox.SelectedIndex + 1, movedItem);

            UpdateChosenChoices();
            movedItem.IsSelected = true;
        }

        private bool ValidateGrammarData()
        {
            bool goodData = true;

            if (Assistant.Grammar.Any(g => g.Name == grammarNameTextBox.Text) || grammarNameTextBox.Text.Length < 1)
                goodData = false;

            if (commandsComboBox.SelectedIndex < 0)
                goodData = false;

            if (chosenChoicesList.Count <= 0)
                goodData = false;

            return goodData;
        }

        private void ChangeAcceptButtonEnabling()
        {
            if (ValidateGrammarData())
            {
                createEditButton.IsEnabled = true;
            }
            else
            {
                createEditButton.IsEnabled = false;
            }
        }

        private void grammarNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (mode == 'e') // EDIT, can be same name
                return;

            ChangeAcceptButtonEnabling();
        }

        private void createEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (mode == 'c') // CREATE
            {
                string description = string.Empty;
                if (descriptionCheckBox.IsChecked is not null && (bool)descriptionCheckBox.IsChecked)
                {
                    string promt = $"Provide short description for grammar \"{grammarNameTextBox.Text}\". \nOr leave blank for no description.";
                    description = Interaction.InputBox(promt, "Grammar description", "e.g. \"Searches for something in browser.\"");
                }

                string[] choiceNames = new string[chosenChoicesList.Count];
                for (int i = 0; i < choiceNames.Length; i++)
                {
                    choiceNames[i] = (string)chosenChoicesList[i].Content;
                }

                AssistantGrammar grammar = new AssistantGrammar(grammarNameTextBox.Text, description, choiceNames);
                Assistant.Grammar.Add(grammar);
            }

            if (mode == 'e') // EDIT
            {
                string description = grammarForEdit.Description;
                string name = grammarForEdit.Name;
                if (grammarNameTextBox.Text.Length > 0)
                    name = grammarNameTextBox.Text;

                if (descriptionCheckBox.IsChecked is not null && (bool)descriptionCheckBox.IsChecked)
                {
                    string promt = $"Provide short description for grammar \"{grammarNameTextBox.Text}\". \nOr leave blank for no description.";
                    description = Interaction.InputBox(promt, "Grammar description", "e.g. \"Searches for something in browser.\"");
                }

                string[] choiceNames = new string[chosenChoicesList.Count];
                for (int i = 0; i < choiceNames.Length; i++)
                {
                    choiceNames[i] = (string)chosenChoicesList[i].Content;
                }

                AssistantGrammar grammar = new AssistantGrammar(name, description, choiceNames);
                Assistant.Grammar.Add(grammar);
                Assistant.Grammar.Remove(grammarForEdit);
                Assistant.Grammar.Sort((x, y) => x.Name.CompareTo(y.Name));
            }

            Close();
        }

        private void newChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            CreateChoicesWindow choicesCreator = new CreateChoicesWindow();
            choicesCreator.Show();
            choicesCreator.Activate();
            choicesCreator.Closing += delegate
            {
                IsEnabled = true;
                ListBoxHelpers.UpdateChoices(availableChoicesListBox);
            };
            choicesCreator.Closed += delegate
            {
                Assistant.SaveDataToFile();
            };

            IsEnabled = false;
        }

        private void commandsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ChangeAcceptButtonEnabling();
            selectCommandTextBlock.Opacity = 0;
        }
    }
}