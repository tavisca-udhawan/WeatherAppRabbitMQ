using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;

namespace Tavisca.WeatherApp.Core
{
    public class FileSystem
    {
        public WeatherReportByCityNameInitResponse CreateFile(string id)
        {
            WeatherReportResultsResponse init_result = new WeatherReportResultsResponse();
            init_result.Status="In Progress";
            string JSONresult = JsonConvert.SerializeObject(init_result);
            var path = @"C:\Users\udhawan\Desktop\"+id+".txt";
            using (var tw = new StreamWriter(path, true))
            {
                tw.WriteLine(JSONresult.ToString());
                tw.Close();
            }
            return new WeatherReportByCityNameInitResponse()
            {
                SessionId = id
            };
        }
    }
}
