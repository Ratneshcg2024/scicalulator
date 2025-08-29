
namespace SCIMetricAPI.Models
{

    public class ServerHardwareSpec
    {
      //  public int CpuUnits { get; set; }
        public int CoreUnits { get; set; }
       
       // public int RamUnits { get; set; }
        public int RamCapacity { get; set; }
       
       // public int DiskUnits { get; set; }
        public string DiskType { get; set; } 
        public int DiskCapacity { get; set; }

    }
}