namespace SCIMetricAPI.Services.Interfaces
{
    public interface IGridEmissionRepository
    {
        Task<double?> GetCarbonIntensityByCountryNameAsync(string countryName);
    }
}
