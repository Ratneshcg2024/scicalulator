using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Controllers
{
    [Route("api/sci")]
    [ApiController]
    public class SciCloudCalculatorController : ControllerBase
    {
        private readonly ISciCloudCalculatorService _sciCalculatorService;

        public SciCloudCalculatorController(ISciCloudCalculatorService sciCalculatorService)
        {
            _sciCalculatorService = sciCalculatorService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> CalculateSCI([FromBody] SCIRequest request)
        {
            if (request == null || request.Instances == null || !request.Instances.Any())
                return BadRequest("Invalid or missing instance data.");

            try
            {
                var result = await _sciCalculatorService.CalculateCloudSCI(request);

                // Logging the final SCI result
                Console.WriteLine($"SCI Result: {JsonConvert.SerializeObject(result)}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating SCI: {ex.Message}");
                return StatusCode(500, "An error occurred while calculating SCI.");
            }
        }
    }
}
