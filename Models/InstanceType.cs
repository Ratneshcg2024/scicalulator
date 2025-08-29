namespace SCIMetricAPI.Models
{
    public class InstanceType
    {
        public int Id { get; set; }
        public int CloudProviderId { get; set; }
        public CloudProvider CloudProvider { get; set; }
        public string InstanceClass { get; set; }
        public int CPUCoresAvailable { get; set; }
        public int MemoryAvailable { get; set; }
        public string CPUModelName { get; set; }
        public int TDP { get; set; } // Thermal Design Power
        public decimal gpuCount { get; set; }
        public string gpuModelName { get; set; }
        public string Mid { get; set; }
        public string storagetype { get; set; }
        public string storage { get; set; }
    }
}
