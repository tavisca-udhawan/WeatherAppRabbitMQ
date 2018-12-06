using System;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.WeatherApp.Service.Data_Contracts.Model
{
    public class WeatherAppJsonRequest
    {
        public string SessionId { get; set; }
        public string Init_request { get; set; }
    }
}
