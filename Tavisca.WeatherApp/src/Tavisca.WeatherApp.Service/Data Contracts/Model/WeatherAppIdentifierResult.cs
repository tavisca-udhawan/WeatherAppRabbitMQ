using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Service.Data_Contracts.Response;

namespace Tavisca.WeatherApp.Service.Data_Contracts.Model
{
    public class WeatherReportResultsResponse
    {
        public string Status { get; set; }
        public WeatherReportResponse WeatherDetails{get;set;}
    }
}
