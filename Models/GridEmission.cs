namespace SCIMetricAPI.Models
{
    public class GridEmission
    {
        public int Id { get; set; }
        public string Region { get; set; }
        public string Location { get; set; }
        public double CO2e { get; set; } // gCoe/kwh
        public int ProviderId { get; set; }
    }
}
