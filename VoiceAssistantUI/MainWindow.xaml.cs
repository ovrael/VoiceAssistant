using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using VoiceAssistant;
using Interaction = Microsoft.VisualBasic.Interaction;
using MessageBox = System.Windows.MessageBox;

namespace VoiceAssistantUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon trayIcon;
        CurrentClick currentClick = CurrentClick.Choices;

        public MainWindow()
        {
            InitializeComponent();
            ConsoleManager.ShowConsoleWindow();
            MoveTabs();

            TestChoices();
            TestGrammar();

            UpdateChoicesTab();
            UpdateGrammarTab();
        }

        #region Tests
        private void TestChoices()
        {
            AssistantChoice show = new AssistantChoice("show", new List<string>() { "show", "print", "display" });
            AssistantChoice apps = new AssistantChoice("apps", new List<string>() { "applications", "apps", "programs" });
            AssistantChoice open = new AssistantChoice("open", new List<string>() { "open", "run" });

            Assistant.Choices.Add(show);
            Assistant.Choices.Add(apps);
            Assistant.Choices.Add(open);
        }

        private void TestGrammar()
        {
            AssistantGrammar printApss = new AssistantGrammar("Print apps", "show", "apps");
            AssistantGrammar openApps = new AssistantGrammar("Open apps", "open", "apps");

            Assistant.Grammar.Add(printApss);
            Assistant.Grammar.Add(openApps);
        }
        #endregion

        #region Main Window
        private void Window_Initialized(object sender, EventArgs e)
        {
            trayIcon = new NotifyIcon();
            trayIcon.DoubleClick += new EventHandler(TrayIconDoubleClick);
            trayIcon.Icon = new Icon(@"..\..\..\src\img\tray.ico");
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            trayIcon.Visible = true;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon = null;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MoveTabs();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                trayIcon.ShowBalloonTip(1000, "App is still working.", "Will listen to you waiting for call " + VoiceAssistant.Assistant.AssistantName, ToolTipIcon.Info);
            }

            base.OnStateChanged(e);
        }
        #endregion

        #region Tray Icon
        private void TrayIconClick(object sender, EventArgs e)
        {
            trayIcon.ShowBalloonTip(800, "App is still working", "Will listen to you waiting for call.", ToolTipIcon.Info);
        }

        private void TrayIconDoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        #endregion

        #region Updates
        private void UpdateGrammarTab()
        {
            grammarListBox.Items.Clear();

            foreach (var grammar in Assistant.Grammar)
            {
                grammarListBox.Items.Add(grammar.Name);
            }
        }
        private void UpdateChoiceWordsTab()
        {
            AssistantChoice currentChoices = GetCurrentAssistantChoice();
            if (currentChoices is null)
            {
                return;
            }

            choiceWordsListBox.Items.Clear();
            choicesListBox.AllowDrop = true;
            foreach (var value in currentChoices.Words)
            {
                choiceWordsListBox.Items.Add(value);
            }
        }
        private void UpdateChoicesTab(bool clearValuesTab = true)
        {
            choicesListBox.Items.Clear();
            foreach (var choice in Assistant.Choices)
            {
                choicesListBox.Items.Add(choice.Name);
            }

            if (clearValuesTab)
                choiceWordsListBox.Items.Clear();
        }
        private void MoveTabs()
        {
            double tabsWidth = 0;
            foreach (var item in mainTabControl.Items)
            {
                tabsWidth += (item as TabItem).Width;
            }

            double marginSpace = Width - tabsWidth;
            int offset = 16;
            settingsTab.Margin = new Thickness(marginSpace - offset, 0, -marginSpace + offset, 0);
            debugTab.Margin = new Thickness(marginSpace - offset, 0, -marginSpace + offset, 0);
        }
        #endregion

        #region Gets
        private AssistantChoice GetCurrentAssistantChoice()
        {
            return Assistant.Choices
                .Where(c => c.Name == (string)choicesListBox.SelectedItem)
                .FirstOrDefault();
        }
        private AssistantGrammar GetCurrentAssistantGrammar()
        {
            return Assistant.Grammar
                .Where(g => g.Name == (string)grammarListBox.SelectedItem)
                .FirstOrDefault();
        }
        private string GetCurrentAssistantChoiceWord()
        {
            return (string)choiceWordsListBox.SelectedItem;
        }

        #endregion

        #region Choices
        private void ChoicesListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentClick = CurrentClick.Choices;
        }
        private void ChoicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentClick = CurrentClick.Choices;
            UpdateChoiceWordsTab();
        }
        private void NewChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            CreateChoicesWindow choicesCreator = new CreateChoicesWindow();
            choicesCreator.Show();
            choicesCreator.Activate();
            choicesCreator.Closing += delegate
            {
                IsEnabled = true;
                UpdateChoicesTab();
            };

            IsEnabled = false;
        }

        #endregion

        #region Choice Words
        private void ChoiceWordsListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentClick = CurrentClick.ChoiceWords;
        }
        private void ChoiceWordsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentClick = CurrentClick.ChoiceWords;
        }
        private void NewChoiceWordButton_Click(object sender, RoutedEventArgs e)
        {
            if (choicesListBox.Items.Count == 0)
                return;

            AssistantChoice currentChoices = GetCurrentAssistantChoice();
            if (currentChoices is null)
                return;

            string nextValue = Interaction.InputBox("Provide choice value", "Choice value");
            if (nextValue == string.Empty)
            {
                MessageBox.Show("Value cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentChoices.AddChoicesValue(nextValue);
            UpdateChoiceWordsTab();
        }

        #endregion

        #region Grammar
        private void GrammarListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentClick = CurrentClick.Grammar;
        }
        private void GrammarListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            grammarChoicesListBox.Items.Clear();

            var currentGrammar = GetCurrentAssistantGrammar();

            foreach (var choice in currentGrammar.ChoiceNames)
            {
                ListBoxItem listItem = new ListBoxItem();
                listItem.Content = choice;
                listItem.ToolTip = CreateChoiceTooltip(choice);
                grammarChoicesListBox.Items.Add(listItem);
            }

        }
        string CreateChoiceTooltip(string choiceName)
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

        #endregion

        #region Enums
        private enum CurrentClick
        {
            Choices,
            ChoiceWords,
            Grammar
        }
        private enum Operation
        {
            Edit,
            Delete
        }

        #endregion
        private void ManageButton_Click(object sender, RoutedEventArgs e)
        {
            Operation operation = Operation.Edit;
            if ((sender as System.Windows.Controls.Button).Name.ToLower().Contains("delete"))
                operation = Operation.Delete;

            if (currentClick == CurrentClick.Choices)
            {
                if (choicesListBox.Items.Count == 0)
                    return;

                var currentChoice = GetCurrentAssistantChoice();
                if (currentChoice is null)
                    return;

                switch (operation)
                {
                    case Operation.Edit:
                        EditChoice(currentChoice);
                        break;
                    case Operation.Delete:
                        DeleteChoice(currentChoice);
                        break;
                    default:
                        break;
                }

                UpdateChoicesTab();
            }

            if (currentClick == CurrentClick.ChoiceWords)
            {
                if (choiceWordsListBox.Items.Count == 0)
                    return;

                var currentChoice = GetCurrentAssistantChoice();
                if (currentChoice is null)
                    return;

                switch (operation)
                {
                    case Operation.Edit:
                        EditChoiceWord(currentChoice);
                        break;
                    case Operation.Delete:
                        DeleteChoiceWords(currentChoice);
                        break;
                    default:
                        break;
                }

                UpdateChoiceWordsTab();
            }

            if (currentClick == CurrentClick.Choices)
            {
                if (grammarListBox.Items.Count == 0)
                    return;

                var currentGrammar = GetCurrentAssistantGrammar();
                if (currentGrammar is null)
                    return;

                switch (operation)
                {
                    case Operation.Edit:
                        EditGrammar(currentGrammar);
                        break;
                    case Operation.Delete:
                        DeleteGrammar(currentGrammar);
                        break;
                    default:
                        break;
                }

                UpdateGrammarTab();
            }
        }

        #region Manage choices
        private void EditChoice(AssistantChoice choice)
        {
            string newName = Interaction.InputBox($"Provide new choice name for choice \"{choice.Name}\"", "Changing choice name");

            if (Assistant.Choices.Any(c => c.Name == newName))
            {
                MessageBox.Show("Couldn't change name! Choice with that name already exists!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newName.Length < 1)
            {
                MessageBox.Show("Name cannot be blank!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            choice.Name = newName;
        }
        private void DeleteChoice(AssistantChoice choice)
        {
            var result = MessageBox.Show($"Are you sure to delete \"{choice.Name}\" choice?", "Delete quesiton", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Assistant.Choices.Remove(choice);
            }
        }

        #endregion

        #region Manage choice wrods
        private void EditChoiceWord(AssistantChoice choice)
        {
            if (choiceWordsListBox.SelectedIndex < 0)
                return;

            string oldWord = GetCurrentAssistantChoiceWord();
            string newWord = Interaction.InputBox($"Provide new choice word for \"{oldWord}\"", "Changing choice name");

            if (choice.Words.Any(c => c == newWord))
            {
                Console.WriteLine("TESTOWY");
                MessageBox.Show("Couldn't change word! That word already exists!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newWord.Length < 1)
            {
                MessageBox.Show("Word cannot be blank!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            choice.RemoveChoicesValue(oldWord);
            choice.AddChoicesValue(newWord);
        }
        private void DeleteChoiceWords(AssistantChoice choice)
        {
            if (choice.Words.Count == 1)
            {
                MessageBox.Show("You can't delete last value of choice.\nDelete choice instead.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            choice.RemoveChoicesValue(GetCurrentAssistantChoiceWord());
        }

        #endregion

        #region Manage grammar
        private void EditGrammar(AssistantGrammar grammar)
        {
            throw new Exception("CREATE NEW WINDOW WITH GRAMMAR ARGUMENT");
        }
        private void DeleteGrammar(AssistantGrammar grammar)
        {
            var result = MessageBox.Show($"Are you sure to delete \"{grammar.Name}\" grammar?", "Delete quesiton", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Assistant.Grammar.Remove(grammar);
            }
        }

        #endregion
    }
}
