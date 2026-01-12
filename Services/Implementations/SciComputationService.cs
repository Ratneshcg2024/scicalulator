using Microsoft.Extensions.Logging;
using SCIMetricAPI.DTOs;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
namespace SCIMetricAPI.Services.Implementations
{
    public class SciComputationService : ISciComputationService
{
    private readonly ILogger<SciComputationService> _logger;

    public SciComputationService(ILogger<SciComputationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public SciResultModel Calculate(SciCalculationInputDto input)
    {
        _logger.LogInformation("Starting SCI calculation for App: {AppName}", input.AppName);
            // _logger.LogInformation("Inputs: {@Input}", input);

            decimal cpuEnergy = (decimal)(input.TDP * input.TdpCoefficient);
       // _logger.LogDebug("CPU Energy (W): {CpuEnergy}", cpuEnergy);
       Console.WriteLine("CPU Energy (W): {0}", cpuEnergy);

        decimal PCPUraw = (cpuEnergy * (decimal)(input.DurationHours * 3600)) / 3600000;
       Console.WriteLine($"[SCI computation:]Total VCPU:{input.TotalVcpu} vcpu used:{input.VcpuUsed}"); 
        decimal vcpuRatio = (decimal)input.VcpuUsed / (decimal)input.TotalVcpu;
            decimal Pcpu = PCPUraw * vcpuRatio;
       // _logger.LogDebug("Pcpu (kWh): {Pcpu}, vCPU Ratio: {VcpuRatio}", Pcpu, vcpuRatio);
       System.Console.WriteLine("Pcpu (kWh): {0}, vCPU Ratio: {1}", Pcpu, vcpuRatio);

        decimal Pmemory = (decimal)(input.MemoryUsedGB * 0.000392 * input.DurationHours);
       // _logger.LogDebug("Pmemory (kWh): {Pmemory}", Pmemory);
    System.Console.WriteLine("Pmemory (kWh): {0}", Pmemory);
        decimal Pstorage = input.StorageType.ToLower() == "hdd"
            ? (decimal)(input.StorageUsedGB * 0.00000065 * input.DurationHours)
            : (decimal)(input.StorageUsedGB * 0.0000012 * input.DurationHours);
        _logger.LogDebug("Pstorage (kWh): {Pstorage}", Pstorage);
    System.Console.WriteLine("Pstorage (kWh): {0}", Pstorage);
        decimal Pall = Pcpu + Pmemory + Pstorage;
        decimal E = Pall;
        _logger.LogDebug("Total Operational Energy (E): {E}", E);
System.Console.WriteLine("Total Operational Energy (E): {0}", E);
        decimal O = E * (decimal)input.GridEmissionFactor;
        _logger.LogDebug("Operational Emissions (O): {O}", O);

        decimal M = input.EmbodiedEmissionsTE * 1000 * ((decimal)input.DurationHours / (6 * 365 * 24)) * vcpuRatio;
        _logger.LogDebug("Embodied Emissions (M): {M}", M);

        decimal SCI = (O + M) / (decimal)input.DurationHours;
        _logger.LogInformation("Final SCI: {SCI}", SCI);


        Console.WriteLine($"[SCI Calculation] App={input.AppName}");
Console.WriteLine($"CPU Energy (W): {cpuEnergy}, Pcpu (kWh): {Pcpu}");
Console.WriteLine($"Memory Energy (kWh): {Pmemory}, Storage Energy (kWh): {Pstorage}");
Console.WriteLine($"Total Operational Energy (E): {E}");
Console.WriteLine($"Operational Emissions (O): {O}, Embodied Emissions (M): {M}");
Console.WriteLine($"Final SCI: {SCI}");

        var result = new SciResultModel
        {
            AppName = input.AppName,
            DurationHours = input.DurationHours,
            TotalOperationalEnergy = E,
            TotalOperationalEmissions = O,
            TotalEmbodiedEmissions = M,
            TotalVcpu= (int)input.TotalVcpu,
            VcpuUsed= (int)input.VcpuUsed,
            SCIvalue = SCI,
            GridEmissionFactorUsed = input.GridEmissionFactor
        };
        Console.WriteLine($"[SCI Calculation Result] {result.AppName}: E={result.TotalOperationalEnergy} kWh, O={result.TotalOperationalEmissions} gCO₂e, M={result.TotalEmbodiedEmissions} gCO₂e, SCI={result.SCIvalue}, vCPU={result.TotalVcpu}, vCPU Used={result.VcpuUsed }, RAM={input.MemoryUsedGB} GB");
        _logger.LogInformation("SCI Calculation completed: {@Result}", result);
        return result;
    }
}
}