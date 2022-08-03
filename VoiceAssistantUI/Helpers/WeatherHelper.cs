using Newtonsoft.Json.Linq;
using System.Net.Http;

namespace VoiceAssistantUI.Helpers
{
    public static class WeatherHelper
    {
        public static readonly string ApiKey = @"d6bee5902ff44fec66206b7abfb6498b";

        public static (double Latitude, double Longitude) GetCoordinates(string city)
        {
            HttpClient client = new HttpClient();
            string url = @$"http://api.openweathermap.org/geo/1.0/direct?q={city}&limit=1&appid={ApiKey}";

            var getResult = client.GetAsync(url).Result;
            var contentResult = getResult.Content.ReadAsStringAsync().Result;
            var jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(contentResult);

            if (jsonResult.Count == 0)
                return (404, 404);

            var latitude = jsonResult[0]["lat"].Value<double>();
            var longitude = jsonResult[0]["lon"].Value<double>();

            return new(latitude, longitude);
        }
    }
}
