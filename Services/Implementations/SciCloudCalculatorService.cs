using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;

public class SciCloudCalculatorService : ISciCloudCalculatorService
{
    private readonly ApplicationDbContext _context;
    private readonly IBoaviztaRepository _boaviztaRepo;
    private readonly ITdpCoefficientRepository _tdpCoeffRepo;
    private readonly ITdpDataRepository _tdpDataRepo;
    private readonly IConfiguration _configuration;

    public SciCloudCalculatorService(
        ApplicationDbContext context,
        IBoaviztaRepository boaviztaRepo,
        ITdpCoefficientRepository tdpCoeffRepo,
        ITdpDataRepository tdpDataRepo,
        IConfiguration configuration)
    {
        _context = context;
        _boaviztaRepo = boaviztaRepo;
        _tdpCoeffRepo = tdpCoeffRepo;
        _tdpDataRepo = tdpDataRepo;
        _configuration = configuration;
    }

    public async Task<SCIResponse> CalculateCloudSCI(SCIRequest request)
    {
        if (request?.Instances == null || !request.Instances.Any())
            throw new ArgumentException("Invalid or missing instance data.");

        decimal totalE = 0, totalM = 0, totalO = 0;
        //CR001: adding duration unit handling
        //double duration = request.Instances.First().Duration;
        string durationUnit = request.Instances.First().DurationUnit?.ToLower();
        double rawDuration = request.Instances.First().Duration;
        double duration;

        switch (durationUnit)
        {
            case "hours":
                duration = rawDuration;
                break;
             case "days":
                duration = rawDuration * 24;
                break;
            case "seconds":
                duration = rawDuration / 3600.0;
                break;
            case "minutes":
            default:
                duration = rawDuration / 60.0;
                break;
        }
        Console.WriteLine($"Duration in hours (converted from {durationUnit}): {duration}");

        int instancecount = request.Instances.Count;
        Console.WriteLine($"instance count:{instancecount}");

        string workloadValuestr = _configuration.GetSection("WorkloadSizes")[request.WorkloadSize];
        Console.WriteLine($"workload: {workloadValuestr}");

        decimal functionalUnit = 1;
        if (!string.IsNullOrEmpty(workloadValuestr) && decimal.TryParse(workloadValuestr, out var parsedFU))
            functionalUnit = parsedFU;

        var instanceIds = request.Instances.Select(i => i.InstanceTypeId).Distinct();
        var regionIds = request.Instances.Select(i => i.RegionId).Distinct();

        var instances = await _context.InstanceTypes
            .AsNoTracking()
            .Where(i => instanceIds.Contains(i.Id))
            .ToDictionaryAsync(i => i.Id);

        var gridEmissions = await _context.GridEmissions
            .AsNoTracking()
            .Where(g => regionIds.Contains(g.Id))
            .ToDictionaryAsync(g => g.Id);

        var provider = await _context.CloudProviders
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.CloudProvider);

        if (provider == null)
            throw new Exception($"CloudProviderId {request.CloudProvider} not found.");

        string providerName = provider.Name.ToLower();
        double gridemm = 0;
        int instanceIndex = 1;
        var instanceResults = new List<object>();

