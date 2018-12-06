using System;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.WeatherApp.Service.Data_Contracts.Model
{
    public class AdditionalInfo
    {
        public string CountryCode { get; set; }
        public DateTime Sunrise { get; set; }
        public DateTime Sunset { get; set; }
        public decimal Cloudiness { get; set; }
        public decimal Visibility { get; set; }
    }
}
