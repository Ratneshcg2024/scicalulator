using Microsoft.AspNetCore.Mvc;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
using System.Threading.Tasks;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmbeddedImpactController : ControllerBase
    {
        private readonly IBoaviztaRepository _boaviztaRepository;

        public EmbeddedImpactController(IBoaviztaRepository boaviztaRepository)
        {
            _boaviztaRepository = boaviztaRepository;
        }

        [HttpPost("server-emissions")]
        public async Task<IActionResult> GetServerEmbeddedEmissions([FromBody] ServerHardwareSpec spec)
        {
            if (spec == null)
            {
                return BadRequest("Server hardware specification is required.");
            }

            try
            {
                var emissions = await _boaviztaRepository.GetServerEmbeddedEmissions(spec);
                return Ok(new { embeddedEmissions = emissions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving emissions: {ex.Message}");
            }
        }

        [HttpPost("cloud-emissions")]
        public async Task<IActionResult> GetCloudEmbeddedEmissions([FromQuery] string provider, [FromQuery] string instanceType)
        {
            try
            {
                var emission = await _boaviztaRepository.GetCloudEmbeddedEmissions(provider,instanceType);
                return Ok(new { embeddedEmissions = emission });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error Fetching cloud emissions: {ex.Message}");
            }
        }
       
    }

}

