using SCIMetricAPI.Models;

namespace SCIMetricAPI.Services.Interfaces
{
    public interface IInstanceTypeLookupService
    {
        //Task<int?> GetInstanceTypeIdByNameAsync(string instanceTypeName);
        Task<int?> GetInstanceTypeIdByNameAsync(string instanceTypeName);
        Task<InstanceType> GetByIdAsync(int id);
        Task<List<InstanceType>> GetByFamilyAsync(int cloudProviderId, string family);

    }
}