
namespace SCIMetricAPI.DTOs
{
    public class RegionOptimizationResult
    {
        public string CurrentRegionName { get; set; }
        public string RecommendedRegionName { get; set; }
        public string Country { get; set; }
        public decimal CurrentGridFactor_gCO2ePerKWh { get; set; }
        public decimal RecommendedGridFactor_gCO2ePerKWh { get; set; }
        public decimal EmissionReductionPercentage { get; set; }
    }
}
