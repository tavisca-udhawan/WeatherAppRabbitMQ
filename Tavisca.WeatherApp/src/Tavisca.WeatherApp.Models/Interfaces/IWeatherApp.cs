using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Model.Models;

namespace Tavisca.WeatherApp.Model.Interfaces
{
   public interface IWeatherApp
    {
        WeatherReportResponseModel GetWeatherReport(WeatherReportRequestModel request);
    }
}
