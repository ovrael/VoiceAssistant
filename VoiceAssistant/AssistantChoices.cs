﻿using Microsoft.Win32;
using System.Speech.Recognition;

namespace VoiceAssistant
{
    public static class AssistantChoices
    {
        public static readonly Choices Initiaton = CreateInitiaton();
        public static readonly Choices Show = BuildChoices("show", "print", "display");
        public static readonly Choices Apps = BuildChoices("applications", "apps", "programs");
        public static readonly Choices Installed = BuildChoices("available", "installed", "all");
        public static readonly Choices InstalledApps = CreateInstalledApps();
        public static readonly Choices Open = BuildChoices("open", "run");
        public static readonly Choices MediaControl = BuildChoices("play", "pause", "stop");
        public static readonly Choices MediaType = BuildChoices("music", "video", "media");
        public static readonly Choices PC_Control = BuildChoices("shutdown", "reboot", "restart");

        private static Choices CreateInitiaton()
        {
            Choices choices = new Choices();

            choices.Add("Hi " + Assistant.AssistantName);
            choices.Add("Ok " + Assistant.AssistantName);
            choices.Add("Hey " + Assistant.AssistantName);
            choices.Add("Okay " + Assistant.AssistantName);
            choices.Add("Hello " + Assistant.AssistantName);

            return choices;
        }

        private static Choices CreateInstalledApps()
        {
            Choices choices = new Choices();

            foreach (var app in Helpers.InstalledApps)
            {
                string[] words = app.Split(' ');
                string appPart = string.Empty;

                for (int i = 0; i < words.Length; i++)
                {
                    if (words[i] != string.Empty)
                    {
                        if (i > 0)
                            appPart += ' ';
                        appPart += words[i];
                        choices.Add(appPart);
                    }
                }
            }

            return choices;
        }

        private static Choices BuildChoices(params string[] choiceCollection)
        {
            Choices choices = new Choices();

            foreach (string choice in choiceCollection)
            {
                choices.Add(choice);
            }


            return choices;
        }
    }
}