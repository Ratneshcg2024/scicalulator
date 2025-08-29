using Microsoft.AspNetCore.Mvc;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services;

namespace ProcessMetricsWebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SystemMetricsController : ControllerBase
    {
        private readonly SystemMetricsService _metricsService;

        public SystemMetricsController()
        {
            _metricsService = new SystemMetricsService();
        }

        [HttpGet]
        public ActionResult<SystemMetricsModel> GetSystemMetrics()
        {
            var metrics = _metricsService.GetSystemMetrics();
            return Ok(metrics);
        }
    }
}
