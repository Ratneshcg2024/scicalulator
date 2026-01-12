using SCIMetricAPI.DTOs;
using SCIMetricAPI.Models;

namespace SCIMetricAPI.Services.Interfaces
{
    public interface ISciComputationService
{
    SciResultModel Calculate(SciCalculationInputDto input);
}

}