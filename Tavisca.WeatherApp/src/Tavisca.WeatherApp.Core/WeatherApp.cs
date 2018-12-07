using System;
using Tavisca.WeatherApp.Model;
using Tavisca.WeatherApp.Model.Interfaces;
using Tavisca.WeatherApp.Model.Models;
using Tavisca.WeatherApp.Plugins;

namespace Tavisca.WeatherApp.Core
{
    public class WeatherApp : IWeatherApp
    {
            private readonly IWeatherDataProvider _weatherDataProvider;
            public WeatherApp()
            {
                _weatherDataProvider = new WeatherReportDataProvider();
            }
        public WeatherReportResponseModel GetWeatherReport(WeatherReportRequestModel request)
            {
                return
                    _weatherDataProvider.GetWeatherReport(request);
            }
        }
}
