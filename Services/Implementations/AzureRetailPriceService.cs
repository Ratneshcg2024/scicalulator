using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using SCIMetricAPI.DTOs;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Services.Implementations
{
    public sealed class AzureRetailPriceService : IAzureRetailPriceService
    {
        private const string ApiVersion = "2023-01-01-preview";          // preview => case-sensitive filters & savingsPlan support
        private const string BaseUrl    = "https://prices.azure.com/";    // Retail Prices API base

        private readonly IHttpClientFactory _httpClientFactory;

        public AzureRetailPriceService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

//get all the prices for a given sku and region
        public async Task<RetailPriceResultDto> GetVmPaygPricesAsync(string sku, string region, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(sku)) throw new ArgumentException("sku required");
            if (string.IsNullOrWhiteSpace(region)) throw new ArgumentException("region required");

            var http = _httpClientFactory.CreateClient();
            http.BaseAddress = new Uri(BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(30);

            // Build filter (case-sensitive in preview)
            var filter = $"serviceName eq 'Virtual Machines' and armRegionName eq '{region}' and armSkuName eq '{sku}' and type eq 'Consumption'";
            var url = $"api/retail/prices?api-version={ApiVersion}&$filter={Uri.EscapeDataString(filter)}";

            var items = new List<RetailItem>();
            while (!string.IsNullOrEmpty(url))
            {
                using var resp = await http.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                var page = await resp.Content.ReadFromJsonAsync<RetailPage>(JsonOptions(), ct)
                           ?? new RetailPage(new(), null);

                if (page.Items is { Count: > 0 }) items.AddRange(page.Items);
                url = page.NextPageLink; // absolute uri is fine
            }

            // Convert "Standard_D32s_v3" -> "D32s v3"
            var friendly = ArmSkuToFriendly(sku);

            // Exclude Spot / Low Priority, and prefer the exact meter line (skuName & meterName match)
            static bool IsSpotOrLow(RetailItem i)
            {
                var a = (i.skuName ?? string.Empty).ToLowerInvariant();
                var b = (i.meterName ?? string.Empty).ToLowerInvariant();
                return a.Contains("spot") || b.Contains("spot") || a.Contains("low priority") || b.Contains("low priority");
            }
            static bool ExactMeter(RetailItem i, string meter) =>
                string.Equals(i.skuName, meter, StringComparison.Ordinal) &&
                string.Equals(i.meterName, meter, StringComparison.Ordinal);

            var payg = items.Where(i => ExactMeter(i, friendly) && !IsSpotOrLow(i)).ToList();
            if (!payg.Any())
                payg = items.Where(i => !IsSpotOrLow(i)).ToList(); // fallback, some sizes may not fill meterName strictly

            RetailItem? PickLatest(IEnumerable<RetailItem> src, bool windows) =>
                src.Where(i => windows
                        ? (i.productName?.Contains("Windows", StringComparison.OrdinalIgnoreCase) ?? false)
                        : !(i.productName?.Contains("Windows", StringComparison.OrdinalIgnoreCase) ?? false))
                   .OrderByDescending(i => i.effectiveStartDate)
                   .FirstOrDefault();

            var linux = PickLatest(payg, windows: false);
            var windows = PickLatest(payg, windows: true);

            return new RetailPriceResultDto
            {
                Sku = sku,
                Region = region,
                LinuxPaygUsdPerHour = linux?.retailPrice,
                WindowsPaygUsdPerHour = windows?.retailPrice
            };
        }
        //get all instance names from the region
        public async Task<IReadOnlyList<string>> GetAvailableSkusAsync(string region, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(region)) throw new ArgumentException("region required");

            var http = _httpClientFactory.CreateClient();
            http.BaseAddress = new Uri(BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(30);

            var filter = $"serviceName eq 'Virtual Machines' and armRegionName eq '{region}' and type eq 'Consumption'";
            var url = $"api/retail/prices?api-version={ApiVersion}&$filter={Uri.EscapeDataString(filter)}";

            var set = new HashSet<string>(StringComparer.Ordinal);

            while (!string.IsNullOrEmpty(url))
            {
                using var resp = await http.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                var page = await resp.Content.ReadFromJsonAsync<RetailPage>(JsonOptions(), ct)
                           ?? new RetailPage(new(), null);

                foreach (var i in page.Items)
                {
                    if (!string.IsNullOrEmpty(i.armSkuName))
                        set.Add(i.armSkuName);
                }

                url = page.NextPageLink;
            }

            return set.OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
        }

        //Get all the region names
        public async Task<IReadOnlyList<string>> GetAvailableRegionsAsync(CancellationToken ct = default)
        {
            var http = _httpClientFactory.CreateClient();
            http.BaseAddress = new Uri(BaseUrl);
            http.Timeout = TimeSpan.FromSeconds(30);

            // Ask Retail Prices API for VM prices without pinning a region.
            // Then collect distinct armRegionName values from the feed.
            var filter = "serviceName eq 'Virtual Machines' and type eq 'Consumption'";
            var url = $"api/retail/prices?api-version={ApiVersion}&$filter={Uri.EscapeDataString(filter)}";

            var regions = new HashSet<string>(StringComparer.Ordinal);

            // Because this can be a large dataset, paginate fully.
            while (!string.IsNullOrEmpty(url))
            {
                using var resp = await http.GetAsync(url, ct);
                resp.EnsureSuccessStatusCode();

                var page = await resp.Content.ReadFromJsonAsync<RetailPage>(JsonOptions(), ct)
                        ?? new RetailPage(new(), null);

                foreach (var i in page.Items)
                {
                    if (!string.IsNullOrWhiteSpace(i.armRegionName))
                        regions.Add(i.armRegionName);
                }

                url = page.NextPageLink;
            }

            // Sort for deterministic output (case-insensitive visual order).
            return regions.OrderBy(r => r, StringComparer.OrdinalIgnoreCase).ToList();
        }

        // ----------------- helpers & internal API models -----------------

        private static string ArmSkuToFriendly(string armSku)
        {
            // "Standard_D32s_v3" -> "D32s v3"
            var name = armSku.StartsWith("Standard_", StringComparison.OrdinalIgnoreCase)
                ? armSku["Standard_".Length..]
                : armSku;
            return name.Replace('_', ' ');
        }

        private static JsonSerializerOptions JsonOptions() => new()
        {
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private sealed record RetailPage(List<RetailItem> Items, string? NextPageLink);
        private sealed record RetailItem(
            string? currencyCode,
            double? tierMinimumUnits,
            double? retailPrice,
            double? unitPrice,
            string? armRegionName,
            string? location,
            DateTimeOffset? effectiveStartDate,
            DateTimeOffset? effectiveEndDate,
            string? meterId,
            string? meterName,
            string? productId,
            string? skuId,
            string? productName,
            string? skuName,
            string? serviceName,
            string? serviceId,
            string? serviceFamily,
            string? unitOfMeasure,
            string? type,
            bool? isPrimaryMeterRegion,
            string? armSkuName
        );
    }
}