        foreach (var instanceInput in request.Instances)
        {
            var instance = instances[instanceInput.InstanceTypeId];
            var gridEmission = gridEmissions[instanceInput.RegionId];
            double I = gridEmission.CO2e;
            gridemm = I;
            Console.WriteLine($"instanceInput results: memoryUtilization={instanceInput.memoryUtilization},memoryUnit={instanceInput.memoryUnit}, Duration={instanceInput.Duration}, DurationUnit={instanceInput.DurationUnit}");

            Console.WriteLine($"Calculating SCI for instance {instanceIndex} (InstanceTypeId: {instanceInput.InstanceTypeId}, RegionId: {instanceInput.RegionId})");

            string storageinfo = instance.storagetype;
            double cpuCores = instance.CPUCoresAvailable;
            double tdp = instance.TDP;
            double memoryAvailable = instance.MemoryAvailable;

            double tdpCoeff = _tdpCoeffRepo.GetTdpCoefficient(instanceInput.CPUUtilization);
            Console.WriteLine($"tdp:{tdp}, cpuCores:{cpuCores}, memoryAvailable:{memoryAvailable}, I:{I}, tdpCoeff:{tdpCoeff} ");

            decimal cpuEnergy = (decimal)(tdp * tdpCoeff);
            Console.WriteLine($"cpuEnergy for instance {instanceIndex}: {cpuEnergy}");
            //CR009 : calculating vcpu_utilized before using it
            Console.WriteLine($"Duration from instanceInput for instance {instanceIndex}: {instanceInput.Duration} {instanceInput.DurationUnit}");
            //decimal PCPUraw = cpuEnergy * (decimal)(instanceInput.Duration * 3600) / 3600000;
            decimal PCPUraw = cpuEnergy * (decimal)(duration * 3600) / 3600000;
            Console.WriteLine($"Pcpu raw for instance {instanceIndex}: {PCPUraw}");

            double vcpu_utilized = cpuCores / instanceInput.CPUCoresAllocated;
            Console.WriteLine($"Vcpu utilized:{vcpu_utilized}");
            decimal vcpu_ratio = (decimal)(vcpu_utilized / cpuCores);   //CR009: fixing vcpu_ratio calculation
            decimal Pcpu = PCPUraw * vcpu_ratio;
             Console.WriteLine($"Pcpu for instance {instanceIndex}: {Pcpu}");

            Console.WriteLine($"Memorytype:{instanceInput.memoryUnit}");
            double memoryUsed = instanceInput.memoryUnit.ToLower() == "percent"
                ? memoryAvailable * (instanceInput.memoryUtilization / 100)
                : instanceInput.memoryUtilization / 1000;

            Console.WriteLine($"converted Memoryused :{memoryUsed} GB from {(instanceInput.memoryUnit.ToLower() == "percent" ? "percentage" : "MB")}");

            //decimal Pmemory = (decimal)(memoryUsed * 0.000392 * instanceInput.Duration);
            decimal Pmemory = (decimal)(memoryUsed * 0.000392 * duration);
            decimal storageVolumeGB = (decimal)instanceInput.StorageVolumeGB;
            decimal durationDecimal = (decimal)instanceInput.Duration;

           // decimal Pstorage = storageVolumeGB * durationDecimal *
            decimal Pstorage = storageVolumeGB * (decimal)duration *
                (storageinfo.ToLower() switch
                {
                    "hdd" => 0.00000065m,
                    "ssd" => 0.0000012m,
                    _ => 0.000000925m
                });

            decimal Pall = Pcpu + Pmemory + Pstorage;
            Console.WriteLine($"storageVolumeGB:{storageVolumeGB}, durationDecimal:{durationDecimal}, Pcpu:{Pcpu}, cpuEnergy:{cpuEnergy}, Pmemory:{Pmemory}, Pstorage:{Pstorage:F10}, Pall:{Pall}");

            // double vcpu_utilized = cpuCores / instanceInput.CPUCoresAllocated;
            // Console.WriteLine($"Vcpu utilized:{vcpu_utilized}");
            //Cr009: fixing vcpu_ratio calculation
            // decimal vcpu_ratio = (decimal)(vcpu_utilized / cpuCores);
            // decimal E = Pall * vcpu_ratio;
            decimal E = Pall;
            Console.WriteLine($"E for instance {instanceIndex}:{E}, vcpu_ratio:{vcpu_ratio}");

            decimal O = E * (decimal)I;
            Console.WriteLine($"O for instance {instanceIndex}:{O}");

            decimal TE = providerName == "azure" && !string.IsNullOrWhiteSpace(instance.Mid)
                ? (decimal)await _boaviztaRepo.GetCloudEmbeddedEmissions(providerName, instance.Mid)
                : (decimal)await _boaviztaRepo.GetCloudEmbeddedEmissions(providerName, instance.InstanceClass);

            Console.WriteLine($"TE for instance {instanceIndex} ={TE}");

            //decimal M = TE * 1000 * ((decimal)(instanceInput.Duration * 3600) / (6 * 365 * 24 * 3600)) * vcpu_ratio;
            decimal M = TE * 1000 * ((decimal)(duration * 3600) / (6 * 365 * 24 * 3600)) * vcpu_ratio;
            Console.WriteLine($"M for instance {instanceIndex}={M}");

            decimal instanceSCI = (O + M) / (functionalUnit * (decimal)instanceInput.Duration);

            instanceResults.Add(new
            {
                tier = instanceInput.tier,
                InstanceTypeId = instanceInput.InstanceTypeId,
                InstanceTypeName = instance.InstanceClass,
                RegionId = instanceInput.RegionId,
                RegionName = gridEmission.Region,
                instanceInput.CPUUtilization,
                instanceInput.memoryUtilization,
                instanceInput.Duration,
                OperationalEnergy = E,
                OperationalEmissions = O,
                EmbodiedEmissions = M,
                SCI = instanceSCI
            });

            totalE += E;
            totalM += M;
            totalO += O;

            Console.WriteLine($"calculating values upto instance {instanceIndex} :Total E:{totalE}  TotalM: {totalM}  TotalO {totalO}");
            instanceIndex++;
        }

        decimal SCI = (totalO + totalM) / (functionalUnit * (decimal)duration);

        var response = new SCIResponse
        {
            Duration = duration,
            TotalOperationalEnergy = totalE,
            TotalOperationalEmissions = totalO,
            TotalEmbodiedEmissions = totalM,
            GridEmissionFactorUsed = gridemm,
            SCI = SCI,
            InstanceResults = instanceResults
        };

       // Console.WriteLine($"SCI Result: {JsonConvert.SerializeObject(response)}");
        return response;
    }
}
