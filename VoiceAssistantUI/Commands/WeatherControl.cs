using System;
using System.Linq;
using System.Threading.Tasks;
using VoiceAssistantUI.Helpers;
using Weather.NET;
using Weather.NET.Models.WeatherModel;

namespace VoiceAssistantUI.Commands
{
    public static class WeatherControl
    {
        private enum WeatherControlType
        {
            Current,
            Forecast,
            TommorowForecast,
            AirPollution
        }

        public static bool IsAvailable { get; set; } = true;

        private static readonly WeatherClient? weatherClient = new WeatherClient(WeatherHelper.ApiKey);

        static WeatherControl()
        {
            if (weatherClient is null)
                IsAvailable = false;
        }


        #region Public commands

        public static void GetCurrentWeather(object city)
        {
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.Current));
        }

        public static void GetCurrentWeatherInMyCity()
        {
            GetCurrentWeather(Assistant.Data.MyCity);
        }

        public static void GetCurrentWeatherAndAirPollution(object city)
        {
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.Current));
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.AirPollution));
        }

        public static void GetCurrentWeatherAndAirPollutionInMyCity()
        {
            GetCurrentWeatherAndAirPollution(Assistant.Data.MyCity);
        }

        public static void GetCurrentAirPollution(object city)
        {
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.AirPollution));
        }

        public static void GetCurrentAirPollutionInMyCity()
        {
            GetCurrentAirPollution(Assistant.Data.MyCity);
        }

        public static void GetForecast(object city)
        {
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.Forecast));
        }

        public static void GetForecastInMyCity()
        {
            GetForecast(Assistant.Data.MyCity);
        }

        public static void GetTommorowWeather(object city)
        {
            Task.Factory.StartNew(() => GetWeather(city, WeatherControlType.TommorowForecast));
        }

        public static void GetTommorowWeatherInMyCity()
        {
            GetTommorowWeather(Assistant.Data.MyCity);
        }
        #endregion

        #region Privates
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

                case WeatherControlType.TommorowForecast:
                    weatherTypeText = "[TOMMOROW FORECAST]";
                    text = CreateTommorowForecastTextAsync(city).Result;
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

        private static async Task<string> CreateCurrentWeatherTextAsync(string city)
        {
            string text = string.Empty;

            var geo = WeatherHelper.GetCoordinates(city);
            if (geo == (404, 404))
                return text;

            var query = await weatherClient.GetCurrentWeatherAsync(geo.Latitude, geo.Longitude, Assistant.Data.WeatherMeasurement, ConvertToWeatherLanguage(Speech.Language));
            if (query is null)
                return text;

            switch (Speech.Language)
            {
                case SpeechLanguage.English:
                    text = $"Now is {query.Weather[0].Description}" +
                        $" and {(int)query.Main.Temperature} degrees in {city}.";
                    break;

                case SpeechLanguage.Polish:
                    text = $"Aktualnie jest {query.Weather[0].Description}" +
                        $" oraz {CreatePolishTemperatureText((int)query.Main.Temperature)}" +
                        $" w miejscowości {city}.";
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
                    text = $"Air pollution is {query.AirDescriptions[0].AirQualityDescription}" +
                        $" at level {query.AirDescriptions[0].Main.AirQuality} out of five in {city}.";
                    break;

                case SpeechLanguage.Polish:
                    string polishAirQuality = AirQualityToPolishDescription(query.AirDescriptions[0].Main.AirQuality);
                    text = $"Zanieczyszczenie powietrza jest na poziomie {polishAirQuality} " +
                        $"w skali {query.AirDescriptions[0].Main.AirQuality} na pięć w miejscowości {city}..";
                    break;

                default:
                    return text;
            }

            return text;
        }
        private static async Task<string> CreateForecastTextAsync(string city)
        {
            string text = string.Empty;

            var geo = WeatherHelper.GetCoordinates(city);
            if (geo == (404, 404))
                return text;

            const int timestamps = 4; // 1 timestamp = 3h weather forecast
            var query = await weatherClient.GetForecastAsync(geo.Latitude, geo.Longitude, timestamps, Assistant.Data.WeatherMeasurement, ConvertToWeatherLanguage(Speech.Language));
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
                    text = $"For next 12 hours weather will be {forecastText} " +
                        $"with average temperature at {averageTemperature:F0} degrees in {city}.";
                    break;

                case SpeechLanguage.Polish:
                    text = $"Przez następne 12 godzin pogoda będzie {forecastText} " +
                        $"ze średnią temperaturą {CreatePolishTemperatureText((int)averageTemperature)} w miejscowości {city}.";
                    break;

                default:
                    return text;
            }

            return text;
        }
        private static async Task<string> CreateTommorowForecastTextAsync(string city)
        {
            string text = string.Empty;

            var geo = WeatherHelper.GetCoordinates(city);
            if (geo == (404, 404))
                return text;

            int timestamps = ComputeTimestampsForTommorowForecast();
            var query = await weatherClient.GetForecastAsync(geo.Latitude, geo.Longitude, timestamps, Assistant.Data.WeatherMeasurement, ConvertToWeatherLanguage(Speech.Language));
            if (query is null)
                return text;

            WeatherModel morning = query.Where(q => q.AnalysisDate.Hour > 10 && q.AnalysisDate.Hour <= 13).Last();
            WeatherModel evening = query.Where(q => q.AnalysisDate.Hour > 16 && q.AnalysisDate.Hour <= 19).Last();

            switch (Speech.Language)
            {
                case SpeechLanguage.English:
                    text = $"Tommorow at twelve a m will be {morning.Weather[0].Description} and {(int)morning.Main.Temperature} degrees. " +
                        $"Six hours later at six pm will be {evening.Weather[0].Description} and {(int)evening.Main.Temperature} degrees in {city}.";
                    break;

                case SpeechLanguage.Polish:
                    text = $"Jutro o dwunastej będzie {morning.Weather[0].Description} oraz {CreatePolishTemperatureText((int)morning.Main.Temperature)}. " +
                        $"Sześć godzin później o osiemnastej będzie {evening.Weather[0].Description} oraz {CreatePolishTemperatureText((int)evening.Main.Temperature)}" +
                        $" w miejscowości {city}.";
                    break;

                default:
                    return text;
            }

            return text;
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
        private static string CreatePolishTemperatureText(int temperature)
        {
            string polishDegrees = "stopni";

            int absTemperature = Math.Abs(temperature);
            if (absTemperature == 1)
                polishDegrees = "stopień";
            else if (absTemperature < 5 || (absTemperature > 20 && absTemperature % 10 < 5 && absTemperature % 10 != 0))
                polishDegrees = "stopnie";

            return $"{temperature} {polishDegrees}";
        }
        private static int ComputeTimestampsForTommorowForecast()
        {
            const int timestampHours = 3;
            int timestamps = 0;
            const int nextSixHoursForecastTimestamps = 2;

            DateTime now = DateTime.Now;
            DateTime targetDate = new DateTime(now.Year, now.Month, now.Day + 1, 10, 00, 00);

            if (now.Hour <= 4)
                return 8;
            else
                return 16;

            //do
            //{
            //    now = now.AddHours(timestampHours);
            //    timestamps++;

            //} while (now <= targetDate);

            //timestamps += nextSixHoursForecastTimestamps;

            //return timestamps;
        }
        #endregion
    }
}
