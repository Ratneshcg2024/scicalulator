using SCIMetricAPI.Models;
namespace SCIMetricAPI.Services.Interfaces
{
    public interface ISciCloudCalculator
    {
        Task<object> CalculateSCIAsync(SCIRequest request);
    }
}