// using Microsoft.AspNetCore.Mvc;
// using SCIMetricAPI.Helpers;
// using SCIMetricAPI.Models;
// using SCIMetricAPI.Services.Interfaces;

// namespace SCIMetricAPI.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class SCIExcelUploadController : ControllerBase
//     {
//         private readonly ISciCalculatorService _sciCalculatorService;
//         private readonly ISciCloudCalculatorService _sciCloudCalculatorService;
//         private readonly ICloudProviderLookupService _cloudProviderLookupService;
//         private readonly IRegionLookupService _regionLookupService;
//         private readonly IInstanceTypeLookupService _instanceTypeLookupService;

//         public SCIExcelUploadController(ISciCalculatorService sciCalculatorService,
//                                         ISciCloudCalculatorService sciCloudCalculatorService,
//                                         ICloudProviderLookupService cloudProviderLookupService,
//                                         IRegionLookupService regionLookupService,
//                                         IInstanceTypeLookupService instanceTypeLookupService)
//         {
//             _sciCalculatorService = sciCalculatorService;
//             _sciCloudCalculatorService = sciCloudCalculatorService;
//             _cloudProviderLookupService = cloudProviderLookupService;
//             _regionLookupService = regionLookupService;
//             _instanceTypeLookupService = instanceTypeLookupService;
//         }

//         [HttpPost("upload-template")]
//         public async Task<IActionResult> UploadTemplate(IFormFile file)
//         {
//             if (file == null || file.Length == 0)
//                 return BadRequest("Please upload a valid Excel file.");

//             var inputModels = ExcelReader.ReadExcel(file.OpenReadStream());
//             var results = new List<SciResultModel>();

//             foreach (var model in inputModels)
//             {
//                 Console.WriteLine($"[Processing Model] App={model.AppName}, appcountper instance={model.AppCountPerInstance}, instance count {model.InstanceCount}");
//                 if (model.DeploymentType.ToLower() == "private")
//                 {
//                     var result = _sciCalculatorService.CalculateSCI(model);
//                     results.Add(result);
//                 }
//                 else if (model.DeploymentType.ToLower() == "public")
//                 {
//                     var providerId = await _cloudProviderLookupService.GetCloudProviderIdByNameAsync(model.CloudProvider);
//                     var regionId = await _regionLookupService.GetRegionIdByNameAsync(model.region);
//                     var instanceTypeId = await _instanceTypeLookupService.GetInstanceTypeIdByNameAsync(model.ProcessorName);

//                     if (providerId == null || regionId == null || instanceTypeId == null)
//                         return BadRequest($"Could not resolve IDs for {model.AppName}. Check names in Excel.");

//                     var cloudRequest = new SCIRequest
//                     {
//                         ApplicationName = model.AppName,
//                         CloudProvider = providerId.Value,
//                         Instances = new List<InstanceInput>
//                         {
//                             new InstanceInput
//                             {
//                                 InstanceTypeId = instanceTypeId.Value,
//                                 RegionId = regionId.Value,
//                                 CPUUtilization = model.CpuUtilization,
//                                 CPUCoresAllocated=model.AppCountPerInstance,
//                                 memoryUtilization = model.MemoryUtilization,
//                                 memoryUnit = model.memoryUtilization_unit,
//                                 StorageVolumeGB = model.StorageUsedGB,
//                                 Duration = model.duration,
//                                 DurationUnit = model.duration_unit
//                             }
//                         }
//                     };
// Console.WriteLine($"[CloudSCI Request FROM CONTROLLER] App={cloudRequest.ApplicationName},appcountperinstance{cloudRequest.Instances[0].CPUCoresAllocated}, ProviderId={cloudRequest.CloudProvider}, InstanceTypeId={cloudRequest.Instances[0].InstanceTypeId}, RegionId={cloudRequest.Instances[0].RegionId}, CPUUtil={cloudRequest.Instances[0].CPUUtilization}%, MemoryUtil={cloudRequest.Instances[0].memoryUtilization} {cloudRequest.Instances[0].memoryUnit}, StorageGB={cloudRequest.Instances[0].StorageVolumeGB}, Duration={cloudRequest.Instances[0].Duration} {cloudRequest.Instances[0].DurationUnit}");
//                     var cloudResult = await _sciCloudCalculatorService.CalculateCloudSCI(cloudRequest);
//                     var instanceResult = cloudResult.InstanceResults.First();

