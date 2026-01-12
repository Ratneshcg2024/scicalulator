
using SCIMetricAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCIMetricAPI.Services.Interfaces
{
    public interface IRecommendationRepository
    {
        Task<List<Recommendations>> GetByLabelsAsync(IEnumerable<string> labels);
    }
}
