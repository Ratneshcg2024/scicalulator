namespace SCIMetricAPI.DTOs
{
    public class SciExcelCloudInputDto
    {
        public string AppName { get; set; }
        public string CloudProviderName { get; set; } // e.g., "aws"
        public string RegionName { get; set; }        // e.g., "us-east-1"
        public string InstanceTypeName { get; set; }  // e.g., "a1.large"
        public double CpuUtilization { get; set; }
        public string MemoryUnit { get; set; }        // percentage or mb
        public double MemoryUtilization { get; set; }
        public string StorageType { get; set; }
        public double StorageUsedGB { get; set; }
        public double Duration { get; set; }
        public string DurationUnit { get; set; }
    }
}