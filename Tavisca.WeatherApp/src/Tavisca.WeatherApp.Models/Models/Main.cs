using System;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.WeatherApp.Model.Models
{
    public class Main
    {
        public double Temp { get; set; }
        public decimal Pressure { get; set; }
        public decimal Humidity { get; set; }
        public double MinTemperature { get; set; }
        public double MaxTemperature { get; set; }
    }
}
