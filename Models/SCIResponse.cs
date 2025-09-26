namespace SCIMetricAPI.Models
{
    public class SCIResponse
    {
        public double Duration { get; set; }
        public decimal TotalOperationalEnergy { get; set; }
        public decimal TotalOperationalEmissions { get; set; }
        public decimal TotalEmbodiedEmissions { get; set; }
        public double GridEmissionFactorUsed { get; set; }
        public decimal SCI { get; set; }
        public List<object> InstanceResults { get; set; }
    }
}