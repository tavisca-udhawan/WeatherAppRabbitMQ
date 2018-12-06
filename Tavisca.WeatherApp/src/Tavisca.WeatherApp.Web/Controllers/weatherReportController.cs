using Microsoft.AspNetCore.Mvc;
using Tavisca.WeatherApp.Service;
using Tavisca.WeatherApp.Service.Data_Contracts;
using Tavisca.WeatherApp.Service.Data_Contracts.Interfaces;

namespace Tavisca.WeatherApp.Web.Controllers
{
    [Route("api/weather_report")]
    [ApiController]
    public class weatherReportController : ControllerBase
    {

        private readonly IWeatherAppService weatherAppService;
        public weatherReportController()
        {
            weatherAppService = new WeatherAppService();
        }
        [HttpPost]
        [Route("get_by_city_name")]
        public IActionResult GetReportByCityName([FromBody] CityNameRequest request)
        {
            var response = weatherAppService.GetReportByCityName(request);
            return Ok(response);
        }

        [HttpPost]
        [Route("get_by_city_id")]
        public IActionResult GetReportByCityId([FromBody] CityIdRequest request)
        {
            var response = weatherAppService.GetReportByCityId(request);
            return Ok(response);
        }

        [HttpPost]
        [Route("get_by_zip_code")]
        public IActionResult GetReportByZipCode([FromBody] ZipCodeRequest request)
        {
            var response = weatherAppService.GetReportByZipCode(request);
            return Ok(response);
        }

        [HttpPost]
        [Route("get_by_geo_code")]
        public IActionResult GetReportByGeoCode([FromBody] GeoCodeRequest request)
        {
            var response = weatherAppService.GetReportByGeoCode(request);
            return Ok(response);
        }



        [HttpPost]
        [Route("init_by_cityname")]
        public IActionResult InitWeatherReportByCityName([FromBody] CityNameRequest request)
        {
            var response = weatherAppService.GetInitWeatherReportByCityName(request);
            return Ok(response);
        }
    }
}
