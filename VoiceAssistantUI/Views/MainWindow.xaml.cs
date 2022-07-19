using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Interaction = Microsoft.VisualBasic.Interaction;
using MessageBox = System.Windows.MessageBox;

namespace VoiceAssistantUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon trayIcon;
        private CurrentClick currentClick = CurrentClick.Choices;
        private readonly char workingMode = 'd'; // d - develop, r - release
        private Task assistantListening;

        public MainWindow()
        {
            InitializeComponent();

            if (workingMode == 'd')
                ConsoleManager.ShowConsoleWindow();
            MoveTabs();
            //Assistant.SaveDataToFile();

            //Assistant.InitBasicHello();
            Assistant.InitConsoles(outputListBox, logsListBox);
            Assistant.LoadDataFromFile();
            //assistantListening = new Task(() => Assistant.StartListening());

            ListBoxHelpers.UpdateChoices(choicesListBox);
            ListBoxHelpers.UpdateGrammar(grammarListBox);
            App.Current.Exit += new ExitEventHandler(OnApplicationExit);
            Closed += new EventHandler(OnApplicationExit);
        }


        private void OnApplicationExit(object sender, EventArgs e)
        {
            try
            {
                if (trayIcon != null)
                {
                    trayIcon.Visible = false;
                    trayIcon.Icon = null;
                    trayIcon.Dispose();
                    trayIcon = null;
                }
            }
            catch { }
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
            AssistantGrammar printApss = new AssistantGrammar("Print apps", "dsc", "show", "apps");
            AssistantGrammar openApps = new AssistantGrammar("Open apps", "dsc", "open", "apps");

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
            trayIcon.Visible = true;
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //assistantListening.Start();
                assistantListening = Task.Run(Assistant.StartListening);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon = null;
            App.Current.Shutdown();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MoveTabs();
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
            logsTab.Margin = new Thickness(marginSpace - offset, 0, -marginSpace + offset, 0);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                trayIcon.ShowBalloonTip(1000, "App is still working.", "Will listen to you waiting for call " + Assistant.ChangeableVariables["AssistantName"], ToolTipIcon.Info);
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

        #region Gets
        private AssistantChoice GetCurrentAssistantChoice()
        {
            return Assistant.Choices
                .Where(c => c.Name == (string)choicesListBox.SelectedItem)
                .FirstOrDefault();
        }
        private AssistantGrammar GetCurrentAssistantGrammar()
        {
            if (grammarListBox.Items.Count == 0 || grammarListBox.SelectedItem is null)
                return null;

            return Assistant.Grammar
                .Where(g => g.Name == (grammarListBox.SelectedItem as ListBoxItem).Content)
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
            ListBoxHelpers.UpdateChoiceWords(choiceWordsListBox, GetCurrentAssistantChoice());
        }
        private void NewChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            CreateChoicesWindow choicesCreator = new CreateChoicesWindow();
            choicesCreator.Show();
            choicesCreator.Activate();
            choicesCreator.Closing += delegate
            {
                IsEnabled = true;
                ListBoxHelpers.UpdateChoices(choicesListBox);
            };
            choicesCreator.Closed += delegate
            {
                Assistant.SaveDataToFile();
            };

            IsEnabled = false;
        }

        #endregion

        #region Choice Sentences
        private void ChoiceSentencesListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentClick = CurrentClick.ChoiceWords;
        }
        private void ChoiceSentencesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentClick = CurrentClick.ChoiceWords;
        }
        private void NewChoiceSentenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (choicesListBox.Items.Count == 0)
                return;

            AssistantChoice currentChoice = GetCurrentAssistantChoice();
            if (currentChoice is null)
                return;

            string newSentence = Interaction.InputBox("Provide choice value", "Choice value");
            if (newSentence == string.Empty)
            {
                MessageBox.Show("Value cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            newSentence = Assistant.ReplaceSpecialVariablesKeysToValues(newSentence);

            currentChoice.AddChoiceSentence(newSentence);
            Assistant.SaveDataToFile();

            ListBoxHelpers.UpdateChoiceWords(choiceWordsListBox, GetCurrentAssistantChoice());
        }

        #endregion

        #region Grammar
        private void GrammarListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentClick = CurrentClick.Grammar;
        }
        private void GrammarListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //currentClick = CurrentClick.Grammar;
            ListBoxHelpers.UpdateGrammarChoices(grammarChoicesListBox, GetCurrentAssistantGrammar());
        }
        private void NewGrammarButton_Click(object sender, RoutedEventArgs e)
        {
            CreateEditGrammarWindow createEditGrammarWindow = new CreateEditGrammarWindow();

            createEditGrammarWindow.Show();
            createEditGrammarWindow.Activate();
            createEditGrammarWindow.Closing += delegate
            {
                IsEnabled = true;
                ListBoxHelpers.UpdateGrammar(grammarListBox);
            };
            createEditGrammarWindow.Closed += delegate
            {
                ResetAssistant();
            };

            IsEnabled = false;
        }
        private void EditGrammarButton_Click(object sender, RoutedEventArgs e)
        {
            var currentGrammar = GetCurrentAssistantGrammar();
            if (currentGrammar is null)
                return;

            CreateEditGrammarWindow createEditGrammarWindow = new CreateEditGrammarWindow(currentGrammar);

            createEditGrammarWindow.Show();
            createEditGrammarWindow.Activate();
            createEditGrammarWindow.Closing += delegate
            {
                IsEnabled = true;
                ListBoxHelpers.UpdateGrammar(grammarListBox);
            };
            createEditGrammarWindow.Closed += delegate
            {
                ResetAssistant();
            };

            IsEnabled = false;
        }

        #endregion

        private void ResetAssistant()
        {
            Assistant.SaveDataToFile();

            Assistant.IsListening = false;
            assistantListening = null;
            //assistantListening = new Task(() => Assistant.StartListening());

            assistantListening = Task.Run(Assistant.StartListening);

            //assistantListening.Start();
        }

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

                ListBoxHelpers.UpdateChoices(choicesListBox);
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
                        EditChoiceSentence(currentChoice);
                        break;
                    case Operation.Delete:
                        DeleteChoiceSentence(currentChoice);
                        break;
                    default:
                        break;
                }

                ListBoxHelpers.UpdateChoiceWords(choiceWordsListBox, GetCurrentAssistantChoice());
            }

            if (currentClick == CurrentClick.Grammar)
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

                ListBoxHelpers.UpdateGrammar(grammarListBox);
                grammarChoicesListBox.Items.Clear();
            }

            Assistant.SaveDataToFile();
        }

        #region Manage choices
        private void EditChoice(AssistantChoice choice)
        {
            string newName = Interaction.InputBox($"Provide new choice name for choice \"{choice.Name}\"", "Changing choice name", choice.Name);

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

        #region Manage choice sentences
        private void EditChoiceSentence(AssistantChoice choice)
        {
            if (choiceWordsListBox.SelectedIndex < 0)
                return;

            string oldSentence = GetCurrentAssistantChoiceWord();
            string newSentence = Interaction.InputBox($"Provide new choice sentence for \"{oldSentence}\"", "Changing choice sentence", oldSentence);
            newSentence = Assistant.ReplaceSpecialVariablesKeysToValues(newSentence);

            if (choice.Sentences.Any(c => c == newSentence))
            {
                MessageBox.Show("Couldn't change sentence! Sentence already exists!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newSentence.Length < 1)
            {
                MessageBox.Show("Sentence cannot be blank!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            choice.RemoveChoiceSentence(oldSentence);
            choice.AddChoiceSentence(newSentence);
        }
        private void DeleteChoiceSentence(AssistantChoice choice)
        {
            if (choice.Sentences.Count == 1)
            {
                MessageBox.Show("You can't delete last value of choice.\nDelete choice instead.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            choice.RemoveChoiceSentence(GetCurrentAssistantChoiceWord());
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
                ResetAssistant();
            }
        }


        #endregion

        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = sender as System.Windows.Controls.TabControl;
            if (tabControl.SelectedIndex < 0)
                return;

            var currentTabHeader = (string)(tabControl.SelectedItem as TabItem).Header;

            SetWindowWidth(currentTabHeader);
        }

        private void SetWindowWidth(string header)
        {
            if (header == "Logs")
            {
                MaxWidth = 1600;
                if (Width < 1000)
                    Width = 1000;

                return;
            }

            if (header == "Output")
            {
                MaxWidth = 1400;
                if (Width < 900)
                    Width = 900;

                return;
            }

            MaxWidth = 710;
            if (Width < 710)
                Width = 710;
        }
    }
}
