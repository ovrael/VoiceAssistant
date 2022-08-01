using OpenWeatherAPI;

namespace VoiceAssistantUI.Commands
{
    public static class WeatherControl
    {
        public static bool IsAvailable { get; set; } = true;

        private static readonly OpenWeatherApiClient? weatherClient = new OpenWeatherApiClient("d6bee5902ff44fec66206b7abfb6498b");

        static WeatherControl()
        {
            if (weatherClient is null)
                IsAvailable = false;
        }

        public static void GetWeather(object city)
        {
            string oldDecimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";

            var query = weatherClient.QueryAsync(city.ToString()).Result;

            string text = "";
            //Assistan


            System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = oldDecimalSeparator;
        }
    }
}
