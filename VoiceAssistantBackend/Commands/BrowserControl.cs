using System.Diagnostics;

namespace VoiceAssistantBackend.Commands
{
    public static class BrowserControl
    {
        public static bool IsAvailable { get; set; } = true;

        public static void OpenBrowser()
        {
            Process.Start("explorer.exe", "http://google.pl");
        }

        public static void OpenWeatherInBrowser()
        {
            string weatherURL = @"https://www.google.com/search?q=weather&sxsrf=ALiCzsbsN4GxHmtXtYKobilFIC3qUI8xAQ%3A1658909409356&source=hp&ei=4fLgYqqsE5S6gAbB3oSIDQ&iflsig=AJiK0e8AAAAAYuEA8eCk2OmP5t7IrLAVm5vzu4BnWf9G&ved=0ahUKEwjqoLqQz5j5AhUUHcAKHUEvAdEQ4dUDCAY&uact=5&oq=weather&gs_lcp=Cgdnd3Mtd2l6EAMyCggjEMkDECcQnQIyBQgAEJIDMgUIABCSAzIFCAAQywEyCwgAEIAEELEDEIMBMgUIABDLATIECAAQQzILCAAQgAQQsQMQgwEyBQgAEMsBMgsIABCABBCxAxCDAToECCMQJzoRCC4QgAQQsQMQgwEQxwEQ0QM6BwgjECcQnQI6BQgAEIAEOgsILhCABBCxAxCDAVAAWJAFYPIFaABwAHgAgAFsiAHSBJIBAzYuMZgBAKABAQ&sclient=gws-wiz";
            OpenURL(weatherURL);
        }

        public static void OpenCityWeatherInBrowser(object city)
        {
            string weatherURL = @$"https://www.google.com/search?client=opera-gx&q=weather+{city}&sourceid=opera&ie=UTF-8&oe=UTF-8";
            OpenURL(weatherURL);
        }

        public static void OpenSite(object site)
        {
            if (site is null || site.ToString().Length == 0)
                return;

            string url = @$"http://{site}.com";
            OpenURL(url);
        }


        private static void OpenURL(string url)
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
