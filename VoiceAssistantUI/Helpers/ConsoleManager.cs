﻿using System;
using System.Runtime.InteropServices;

namespace VoiceAssistantUI.Helpers
{
    public static class ConsoleManager
    {
        [DllImport(@"kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport(@"kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport(@"user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SwHide = 0;
        private const int SwShow = 5;


        public static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();

            if (handle == IntPtr.Zero)
            {
                AllocConsole();
            }
            else
            {
                ShowWindow(handle, SwShow);
            }
        }

        public static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SwHide);
        }
    }
}
