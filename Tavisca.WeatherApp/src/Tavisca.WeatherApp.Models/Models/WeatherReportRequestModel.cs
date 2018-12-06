using System;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.WeatherApp.Model
{
    public class WeatherReportRequestModel
    {
        public string cityName { get; set; }
        public int cityId { get; set; }
        public string zipCode { get; set; }
        public GeoCode geoCode { get; set; }
    }
}
