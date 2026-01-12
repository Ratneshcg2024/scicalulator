
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SCIMetricAPI.DTOs;
using SCIMetricAPI.Services.Interfaces;
using System.Threading.Tasks;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OptimizationController : ControllerBase
    {
        private readonly IOptimizationService _optimization;
        private readonly ILogger<OptimizationController> _logger;

        public OptimizationController(IOptimizationService optimization, ILogger<OptimizationController> logger)
        {
            _optimization = optimization;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(OptimizationResponse), 200)]
        public async Task<IActionResult> Optimize([FromBody] OptimizationRequest request)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Validation failed: {Errors}", ModelState);
                return ValidationProblem(ModelState);
            }

            // Log incoming request details
            _logger.LogInformation("[Optimize] Incoming request: Recommendations={@Recommendations}, InstanceContext={@InstanceContext}, RegionContext={@RegionContext}, OperationalEnergyKwh={Energy}",
                request.Recommendations, request.InstanceContext, request.RegionContext, request.OperationalEnergyKwh);

            var resp = await _optimization.GenerateOptimizationAsync(request);

            // Log response summary
            _logger.LogInformation("[Optimize] Response summary: RightSizeOptions={CountRS}, RegionOptions={CountRegion}, RightSizeSentence={RightSizeSentence}, RegionChangeSentence={RegionChangeSentence}",
                resp.RightSizeOptions?.Count ?? 0, resp.RegionOptimizationOptions?.Count ?? 0, resp.RightSizeSentence, resp.RegionChangeSentence);

            // Log enriched recommendations
            if (resp.EnrichedRecommendations?.Count > 0)
            {
                foreach (var rec in resp.EnrichedRecommendations)
                {
                    _logger.LogInformation("[Optimize] EnrichedRecommendation: Label={Label}, Template={Template}", rec.Label, rec.ConfigurationTemplate);
                }
            }

            return Ok(resp);
        }
    }
}
