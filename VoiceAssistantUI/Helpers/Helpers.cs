using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Speech.Recognition;

namespace VoiceAssistantUI
{
    public static class Helpers
    {
        public static readonly string[] InstalledApps = GetInstalledApps();
        private static string[] GetInstalledApps()
        {
            List<string> installedApps = new List<string>();
            string registry_key = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(registry_key))
            {
                foreach (string subkey_name in key.GetSubKeyNames())
                {
                    using (RegistryKey subkey = key.OpenSubKey(subkey_name))
                    {
                        if (subkey is not null && subkey.GetValue("DisplayName") is not null)
                        {
                            string appName = (string)subkey.GetValue("DisplayName");
                            if (appName != "")
                            {
                                installedApps.Add(appName);
                            }
                        }
                    }
                }
            }

            return installedApps.OrderBy(n => n).ToArray();
        }
        private static string[] GetFilesFromDirectory(string directory)
        {
            List<string> files = new List<string>();
            try
            {
                foreach (var dir in Directory.GetDirectories(directory))
                {
                    files.AddRange(GetFilesFromDirectory(dir));
                }

            }
            catch (Exception)
            {
            }

            string pattern = "*.exe";
            files.AddRange(Directory.GetFiles(directory, pattern));

            return files.ToArray();
        }
        public static string[] GetInstalledApps2()
        {
            List<string> installedApps = new List<string>();

            var x = GetFilesFromDirectory(@"C:\Program Files");
            var y = GetFilesFromDirectory(@"C:\Program Files (x86)");

            Console.WriteLine("X64");
            foreach (var x2 in x)
            {
                Console.WriteLine(x2);
            }

            Console.WriteLine("X86");
            foreach (var y2 in y)
            {
                Console.WriteLine(y2);
            }

            return installedApps.OrderBy(n => n).ToArray();
        }
        public static Choices CreateNumberChoices(int min = 0, int max = 100)
        {
            string[] numbers = new string[101];
            for (int i = min; i <= max; i++)
            {
                numbers[i] = i.ToString();
            }
            Choices numbersChoices = new Choices(numbers);

            return numbersChoices;
        }
    }
}
