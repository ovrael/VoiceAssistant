using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net.Http;

namespace VoiceAssistantUI.Helpers
{
    public static class WeatherHelper
    {
        public static (double Latitude, double Longitude) GetCoordinates(string city)
        {
            HttpClient client = new HttpClient();
            string url = @$"http://api.positionstack.com/v1/forward?access_key=70b22fe4487feda6b513c1423c708df7&query={city}&output=json";

            var getResult = client.GetAsync(url).Result;
            var contentResult = getResult.Content.ReadAsStringAsync().Result;
            var jsonResult = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(contentResult);

            var data = jsonResult["data"];
            if (data.Count() == 0)
                return new(404, 404);

            var latitude = data[0]["latitude"].Value<double>();
            var longitude = data[0]["longitude"].Value<double>();

            return new(latitude, longitude);
        }

        public static double KelvinToCelsius(double kelvins)
        {
            return kelvins - 273.15;
        }
    }
}
