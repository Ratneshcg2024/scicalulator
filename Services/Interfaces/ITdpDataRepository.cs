namespace SCIMetricAPI.Services.Interfaces
{
    public interface ITdpDataRepository
    {
        Task<float?> GetTdpByProcessorNameAsync(string processorName);
    }
}
