using System;
using System.Collections.Generic;
using System.Text;
using Tavisca.WeatherApp.Model;
using Tavisca.WeatherApp.Service.Data_Contracts;

namespace Tavisca.WeatherApp.Service.Translators
{
    public static class WeatherAppRequestTranslator
    {
        public static WeatherReportRequestModel ToModel(this CityNameRequest request)
        {
            return new WeatherReportRequestModel
            {
                cityName = request.cityName
            };
        }

      

        public static WeatherReportRequestModel ToModel(this CityIdRequest request)
        {
            return new WeatherReportRequestModel
            {
                cityId = request.cityId
            };
        }
        public static WeatherReportRequestModel ToModel(this ZipCodeRequest request)
        {
            return new WeatherReportRequestModel
            {
                zipCode = request.zipCode
            };
        }
        public static WeatherReportRequestModel ToModel(this GeoCodeRequest request)
        {
            return new WeatherReportRequestModel
            {
                geoCode = new Model.GeoCode
                {
                    Latitude = request.GeoCode?.Latitude,
                    Longitude = request.GeoCode?.Longitude
                }
            };
        }

    }
}

