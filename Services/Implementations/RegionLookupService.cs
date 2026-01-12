using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Services.Implementations
{
    public class RegionLookupService : IRegionLookupService
    {
        private readonly ApplicationDbContext _context;

        public RegionLookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetRegionIdByNameAsync(string regionName)
        {
            if (string.IsNullOrWhiteSpace(regionName)) return null;

            var region = await _context.GridEmissions
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Region.ToLower() == regionName.ToLower());

            return region?.Id;
        }
    }
}