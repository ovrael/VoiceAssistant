using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace VoiceAssistantUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NotifyIcon trayIcon;

        public MainWindow()
        {
            InitializeComponent();
            ConsoleManager.ShowConsoleWindow();
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
            Console.WriteLine(marginSpace);
            settingsTab.Margin = new Thickness(marginSpace - 10, 0, -marginSpace + 10, 0);
            debugTab.Margin = new Thickness(marginSpace - 10, 0, -marginSpace + 10, 0);
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            trayIcon = new NotifyIcon();
            //trayIcon.Click += new EventHandler(TrayIconClick);
            trayIcon.DoubleClick += new EventHandler(TrayIconDoubleClick);
            trayIcon.Icon = new Icon(@"..\..\..\src\img\tray.ico");
        }

        private void TrayIconClick(object sender, EventArgs e)
        {
            trayIcon.ShowBalloonTip(1000, "App is still working", "Will listen to you waiting for call.", ToolTipIcon.Info);
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
    }
}
