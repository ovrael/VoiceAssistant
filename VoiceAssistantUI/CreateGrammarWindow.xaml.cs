using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using VoiceAssistant;

namespace VoiceAssistantUI
{
    /// <summary>
    /// Logika interakcji dla klasy CreateGrammar.xaml
    /// </summary>
    public partial class CreateEditGrammarWindow : Window
    {
        private char mode = 'c'; // c - create, e - edit
        List<ListBoxItem> chosenChoicesList = new List<ListBoxItem>();
        AssistantGrammar grammarForEdit = null;

        public CreateEditGrammarWindow()
        {
            InitializeComponent();
            mode = 'c';
            createEditButton.IsEnabled = false;
            ListBoxHelpers.UpdateChoicesWithTooltips(availableChoicesListBox);

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

            //Console.WriteLine("chosenChoicesList");
            //foreach (var item in chosenChoicesList)
            //{
            //    Console.WriteLine(item.Name);
            //};

            ListBoxHelpers.UpdateChoicesWithTooltips(availableChoicesListBox);
            ListBoxHelpers.UpdateGrammarChoices(chosenChoicesListBox, grammar);

            Style itemContainerStyle = new Style(typeof(ListBoxItem));
            itemContainerStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(PreviewMouseLeftButtonDown)));
            itemContainerStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(ChosenChoicesListBox_Drop)));
            chosenChoicesListBox.ItemContainerStyle = itemContainerStyle;
        }

        private void UpdateChosenChoices()
        {
            chosenChoicesListBox.Items.Clear();
            foreach (var item in chosenChoicesList)
            {
                chosenChoicesListBox.Items.Add(item);
            }
        }

        void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListBoxItem)
            {
                ListBoxItem draggedItem = sender as ListBoxItem;
                DragDrop.DoDragDrop(draggedItem, draggedItem, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        void ChosenChoicesListBox_Drop(object sender, DragEventArgs e)
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
        }

        private void addChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (availableChoicesListBox.SelectedIndex < 0)
                return;

            var originalItem = (ListBoxItem)availableChoicesListBox.Items[availableChoicesListBox.SelectedIndex];

            chosenChoicesList.Add(CopyListBoxItem(originalItem));
            UpdateChosenChoices();
        }

        private void removeChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            if (chosenChoicesListBox.SelectedIndex < 0)
                return;

            chosenChoicesList.Remove((ListBoxItem)chosenChoicesListBox.Items[chosenChoicesListBox.SelectedIndex]);
            UpdateChosenChoices();
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

        private void grammarNameTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (mode == 'e') // EDIT, can be same name
                return;

            TextBox choiceWordTextBox = (TextBox)sender;
            if (Assistant.Grammar.Any(g => g.Name == choiceWordTextBox.Text) || choiceWordTextBox.Text.Length < 1)
            {
                warningLabel.Opacity = 100.0;
                createEditButton.IsEnabled = false;
            }
            else
            {
                createEditButton.IsEnabled = true;
                warningLabel.Opacity = 0.0;
            }
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
    }
}
