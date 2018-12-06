using Tavisca.WeatherApp.Model;
using Tavisca.WeatherApp.Model.Interfaces;
using Tavisca.WeatherApp.Model.Models;
using Tavisca.WeatherApp.Plugins.Factory;
namespace Tavisca.WeatherApp.Plugins
{
    public class WeatherReportDataProvider : IWeatherDataProvider
    {
        public WeatherReportResponseModel GetWeatherReport(WeatherReportRequestModel requestModel)
        {
            var adapterInstance = new WeatherAdapterFactory().GetInstance("OpenWeather");
            var response = adapterInstance.GetWeatherReport(requestModel);
            return response;
        }
    }
}
