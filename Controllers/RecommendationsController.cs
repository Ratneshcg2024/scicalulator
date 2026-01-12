using Microsoft.AspNetCore.Mvc;
using SCIMetricAPI.DTOs;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecommendationsController : ControllerBase
    {
        private readonly IRecommendationService _recommendationService;

        public RecommendationsController(IRecommendationService recommendationService)
        {
            _recommendationService = recommendationService;
        }

        [HttpPost]
        public IActionResult GetRecommendations([FromBody] RecommendationRequest request)
        {
            if (request == null) return BadRequest("Invalid input");
            var recs = _recommendationService.GetRecommendations(request);
            return Ok(recs);
        }
    }
}