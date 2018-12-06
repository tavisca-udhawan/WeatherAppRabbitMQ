using System;
using Tavisca.WeatherApp.Core;
using Tavisca.WeatherApp.Model.Interfaces;
using Tavisca.WeatherApp.Service.Data_Contracts;
using Tavisca.WeatherApp.Service.Data_Contracts.Interfaces;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;
using Tavisca.WeatherApp.Service.Data_Contracts.Response;
using Tavisca.WeatherApp.Service.Queues;
using Tavisca.WeatherApp.Service.Translators;
using Tavisca.WeatherApp.Service.Validators;
using Core = Tavisca.WeatherApp.Core;
namespace Tavisca.WeatherApp.Service
{
    public class WeatherAppService : IWeatherAppService
    {
        private readonly IWeatherApp weatherApp;

        //private readonly ISessionStore sessionStore;

        //private readonly IQueue queue;
        public WeatherAppService()
        {
            weatherApp = new Core.WeatherApp();
        }
        public WeatherReportResponse GetReportByCityName(CityNameRequest request)
        {
            Validation.EnsureValid(request, new WeatherReportByCityNameRequestValidator());
            var requestModel = request.ToModel();
            //Here we will call the core service
            //Convert back the model to data contract
            var responseModel = weatherApp.GetWeatherReport(requestModel);
            return responseModel.ToDataContract();
        }
        public WeatherReportResponse GetReportByCityId(CityIdRequest request)
        {
            Validation.EnsureValid(request, new WeatherReportByCityIdRequestValidator());
            var requestModel = request.ToModel();
            //Here we will call the core service
            //Convert back the model to data contract
            var responseModel = weatherApp.GetWeatherReport(requestModel);
            return responseModel.ToDataContract();
        }
        public WeatherReportResponse GetReportByZipCode(ZipCodeRequest request)
        {
            Validation.EnsureValid(request, new WeatherReportByZipCodeRequestValidator());
            var requestModel = request.ToModel();
            //Here we will call the core service
            //Convert back the model to data contract
            var responseModel = weatherApp.GetWeatherReport(requestModel);
            return responseModel.ToDataContract();
        }
        public WeatherReportResponse GetReportByGeoCode(GeoCodeRequest request)
        {
            Validation.EnsureValid(request.GeoCode, new GeoCodeValidator());
            var requestModel = request.ToModel();
            //Here we will call the core service
            //Convert back the model to data contract
            var responseModel = weatherApp.GetWeatherReport(requestModel);
            return responseModel.ToDataContract();
        }

        public WeatherReportByCityNameInitResponse GetInitWeatherReportByCityName(CityNameRequest request)
        {
            //1. validate request
            Validation.EnsureValid(request, new WeatherReportByCityNameRequestValidator());
            //var requestModel = request.ToModel();
            string sessionId = Guid.NewGuid().ToString() ;

            //2. create data and store somewhere
            FileSystem dataStore = new FileSystem();
            WeatherReportByCityNameInitResponse data = dataStore.CreateFile(sessionId);

            //. push into queue
            RabbitMQDataStore store = new RabbitMQDataStore();
            WeatherReportByCityNameInitResponse storeId=store.Enqueue(data,request);

            //Here we will call the core service
            //Convert back the model to data contract
            //var responseModel = weatherApp.GetWeatherReport(requestModel);
            //return responseModel.ToDataContract();
            return new WeatherReportByCityNameInitResponse() { SessionId = storeId.SessionId};
        }




        public WeatherReportResultsResponse GetWeatherResult(WeatherReportByCityNameInitResponse request)
        {

            //1. validate request
            //var requestModel = request.ToModel();
           
            //var data = sessionStore.Get(request.SessionId);

         

            return new WeatherReportResultsResponse() {  };
        }

    }
}
