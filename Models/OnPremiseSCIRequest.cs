namespace SCIMetricAPI.Models
{
    public class OnPremiseSCIRequest
    {
        public ProcessMetrics Metrics { get; set; }
        public string RegionCode { get; set; }
        public ServerHardwareSpec HardwareSpec { get; set; }
    }
}
