using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SCIMetricAPI.Configurations;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OnPremiseSCIController : ControllerBase
    {
        private readonly CarbonSettings _settings;
        private readonly IBoaviztaRepository _boaviztaRepository;
        private readonly IGridEmissionRepository _gridEmissionRepository;
        private readonly ITdpCoefficientRepository _tdpCoefficientRepository;
        private readonly IProcessMetricsService _processMetricsService;

        public OnPremiseSCIController(
            IOptions<CarbonSettings> settings,
            IBoaviztaRepository boaviztaRepository,
            IGridEmissionRepository gridEmissionRepository,
            ITdpCoefficientRepository tdpCoefficientRepository,
            IProcessMetricsService processMetricsService)
        {
            _settings = settings.Value;
            _boaviztaRepository = boaviztaRepository;
            _gridEmissionRepository = gridEmissionRepository;
            _tdpCoefficientRepository = tdpCoefficientRepository;
            _processMetricsService = processMetricsService;
        }

        // Step 1: Get process metrics
        [HttpGet("metrics")]
        public async Task<IActionResult> GetMetrics([FromQuery] string processName, [FromQuery] int durationInMinutes)
        {
            var metrics = await _processMetricsService.GetProcessMetricsAsync(processName, durationInMinutes);
            if (metrics == null)
                return NotFound("Process not found.");
            return Ok(metrics);
        }

        // Step 2: Generate SCI score
        [HttpPost("generate-sci")]
        public async Task<IActionResult> GenerateSCI([FromBody] SciRequestModel request)
        {
            var metrics = await _processMetricsService.GetProcessMetricsAsync(request.ProcessName, request.DurationInMinutes);
            if (metrics == null)
                return NotFound("Process not found.");

            double durationHours = request.DurationInMinutes / 60.0;
            double gridEmissionFactor = await _gridEmissionRepository  //gCO2e/kWh
                .GetCarbonIntensityByCountryNameAsync(request.CountryName)
                ?? _settings.GridEmissionFactor;

            double tdpCoeff = _tdpCoefficientRepository.GetTdpCoefficient(metrics.CpuUsage);
            double tdp = _settings.DefaultTDP; //in watts
            decimal cpuEnergy = (decimal)(tdp * tdpCoeff);
            decimal Pcpu = (cpuEnergy * (decimal)(durationHours * 3600)) / 3600000; // Converted to kWh

            double memoryUsedGB = metrics.UsedMemoryMB / 1024.0; // Convert MB to GB
            double memoryCoefficient = 0.000392; // kWh/GB
                                                 // decimal Pmemory = (decimal)(memoryUsedGB * 0.000392 * durationHours);  //  kWh
            decimal Pmemory = (decimal)(memoryUsedGB * memoryCoefficient * durationHours);  //  kWh

            decimal storageVolumeGB = 1000;
            decimal Pstorage = metrics.HardDiskType == "HDD"
                ? storageVolumeGB * 0.00000065m * (decimal)durationHours
                : storageVolumeGB * 0.0000012m * (decimal)durationHours;

            decimal Pall = Pcpu + Pmemory + Pstorage; // Total power in kWh
            decimal vcpu_ratio = (decimal)metrics.CpuCoresUsed / metrics.CpuCores;
            decimal E = Pall * vcpu_ratio; //in KWh
            decimal O = E * (decimal)gridEmissionFactor;

            var spec = new ServerHardwareSpec
            {
               // CpuUnits = 2,
                CoreUnits = metrics.CpuCores,
               // RamUnits = 4,
                RamCapacity = 32,
              //  RamDensity = 1.79,
               // DiskUnits = 2,
                DiskType = metrics.HardDiskType.ToLower(),
                DiskCapacity = 512,
               // DiskDensity = 50.6,
               // PowerSupplyUnits = 2,
               // PowerSupplyWeight = 2.99
            };

            decimal TE;
            try
            {
                TE = await _boaviztaRepository.GetEmbeddedEmissionsAsync(spec); //in kgCO2e
            }
            catch
            {
                TE = (decimal)_settings.DefaultTE;  //kgCO2e
            }

            decimal M = TE * ((decimal)durationHours / (6 * 365 * 24)) * vcpu_ratio;
            decimal functionalUnit = 1;
            decimal SCI = (O + M) / (functionalUnit * (decimal)durationHours);

            var result = new
            {
                DurationHours = durationHours,
                TotalOperationalEnergy = E,
                TotalOperationalEmissions = O,
                TotalEmbodiedEmissions = M,
                SCIvalue = SCI,
                CountryUsed = request.CountryName,
                GridEmissionFactorUsed = gridEmissionFactor
            };
            Console.WriteLine($"SCI Result: {JsonSerializer.Serialize(result)}");
            return Ok(result);
            

        }
    }

    public class SciRequestModel
    {
        public string ProcessName { get; set; }
        public string CountryName { get; set; }
        public int DurationInMinutes { get; set; }
    }
}
