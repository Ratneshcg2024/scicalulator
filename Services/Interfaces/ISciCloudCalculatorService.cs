using SCIMetricAPI.Models;
namespace SCIMetricAPI.Services.Interfaces
{
public interface ISciCloudCalculatorService
{
    Task<SCIResponse> CalculateCloudSCI(SCIRequest request);
}
}