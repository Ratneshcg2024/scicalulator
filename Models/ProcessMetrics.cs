namespace SCIMetricAPI.Models

{
    public class ProcessMetrics
    {
        public string ProcessName { get; set; }
        public double CpuUsage { get; set; }
        public double MemoryUsage { get; set; }
        public double NetworkUsage { get; set; }
        public double RamUsage { get; set; }
        public double AverageCpuUsage { get; set; }
        public string HardDiskType { get; set; }
        public string OsArchitecture { get; set; }
        public double ElectricityConsumption { get; set; }
        public string CpuModel { get; set; }
        public int CpuCores { get; set; }
        public int CpuCoresUsed { get; set; }
        public int LogicalProcessors { get; set; }
        public double FreeMemoryMB { get; set; }
        public double UsedMemoryMB { get; set; }
        public int DurationInMinutes { get; set; }
    }
}
