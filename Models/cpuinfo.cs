using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SCIMetricAPI.Models
{
     
[Table("CpuInfo")]
public class CpuInfo
{
     [Key]
    [Column("cpu_id")]
    public int CpuId { get; set; }

    [Column("H_id")]
    public int HId { get; set; }

    [Column("system")]
    public string System { get; set; }

    [Column("nodes")]
    public int Nodes { get; set; }

    [Column("maxwatts")]
 public decimal MaxWatts { get; set; }

[Column("minwatts")]
    public decimal MinWatts { get; set; }

    [Column("cores")]
    public int Cores { get; set; }

    [Column("chips")]
    public int Chips { get; set; }

    [Column("processor")]
    public string Processor { get; set; }

    [Column("memory")]
    public int Memory { get; set; }

    [Column("jvm_vendor")]
    public string JvmVendor { get; set; }

    [Column("jvm_instance")]
    public int JvmInstance { get; set; }

    [Column("diskdrive")]
    public string DiskDrive { get; set; }
}

}