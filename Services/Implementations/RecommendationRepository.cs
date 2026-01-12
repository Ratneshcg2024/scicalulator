
using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCIMetricAPI.Services.Implementations
{
    public class RecommendationRepository : IRecommendationRepository
    {
        private readonly ApplicationDbContext _db;
        public RecommendationRepository(ApplicationDbContext db) => _db = db;

        public Task<List<Recommendations>> GetByLabelsAsync(IEnumerable<string> labels)
        {
            var set = labels?.Where(l => !string.IsNullOrWhiteSpace(l)).ToHashSet() ?? new HashSet<string>();
            return _db.Recommendations
                      .AsNoTracking()
                      .Where(r => set.Contains(r.RecommenderLabel))
                      .ToListAsync();
        }
    }
}
