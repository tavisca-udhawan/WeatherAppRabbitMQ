using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Tavisca.WeatherApp.OpenWeatherAdapter
{
    public class WeatherReportBase
    {
        private readonly WebClient _webClient;
        public WeatherReportBase()
        {
            _webClient = new WebClient();
        }
        public T Execute<T>(string url)
        {
            //try
            //{
            //    var responseString = _webClient.DownloadString(url);
            //    return JsonConvert.DeserializeObject<T>(responseString);
            //}
            //catch (WebException)
            //{
            //    return default(T);
            //}
            var responseString = _webClient.DownloadString(url);
            return JsonConvert.DeserializeObject<T>(responseString);
        }
    }
}