//                     results.Add(new SciResultModel
//                     {
//                         AppName = model.AppName,
//                         DurationHours = cloudResult.Duration,
//                         TotalOperationalEnergy = instanceResult.OperationalEnergy,
//                         TotalOperationalEmissions = instanceResult.OperationalEmissions,
//                         TotalEmbodiedEmissions = instanceResult.EmbodiedEmissions,
//                         SCIvalue = instanceResult.SCI,
//                         GridEmissionFactorUsed = cloudResult.GridEmissionFactorUsed,
//                         TotalVcpu= instanceResult.TotalVcpu,
//                         VcpuUsed= instanceResult.VcpuUsed,
//                         RamCapacityGB = instanceResult.RamCapacityGB
//                     });
//                 }
//             }
//  //var excelBytes = ExcelReader.WriteResultsToExcel(inputModels, results);
//            var excelBytes = ExcelReader.WriteResultsToExcel(inputModels, results);
//            // return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SCI_Results.xlsx");
//            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
//             //var folderPath = Path.Combine(Environment.CurrentDirectory, "files");

//             if (!Directory.Exists(folderPath))
//                         Directory.CreateDirectory(folderPath);

//             // Save the file
//             var fileName = $"SCI_Results_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
//             var filePath = Path.Combine(folderPath, fileName);
//             await System.IO.File.WriteAllBytesAsync(filePath, excelBytes);

//             // Build the public URL
//             var fileUrl = $"{Request.Scheme}://{Request.Host}/files/{fileName}";
//             Console.WriteLine($"Saved Excel file at: {filePath}");
//             Console.WriteLine($"Accessible via: {fileUrl}");

