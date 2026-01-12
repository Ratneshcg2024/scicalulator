
using SCIMetricAPI.DTOs;
using System.Threading.Tasks;

namespace SCIMetricAPI.Services.Interfaces
{
    public interface IOptimizationService
    {
        Task<OptimizationResponse> GenerateOptimizationAsync(OptimizationRequest request);
    }
}
