namespace SCIMetricAPI.DTOs
{
    public class RecommendationRequest
    {
        public decimal SCIvalue { get; set; }
        public decimal TotalOperationalEnergy { get; set; }
        public decimal TotalOperationalEmissions { get; set; }
        public decimal TotalEmbodiedEmissions { get; set; }
        public decimal GridEmissions { get; set; }
        public string DeploymentType { get; set; }
        public float CpuUtilization { get; set; }
        public float MemoryUtilization { get; set; }
        public float StorageUsedGB { get; set; }
        public int CountOfInstances { get; set; }
        public int AppCountPerInstance { get; set; }
        public string AppCriticality { get; set; }
        public int vCPUAvailable   {get;set;}
    }
}