using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using VoiceAssistantUI.Helpers;
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
        private Task assistantListening;

        public MainWindow()
        {
            try
            {
                CommandsData.LoadAvailableCommands();
                Assistant.LoadDataFromFile();
                if (Assistant.Data.WorkingMode == VoiceAssistant.WorkingMode.Debug)
                    ConsoleManager.ShowConsoleWindow();

                InitializeComponent();
                Assistant.InitConsoles(outputListBox, logsListBox);
                Assistant.WriteLog($"Starting program in {Assistant.Data.WorkingMode} working mode.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Console.WriteLine(ex.ToString());
                Console.WriteLine("Press enter to exit");
                Console.ReadKey();
            }

            MoveTabs();
            //Assistant.LoadDataFromFile();

            ListBoxHelpers.UpdateChoices(choicesListBox);
            ListBoxHelpers.UpdateGrammar(grammarListBox);
            App.Current.Exit += new ExitEventHandler(OnApplicationExit);
            Closed += new EventHandler(OnApplicationExit);
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

        #region Main Window
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
            catch (Exception ex)
            {
                Assistant.WriteLog(ex.ToString());
            }
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                assistantListening = Task.Run(Assistant.StartListening);
            }
            catch (Exception ex)
            {
                Assistant.WriteLog(e.ToString());
            }

            try
            {
                trayIcon = new NotifyIcon();
                trayIcon.DoubleClick += new EventHandler(TrayIconDoubleClick);
                trayIcon.Icon = new Icon(Assistant.Data.FullFilePaths[VoiceAssistant.AssistantFile.TrayIcon]);
                trayIcon.Visible = true;
            }
            catch (Exception ex)
            {
                Assistant.WriteLog(ex.ToString());
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
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
            catch (Exception ex)
            {
                Assistant.WriteLog(ex.ToString());
            }

            App.Current.Shutdown();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MoveTabs();
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
        private void mainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tabControl = sender as System.Windows.Controls.TabControl;
            if (tabControl.SelectedIndex < 0)
                return;

            var currentTabHeader = (string)(tabControl.SelectedItem as TabItem).Header;

            SetWindowWidth(currentTabHeader);
        }
        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                Hide();
                trayIcon.ShowBalloonTip(1000, "App is still working.", "Will listen to you waiting for call " + Assistant.Data.ChangeableVariables["AssistantName"], ToolTipIcon.Info);
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
            return Assistant.Data.Choices
                .Where(c => c.Name == (string)choicesListBox.SelectedItem)
                .FirstOrDefault();
        }
        private AssistantGrammar GetCurrentAssistantGrammar()
        {
            if (grammarListBox.Items.Count == 0 || grammarListBox.SelectedItem is null)
                return null;

            return Assistant.Data.Grammars
                .Where(g => g.Name == (grammarListBox.SelectedItem as ListBoxItem).Content)
                .FirstOrDefault();
        }
        private string GetCurrentAssistantChoiceSentence()
        {
            return (string)choiceSentencesListBox.SelectedItem;
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
            var currentChoice = GetCurrentAssistantChoice();

            ListBoxHelpers.UpdateChoiceSentences(choiceSentencesListBox, currentChoice);

            if (currentChoice is not null)
            {
                editButton.IsEnabled = currentChoice.CanBeEdited;
                deleteButton.IsEnabled = currentChoice.CanBeDeleted;
            }
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
                choicesListBox.SelectedIndex = choicesListBox.Items.Count - 1;
                choicesListBox.SelectedItem = choicesListBox.Items[choicesListBox.SelectedIndex];
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

            ListBoxHelpers.UpdateChoiceSentences(choiceSentencesListBox, GetCurrentAssistantChoice());
        }

        #endregion

        #region Grammar
        private void ChangeGrammarButtonsEnablings()
        {
            var currentGrammar = GetCurrentAssistantGrammar();
            if (currentGrammar is null)
                return;

            deleteGrammarButton.IsEnabled = currentGrammar.CanBeDeleted;
            editGrammarButton.IsEnabled = currentGrammar.CanBeEdited;

        }

        private void GrammarListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentClick = CurrentClick.Grammar;
        }
        private void GrammarListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //currentClick = CurrentClick.Grammar;
            ChangeGrammarButtonsEnablings();
            ListBoxHelpers.UpdateGrammarChoices(grammarChoicesListBox, GetCurrentAssistantGrammar());
        }
        private void NewGrammarButton_Click(object sender, RoutedEventArgs e)
        {
            CreateEditGrammarWindow createEditGrammarWindow = new CreateEditGrammarWindow();

            StopAssistant();
            createEditGrammarWindow.Show();
            createEditGrammarWindow.Activate();
            createEditGrammarWindow.Closing += delegate
            {
                IsEnabled = true;
                ListBoxHelpers.UpdateGrammar(grammarListBox);
            };
            createEditGrammarWindow.Closed += delegate
            {
                StartAssistant();
                //ResetAssistant();
            };

            IsEnabled = false;
        }
        private void EditGrammarButton_Click(object sender, RoutedEventArgs e)
        {
            var currentGrammar = GetCurrentAssistantGrammar();
            if (currentGrammar is null)
                return;

            CreateEditGrammarWindow createEditGrammarWindow = new CreateEditGrammarWindow(currentGrammar);

            StopAssistant();
            createEditGrammarWindow.Show();
            createEditGrammarWindow.Activate();
            createEditGrammarWindow.Closing += delegate
            {
                IsEnabled = true;
                ListBoxHelpers.UpdateGrammar(grammarListBox);
            };
            createEditGrammarWindow.Closed += delegate
            {
                StartAssistant();
                ListBoxHelpers.UpdateGrammarChoices(grammarChoicesListBox, currentGrammar);
            };

            IsEnabled = false;
        }

        #endregion

        #region Assistant

        private void ResetAssistant()
        {
            Assistant.SaveDataToFile();

            Assistant.IsListening = false;
            assistantListening = null;
            //assistantListening = new Task(() => Assistant.StartListening());

            assistantListening = Task.Run(Assistant.StartListening);

            //assistantListening.Start();
        }

        private void StopAssistant()
        {
            Assistant.SaveDataToFile();

            Assistant.IsListening = false;
            assistantListening = null;
        }

        private void StartAssistant()
        {
            Assistant.SaveDataToFile();

            assistantListening = Task.Run(Assistant.StartListening);
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
                if (choiceSentencesListBox.Items.Count == 0)
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

                ListBoxHelpers.UpdateChoiceSentences(choiceSentencesListBox, GetCurrentAssistantChoice());
            }

            if (currentClick == CurrentClick.Grammar)
            {
                if (grammarListBox.Items.Count == 0)
                    return;

                var currentGrammar = GetCurrentAssistantGrammar();
                if (currentGrammar is null)
                    return;

                if (operation == Operation.Delete)
                    DeleteGrammar(currentGrammar);

                ListBoxHelpers.UpdateGrammar(grammarListBox);
                grammarChoicesListBox.Items.Clear();
            }

            Assistant.SaveDataToFile();
        }

        #region Manage choices
        private void EditChoice(AssistantChoice choice)
        {
            if (!choice.CanBeEdited)
            {
                MessageBox.Show($"\"{choice.Name}\" cannot by edited.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var choiceIndex = choicesListBox.Items.IndexOf(choice.Name);
            string newName = Interaction.InputBox($"Provide new choice name for choice \"{choice.Name}\"", "Changing choice name", choice.Name);

            if (Assistant.Data.Choices.Any(c => c.Name == newName))
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
            ListBoxHelpers.UpdateGrammarChoices(grammarChoicesListBox, GetCurrentAssistantGrammar());
            choiceSentencesListBox.Items.Clear();
            //choicesListBox.SelectedIndex = choiceIndex;
            //choicesListBox.SelectedItem = choicesListBox.Items[choicesListBox.SelectedIndex];
        }

        private List<AssistantGrammar> ChoiceInGrammars(AssistantChoice choice)
        {
            return Assistant.Data.Grammars.Where(g => g.AssistantChoices.Contains(choice)).ToList();
        }

        private void DeleteChoice(AssistantChoice choice)
        {
            if (!choice.CanBeDeleted)
            {
                MessageBox.Show($"\"{choice.Name}\" cannot by deleted.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var usedGrammars = ChoiceInGrammars(choice);
            if (usedGrammars.Count > 0)
            {
                string grammarsList = Environment.NewLine;
                for (int i = 0; i < usedGrammars.Count; i++)
                {
                    grammarsList += usedGrammars[i].Name;

                    if (i < usedGrammars.Count - 1)
                        grammarsList += ",";

                    grammarsList += Environment.NewLine;
                }

                MessageBox.Show($"\"{choice.Name}\" cannot by deleted because is used in listed grammars: {grammarsList}", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show($"Are you sure to delete \"{choice.Name}\" choice?", "Delete quesiton", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Assistant.Data.Choices.Remove(choice);
                choiceSentencesListBox.Items.Clear();
            }
        }

        #endregion

        #region Manage choice sentences
        private void EditChoiceSentence(AssistantChoice choice)
        {
            if (choiceSentencesListBox.SelectedIndex < 0)
                return;

            string oldSentence = GetCurrentAssistantChoiceSentence();
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
            choice.RemoveChoiceSentence(GetCurrentAssistantChoiceSentence());
        }

        #endregion

        #region Manage grammar
        private void DeleteGrammar(AssistantGrammar grammar)
        {
            var result = MessageBox.Show($"Are you sure to delete \"{grammar.Name}\" grammar?", "Delete quesiton", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                StopAssistant();
                Assistant.Data.Grammars.Remove(grammar);
                StartAssistant();
            }
        }
        #endregion

        private void testButton_Click(object sender, RoutedEventArgs e)
        {
            //var engVoice = Speech.GetVoiceNamesWithCulture().Where(v => v.Contains("en-")).First();

            //Speech.SetVoice(engVoice);

            Commands.WeatherControl.GetCurrentWeatherInMyCity();
            Commands.WeatherControl.GetCurrentAirPollutionInMyCity();
            Commands.WeatherControl.GetForecastInMyCity();
            Commands.WeatherControl.GetTommorowWeatherInMyCity();

        }
    }
}
