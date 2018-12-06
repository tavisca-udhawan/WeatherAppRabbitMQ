using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;

namespace Tavisca.WeatherApp.Service.Data_Contracts.Interfaces
{
    public interface IQueue
    {
        WeatherReportByCityNameInitResponse Enqueue(WeatherReportByCityNameInitResponse data, CityNameRequest request);
    }
}
