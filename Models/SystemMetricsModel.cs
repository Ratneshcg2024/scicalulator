namespace SCIMetricAPI.Models
{
    public class SystemMetricsModel
    {
        public string ProcessorName { get; set; }
        public double CpuUtilizationPercent { get; set; }
        public double MemoryUtilizationGB { get; set; }
        public string StorageType { get; set; }
        public double TotalStorageGB { get; set; }
        public int TotalVcpu { get; set; }
        public int VcpuUsed { get; set; }
        public double RamCapacityGB { get; set; }
    }
}
