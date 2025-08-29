// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.Configuration;
// using Newtonsoft.Json;
// using SCIMetricAPI.Data;
// using SCIMetricAPI.Models;
// using SCIMetricAPI.Services;
// using SCIMetricAPI.Services.Interfaces;
// using System.Net;

// public class SciCloudCalculator : ISciCloudCalculator
// {
//     private readonly ApplicationDbContext _context;
//     private readonly IHttpClientFactory _httpClientFactory;
//     private readonly IConfiguration _configuration;
//     private readonly ITdpService _tdpService;

//     public SciCloudCalculator(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration, ITdpService tdpService)
//     {
//         _context = context;
//         _httpClientFactory = httpClientFactory;
//         _configuration = configuration;
//         _tdpService = tdpService;
//     }

//     public async Task<object> CalculateSCIAsync(SCIRequest request)
//     {
//         if (request == null || request.Instances == null || !request.Instances.Any())
//             throw new ArgumentException("Invalid or missing instance data.");

//         decimal totalE = 0, totalM = 0, totalO = 0;
//         double dur = 0;
//         string workloadValuestr = _configuration.GetSection("WorkloadSizes")[request.WorkloadSize];
//         decimal functionalUnit = string.IsNullOrEmpty(workloadValuestr) || !decimal.TryParse(workloadValuestr, out var parsedUnit)
//             ? 1 : parsedUnit;

//         var provider = await _context.CloudProviders.FirstOrDefaultAsync(p => p.Id == request.CloudProvider)
//             ?? throw new Exception($"CloudProviderId {request.CloudProvider} not found.");

//         string providerName = provider.Name.ToLower();
//         double gridemm = 0;
//         var instanceResults = new List<object>();

//         foreach (var instanceInput in request.Instances)
//         {
//             var instance = await _context.InstanceTypes.FindAsync(instanceInput.InstanceTypeId)
//                 ?? throw new Exception($"InstanceTypeId {instanceInput.InstanceTypeId} not found.");

//             var gridEmission = await _context.GridEmissions.FindAsync(instanceInput.RegionId)
//                 ?? throw new Exception($"RegionId {instanceInput.RegionId} not found.");

//             double I = gridEmission.CO2e;
//             gridemm = I;
//             double cpuCores = instance.CPUCoresAvailable;
//             double tdp = instance.TDP;
//             double memoryAvailable = instance.MemoryAvailable;
//             double durationSeconds = instanceInput.Duration * 3600;
//             dur = instanceInput.Duration;

//             double tdpCoeff = _tdpService.GetTdpCoefficient(instanceInput.CPUUtilization);
//             decimal cpuEnergy = (decimal)(tdp * tdpCoeff);
//             decimal Pcpu = (cpuEnergy * (decimal)durationSeconds) / 3600000;

//             double memoryUsed = string.Equals(instanceInput.memoryUnit, "percentage", StringComparison.OrdinalIgnoreCase)
//                 ? (memoryAvailable * (instanceInput.memoryUtilization / 100))
//                 : instanceInput.memoryUtilization / 1024;

//             decimal Pmemory = (decimal)(memoryUsed * 0.000392 * instanceInput.Duration);
//             decimal storageVolumeGB = (decimal)instanceInput.StorageVolumeGB;
//             decimal durationDecimal = (decimal)instanceInput.Duration;

//             decimal Pstorage = instance.storagetype.ToLower() switch
//             {
//                 "hdd" => storageVolumeGB * 0.00000065m * durationDecimal,
//                 "ssd" => storageVolumeGB * 0.0000012m * durationDecimal,
//                 _ => storageVolumeGB * 0.00000085m * durationDecimal
//             };

//             decimal Pall = Pcpu + Pmemory + Pstorage;
//             double vcpu_utilized = cpuCores / instanceInput.CPUCoresAllocated;
//             decimal vcpu_ratio = (decimal)(vcpu_utilized / cpuCores);
//             decimal E = Pall * vcpu_ratio;
//             decimal O = E * (decimal)I;

//             decimal TE = 105;
//             if (providerName == "azure" && !string.IsNullOrWhiteSpace(instance.Mid))
//                 TE = (decimal)await GetBoaviztaEmbeddedEmission(instance.Mid, providerName);
//             else
//                 TE = (decimal)await GetBoaviztaEmbeddedEmission(instance.InstanceClass, providerName);

//             decimal M = TE * 1000 * ((decimal)(instanceInput.Duration * 3600) / (6 * 365 * 24 * 3600)) * vcpu_ratio;
//             decimal instanceSCI = (O + M) / (functionalUnit * (decimal)instanceInput.Duration);

//             instanceResults.Add(new
//             {
//                 InstanceTypeId = instanceInput.InstanceTypeId,
//                 InstanceTypeName = instance.InstanceClass,
//                 RegionId = instanceInput.RegionId,
//                 RegionName = gridEmission.Region,
//                 CPUUtilization = instanceInput.CPUUtilization,
//                 MemoryUtilization = instanceInput.memoryUtilization,
//                 Duration = instanceInput.Duration,
//                 OperationalEnergy = E,
//                 OperationalEmissions = O,
//                 EmbodiedEmissions = M,
//                 SCI = instanceSCI
//             });

//             totalE += E;
//             totalM += M;
//             totalO += O;
//         }

//         decimal SCI = (totalO + totalM) / (functionalUnit * (decimal)dur);
//         return new
//         {
//             duration = dur,
//             TotalOperationalEnergy = totalE,
//             TotalOperationalEmissions = totalO,
//             TotalEmbodiedEmissions = totalM,
//             GridEmissionFactorUsed = gridemm,
//             SCI = SCI,
//             InstanceResults = instanceResults
//         };
//     }

//     private async Task<double> GetBoaviztaEmbeddedEmission(string instanceType, string provider)
//     {
//         try
//         {
//             var client = _httpClientFactory.CreateClient();
//             var url = $"https://api.boavizta.org/v1/cloud/instance?provider={provider}&instance_type={instanceType.ToLower()}&verbose=false&criteria=gwp";
//             var response = await client.GetAsync(url);
//             if (!response.IsSuccessStatusCode) return 10;

//             var content = await response.Content.ReadAsStringAsync();
//             dynamic data = JsonConvert.DeserializeObject(content);
//             double? te = data?.impacts?.gwp?.embedded?.value;
//             return (te == null || te == 0) ? 10 : te.Value;
//         }
//         catch
//         {
//             return 10;
//         }
//     }
// }
