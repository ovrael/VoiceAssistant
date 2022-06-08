﻿using System;
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
        List<object> choicesManagementControls = new List<object>();
        List<object> choicesValueManagementControls = new List<object>();

        public MainWindow()
        {
            InitializeComponent();
            ConsoleManager.ShowConsoleWindow();
            MoveTabs();
            TestChoices();
            FilterControls();
            UpdateChoicesTab();
        }

        private void FilterControls()
        {
            choicesValueManagementControls.Add(newChoiceValueButton);
        }

        private void TestChoices()
        {
            AssistantChoices show = new AssistantChoices("show", new List<string>() { "show", "print", "display" });
            AssistantChoices apps = new AssistantChoices("apps", new List<string>() { "applications", "apps", "programs" });
            AssistantChoices open = new AssistantChoices("open", new List<string>() { "open", "run" });

            Assistant.AssistantChoices.Add(show);
            Assistant.AssistantChoices.Add(apps);
            Assistant.AssistantChoices.Add(open);
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

        private void Window_Initialized(object sender, EventArgs e)
        {
            trayIcon = new NotifyIcon();
            trayIcon.DoubleClick += new EventHandler(TrayIconDoubleClick);
            trayIcon.Icon = new Icon(@"..\..\..\src\img\tray.ico");
        }

        private void TrayIconClick(object sender, EventArgs e)
        {
            trayIcon.ShowBalloonTip(800, "App is still working", "Will listen to you waiting for call.", ToolTipIcon.Info);
        }

        private void TrayIconDoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = WindowState.Normal;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            trayIcon.Visible = true;
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            trayIcon.Visible = false;
            trayIcon = null;
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MoveTabs();
        }

        private void ChoicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentClick = CurrentClick.Choices;
            UpdateChoiceValuesTab();
        }

        private void ChoicesValueListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentClick = CurrentClick.ChoicesValues;
        }

        private enum CurrentClick
        {
            Choices,
            ChoicesValues
        }

        private void NewChoicesButton_Click(object sender, RoutedEventArgs e)
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

        private void NewChoiceValueButton_Click(object sender, RoutedEventArgs e)
        {
            if (choicesListBox.Items.Count == 0)
                return;

            AssistantChoices currentChoices = GetCurrentAssistantChoice();
            if (currentChoices is null)
                return;

            string nextValue = Interaction.InputBox("Provide choice value", "Choice value");
            if (nextValue == string.Empty)
            {
                MessageBox.Show("Value cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            currentChoices.AddChoicesValue(nextValue);
            UpdateChoiceValuesTab();
        }

        private void UpdateChoiceValuesTab()
        {
            AssistantChoices currentChoices = GetCurrentAssistantChoice();
            if (currentChoices is null)
            {
                return;
            }

            choicesValueListBox.Items.Clear();
            choicesListBox.AllowDrop = true;
            foreach (var value in currentChoices.ChoiceValues)
            {
                choicesValueListBox.Items.Add(value);
            }
        }

        private AssistantChoices GetCurrentAssistantChoice()
        {
            return Assistant.AssistantChoices
                .Where(c => c.Name == (string)choicesListBox.SelectedItem)
                .FirstOrDefault();
        }

        private string GetCurrentAssistantChoiceValue()
        {
            return (string)choicesValueListBox.SelectedItem;
        }

        private void UpdateChoicesTab(bool clearValuesTab = true)
        {
            choicesListBox.Items.Clear();
            foreach (var choice in Assistant.AssistantChoices)
            {
                choicesListBox.Items.Add(choice.Name);
            }

            if (clearValuesTab)
                choicesValueListBox.Items.Clear();
        }

        private void choicesListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentClick = CurrentClick.Choices;
        }

        private void choicesValueListBox_GotFocus(object sender, RoutedEventArgs e)
        {
            currentClick = CurrentClick.ChoicesValues;
        }

        private enum Operation
        {
            Edit,
            Delete
        }

        private void DeleteChoice(AssistantChoices choice)
        {
            var result = MessageBox.Show($"Are you sure to delete \"{choice.Name}\" choice?", "Delete quesiton", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Assistant.AssistantChoices.Remove(choice);
            }
        }

        private void DeleteChoiceValues(AssistantChoices choice)
        {
            if (choice.ChoiceValues.Count == 1)
            {
                MessageBox.Show("You can't delete last value of choice.\nDelete choice instead.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            choice.RemoveChoicesValue(GetCurrentAssistantChoiceValue());
        }

        private void EditChoice(AssistantChoices choice)
        {
            string newName = Interaction.InputBox($"Provide new choice name for choice \"{choice.Name}\"", "Changing choice name");

            if (Assistant.AssistantChoices.Any(c => c.Name == newName))
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

        private void EditChoiceValue(AssistantChoices choice)
        {
            string oldValue = GetCurrentAssistantChoiceValue();
            string newValue = Interaction.InputBox($"Provide new choice value for \"{oldValue}\"", "Changing choice name");

            if (Assistant.AssistantChoices.Any(c => c.Name == newValue))
            {
                MessageBox.Show("Couldn't change value! That value already exists!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newValue.Length < 1)
            {
                MessageBox.Show("Value cannot be blank!", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            choice.RemoveChoicesValue(oldValue);
            choice.AddChoicesValue(newValue);
        }


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

            if (currentClick == CurrentClick.ChoicesValues)
            {
                if (choicesValueListBox.Items.Count == 0)
                    return;

                var currentChoice = GetCurrentAssistantChoice();
                if (currentChoice is null)
                    return;

                switch (operation)
                {
                    case Operation.Edit:
                        EditChoiceValue(currentChoice);
                        break;
                    case Operation.Delete:
                        DeleteChoiceValues(currentChoice);
                        break;
                    default:
                        break;
                }

                UpdateChoiceValuesTab();
            }
        }
    }
}
