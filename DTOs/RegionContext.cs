
namespace SCIMetricAPI.DTOs
{
    public class RegionContext
    {
        public int CurrentRegionId { get; set; }
        public string CloudProvider { get; set; }
        public int? TargetRegionId { get; set; }
        public string CurrentRegionName { get; set; }
        public string PreferredRegionName { get; set; }
    }
}
