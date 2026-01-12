using SCIMetricAPI.DTOs;

namespace SCIMetricAPI.Services.Interfaces
{
    public interface IAzureRetailPriceService
    {
        
        // Returns Linux & Windows PAYG USD/hour for the given VM SKU and region.
        Task<RetailPriceResultDto> GetVmPaygPricesAsync(string sku, string region, CancellationToken ct = default);


        // Returns all VM ARM SKUs (armSkuName) the Retail Prices API publishes for the region.
        Task<IReadOnlyList<string>> GetAvailableSkusAsync(string region, CancellationToken ct = default);
        
        //Returns all the region names
        Task<IReadOnlyList<string>> GetAvailableRegionsAsync(CancellationToken ct = default);
    }
}