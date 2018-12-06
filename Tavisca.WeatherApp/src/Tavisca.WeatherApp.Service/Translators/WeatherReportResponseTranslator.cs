using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tavisca.WeatherApp.Model.Models;
using Tavisca.WeatherApp.Service.Data_Contracts.Response;

namespace Tavisca.WeatherApp.Service.Translators
{
    public static class WeatherReportResponseTranslator
    {
        public static WeatherReportResponse ToDataContract(this WeatherReportResponseModel responseModel)
        {
            return new WeatherReportResponse
            {
                Id = responseModel.Id,
                Name = responseModel.Name,
                Coordinates = new Data_Contracts.Model.GeoCode
                {
                    Latitude = responseModel.Coordinates.Latitude,
                    Longitude = responseModel.Coordinates.Longitude
                },
                Main = new Data_Contracts.Model.Main()
                {
                    Temp = responseModel.Main.Temp,
                    Pressure = responseModel.Main.Pressure,
                    Humidity = responseModel.Main.Humidity,
                    MinTemperature = responseModel.Main.MinTemperature,
                    MaxTemperature = responseModel.Main.MaxTemperature
                },
                Wind = new Data_Contracts.Model.Wind()
                {
                    Speed = responseModel.Wind.Speed,
                    Degree = responseModel.Wind.Degree
                },
                AdditionalInfo = new Data_Contracts.Model.AdditionalInfo()
                {
                    CountryCode = responseModel.AdditionalInfo.CountryCode,
                    Cloudiness = responseModel.AdditionalInfo.Cloudiness,
                    Visibility = responseModel.AdditionalInfo.Visibility,
                    Sunrise = responseModel.AdditionalInfo.Sunrise,
                    Sunset = responseModel.AdditionalInfo.Sunset

                },
                TimeSpan= responseModel.TimeSpan,
                Weather = responseModel.Weather.Select(x => new Data_Contracts.Model.Weather()
                {
                    Type = x.Type,
                    Description = x.Description
                }).ToList()


            };
        }
    }
}
