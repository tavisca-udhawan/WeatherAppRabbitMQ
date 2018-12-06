using System;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.WeatherApp.Model.Models
{
    public class WeatherReportResponseModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GeoCode Coordinates { get; set; }
        public List<Weather> Weather { get; set; }
        public Main Main { get; set; }
        public Wind Wind { get; set; }
        public DateTime TimeSpan { get; set; }
        public AdditionalInfo AdditionalInfo { get; set; }
    }
}
