
namespace SCIMetricAPI.DTOs
{
    public class OptimizationResponse
    {
        public List<RightSizeResult> RightSizeOptions { get; set; } = new List<RightSizeResult>();
        public List<RegionOptimizationResult> RegionOptimizationOptions { get; set; } = new List<RegionOptimizationResult>();
        public string RightSizeSentence { get; set; }
        public string RegionChangeSentence { get; set; }
        public List<RecommendationDetailDto> EnrichedRecommendations { get; set; } = new();
    }
}
