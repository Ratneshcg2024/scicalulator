using Microsoft.AspNetCore.Mvc;

namespace SCIMetricAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WeatherController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetWeather()
        {
            var weatherForecast = new[]
            {
                new { Date = DateTime.Now, TemperatureC = 25, Summary = "Sunny" },
                new { Date = DateTime.Now.AddDays(1), TemperatureC = 22, Summary = "Cloudy" }
            };
            return Ok(weatherForecast);
        }
    }
}