//             return Ok(new { downloadUrl = fileUrl });
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SCIMetricAPI.Helpers;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
using SCIMetricAPI.DTOs; // Optimization/Recommendation DTOs + AppRecommendationAggregate
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SCIExcelUploadController : ControllerBase
    {
        private readonly ISciCalculatorService _sciCalculatorService;
        private readonly ISciCloudCalculatorService _sciCloudCalculatorService;
        private readonly ICloudProviderLookupService _cloudProviderLookupService;
        private readonly IRegionLookupService _regionLookupService;
        private readonly IInstanceTypeLookupService _instanceTypeLookupService;
        private readonly IRecommendationService _recommendationService;
        private readonly IOptimizationService _optimizationService;
        private readonly ILogger<SCIExcelUploadController> _logger;

        public SCIExcelUploadController(
            ISciCalculatorService sciCalculatorService,
            ISciCloudCalculatorService sciCloudCalculatorService,
            ICloudProviderLookupService cloudProviderLookupService,
            IRegionLookupService regionLookupService,
            IInstanceTypeLookupService instanceTypeLookupService,
            IRecommendationService recommendationService,
            IOptimizationService optimizationService,
            ILogger<SCIExcelUploadController> logger)
        {
            _sciCalculatorService = sciCalculatorService;
            _sciCloudCalculatorService = sciCloudCalculatorService;
            _cloudProviderLookupService = cloudProviderLookupService;
            _regionLookupService = regionLookupService;
            _instanceTypeLookupService = instanceTypeLookupService;
            _recommendationService = recommendationService;
            _optimizationService = optimizationService;
            _logger = logger;
        }

        [HttpPost("upload-template")]
        public async Task<IActionResult> UploadTemplate(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a valid Excel file.");

            var inputModels = ExcelReader.ReadExcel(file.OpenReadStream());

            var results = new List<SciResultModel>();
            var aggregates = new List<AppRecommendationAggregate>(); // single DTO type

            foreach (var model in inputModels)
            {
                _logger.LogInformation("[Processing] App={App} Deployment={Deploy} Provider={Provider} Region={Region}",
                    model.AppName, model.DeploymentType, model.CloudProvider, model.region);

                // --- A) SCI calculation ---
                SciResultModel sciRow;
                SCIResponse cloudResult = null;
                int? providerId = null;
                int? regionId = null;
                int? instanceTypeId = null;

                if (model.DeploymentType.Equals("private", StringComparison.OrdinalIgnoreCase))
                {
                    sciRow = _sciCalculatorService.CalculateSCI(model);
                    results.Add(sciRow);
                }
                else if (model.DeploymentType.Equals("public", StringComparison.OrdinalIgnoreCase))
                {
                    providerId     = await _cloudProviderLookupService.GetCloudProviderIdByNameAsync(model.CloudProvider);
                    regionId       = await _regionLookupService.GetRegionIdByNameAsync(model.region);
                    instanceTypeId = await _instanceTypeLookupService.GetInstanceTypeIdByNameAsync(model.ProcessorName);

                    if (providerId == null || regionId == null || instanceTypeId == null)
                        return BadRequest($"Could not resolve IDs for {model.AppName}. Check names in Excel.");

                    var cloudRequest = new SCIRequest
                    {
                        ApplicationName = model.AppName,
                        CloudProvider   = providerId.Value,
                        Instances = new List<InstanceInput>
                        {
                            new InstanceInput
                            {
                                InstanceTypeId    = instanceTypeId.Value,
                                RegionId          = regionId.Value,
                                // These DTO fields are float -> use Convert.ToSingle
                                CPUUtilization    = ToFloat(model.CpuUtilization),
                                CPUCoresAllocated = model.AppCountPerInstance,
                                memoryUtilization = ToFloat(model.MemoryUtilization),
                                memoryUnit        = model.memoryUtilization_unit,
                                StorageVolumeGB   = ToFloat(model.StorageUsedGB),
                                Duration          = (int)model.duration,
                                DurationUnit      = model.duration_unit
                            }
                        }
                    };

                    _logger.LogInformation("[CloudSCI Request] App={App}, ProviderId={Pid}, InstanceTypeId={Tid}, RegionId={Rid}, CPU={CPU}%, Mem={Mem}{MemUnit}, StorageGB={StorageGB}, Duration={Duration}{DurUnit}",
                        cloudRequest.ApplicationName, cloudRequest.CloudProvider, cloudRequest.Instances[0].InstanceTypeId,
                        cloudRequest.Instances[0].RegionId, cloudRequest.Instances[0].CPUUtilization,
                        cloudRequest.Instances[0].memoryUtilization, cloudRequest.Instances[0].memoryUnit,
                        cloudRequest.Instances[0].StorageVolumeGB, cloudRequest.Instances[0].Duration, cloudRequest.Instances[0].DurationUnit);

                    cloudResult = await _sciCloudCalculatorService.CalculateCloudSCI(cloudRequest);

                    var instanceResult = cloudResult.InstanceResults.First();

                    sciRow = new SciResultModel
                    {
                        AppName                   = model.AppName,
                        DurationHours             = cloudResult.Duration,
                        TotalOperationalEnergy    = instanceResult.OperationalEnergy,     // double (from service)
                        TotalOperationalEmissions = instanceResult.OperationalEmissions,  // double
                        TotalEmbodiedEmissions    = instanceResult.EmbodiedEmissions,     // double
                        SCIvalue                  = instanceResult.SCI,                   // double
                        GridEmissionFactorUsed    = cloudResult.GridEmissionFactorUsed,   // double
                        TotalVcpu                 = instanceResult.TotalVcpu,
                        VcpuUsed                  = instanceResult.VcpuUsed,
                        RamCapacityGB             = instanceResult.RamCapacityGB
                    };
                    results.Add(sciRow);
                }
                else
                {
                    return BadRequest($"Unknown DeploymentType for {model.AppName}: {model.DeploymentType}");
                }

                // --- B) Recommendations (service expects decimals) ---
                var recReq = new RecommendationRequest
                {
                    SCIvalue                  = ToDecimal(sciRow.SCIvalue),
                    TotalOperationalEnergy    = ToDecimal(sciRow.TotalOperationalEnergy),     // kWh
                    TotalOperationalEmissions = ToDecimal(sciRow.TotalOperationalEmissions),  // gCO2e
                    TotalEmbodiedEmissions    = ToDecimal(sciRow.TotalEmbodiedEmissions),
                    GridEmissions             = ToDecimal(sciRow.GridEmissionFactorUsed),     // gCO2e/kWh
                    DeploymentType            = model.DeploymentType,
                    CpuUtilization            = (float)ToDecimal(model.CpuUtilization),
                    MemoryUtilization         = (float)ToDecimal(model.MemoryUtilization),
                    StorageUsedGB             = (float)(ToDecimal(model.StorageUsedGB)),
                    CountOfInstances          = model.InstanceCount <= 0 ? 1 : model.InstanceCount,
                    AppCountPerInstance       = model.AppCountPerInstance <= 0 ? 1 : model.AppCountPerInstance,
                    AppCriticality            = model.AppCriticality,
                    vCPUAvailable             =(int) (model.DeploymentType.Equals("public", StringComparison.OrdinalIgnoreCase)
                                               ? ToDecimal(cloudResult?.InstanceResults.First().TotalVcpu ?? model.TotalVcpu)
                                               : ToDecimal(model.TotalVcpu))
                };

                var recFlags = _recommendationService.GetRecommendations(recReq);

                // --- C) Optimization ---
                var optReq = new OptimizationRequest
                {
                    Recommendations = new RecommendationResponse
                    {
                        SwitchToRenewablePower = recFlags.SwitchToRenewablePower,
                        ChangeHostingRegion    = recFlags.ChangeHostingRegion,
                        ScheduleWorkloads      = recFlags.ScheduleWorkloads,
                        RightSizeInstances     = recFlags.RightSizeInstances,
                        ConsolidateWorkloads   = recFlags.ConsolidateWorkloads,
                        SoftwareEfficiency     = recFlags.SoftwareEfficiency,
                        ExtendHardwareLife     = recFlags.ExtendHardwareLife,
                        ReduceStorageFootprint = recFlags.ReduceStorageFootprint,
                        ShutdownPolicies       = recFlags.ShutdownPolicies,
                        MoveToServerless       = recFlags.MoveToServerless
                    },
                    InstanceContext = (model.DeploymentType.Equals("public", StringComparison.OrdinalIgnoreCase) && instanceTypeId.HasValue)
                                      ? new InstanceContext
                                      {
                                          InstanceTypeId    = instanceTypeId.Value,
                                          Region            = model.region,
                                          // Your DTO uses double for these two properties
                                          CPUUtilization    = model.CpuUtilization,
                                          MemoryUtilization = model.MemoryUtilization
                                      }
                                      : null,
                    RegionContext = (model.DeploymentType.Equals("public", StringComparison.OrdinalIgnoreCase) && regionId.HasValue)
                                    ? new RegionContext
                                    {
                                        CurrentRegionId     = regionId.Value,
                                        CloudProvider       = model.CloudProvider,
                                        CurrentRegionName   = model.region,
                                        PreferredRegionName = ""
                                    }
                                    : null,
                    // This property is double per your OptimizationRequest—ensure we pass double
                    OperationalEnergyKwh = ToDecimal(sciRow.TotalOperationalEnergy)
                };

                var optResp = await _optimizationService.GenerateOptimizationAsync(optReq);

                // --- D) Aggregate for Excel writer ---
                aggregates.Add(new AppRecommendationAggregate
                {
                    AppName              = model.AppName,
                    DeploymentType       = model.DeploymentType,
                    CloudProvider        = model.CloudProvider,
                    Region               = model.region,
                    SCI                  = (double)sciRow.SCIvalue,
                    Flags                = recFlags,
                    Optimization         = optResp,
                    OperationalEnergyKwh = ToDouble(sciRow.TotalOperationalEnergy) // ensure double
                });

                // --- Optional: quick debug dump ---
                DumpValue("SCI.SCIvalue", sciRow.SCIvalue);
                DumpValue("SCI.OperationalEnergy(kWh)", sciRow.TotalOperationalEnergy);
                DumpValue("SCI.OperationalEmissions(gCO2e)", sciRow.TotalOperationalEmissions);
            }

            // --- E) Write Excel (multi-sheet) ---
            var excelBytes = ExcelReader.WriteResultsAndRecommendationsToExcel(inputModels, results, aggregates);

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = $"SCI_Results_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            var filePath = Path.Combine(folderPath, fileName);
            await System.IO.File.WriteAllBytesAsync(filePath, excelBytes);

            var fileUrl = $"{Request.Scheme}://{Request.Host}/files/{fileName}";
            _logger.LogInformation("Saved Excel file at: {Path} | URL: {Url}", filePath, fileUrl);

            return Ok(new { downloadUrl = fileUrl });
        }

        // -----------------------
        // Cast helpers (centralize all explicit conversions)
        // -----------------------
        private static int ToInt(object v)
        {
            if (v == null) return 0;
            if (v is int i) return i;
            if (v is decimal d) return Convert.ToInt32(d);
            if (v is double db) return Convert.ToInt32(db);
            if (v is float f) return Convert.ToInt32(f);
            return Convert.ToInt32(v);
        }

        private static float ToFloat(object v)
        {
            if (v == null) return 0f;
            if (v is float f) return f;
            if (v is decimal d) return Convert.ToSingle(d);
            if (v is double db) return Convert.ToSingle(db);
            return Convert.ToSingle(v);
        }

        private static double ToDouble(object v)
        {
            if (v == null) return 0d;
            if (v is double db) return db;
            if (v is decimal d) return Convert.ToDouble(d);
            if (v is float f) return Convert.ToDouble(f);
            return Convert.ToDouble(v);
        }

        private static decimal ToDecimal(object v)
        {
            if (v == null) return 0m;
            if (v is decimal d) return d;
            if (v is double db) return Convert.ToDecimal(db);
            if (v is float f) return Convert.ToDecimal(f);
            if (v is int i) return Convert.ToDecimal(i);
            return Convert.ToDecimal(v);
        }

        // -----------------------
        // Debug helper (value + runtime type)
        // -----------------------
        private void DumpValue<T>(string name, T value)
        {
            _logger.LogInformation("[DBG] {Name} -> Value={Value} Type={Type}", name, value, value?.GetType().FullName ?? "null");
        }
    }
}
