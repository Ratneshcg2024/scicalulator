using SCIMetricAPI.Models;


namespace SCIMetricAPI.Services.Interfaces
{
    public interface IBoaviztaRepository
    {
        Task<decimal> GetEmbeddedEmissionsAsync(ServerHardwareSpec spec);
    }
}