using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CloudImpactController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public CloudImpactController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        [HttpGet("embedded")]
        public async Task<IActionResult> GetEmbeddedImpact([FromQuery] string provider, [FromQuery] string instanceType)
        {
            if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(instanceType))
            {
                return BadRequest("Both 'provider' and 'instanceType' query parameters are required.");
            }

            var url = $"https://api.boavizta.org/v1/cloud/instance" +
                      $"?provider={provider}" +
                      $"&instance_type={instanceType}" +
                      $"&criteria=gwp" +
                      $"&verbose=true";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                if (doc.RootElement.TryGetProperty("impacts", out var impacts) &&
                    impacts.TryGetProperty("gwp", out var gwp) &&
                    gwp.TryGetProperty("embedded", out var embedded) &&
                    embedded.TryGetProperty("value", out var value))
                {
                    return Ok(new
                    {
                        provider,
                        instanceType,
                        embeddedImpactKgCO2e = value.GetDecimal()
                    });
                }

                return NotFound("Embedded GWP value not found in Boavizta response.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"Error calling Boavizta API: {ex.Message}");
            }
            catch (JsonException ex)
            {
                return StatusCode(500, $"Error parsing Boavizta response: {ex.Message}");
            }
        }
    }
}
