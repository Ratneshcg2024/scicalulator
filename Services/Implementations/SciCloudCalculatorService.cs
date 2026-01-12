using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SCIMetricAPI.Data;
using SCIMetricAPI.DTOs;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SciCloudCalculatorService : ISciCloudCalculatorService
{
    private readonly ApplicationDbContext _context;
    private readonly IBoaviztaRepository _boaviztaRepo;
    private readonly ITdpCoefficientRepository _tdpCoeffRepo;
    private readonly ITdpDataRepository _tdpDataRepo;
    private readonly IConfiguration _configuration;
    private readonly ISciComputationService _sciComputationService;
    private readonly ILogger<SciCloudCalculatorService> _logger;

    public SciCloudCalculatorService(
        ApplicationDbContext context,
        IBoaviztaRepository boaviztaRepo,
        ITdpCoefficientRepository tdpCoeffRepo,
        ITdpDataRepository tdpDataRepo,
        IConfiguration configuration,
        ISciComputationService sciComputationService,
        ILogger<SciCloudCalculatorService> logger)
    {
        _context = context;
        _boaviztaRepo = boaviztaRepo;
        _tdpCoeffRepo = tdpCoeffRepo;
        _tdpDataRepo = tdpDataRepo;
        _configuration = configuration;
        _sciComputationService = sciComputationService;
        _logger = logger;
    }

   public async Task<SCIResponse> CalculateCloudSCI(SCIRequest request)
{
    if (request?.Instances == null || !request.Instances.Any())
        throw new ArgumentException("Invalid or missing instance data.");

    Console.WriteLine($"[CloudSCI] Starting SCI calculation for CloudProviderId={request.CloudProvider}, App={request.ApplicationName} memoryunit={request.Instances.First().memoryUnit}");

    double rawDuration = request.Instances.First().Duration;
    string durationUnit = request.Instances.First().DurationUnit?.ToLower();
    double duration = durationUnit switch
    {
        "hours" => rawDuration,
        "days" => rawDuration * 24,
        "seconds" => rawDuration / 3600.0,
        _ => rawDuration / 60.0
    };

    Console.WriteLine($"[CloudSCI] Duration normalized to hours: {duration} (Original={rawDuration} {durationUnit})");

    var instanceIds = request.Instances.Select(i => i.InstanceTypeId).Distinct();
    var regionIds = request.Instances.Select(i => i.RegionId).Distinct();

    var instances = await _context.InstanceTypes.AsNoTracking()
        .Where(i => instanceIds.Contains(i.Id))
        .ToDictionaryAsync(i => i.Id);

    var gridEmissions = await _context.GridEmissions.AsNoTracking()
        .Where(g => regionIds.Contains(g.Id))
        .ToDictionaryAsync(g => g.Id);

    var provider = await _context.CloudProviders.AsNoTracking()
        .FirstOrDefaultAsync(p => p.Id == request.CloudProvider);

    if (provider == null)
        throw new Exception($"CloudProviderId {request.CloudProvider} not found.");

    decimal totalE = 0, totalM = 0, totalO = 0;
    var instanceResults = new List<InstanceResultDto>();

    foreach (var instanceInput in request.Instances)
    {
        var instance = instances[instanceInput.InstanceTypeId];
        var gridEmission = gridEmissions[instanceInput.RegionId];

        double cpuCores = instance.CPUCoresAvailable;
        double cpucoresused = instanceInput.CPUCoresAllocated;
        double memoryAvailable = instance.MemoryAvailable;
        double tdp = instance.TDP;

        Console.WriteLine($"[CloudSCI] memoryunit={instanceInput.memoryUnit} Instance={instance.InstanceClass}, CPU Cores={cpuCores}, RAM={memoryAvailable} GB, TDP={tdp}");

        double I = gridEmission.CO2e;
        double tdpCoeff = _tdpCoeffRepo.GetTdpCoefficient(instanceInput.CPUUtilization);
        Console.WriteLine($"[memory unit] memory unit from input {instanceInput.memoryUnit}");
        double memoryUsed = instanceInput.memoryUnit.ToLower() == "mb"
            ?instanceInput.memoryUtilization / 1000
            :memoryAvailable * (instanceInput.memoryUtilization / 100);


        decimal TE = (decimal)await _boaviztaRepo.GetCloudEmbeddedEmissions(provider.Name.ToLower(), instance.InstanceClass);

        Console.WriteLine($"[CloudSCI] Inputs: CPUUtil={instanceInput.CPUUtilization}%, MemoryUsed={memoryUsed} GB, Storage={instanceInput.StorageVolumeGB} GB, GridEmissionFactor={I}, EmbodiedEmissionsTE={TE}");

        var inputDto = new SciCalculationInputDto
        {
            AppName = $"{request.ApplicationName}-Instance-{instanceInput.InstanceTypeId}",
            DurationHours = duration,
            CpuUtilization = instanceInput.CPUUtilization,
            TotalVcpu = cpuCores,
            VcpuUsed = cpuCores/instanceInput.CPUCoresAllocated,
            TDP = tdp,
            TdpCoefficient = tdpCoeff,
            MemoryUsedGB = memoryUsed,
            StorageType = instance.storagetype,
            StorageUsedGB = instanceInput.StorageVolumeGB,
            GridEmissionFactor = I,
            EmbodiedEmissionsTE = TE
        };

        var result = _sciComputationService.Calculate(inputDto);
        Console.WriteLine($"[CloudSCI] Calculation for {instance.InstanceClass}:");
     //   Console.WriteLine($"E={result.TotalOperationalEnergy} kWh, O={result.TotalOperationalEmissions} gCO₂e, M={result.TotalEmbodiedEmissions} gCO₂e, SCI={result.SCIvalue}, vCPU={sciResult.TotalVcpu}, RAM={sciResult.RamCapacityGB}");

        instanceResults.Add(new InstanceResultDto
        {
            InstanceTypeName = instance.InstanceClass,
            OperationalEnergy = result.TotalOperationalEnergy,
            OperationalEmissions = result.TotalOperationalEmissions,
            EmbodiedEmissions = result.TotalEmbodiedEmissions,
            SCI = result.SCIvalue,
            RamCapacityGB = memoryAvailable,
            TotalVcpu = (int)cpuCores,
            VcpuUsed = (int)(cpuCores/instanceInput.CPUCoresAllocated),
        });

        totalE += result.TotalOperationalEnergy;
        totalM += result.TotalEmbodiedEmissions;
        totalO += result.TotalOperationalEmissions;
    }

    decimal SCI = (totalO + totalM) / (decimal)duration;

    Console.WriteLine($"[CloudSCI] Final Aggregated Results: TotalE={totalE}, TotalO={totalO}, TotalM={totalM}, SCI={SCI}");

    return new SCIResponse
    {
        Duration = duration,
        TotalOperationalEnergy = totalE,
        TotalOperationalEmissions = totalO,
        TotalEmbodiedEmissions = totalM,
        GridEmissionFactorUsed = gridEmissions.First().Value.CO2e,
        SCI = SCI,
        InstanceResults = instanceResults
    };
}
}