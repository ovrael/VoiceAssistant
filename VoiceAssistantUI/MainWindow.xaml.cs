using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using VoiceAssistant;

namespace VoiceAssistantUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon trayIcon;
        List<AssistantChoices> availableChoices = new List<AssistantChoices>();
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

            availableChoices.Add(show);
            availableChoices.Add(apps);
            availableChoices.Add(open);
        }

        private void MoveTabs()
        {
            double tabsWidth = 0;
            foreach (var item in mainTabControl.Items)
            {
                tabsWidth += (item as TabItem).Width;
            }

            double marginSpace = Width - tabsWidth;
            int offset = 18;
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
            string name = string.Empty;

            while (name == string.Empty)
            {
                name = Microsoft.VisualBasic.Interaction.InputBox("Provide choice name", "Choice name");
            }

        }

        private void NewChoiceValueButton_Click(object sender, RoutedEventArgs e)
        {
            string nextValue = Microsoft.VisualBasic.Interaction.InputBox("Provide choice value", "Choice value");
            if (nextValue == string.Empty)
            {
                System.Windows.MessageBox.Show("Value cannot be empty!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            AssistantChoices currentChoices = GetCurrentAssistantChoices();
            if (currentChoices is null)
                return;

            currentChoices.AddChoicesValue(nextValue);
            UpdateChoiceValuesTab();
        }

        private void UpdateChoiceValuesTab()
        {
            AssistantChoices currentChoices = GetCurrentAssistantChoices();
            if (currentChoices is null)
                return;

            choicesValueListBox.Items.Clear();
            choicesListBox.AllowDrop = true;
            foreach (var value in currentChoices.ChoicesValues)
            {
                choicesValueListBox.Items.Add(value);
            }
        }

        private AssistantChoices GetCurrentAssistantChoices()
        {
            return availableChoices.Where(c => c.Name == (string)choicesListBox.SelectedItem).FirstOrDefault();
        }

        private string GetCurrentAssistantChoiceValue()
        {
            return (string)choicesValueListBox.SelectedItem;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentClick == CurrentClick.Choices)
            {
                availableChoices.Remove(GetCurrentAssistantChoices());
                UpdateChoicesTab();
            }

            if (currentClick == CurrentClick.ChoicesValues)
            {
                var currentChoices = GetCurrentAssistantChoices();
                currentChoices.RemoveChoicesValue(GetCurrentAssistantChoiceValue());
                UpdateChoiceValuesTab();
            }
        }

        private void UpdateChoicesTab(bool clearValuesTab = true)
        {
            choicesListBox.Items.Clear();
            foreach (var choice in availableChoices)
            {
                choicesListBox.Items.Add(choice.Name);
            }

            if (clearValuesTab)
                choicesValueListBox.Items.Clear();
        }
    }
}
