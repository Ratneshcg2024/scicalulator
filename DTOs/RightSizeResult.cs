
namespace SCIMetricAPI.DTOs
{
    public class RightSizeResult
    {
        public string CurrentInstanceTypeName { get; set; }
        public int CurrentvCPUs { get; set; }
        public int CurrentRAM_GB { get; set; }
        public decimal CurrentHourlyCost { get; set; }
        public string Currency { get; set; }
        public string RecommendedInstanceTypeName { get; set; }
        public int RecommendedvCPUs { get; set; }
        public int RecommendedRAM_GB { get; set; }
        public decimal RecommendedHourlyCost { get; set; }
        public decimal HourlyCostSavings { get; set; }
        public decimal MonthlyCostSavings { get; set; }
        public string FamilyKey { get; set; }
    }
}
