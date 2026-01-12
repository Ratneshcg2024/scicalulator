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
        public List<InstanceResultDto> InstanceResults { get; set; } = new List<InstanceResultDto>();
    }

    public class InstanceResultDto
    {
        public string InstanceTypeName { get; set; }
        public decimal OperationalEnergy { get; set; }
        public double RamCapacityGB { get; set; }
        public int TotalVcpu { get; set; }
        public int VcpuUsed { get; set; }
        public decimal OperationalEmissions { get; set; }
        public decimal EmbodiedEmissions { get; set; }
        public decimal SCI { get; set; }
    }
}