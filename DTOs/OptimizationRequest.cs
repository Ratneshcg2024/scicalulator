
namespace SCIMetricAPI.DTOs
{
    public class OptimizationRequest
    {
        public RecommendationResponse Recommendations { get; set; }
        public InstanceContext InstanceContext { get; set; }
        public RegionContext RegionContext { get; set; }
        public decimal? OperationalEnergyKwh { get; set; }
    }
}
