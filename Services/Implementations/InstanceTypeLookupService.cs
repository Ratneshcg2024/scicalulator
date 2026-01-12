using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Services.Implementations
{
    public class InstanceTypeLookupService : IInstanceTypeLookupService
    {
        private readonly ApplicationDbContext _context;

        public InstanceTypeLookupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<int?> GetInstanceTypeIdByNameAsync(string instanceTypeName)
        {
            if (string.IsNullOrWhiteSpace(instanceTypeName)) return null;

            var instanceType = await _context.InstanceTypes
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.InstanceClass.ToLower() == instanceTypeName.ToLower());

            return instanceType?.Id;
        }
        
        public async Task<InstanceType> GetByIdAsync(int id)
        {
            return await _context.InstanceTypes.AsNoTracking().FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<List<InstanceType>> GetByFamilyAsync(int cloudProviderId, string family)
        {
            return await _context.InstanceTypes
                .AsNoTracking()
                .Where(i => i.CloudProviderId == cloudProviderId && i.InstanceFamily == family)
                .OrderBy(i => i.InstanceCost)
                .ToListAsync();
        }

    }
}