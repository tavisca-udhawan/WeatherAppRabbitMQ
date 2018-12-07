using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tavisca.WeatherApp.Service.Data_Contracts.Response;
using Tavisca.WeatherApp.Models;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Tavisca.WeatherApp.Service.Data_Contracts.Interfaces;

namespace Tavisca.WeatherApp.Service.FileSystem
{
    public class ReadFile:IFileOperations
    {
        public void WriteToFile(WeatherReportResponse result, string id)
        {
            string json = File.ReadAllText(@"C:\Users\udhawan\Desktop\" + id + ".txt");
            WeatherReportResultsResponse jsonObj = JsonConvert.DeserializeObject<WeatherReportResultsResponse>(json);

            jsonObj.Status = "Success";
            jsonObj.WeatherDetails = result;
            string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
            File.WriteAllText(@"C:\Users\udhawan\Desktop\" + id + ".txt", output);
        }
        public WeatherReportResultsResponse GetFileResult(string id)
        {
            string json = File.ReadAllText(@"C:\Users\udhawan\Desktop\" + id + ".txt");
            WeatherReportResultsResponse jsonObj = JsonConvert.DeserializeObject<WeatherReportResultsResponse>(json);
            return jsonObj;
        }
    }
}