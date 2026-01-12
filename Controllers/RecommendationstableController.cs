
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationstableController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public RecommendationstableController(ApplicationDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Get all recommendations from the database
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<Recommendations>>> GetAll()
        {
            var result = await _db.Recommendations.AsNoTracking().ToListAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get recommendations by labels
        /// Example: api/recommendationstable/bylabels?labels=SwitchToRenewablePower&labels=ChangeHostingRegion
        /// </summary>
        [HttpGet("bylabels")]
        public async Task<ActionResult<List<Recommendations>>> GetByLabels([FromQuery] List<string> labels)
        {
            if (labels == null || labels.Count == 0)
                return BadRequest("Please provide at least one label.");

            var result = await _db.Recommendations
                                  .AsNoTracking()
                                  .Where(r => labels.Contains(r.RecommenderLabel))
                                  .ToListAsync();

            return Ok(result);
        }
    }
}