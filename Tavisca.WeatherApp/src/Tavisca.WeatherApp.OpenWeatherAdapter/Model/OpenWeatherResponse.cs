using System;
using System.Collections.Generic;
using System.Text;

namespace Tavisca.WeatherApp.OpenWeatherAdapter.Model
{
    public class OpenWeatherResponse
    {
        public Coord coord { get; set; }
        public List<Weather> weather { get; set; }
        public string @base { get; set; }
        public Main main { get; set; }
        public decimal visibility { get; set; }
        public Wind wind { get; set; }
        public Clouds clouds { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public decimal cod { get; set; }
    }
    public class Coord
    {
        public double lon { get; set; }
        public double lat { get; set; }
    }
    public class Weather
    {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }
    public class Main
    {
        public double temp { get; set; }
        public decimal pressure { get; set; }
        public decimal humidity { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
    }
    public class Wind
    {
        public double speed { get; set; }
        public decimal deg { get; set; }
    }
    public class Clouds
    {
        public decimal all { get; set; }
    }
    public class Sys
    {
        public decimal type { get; set; }
        public int id { get; set; }
        public double message { get; set; }
        public string country { get; set; }
        public int sunrise { get; set; }
        public int sunset { get; set; }
    }
}
