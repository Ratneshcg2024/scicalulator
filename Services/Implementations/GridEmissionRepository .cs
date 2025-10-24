using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Services.Implementations
{
    public class GridEmissionRepository : IGridEmissionRepository
    {
        private readonly ApplicationDbContext _context;

        public GridEmissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<double?> GetCarbonIntensityByCountryNameAsync(string countryName)
        {
            var emission = await _context.CountryGridEmissions.AsNoTracking()
                .Where(e => e.CountryName.ToLower() == countryName.ToLower())
                .FirstOrDefaultAsync();

            return emission?.carbonIntensity != null ? (double?)emission.carbonIntensity : 415.755; // Default value if not found
        }

    }
}
