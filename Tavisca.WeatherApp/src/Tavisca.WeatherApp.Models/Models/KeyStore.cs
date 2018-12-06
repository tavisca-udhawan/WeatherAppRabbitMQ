using System;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.WeatherApp.Models.Models
{
    public static class KeyStore
    {
        public const string ApplicationName = "weather_app";
        public static class SectionNames
        {
            public const string AppSettings = "app_settings";
        }
        public static class AppSettingsKeys
        {
            public const string OpenWeatherAppSettings = "open_weather_settings";
        }
    }
}
