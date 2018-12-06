using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;

namespace Tavisca.WeatherApp.Models.Interfaces
{
    public interface IFileInit
    {
        WeatherReportByCityNameInitResponse CreateInitFile(string id);
    }
}
