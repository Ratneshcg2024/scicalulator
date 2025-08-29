using SCIMetricAPI.Models;

namespace SCIMetricAPI.Services.Interfaces
{
    public interface IProcessMetricsService
    {
        Task<ProcessMetrics> GetProcessMetricsAsync(string processName, int durationInMinutes);
    }
}
