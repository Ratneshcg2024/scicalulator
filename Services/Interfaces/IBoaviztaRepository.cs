using SCIMetricAPI.Models;


namespace SCIMetricAPI.Services.Interfaces
{
    public interface IBoaviztaRepository
    {
        Task<decimal> GetServerEmbeddedEmissions(ServerHardwareSpec spec);
        Task<double> GetCloudEmbeddedEmissions(string provider, string instanceType);

    }
}