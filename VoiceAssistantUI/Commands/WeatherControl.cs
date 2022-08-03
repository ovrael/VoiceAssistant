using System;
using System.Threading.Tasks;
using VoiceAssistantUI.Helpers;
using Weather.NET;

namespace VoiceAssistantUI.Commands
{
    public static class WeatherControl
    {
        private enum WeatherControlType
        {
            Current,
            Forecast,
            AirPollution
        }

        public static bool IsAvailable { get; set; } = true;

        private static readonly WeatherClient? weatherClient = new WeatherClient("d6bee5902ff44fec66206b7abfb6498b");

        static WeatherControl()
        {
            if (weatherClient is null)
                IsAvailable = false;
        }

        private static Weather.NET.Enums.Language ConvertToWeatherLanguage(SpeechLanguage speechLanguage)
        {
            return speechLanguage switch
            {
                SpeechLanguage.English => Weather.NET.Enums.Language.English,
                SpeechLanguage.Polish => Weather.NET.Enums.Language.Polish,
                _ => Weather.NET.Enums.Language.English,
            };
        }

        private static void GetWeather(object cityName, WeatherControlType weatherType)
        {
            string city = cityName.ToString();
            if (city.ToString().Length == 0)
                return;

            //string oldDecimalSeparator = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            //System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = ".";

            string text = string.Empty;
            string weatherTypeText = "[WEATHER]";
            switch (weatherType)
            {
                case WeatherControlType.Current:
                    weatherTypeText = "[CURRENT WEATHER]";
                    text = CreateCurrentWeatherTextAsync(city).Result;
                    break;
                case WeatherControlType.Forecast:
                    weatherTypeText = "[WEATHER FORECAST]";
                    text = CreateForecastTextAsync(city).Result;
                    break;
                case WeatherControlType.AirPollution:
                    weatherTypeText = "[AIR POLLUTION]";
                    text = CreatePollutionTextAsync(city).Result;
                    break;

                default:
                    break;
            }

            if (text.Length == 0)
            {
                Assistant.WriteLog($"{weatherTypeText} ERROR - can't create text.", MessageType.Error);
                return;
            }

            Assistant.WriteOutput($"{weatherTypeText} {text}", MessageType.Success);
            if (Assistant.Data.UseSpeech)
                Speech.Speak(text);

            //System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator = oldDecimalSeparator;
        }

        public static void GetCurrentWeather(object city)
        {
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.Current));
        }

        public static void GetCurrentWeatherAndAirPollution(object city)
        {
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.Current));
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.AirPollution));
        }

        public static void GetCurrentAirPollution(object city)
        {
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.AirPollution));
        }

        public static void GetForecast(object city)
        {
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.Forecast));
        }

        private static async Task<string> CreateCurrentWeatherTextAsync(string city)
        {
            string text = string.Empty;
            var query = await weatherClient.GetCurrentWeatherAsync(city, Assistant.Data.WeatherMeasurement, ConvertToWeatherLanguage(Speech.Language));
            if (query is null)
                return text;

            switch (Speech.Language)
            {
                case SpeechLanguage.English:
                    text = $@"Now is {query.Weather[0].Description} and {query.Main.Temperature:F0} degrees.";
                    break;

                case SpeechLanguage.Polish:

                    string polishDegrees = "stopni";
                    int temperature = (int)query.Main.Temperature;
                    if (Assistant.Data.WeatherMeasurement == Weather.NET.Enums.Measurement.Metric)
                    {
                        int absTemperature = Math.Abs(temperature);
                        if (absTemperature == 1)
                            polishDegrees = "stopień";
                        else if (absTemperature < 5 || (absTemperature > 20 && absTemperature % 10 < 5 && absTemperature % 10 != 0))
                            polishDegrees = "stopnie";
                    }

                    text = $@"Aktualnie jest {query.Weather[0].Description} oraz {temperature} {polishDegrees}.";
                    break;

                default:
                    return text;
            }

            return text;
        }

        private static async Task<string> CreatePollutionTextAsync(string city)
        {
            string text = string.Empty;

            var geocode = WeatherHelper.GetCoordinates(city);
            if (geocode == (404, 404))
                return text;

            var query = await weatherClient.GetCurrentAirPollutionAsync(geocode.Latitude, geocode.Longitude);
            if (query is null)
                return text;

            switch (Speech.Language)
            {
                case SpeechLanguage.English:
                    text = $@"Air pollution is {query.AirDescriptions[0].AirQualityDescription} at level {query.AirDescriptions[0].Main.AirQuality} out of five.";
                    break;

                case SpeechLanguage.Polish:
                    string polishAirQuality = AirQualityToPolishDescription(query.AirDescriptions[0].Main.AirQuality);
                    text = $@"Zanieczyszczenie powietrza jest na poziomie {polishAirQuality} w skali {query.AirDescriptions[0].Main.AirQuality} na pięć.";
                    break;

                default:
                    return text;
            }

            return text;
        }

        private static string AirQualityToPolishDescription(int airQuality)
        {
            return airQuality switch
            {
                1 => "dobrym",
                2 => "umiarkowanym",
                3 => "średnim",
                4 => "słabym",
                5 => "bardzo słabym",
                _ => "brak danych",
            };
        }

        private static async Task<string> CreateForecastTextAsync(string city)
        {
            string text = string.Empty;

            var geocode = WeatherHelper.GetCoordinates(city);
            if (geocode == (404, 404))
                return text;

            const int timestamps = 4; // 1 timestamp = 3h weather forecast
            var query = await weatherClient.GetForecastAsync(city, timestamps, Assistant.Data.WeatherMeasurement, ConvertToWeatherLanguage(Speech.Language));
            if (query is null)
                return text;

            string forecastText = string.Empty;
            double averageTemperature = 0;

            for (int i = 0; i < query.Count; i++)
            {
                averageTemperature += query[i].Main.Temperature;
                forecastText += query[i].Weather[0].Description;
                if (i < query.Count - 1)
                    forecastText += ", ";
            }

            averageTemperature /= query.Count;

            switch (Speech.Language)
            {
                case SpeechLanguage.English:
                    text = $"For next 12 hours weather will be {forecastText} with average temperature at {averageTemperature:F0} degrees.";
                    break;

                case SpeechLanguage.Polish:

                    string polishDegrees = "stopni";
                    int temperature = (int)averageTemperature;
                    if (Assistant.Data.WeatherMeasurement == Weather.NET.Enums.Measurement.Metric)
                    {
                        int absTemperature = Math.Abs(temperature);
                        if (absTemperature == 1)
                            polishDegrees = "stopień";
                        else if (absTemperature < 5 || (absTemperature > 20 && absTemperature % 10 < 5 && absTemperature % 10 != 0))
                            polishDegrees = "stopnie";
                    }

                    text = $"Przez następne 12 godzin pogoda będzie {forecastText} ze średnią temperaturą {temperature} {polishDegrees}.";
                    break;

                default:
                    return text;
            }

            return text;
        }
    }
}
