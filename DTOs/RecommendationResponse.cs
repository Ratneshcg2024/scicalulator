namespace SCIMetricAPI.DTOs
{
    public class RecommendationResponse
    {
        public string SwitchToRenewablePower { get; set; }
        public string ChangeHostingRegion { get; set; }
        public string ScheduleWorkloads { get; set; }
        public string RightSizeInstances { get; set; }
        public string ConsolidateWorkloads { get; set; }
        public string SoftwareEfficiency { get; set; }
        public string ExtendHardwareLife { get; set; }
        public string ReduceStorageFootprint { get; set; }
        public string ShutdownPolicies { get; set; }
        public string MoveToServerless { get; set; }
    }
}