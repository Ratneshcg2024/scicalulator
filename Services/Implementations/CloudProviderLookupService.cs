using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Services.Implementations
{
    public class CloudProviderLookupService : ICloudProviderLookupService
    {
        private readonly ApplicationDbContext _context;

        public CloudProviderLookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetCloudProviderIdByNameAsync(string providerName)
        {
            if (string.IsNullOrWhiteSpace(providerName)) return null;

            var provider = await _context.CloudProviders
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name.ToLower() == providerName.ToLower());

            return provider?.Id;
        }
    }
}