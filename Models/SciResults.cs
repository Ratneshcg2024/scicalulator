namespace SCIMetricAPI.Models
{
    public class SciResults
    {
        public string AppName { get; set; }
        public string Processor { get; set; }
        public decimal SCI { get; set; }
        public decimal OperationalEnergy_kWh { get; set; }
        public decimal OperationalEmissions_gCO2e { get; set; }
        public decimal EmbodiedEmissions_kgCO2e { get; set; }
    }
}
