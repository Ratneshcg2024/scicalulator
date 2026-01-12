using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Models;

namespace SCIMetricAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<CloudProvider> CloudProviders { get; set; }
        public DbSet<InstanceType> InstanceTypes { get; set; }
        public DbSet<GridEmission> GridEmissions { get; set; }
        public DbSet<CountryEmission> CountryGridEmissions { get; set; }
        public DbSet<HardwareVendor> HardwareVendor { get; set; }
        public DbSet<TdpData> TdpData { get; set; }
        public DbSet<Recommendations> Recommendations { get; set; }
       // public DbSet<SCIInstanceRecord> SCIInstanceRecords { get; set; }

        //public DbSet<UserInput> UserInputs { get; set; }
    }
}
