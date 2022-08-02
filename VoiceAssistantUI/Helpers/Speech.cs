using System.Linq;
using System.Speech.Synthesis;

namespace VoiceAssistantUI.Helpers
{
    public enum SpeechLanguage
    {
        English,
        Polish
    }

    public static class Speech
    {
        private static readonly SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();
        public static SpeechLanguage Language { get; private set; } = SpeechLanguage.English;

        static Speech()
        {
            if (speechSynthesizer.Voice.Culture.Name.Contains("pl-PL"))
                Language = SpeechLanguage.Polish;
        }

        public static void SetVoice(string name)
        {
            if (!GetVoiceNamesWithCulture().Contains(name))
                return;

            if (name.Contains("pl-PL"))
                Language = SpeechLanguage.Polish;
            else
                Language = SpeechLanguage.English;

            int bracketIndex = name.IndexOf('[');
            if (bracketIndex >= 0)
            {
                name = name.Substring(0, bracketIndex - 1);
            }

            speechSynthesizer.SelectVoice(name);
        }

        public static string[] GetVoiceNamesWithCulture()
        {
            return speechSynthesizer
                .GetInstalledVoices()
                .Select(v => v.VoiceInfo.Name + " [" + v.VoiceInfo.Culture.Name + "]")
                .ToArray();
        }

        public static void Speak(string text)
        {
            speechSynthesizer.SpeakAsync(text);
        }
    }
}
