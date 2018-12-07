using System;
using Tavisca.Platform.Common.Models;
using Tavisca.WeatherApp.Model;
using Tavisca.WeatherApp.Model.Interfaces;
using Tavisca.WeatherApp.Model.Models;
using Tavisca.WeatherApp.Models.Logging;
using Tavisca.WeatherApp.OpenWeatherAdapter.Model;
using Tavisca.WeatherApp.OpenWeatherAdapter.Translators;

namespace Tavisca.WeatherApp.OpenWeatherAdapter
{
    public class OpenWeatherAdapter : WeatherReportBase, IWeatherAdapter
    {
        private readonly OpenWeatherAppSvcSettings _settings;
        public OpenWeatherAdapter(OpenWeatherAppSvcSettings settings)
        {
            _settings = settings;
        }
        public WeatherReportResponseModel GetWeatherReport(WeatherReportRequestModel requestModel)
        {
            if (_settings == null)
                throw new BaseApplicationException(ErrorMessages.MandatoryFieldMissing("OpenWeatherSvcSettings"), FaultCodes.MandatoryFieldMissing);
            var url = GenerateUrl(requestModel);
            var responseObj = Execute<OpenWeatherResponse>(url);
            Loggers.AddTrace("Getting Response from third-party Api");
            return responseObj.ToModel();
        }
        private string GenerateUrl(WeatherReportRequestModel requestModel)
        {
            string url = null;
            if(requestModel.cityName!=null) 
                url=$"{_settings.Url}?q={requestModel.cityName}&APPID={_settings.ApiKey}";
            else if (requestModel.cityId!=0)
                url= $"{_settings.Url}?id={requestModel.cityId}&APPID={_settings.ApiKey}";
            else if (requestModel.zipCode != null)
                url = $"{_settings.Url}?zip={requestModel.zipCode}&APPID={_settings.ApiKey}";
            else if (requestModel.geoCode.Latitude != null && requestModel.geoCode.Longitude != null)
                url = $"{_settings.Url}?lat={requestModel.geoCode.Latitude}&lon={requestModel.geoCode.Longitude}&APPID={_settings.ApiKey}";
            Loggers.AddTrace("generating URL");
            return url;
        }
    }
}
