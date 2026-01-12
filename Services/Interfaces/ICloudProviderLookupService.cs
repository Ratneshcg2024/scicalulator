namespace SCIMetricAPI.Services.Interfaces
{
    public interface ICloudProviderLookupService
    {
        Task<int?> GetCloudProviderIdByNameAsync(string providerName);
    }
}