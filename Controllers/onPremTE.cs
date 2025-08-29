using Microsoft.AspNetCore.Mvc;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
using System.Threading.Tasks;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnPremTEController : ControllerBase
    {
        private readonly IBoaviztaRepository _boaviztaRepository;

        public OnPremTEController(IBoaviztaRepository boaviztaRepository)
        {
            _boaviztaRepository = boaviztaRepository;
        }

        [HttpPost]
        public async Task<IActionResult> GetEmbeddedEmissions([FromBody] ServerHardwareSpec spec)
        {
            if (spec == null)
            {
                return BadRequest("Server hardware specification is required.");
            }

            try
            {
                var emissions = await _boaviztaRepository.GetEmbeddedEmissionsAsync(spec);
                return Ok(new { embeddedEmissions = emissions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving emissions: {ex.Message}");
            }
        }
    }
}
