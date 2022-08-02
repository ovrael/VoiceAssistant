using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VoiceAssistantUI.Helpers;

namespace VoiceAssistantUI
{
    /// <summary>
    /// Logika interakcji dla klasy CreateGrammar.xaml
    /// </summary>
    public partial class CreateEditGrammarWindow : Window
    {
        private readonly List<ListBoxItem> chosenChoicesList = new List<ListBoxItem>();
        private readonly AssistantGrammar grammarForEdit = null;
        private enum Mode
        {
            Create,
            Edit
        }

        private readonly Mode mode;

        public CreateEditGrammarWindow()
        {
            InitializeComponent();
            mode = Mode.Create;
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
            mode = Mode.Edit;
            enterGrammarNameWatermark.Text = "Change name";
            grammarNameLabel.Content = grammar.Name;
            createEditButton.Content = "Edit";
            descriptionCheckBox.Content = "Edit description?";
            createEditButton.Background = new SolidColorBrush(Color.FromArgb(127, 251, 206, 48));

            foreach (var choice in grammar.AssistantChoices)
            {
                ListBoxItem boxItem = new ListBoxItem();
                boxItem.Content = choice.Name;
                chosenChoicesList.Add(boxItem);
            }

            LoadCommandsToComboBox(grammar.CommandName);

            ListBoxHelpers.UpdateChoicesWithTooltips(availableChoicesListBox);
            ListBoxHelpers.UpdateGrammarChoices(chosenChoicesListBox, grammar);

            Style itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PreviewMouseLeftButtonDown)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ChosenChoicesListBox_Drop)));
            chosenChoicesListBox.ItemContainerStyle = itemContainerStyle;
        }

        private int IndexOfCommand(ItemCollection commands, string commandName)
        {
            int index = -1;

            for (int i = 0; i < commands.Count; i++)
            {
                if (commands[i].ToString().Contains(commandName))
                {
                    return i;
                }
            }

            return index;
        }

        private void LoadCommandsToComboBox(string commandName = "")
        {
            string[] availableCommands = CommandsData.GetCommandsNames();

            commandsComboBox.Items.Clear();

            foreach (var feature in availableCommands)
            {
                commandsComboBox.Items.Add(feature);
            }

            if (commandName.Length > 0)
            {
                selectCommandTextBlock.Opacity = 0;
                commandsComboBox.SelectedValue = commandName;
                commandsComboBox.SelectedIndex = IndexOfCommand(commandsComboBox.Items, commandName);
            }
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
            if (Assistant.Data.Grammars.Any(g => g.Name == grammarNameTextBox.Text) || grammarNameTextBox.Text.Length < 1)
                return false;

            if (commandsComboBox.SelectedIndex < 0)
                return false;

            if (chosenChoicesList.Count <= 0)
                return false;

            if (GetCommandParametersCount() != GetSpecialChoicesCount())
                return false;

            return true;
        }

        private int GetCommandParametersCount()
        {
            string commandName = commandsComboBox.SelectedItem.ToString();
            var commandInfo = Helpers.CommandsData.GetCommand(commandName);
            if (commandInfo is null)
                return -1;

            return commandInfo.GetParameters().Length;
        }

        private int GetSpecialChoicesCount()
        {
            int count = 0;

            foreach (var choices in chosenChoicesList)
            {
                string choicesName = choices.Content.ToString();
                count += Assistant.GetChoice(choicesName).IsSpecial ? 1 : 0;
            }

            return count;
        }

        private void ChangeAcceptButtonEnabling()
        {
            //if (mode == Mode.Edit)
            //    return;

            createEditButton.IsEnabled = ValidateGrammarData();
        }

        private void grammarNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            ChangeAcceptButtonEnabling();
        }

        private void createEditButton_Click(object sender, RoutedEventArgs e)
        {
            if (mode == Mode.Create) // CREATE
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

                string selectedCommand = string.Empty;
                if (commandsComboBox.SelectedIndex >= 0)
                    selectedCommand = commandsComboBox.SelectedItem.ToString();

                AssistantGrammar grammar = new AssistantGrammar(grammarNameTextBox.Text, selectedCommand, description, choiceNames);
                Assistant.Data.Grammars.Add(grammar);
            }

            if (mode == Mode.Edit) // EDIT
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

                string selectedCommand = grammarForEdit.CommandName;
                if (commandsComboBox.SelectedIndex >= 0)
                    selectedCommand = commandsComboBox.SelectedItem.ToString();

                AssistantGrammar grammar = new AssistantGrammar(name, selectedCommand, description, choiceNames);
                Assistant.Data.Grammars.Add(grammar);
                Assistant.Data.Grammars.Remove(grammarForEdit);
                Assistant.Data.Grammars.Sort((x, y) => x.Name.CompareTo(y.Name));
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
                //availableChoicesListBox.SelectedIndex = availableChoicesListBox.Items.Count - 1;
                //availableChoicesListBox.SelectedItem = availableChoicesListBox.Items[availableChoicesListBox.SelectedIndex];
            };
            choicesCreator.Closed += delegate
            {
                Assistant.SaveDataToFile();
                ListBoxHelpers.UpdateChoicesWithTooltips(availableChoicesListBox);
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
