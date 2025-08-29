// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using SCIMetricAPI.Data;
// using SCIMetricAPI.Models;
// using Newtonsoft.Json;
// using System.Net;
// [Route("api/onpremsci")]
// [ApiController]
// public class onpremSCICalculatorController : ControllerBase
// {
//     private readonly ApplicationDbContext _context;
//     private readonly IHttpClientFactory _httpClientFactory;
//     private readonly IConfiguration _configuration;
//     public onpremSCICalculatorController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
//     {
//         _context = context;
//         _httpClientFactory = httpClientFactory;
//         _configuration = configuration;
//     }
//     [HttpPost("calculate")]
//     public async Task<IActionResult> CalculateonpremSCI([FromBody] onpremSCIRequest request)
//     {
//         if (request == null || request.Instances == null || !request.Instances.Any())
//             return BadRequest("Invalid or missing instance data.");
        
//         string workloadValuestr = _configuration.GetSection("WorkloadSizes")[request.WorkloadSize];
//         Console.WriteLine($"workload: {workloadValuestr}");

//          decimal functionalUnit = 1; // Default fallback value
//         if (!string.IsNullOrEmpty(workloadValuestr) && decimal.TryParse(workloadValuestr, out decimal parsedFunctionalUnit))
//         {
//             functionalUnit = parsedFunctionalUnit;
//         }

//         //var vendor=await _context.HardwareVendor.FirstOrDefaultAsync(p => p.id == request.CloudProvider);
//         double totalOperationalEmissions = 0;
//         foreach (var instanceInput in request.Instances)
//         {
//              var instance = await _context.CpuInfo.FindAsync(instanceInput.CpuId);
//              if (instance == null)
//                 return NotFound($"InstanceTypeId {instanceInput.CpuId} not found.");
            
//             var gridEmission = await _context.CountryGridEmissions.FindAsync(instanceInput.countryId);
//             if (gridEmission == null)
//                 return NotFound($"RegionId {instanceInput.countryId} not found.");
            
//             decimal I = gridEmission.carbonIntensity;  //localized emission of datacenter
           
//             decimal totalcpuCores = instance.Cores; //number of virtual servers
//             decimal CPUCoresAllocated=(decimal)instanceInput.CPUCoresAllocated/totalcpuCores;

            

//            // decimal percentageAssigned=;



//             //calculate average watts(Power)

//             decimal minwatts=instance.MinWatts;
//             decimal maxwatts=instance.MaxWatts;
//             decimal utilization=(decimal) instanceInput.CPUUtilization;

//             decimal Pavg=0;

//             Pavg=minwatts+utilization*(maxwatts-minwatts); //physical server average power

//             //Calculate operationalemission

//             //decimal operationalemission=I**Pavg;


           
//         }
//          return Ok();

//     }
// }

// public class onpremSCIRequest
// {
//     public string ApplicationName { get; set; }
//     public int hardwarevendor { get; set; }
//     public string WorkloadSize { get; set; }
//     public List<cpuinput> Instances { get; set; }
// }
// public class cpuinput
// {
//     public int CpuId { get; set; }
//     public int countryId { get; set; }
//     public double CPUUtilization { get; set; }
//     public double memoryUtilization { get; set; }
//     public double Duration { get; set; }
//     public double CPUCoresAllocated { get; set; }
//     public double StorageVolumeGB { get; set; }
//     public string memoryUnit {get; set;}
// }