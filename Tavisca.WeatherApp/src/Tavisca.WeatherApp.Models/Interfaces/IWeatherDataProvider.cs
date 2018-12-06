using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Model.Models;

namespace Tavisca.WeatherApp.Model.Interfaces
{
    public interface IWeatherDataProvider
    {
        WeatherReportResponseModel GetWeatherReport(WeatherReportRequestModel requestModel);
    }
}
