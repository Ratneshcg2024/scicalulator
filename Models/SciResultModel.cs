namespace SCIMetricAPI.Models
{
    public class SciResultModel
    {
        public string AppName { get; set; }
        public double DurationHours { get; set; }
        public decimal TotalOperationalEnergy { get; set; }
        public decimal TotalOperationalEmissions { get; set; }
        public decimal TotalEmbodiedEmissions { get; set; }
        public decimal SCIvalue { get; set; }
        public string CountryUsed { get; set; }
        public double GridEmissionFactorUsed { get; set; }
    }
}
