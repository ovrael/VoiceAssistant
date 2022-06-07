using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Windows.Forms;
using System.Drawing;

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
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            trayIcon = new NotifyIcon();
            //trayIcon.Click += new EventHandler(TrayIconClick);
            trayIcon.DoubleClick += new EventHandler(TrayIconDoubleClick);
            trayIcon.Icon = new Icon(@"C:\Users\ovrae\OneDrive\VisualStudio2022\VoiceAssistant\VoiceAssistantUI\src\img\tray.ico");
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
            trayIcon.Visible = true;
        }
    }
}
