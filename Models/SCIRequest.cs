namespace SCIMetricAPI.Models
{
    public class SCIRequest
    {
        public string ApplicationName { get; set; }
        public int CloudProvider { get; set; }
        public string WorkloadSize { get; set; }
        public List<InstanceInput> Instances { get; set; }
    }
    public class InstanceInput
    {
        public int InstanceTypeId { get; set; }
        public int RegionId { get; set; }
        public int tier { get; set; }     
        public double CPUUtilization { get; set; }
        public double memoryUtilization { get; set; }
        public string DurationUnit { get; set; } //added duration unit
        public double Duration { get; set; }
        public double CPUCoresAllocated { get; set; }
        public double StorageVolumeGB { get; set; }
        public string memoryUnit { get; set; }
        public string appCriticality { get; set; }

    }
}