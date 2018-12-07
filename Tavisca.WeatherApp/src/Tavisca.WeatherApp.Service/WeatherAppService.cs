using System;
using Tavisca.Common.Plugins.SessionStore;
using Tavisca.WeatherApp.Core;
using Tavisca.WeatherApp.Model;
using Tavisca.WeatherApp.Model.Interfaces;
using Tavisca.WeatherApp.Service.Data_Contracts;
using Tavisca.WeatherApp.Service.Data_Contracts.Interfaces;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;
using Tavisca.WeatherApp.Service.Data_Contracts.Response;
using Tavisca.WeatherApp.Service.FileSystem;
using Tavisca.WeatherApp.Service.Translators;
using Tavisca.WeatherApp.Service.Validators;
using Core = Tavisca.WeatherApp.Core;
namespace Tavisca.WeatherApp.Service
{
    public class WeatherAppService : IWeatherAppService
    {
        private readonly IWeatherApp _weatherApp;
        private readonly IFileOperations _FileOperation;
        private readonly ISessionStore _sessionStore;
        private readonly ISessionStore _queueBus;
      
        public WeatherAppService(IWeatherApp weatherApp,ISessionStore sessionStore,ISessionStore queueBus,IFileOperations fileOperations)
        {
            this._weatherApp = weatherApp;
            this._sessionStore = sessionStore;
            this._queueBus = queueBus;
            this._FileOperation = fileOperations;
        }
        public WeatherReportResponse GetReportByCityName(CityNameRequest request)
        {
            Validation.EnsureValid(request, new WeatherReportByCityNameRequestValidator());
            var requestModel = request.ToModel();
            //Here we will call the core service
            //Convert back the model to data contract
            var responseModel = _weatherApp.GetWeatherReport(requestModel);
            return responseModel.ToDataContract();
        }
        public WeatherReportResponse GetReportByCityId(CityIdRequest request)
        {
            Validation.EnsureValid(request, new WeatherReportByCityIdRequestValidator());
            var requestModel = request.ToModel();
            var responseModel = _weatherApp.GetWeatherReport(requestModel);
            return responseModel.ToDataContract();
        }
        public WeatherReportResponse GetReportByZipCode(ZipCodeRequest request)
        {
            Validation.EnsureValid(request, new WeatherReportByZipCodeRequestValidator());
            var requestModel = request.ToModel();
            var responseModel = _weatherApp.GetWeatherReport(requestModel);
            return responseModel.ToDataContract();
        }
        public WeatherReportResponse GetReportByGeoCode(GeoCodeRequest request)
        {
            Validation.EnsureValid(request.GeoCode, new GeoCodeValidator());
            var requestModel = request.ToModel();
            var responseModel = _weatherApp.GetWeatherReport(requestModel);
            return responseModel.ToDataContract();
        }

        public WeatherReportByCityNameInitResponse GetInitWeatherReportByCityName(CityNameRequest request)
        {
            Validation.EnsureValid(request, new WeatherReportByCityNameRequestValidator());
            string sessionId = Guid.NewGuid().ToString();
            WeatherReportByCityNameInitResponse data = _sessionStore.CreateFile(sessionId);
            WeatherReportByCityNameInitResponse QueueId=_queueBus.Enqueue(data,request);
            return new WeatherReportByCityNameInitResponse() {
               SessionId = QueueId.SessionId
           };
        }

        public WeatherReportResultsResponse GetWeatherResult(WeatherReportByCityNameInitResponse request)
        {
          
            WeatherReportResultsResponse result= _FileOperation.GetFileResult(request.SessionId);
            return new WeatherReportResultsResponse() {
                Status = result.Status,
                WeatherDetails = result.WeatherDetails
            };
        }

    }
}
