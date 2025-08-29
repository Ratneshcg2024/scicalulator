using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
using System.Threading.Tasks;

namespace SCIMetricAPI.Services.Implementations
{
    public class TdpDataRepository : ITdpDataRepository
    {
        private readonly ApplicationDbContext _context;

        public TdpDataRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<float?> GetTdpByProcessorNameAsync(string processorName)
        {
           var entry = await _context.TdpData
            .FirstOrDefaultAsync(x => processorName.ToLower().Contains(x.ProcessorName.ToLower()));


            return entry?.Tdp;
        }
    }
}
