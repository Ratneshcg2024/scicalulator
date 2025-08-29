namespace SCIMetricAPI.Models
{
    public class CloudProvider
    {
        public int Id { get; set; }
        public string Name { get; set; } // AWS, Azure, etc.
        public List<InstanceType> InstanceTypes { get; set; } = new List<InstanceType>();
    }
}
