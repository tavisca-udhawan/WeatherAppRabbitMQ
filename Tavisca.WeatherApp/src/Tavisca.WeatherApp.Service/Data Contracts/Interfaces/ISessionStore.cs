using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;

namespace Tavisca.WeatherApp.Service.Data_Contracts.Interfaces
{
    public interface ISessionStore
    {
        WeatherReportByCityNameInitResponse CreateFile(string id);
        WeatherReportByCityNameInitResponse Enqueue(WeatherReportByCityNameInitResponse data, CityNameRequest request);
    }
}
