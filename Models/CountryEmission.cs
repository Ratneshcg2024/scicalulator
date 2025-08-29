namespace SCIMetricAPI.Models
{
    public class CountryEmission
    {
        public int countryEmissionID { get; set; }
        public string CountryName { get; set; }
       // public string CountryCode { get; set; }
        public int year { get; set; }
        public decimal carbonIntensity { get; set; } // gCoe/kwh
        
    }
}