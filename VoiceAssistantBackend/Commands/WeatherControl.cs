using OpenWeatherAPI;

namespace VoiceAssistantBackend.Commands
{
    public static class WeatherControl
    {
        public static bool IsAvailable { get; set; } = true;

        private static readonly OpenWeatherApiClient? weatherClient = new OpenWeatherApiClient("d6bee5902ff44fec66206b7abfb6498b");

        static WeatherControl()
        {
            if (weatherClient == null)
                IsAvailable = false;

            Console.WriteLine("test");
        }
    }
}
