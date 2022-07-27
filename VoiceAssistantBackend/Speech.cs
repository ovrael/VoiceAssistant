using System.Speech.Synthesis;

namespace VoiceAssistantBackend
{
    public static class Speech
    {
        private static readonly SpeechSynthesizer speechSynthesizer = new SpeechSynthesizer();

        public static void SetVoice(string name)
        {
            if (!GetVoiceNamesWithCulture().Contains(name))
                return;

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
