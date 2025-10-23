using Microsoft.Extensions.Options;
using SCIMetricAPI.Configurations;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
using System;

namespace SCIMetricAPI.Services
{
    public class SciCalculatorService : ISciCalculatorService
    {
        private readonly CarbonSettings _settings;
        private readonly IGridEmissionRepository _gridEmissionRepository;
        private readonly IBoaviztaRepository _boaviztaRepository;
        private readonly ITdpCoefficientRepository _tdpCoefficientRepository;
        private readonly ITdpDataRepository _tdpDataRepository;
        public SciCalculatorService(
            IOptions<CarbonSettings> settings,
            IGridEmissionRepository gridEmissionRepository,
            IBoaviztaRepository boaviztaRepository,
            ITdpCoefficientRepository tdpCoefficientRepository,
            ITdpDataRepository tdpDataRepository)
        {
            _settings = settings.Value;
            _gridEmissionRepository = gridEmissionRepository;
            _boaviztaRepository = boaviztaRepository;
            _tdpCoefficientRepository = tdpCoefficientRepository;
            _tdpDataRepository = tdpDataRepository;
        }

        public SciResultModel CalculateSCI(SciExcelModel model)
        {
            Console.WriteLine("Received Excel Data:");
            // Console.WriteLine($"AppName: {model.AppName}, Processor: {model.ProcessorName}, CPU Util: {model.CpuUtilization}, Mem Util: {model.MemoryUtilization}, Storage: {model.StorageType}, StorageUsedGB: {model.StorageUsedGB}, TotalVcpu: {model.TotalVcpu}, VcpuUsed: {model.VcpuUsed}, RAM: {model.RamCapacityGB}, Country: {model.region}, Duration: {model.duration} min");

            // double durationHours = model.duration / 60.0;
            //CR001: adding duration unit handling
            Console.WriteLine($"AppName: {model.AppName}, Processor: {model.ProcessorName}, CPU Util: {model.CpuUtilization}, Mem Util: {model.MemoryUtilization}, Storage: {model.StorageType}, StorageUsedGB: {model.StorageUsedGB}, TotalVcpu: {model.TotalVcpu}, VcpuUsed: {model.VcpuUsed}, RAM: {model.RamCapacityGB}, Country: {model.region}, Duration: {model.duration} {model.duration_unit}");

            double durationHours;
            switch (model.duration_unit?.ToLower())
            {
                case "hours":
                    durationHours = model.duration;
                    break;
                case "seconds":
                    durationHours = model.duration / 3600.0;
                    break;
                case "minutes":
                default:
                    durationHours = model.duration / 60.0;
                    break;
            }
            Console.WriteLine($"Duration in hours (converted from {model.duration_unit}): {durationHours}");
            Console.WriteLine($"Duration in hours: {durationHours}");

            double gridEmissionFactor = _gridEmissionRepository
                .GetCarbonIntensityByCountryNameAsync(model.region).Result
                ?? _settings.GridEmissionFactor;
            Console.WriteLine($"Grid Emission Factor: {gridEmissionFactor}");

            double tdpCoeff = _tdpCoefficientRepository.GetTdpCoefficient(model.CpuUtilization);
            float? tdpFromDb = _tdpDataRepository.GetTdpByProcessorNameAsync(model.ProcessorName).Result;
            double tdp = tdpFromDb?? _settings.DefaultTDP;

            decimal cpuEnergy = (decimal)(tdp * tdpCoeff);
            decimal PCPUraw = (cpuEnergy * (decimal)(durationHours * 3600)) / 3600000;
            decimal vcpu_ratio = (decimal)model.VcpuUsed / model.TotalVcpu;
            decimal Pcpu = PCPUraw * vcpu_ratio;
            Console.WriteLine($"TDP: {tdp}, TDP Coeff: {tdpCoeff}, CPU Energy (W): {cpuEnergy}, Pcpu (kWh): {Pcpu}");


            double memoryUsedGB = model.MemoryUtilization;
            if (string.Equals(model.memoryUtilization_unit, "percentage", StringComparison.OrdinalIgnoreCase))
            {
                memoryUsedGB = (model.RamCapacityGB * (model.MemoryUtilization / 100));
                Console.WriteLine($"converted Memoryused :{memoryUsedGB} GB from percentage");
            }
            else // Assume MB
            {
                memoryUsedGB = model.MemoryUtilization / 1000; // Convert MB to GB
                Console.WriteLine($"converted Memoryused :{memoryUsedGB} GB from MB");
            }
            // double memoryUsedGB = model.MemoryUtilization / 1024.0;
            
            decimal Pmemory = (decimal)(memoryUsedGB * 0.000392 * durationHours);
            Console.WriteLine($"Memory Used: {memoryUsedGB:F2} GB, Pmemory (kWh): {Pmemory}");

            decimal Pstorage = string.Equals(model.StorageType, "hdd", StringComparison.OrdinalIgnoreCase)
                ? (decimal)(model.StorageUsedGB * 0.00000065 * durationHours)
                : (decimal)(model.StorageUsedGB * 0.0000012 * durationHours);
            Console.WriteLine($"Storage Type: {model.StorageType}, Storage Used: {model.StorageUsedGB} GB, Pstorage (kWh): {Pstorage}");

            decimal Pall = Pcpu + Pmemory + Pstorage;
            // decimal vcpu_ratio = (decimal)model.VcpuUsed / model.TotalVcpu;
            // decimal E = Pall * vcpu_ratio;
            decimal E = Pall;
             Console.WriteLine($"Pall: {Pall}, vCPU Ratio: {vcpu_ratio}, Operational Energy (E): {E}");
            decimal O = E * (decimal)gridEmissionFactor;
            Console.WriteLine($"Pall: {Pall}, vCPU Ratio: {vcpu_ratio}, Operational Energy (E): {E}, Operational Emissions (O): {O}");

            var spec = new ServerHardwareSpec
            {
               CoreUnits = model.TotalVcpu,
               RamCapacity = (int)model.RamCapacityGB,
               DiskType = model.StorageType.ToLower(),
               DiskCapacity = (int)model.StorageUsedGB,
               
            };

            decimal TE;
            try
            {
                TE = _boaviztaRepository.GetServerEmbeddedEmissions(spec).Result;
            }
            catch
            {
                TE = (decimal)_settings.DefaultTE;
            }
            Console.WriteLine($"Embodied Emissions (TE): {TE}");

            decimal M = TE*1000 * ((decimal)durationHours / (6 * 365 * 24)) * vcpu_ratio; //in gCo2e
            Console.WriteLine($"Total Embodied Emissions (M): {M}");

            decimal SCI = (O + M) / ((decimal)durationHours); 
            Console.WriteLine($"Final SCI for {model.AppName}: {SCI}");

            return new SciResultModel
            {
                AppName = model.AppName,
                DurationHours = durationHours,
                TotalOperationalEnergy = E,
                TotalOperationalEmissions = O,
                TotalEmbodiedEmissions = M,
                SCIvalue = SCI,
                CountryUsed = model.region,
                GridEmissionFactorUsed = gridEmissionFactor
            };
        }
    }
}
