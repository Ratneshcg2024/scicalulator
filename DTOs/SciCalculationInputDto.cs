namespace SCIMetricAPI.DTOs
{
    public class SciCalculationInputDto
    {
       public string AppName { get; set; }
        public double DurationHours { get; set; }
        public double CpuUtilization { get; set; }
        public double TotalVcpu { get; set; }
        public double VcpuUsed { get; set; }
        public double TDP { get; set; }
        public double TdpCoefficient { get; set; }
        public double MemoryUsedGB { get; set; }
        public string StorageType { get; set; }
        public double StorageUsedGB { get; set; }
        public double GridEmissionFactor { get; set; }
        public decimal EmbodiedEmissionsTE { get; set; }
    }
}