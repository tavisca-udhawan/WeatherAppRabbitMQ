using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Model;

namespace Tavisca.WeatherApp.Service.Data_Contracts.Model
{
    public class WeatherAppJsonRequest
    {
        public string SessionId { get; set; }
        public string Init_request { get; set; }
    }
}
