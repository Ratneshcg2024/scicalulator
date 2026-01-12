using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using System.Text.Json;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/v1/prices")]
    public class newPricesController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public newPricesController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public async Task<IActionResult> GetPrice([FromQuery] string instance_name, [FromQuery] string region)
        {
            if (string.IsNullOrWhiteSpace(instance_name) || string.IsNullOrWhiteSpace(region))
                return BadRequest(new { error = "Both 'sku' and 'region' are required." });

            var client = _httpClientFactory.CreateClient();
            client.BaseAddress = new Uri("https://prices.azure.com/");
            client.Timeout = TimeSpan.FromSeconds(30);

            string filter = $"serviceName eq 'Virtual Machines' and armRegionName eq '{region}' and armSkuName eq '{instance_name}' and type eq 'Consumption'";
            string url = $"api/retail/prices?api-version=2023-01-01-preview&$filter={Uri.EscapeDataString(filter)}";

            var allItems = new List<RetailItem>();

            while (!string.IsNullOrEmpty(url))
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var page = await response.Content.ReadFromJsonAsync<RetailPage>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (page?.Items != null) allItems.AddRange(page.Items);

                url = page?.NextPageLink?.Replace("https://prices.azure.com/", string.Empty);
            }

            string friendlyName = instance_name.StartsWith("Standard_") ? instance_name.Substring("Standard_".Length).Replace('_', ' ') : instance_name;

            var paygItems = allItems.Where(i =>
                i.skuName == friendlyName &&
                i.meterName == friendlyName &&
                !IsSpotOrLowPriority(i)).ToList();

            RetailItem? linux = PickLatest(paygItems, false);
            RetailItem? windows = PickLatest(paygItems, true);

            return Ok(new
            {
                instance_name,
                region,
                linuxPaygUsdPerHour = linux?.retailPrice,
                windowsPaygUsdPerHour = windows?.retailPrice
            });
        }

        private static bool IsSpotOrLowPriority(RetailItem item)
        {
            var s1 = (item.skuName ?? "").ToLower();
            var s2 = (item.meterName ?? "").ToLower();
            return s1.Contains("spot") || s2.Contains("spot") || s1.Contains("low priority") || s2.Contains("low priority");
        }

        private static RetailItem? PickLatest(IEnumerable<RetailItem> items, bool windows)
        {
            return items
                .Where(i => windows ? (i.productName?.Contains("Windows", StringComparison.OrdinalIgnoreCase) ?? false)
                                    : !(i.productName?.Contains("Windows", StringComparison.OrdinalIgnoreCase) ?? false))
                .OrderByDescending(i => i.effectiveStartDate)
                .FirstOrDefault();
        }
    }

    public record RetailPage(List<RetailItem>? Items, string? NextPageLink);
    public record RetailItem(string? productName, string? skuName, string? meterName, double? retailPrice, DateTimeOffset? effectiveStartDate);
}