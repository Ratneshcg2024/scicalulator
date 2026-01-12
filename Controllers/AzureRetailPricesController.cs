using Microsoft.AspNetCore.Mvc;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/v1/retail-prices")]
    public sealed class AzureRetailPricesController : ControllerBase
    {
        private readonly IAzureRetailPriceService _svc;

        public AzureRetailPricesController(IAzureRetailPriceService svc)
        {
            _svc = svc;
        }

        // GET /api/v1/retail-prices/payg?sku=Standard_D32s_v3&region=eastus
        [HttpGet("payg")]
        public async Task<IActionResult> GetPayg([FromQuery] string sku, [FromQuery] string region, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(sku) || string.IsNullOrWhiteSpace(region))
                return BadRequest(new { error = "Both 'sku' (e.g., Standard_D32s_v3) and 'region' (e.g., eastus) are required." });

            var result = await _svc.GetVmPaygPricesAsync(sku, region, ct);

            if (result.LinuxPaygUsdPerHour is null && result.WindowsPaygUsdPerHour is null)
                return NotFound(new { error = "No PAYG prices found. Check casing or SKU availability in Retail Prices API." });

            return Ok(result);
        }

        // GET /api/v1/retail-prices/skus?region=eastus
        [HttpGet("skus")]
        public async Task<IActionResult> GetSkus([FromQuery] string region, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(region))
                return BadRequest(new { error = "Query param 'region' is required (e.g., eastus, westeurope)." });

            var skus = await _svc.GetAvailableSkusAsync(region, ct);

            if (skus.Count == 0)
                return NotFound(new { region, error = "No SKUs returned by Retail Prices API for this region." });

            return Ok(new { region, count = skus.Count, skus });
        }

        //get all the available regions
        // GET /api/v1/retail-prices/regions
        [HttpGet("regions")]
        public async Task<IActionResult> GetRegions(CancellationToken ct)
        {
            var regions = await _svc.GetAvailableRegionsAsync(ct);

            if (regions.Count == 0)
                return NotFound(new { error = "Retail Prices API returned no regions for Virtual Machines." });

            return Ok(new { count = regions.Count, regions });
        }

    }
}