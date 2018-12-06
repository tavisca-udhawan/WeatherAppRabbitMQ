using Microsoft.AspNetCore.Mvc;
using System;
using Tavisca.WeatherApp.Service;
using Tavisca.WeatherApp.Service.Data_Contracts;
using Tavisca.WeatherApp.Service.Data_Contracts.Interfaces;
using Tavisca.WeatherApp.Service.Data_Contracts.Model;

namespace Tavisca.WeatherApp.Web.Controllers
{
    [Route("api/weather_report")]
    [ApiController]
    public class weatherReportController : ControllerBase
    {

        private readonly IWeatherAppService _weatherAppService;
        public weatherReportController(IWeatherAppService weatherAppService)
        {
            _weatherAppService = weatherAppService;
        }
        [HttpPost]
        [Route("get_by_city_name")]
        public IActionResult GetReportByCityName([FromBody] CityNameRequest request)
        {
            var response = _weatherAppService.GetReportByCityName(request);
            return Ok(response);
        }

        [HttpPost]
        [Route("get_by_city_id")]
        public IActionResult GetReportByCityId([FromBody] CityIdRequest request)
        {
            var response = _weatherAppService.GetReportByCityId(request);
            return Ok(response);
        }

        [HttpPost]
        [Route("get_by_zip_code")]
        public IActionResult GetReportByZipCode([FromBody] ZipCodeRequest request)
        {
            var response = _weatherAppService.GetReportByZipCode(request);
            return Ok(response);
        }

        [HttpPost]
        [Route("get_by_geo_code")]
        public IActionResult GetReportByGeoCode([FromBody] GeoCodeRequest request)
        {
            var response = _weatherAppService.GetReportByGeoCode(request);
            return Ok(response);
        }



        [HttpPost]
        [Route("init_by_cityname")]
        public IActionResult InitWeatherReportByCityName([FromBody] CityNameRequest request)
        {
            
                var response = _weatherAppService.GetInitWeatherReportByCityName(request);
                return Ok(response);
            
        }

        [HttpPost]
        [Route("result_by_sessionid")]
        public IActionResult WeatherReportResultByCityName([FromBody] WeatherReportByCityNameInitResponse request)
        {
            var response = _weatherAppService.GetWeatherResult(request);
            return Ok(response);
        }
    }
}
