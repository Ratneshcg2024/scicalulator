using SCIMetricAPI.DTOs;
using SCIMetricAPI.Models;
namespace SCIMetricAPI.Services.Interfaces
{
    public interface IInstanceOptimizationService
{
    Task<List<RightSizeResult>> GetRightSizeSuggestionsAsync(InstanceContext telemetry, int topN = 5);
    Task<List<RegionOptimizationResult>> GetRegionOptimizationAsync(int currentRegionId, int cloudProviderId);
    //Task<InstanceType> GetInstanceByIdAsync(int instanceId);
}

}