using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCIMetricAPI.Models
{
    [Table("hardwarevendor")]
     public class HardwareVendor
    {
        [Key]
        [Column("Hid")]
        public int id {get; set;}
        public string HardwareName { get; set; }
        // public ICollection<CpuInfo> CpuInfos { get; set; }
    }
}