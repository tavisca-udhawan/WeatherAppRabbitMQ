using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;
using Tavisca.WeatherApp.Service.Data_Contracts.Response;

namespace Tavisca.WeatherApp.Service.Data_Contracts.Interfaces
{
    public interface IFileOperations
    {
        void ReadFromFile(WeatherReportResponse result, string id);
        WeatherReportResultsResponse GetFileResult(string id);
    }
}
