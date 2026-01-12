
namespace SCIMetricAPI.DTOs
{
    public class AppRecommendationAggregate
    {
        public string AppName { get; set; }
        public string DeploymentType { get; set; }
        public string CloudProvider { get; set; }
        public string Region { get; set; }
        public double SCI { get; set; }

        public RecommendationResponse Flags { get; set; }
        public OptimizationResponse Optimization { get; set; }

        public double OperationalEnergyKwh { get; set; }
    }
}
