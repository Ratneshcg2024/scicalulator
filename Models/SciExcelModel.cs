namespace SCIMetricAPI.Models
{
    public class SciExcelModel
    {
        public string AppName { get; set; }
        public string ProcessorName { get; set; }
        public double CpuUtilization { get; set; }
        public string memoryUtilization_unit { get; set; }
        public double MemoryUtilization { get; set; }
        public string StorageType { get; set; }
        public double StorageUsedGB { get; set; }
        public int TotalVcpu { get; set; }
        public int VcpuUsed { get; set; }
        public double RamCapacityGB { get; set; }
        public string region { get; set; }
        public string duration_unit { get; set; }
        public int duration { get; set; }
    }

}