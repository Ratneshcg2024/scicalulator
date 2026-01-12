
namespace SCIMetricAPI.DTOs
{
    public class InstanceContext
    {
        public int InstanceTypeId { get; set; }
        public string Region { get; set; }
        public double CPUUtilization { get; set; }
        public double MemoryUtilization { get; set; }
        public int? TargetVCPUsFloor { get; set; } = 2;
        public int? TargetRAMFloorGB { get; set; } = 4;
    }
}
