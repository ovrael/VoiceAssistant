using VoiceAssistantUI.Helpers;
using Weather.NET;
using Weather.NET.Models.PollutionModel;
using Weather.NET.Models.WeatherModel;

namespace VoiceAssistantUI.Commands
{
    public static class WeatherControl
    {
        public static bool IsAvailable { get; set; } = true;

        private static readonly WeatherClient? weatherClient = new WeatherClient("d6bee5902ff44fec66206b7abfb6498b");

        static WeatherControl()
        {
            if (weatherClient is null)
                IsAvailable = false;
        }

        public static void GetCurrentWeather(object city)
        {
            if (city.ToString().Length == 0)
                return;

            string oldDecimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";

            switch (Speech.Language)
            {
                case SpeechLanguage.English:
                    break;
                case SpeechLanguage.Polish:
                    break;
                default:
                    break;
            }

            // THIS CAN CREATE POLISH TEXT TOO!!
            var query = weatherClient.GetCurrentWeather();
            if (query is null)
                return;

            System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = oldDecimalSeparator;
        }

        public static void GetCurrentWeatherAndAirPollution(object city)
        {
            if (city.ToString().Length == 0)
                return;

            string oldDecimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";

            var weatherQuery = weatherClient.GetCurrentWeather(city.ToString());
            if (weatherQuery is null)
                return;

            var cityGeolocalization = Helpers.WeatherHelper.GetCoordinates(city.ToString());
            if (cityGeolocalization.Longitude == 404)
                return;

            var pollutionQuery = weatherClient.GetCurrentAirPollution(cityGeolocalization.Latitude, cityGeolocalization.Longitude);
            if (pollutionQuery is null)
                return;



            System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = oldDecimalSeparator;
        }

        public static void GetCurrentAirPollution(object city)
        {
            if (city.ToString().Length == 0)
                return;

            string oldDecimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";

            var cityGeolocalization = Helpers.WeatherHelper.GetCoordinates(city.ToString());
            if (cityGeolocalization.Longitude == 404)
                return;

            var pollutionQuery = weatherClient.GetCurrentAirPollution(cityGeolocalization.Latitude, cityGeolocalization.Longitude);
            if (pollutionQuery is null)
                return;



            System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = oldDecimalSeparator;
        }

        private static string GetCurrentPolishWeather(string city)
        {
            string text = string.Empty;



            return text;
        }

        private static string CreateCurrentEnglishWeatherText(WeatherModel weather)
        {
            string text = string.Empty;

            weather.Weather[0].Description;



            return text;
        }

        private static string CreatePolishWeatherText(WeatherModel weather)
        {
            string text = string.Empty;


            return text;
        }

        private static string CreateAirPollutionText(PollutionModel pollution)
        {
            string text = string.Empty;


            return text;
        }

        private static string CreateEnglishAirPollutionText(PollutionModel pollution)
        {
            string text = string.Empty;



            return text;
        }

        private static string CreatePolishAirPoluttionText(PollutionModel pollution)
        {
            string text = string.Empty;


            return text;
        }
    }
}
