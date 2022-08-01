using Weather.NET;
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

        public static void GetWeather(object city)
        {
            if (city.ToString().Length == 0)
                return;

            string oldDecimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";

            var query = weatherClient.GetCurrentWeather(city.ToString());

            if (query is null)
                return;

            System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = oldDecimalSeparator;
        }

        public static void GetWeatherAndAirPollution(object city)
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

        public static void GetAirPollution(object city)
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

        private static string CreateWeatherText(WeatherModel weather)
        {
            string text = string.Empty;


            return text;
        }

        private static string CreateEnglishText(WeatherModel weather)
        {
            string text = string.Empty;

            Clouds x = weather.Clouds;



            return text;
        }

        private static string CreatePolishText(WeatherModel weather)
        {
            string text = string.Empty;


            return text;
        }
    }
}
