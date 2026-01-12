namespace SCIMetricAPI.Services.Interfaces
{
    public interface IRegionLookupService
    {
        Task<int?> GetRegionIdByNameAsync(string regionName);
    }
}