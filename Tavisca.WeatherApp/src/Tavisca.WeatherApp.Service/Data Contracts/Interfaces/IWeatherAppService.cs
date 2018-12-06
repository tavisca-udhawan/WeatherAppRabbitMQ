using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;
using Tavisca.WeatherApp.Service.Data_Contracts.Response;

namespace Tavisca.WeatherApp.Service.Data_Contracts.Interfaces
{
   public interface IWeatherAppService
    {
        WeatherReportResponse GetReportByCityName(CityNameRequest request);
        WeatherReportResponse GetReportByCityId(CityIdRequest request);
        WeatherReportResponse GetReportByZipCode(ZipCodeRequest request);
        WeatherReportResponse GetReportByGeoCode(GeoCodeRequest request);
        WeatherReportByCityNameInitResponse GetInitWeatherReportByCityName(CityNameRequest request);
        WeatherReportResultsResponse GetWeatherResult(WeatherReportByCityNameInitResponse request);
    }
}
