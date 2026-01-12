namespace SCIMetricAPI.DTOs
{
   public class CombinedSCIResponse
{
    public decimal SCIvalue { get; set; }
    public decimal TotalOperationalEnergy { get; set; }
    public decimal TotalOperationalEmissions { get; set; }
    public decimal TotalEmbodiedEmissions { get; set; }
    public decimal GridEmissions { get; set; }
    public RecommendationResponse Recommendations { get; set; }
    public RightSizeResult RightSizeRecommendation { get; set; }
    public RegionOptimizationResult RegionOptimization { get; set; }
}
}