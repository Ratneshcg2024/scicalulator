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
        
        private readonly ILogger<TdpDataRepository> _logger;


        public TdpDataRepository(ApplicationDbContext context, ILogger<TdpDataRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // public async Task<float?> GetTdpByProcessorNameAsync(string processorName)
        // {
        //    var entry = await _context.TdpData
        //     .FirstOrDefaultAsync(x => processorName.ToLower().Contains(x.ProcessorName.ToLower()));


        //     return entry?.Tdp;
        // }
        public async Task<float?> GetTdpByProcessorNameAsync(string processorName)
{
            if (string.IsNullOrWhiteSpace(processorName))
            {
                _logger.LogWarning("Processor name is null or empty.");
                throw new ArgumentException("Processor name must not be empty.");
            }

    var normalizedName = processorName.Trim().ToLower();

    try
    {
        var entry = await _context.TdpData.AsNoTracking()
            .FirstOrDefaultAsync(x => x.ProcessorName.ToLower().Contains(normalizedName));

        if (entry == null)
        {
            _logger.LogInformation("No matching processor found for '{ProcessorName}'. Returning default TDP.", processorName);
        }

        return entry?.Tdp ?? 165f;
    }
    catch (Exception ex)
    {
        // Log the error if needed
         _logger.LogError(ex, "Error fetching TDP for processor: {ProcessorName}", processorName);
        return 165f;
    }
}

    }
}
