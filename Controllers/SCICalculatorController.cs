using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;
using Newtonsoft.Json;
using System.Net;
[Route("api/sci")]
[ApiController]
public class SCICalculatorController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    public SCICalculatorController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }
    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateSCI([FromBody] SCIRequest request)
    {
        if (request == null || request.Instances == null || !request.Instances.Any())
            return BadRequest("Invalid or missing instance data.");
        decimal totalE = 0, totalM = 0, totalO = 0;
        double totalI = 0;
        double dur = 0;
        int instancecount = request.Instances.Count;
        Console.WriteLine($"instance count:{instancecount}");
        string workloadValuestr = _configuration.GetSection("WorkloadSizes")[request.WorkloadSize];
        Console.WriteLine($"workload: {workloadValuestr}");
        decimal functionalUnit = 1; // Default fallback value
        if (!string.IsNullOrEmpty(workloadValuestr) && decimal.TryParse(workloadValuestr, out decimal parsedFunctionalUnit))
        {
            functionalUnit = parsedFunctionalUnit;
        }
        var provider = await _context.CloudProviders.FirstOrDefaultAsync(p => p.Id == request.CloudProvider);
        if (provider == null)
            return NotFound($"CloudProviderId {request.CloudProvider} not found.");
        string providerName = provider.Name.ToLower();
        double gridemm = 0;
        int instanceIndex = 1;
        var instanceResults = new List<object>(); // Store results for each instance

        foreach (var instanceInput in request.Instances)
        {
            var instance = await _context.InstanceTypes.FindAsync(instanceInput.InstanceTypeId);
            if (instance == null)
                return NotFound($"InstanceTypeId {instanceInput.InstanceTypeId} not found.");
            var gridEmission = await _context.GridEmissions.FindAsync(instanceInput.RegionId);
            if (gridEmission == null)
                return NotFound($"RegionId {instanceInput.RegionId} not found.");
            double I = gridEmission.CO2e;
            gridemm = I;
            Console.WriteLine($"Calculating SCI for instance {instanceIndex} (InstanceTypeId: {instanceInput.InstanceTypeId}, RegionId: {instanceInput.RegionId})");

            //storage info:
            string storageinfo = instance.storagetype;
            double cpuCores = instance.CPUCoresAvailable;
            double tdp = instance.TDP;
            double memoryAvailable = instance.MemoryAvailable;
            //double memoryAvailable = 16;
            double memoryUtilization = instanceInput.memoryUtilization;
            double durationSeconds = instanceInput.Duration * 3600;
            dur = instanceInput.Duration;
            double[] xAxis = { 0, 10, 50, 100 };
            double[] yAxis = { 0.12, 0.32, 0.75, 1.02 };
            double tdpCoeff = InterpolateTdpCoeff(xAxis, yAxis, instanceInput.CPUUtilization);
            Console.WriteLine($"tdp:{tdp}, cpuCores:{cpuCores}, memoryAvailable:{memoryAvailable}, I:{I}, tdpCoeff:{tdpCoeff} ");
            // decimal Pcpu = (decimal)(tdp * tdpCoeff);
            decimal cpuEnergy = (decimal)(tdp * tdpCoeff); //in watts
            Console.WriteLine($"cpuEnergy for instance {instanceIndex}: {cpuEnergy}");
            decimal Pcpu = (cpuEnergy * (decimal)durationSeconds) / 3600000; //in kWh
            Console.WriteLine($"Pcpu for instance {instanceIndex}: {Pcpu}");
            //calculating memory 
            double memoryUsed;
            Console.WriteLine($"Memorytype:{instanceInput.memoryUnit}");
            //if (instanceInput.memoryUnit == "percentage")
            if (string.Equals(instanceInput.memoryUnit, "percent", StringComparison.OrdinalIgnoreCase))
            {
                memoryUsed = (memoryAvailable * (instanceInput.memoryUtilization / 100));
                Console.WriteLine($"converted Memoryused :{memoryUsed} GB from percentage");
            }
            else // Assume MB
            {
                memoryUsed = instanceInput.memoryUtilization / 1000; // Convert MB to GB
                Console.WriteLine($"converted Memoryused :{memoryUsed} GB from MB");
            }
            //Console.WriteLine($"Memoryused:{memoryUsed} GB");
            //Console.WriteLine($"Memory used:{memoryUsed}");
            // decimal Pmemory = (decimal)(memoryAvailable * (memoryUtilization / 100.0) * 0.000392 * instanceInput.Duration);
            //calculating Pmemory:
            decimal Pmemory = (decimal)(memoryUsed * 0.000392 * instanceInput.Duration); //in kWh

            decimal storageVolumeGB = (decimal)instanceInput.StorageVolumeGB;
            decimal durationDecimal = (decimal)instanceInput.Duration;
            decimal Pstorage;

            //calculating Pstorage:

            if (string.Equals(storageinfo, "HDD", StringComparison.OrdinalIgnoreCase))
            {
                Pstorage = storageVolumeGB * 0.00000065m * durationDecimal;  
            }
            else if (string.Equals(storageinfo, "SSD", StringComparison.OrdinalIgnoreCase))
            {
                Pstorage = storageVolumeGB * 0.0000012m * durationDecimal;
            }
            else // Default case for unknown storage type
            {
                Pstorage = storageVolumeGB * 0.000000925m * durationDecimal;
            }
            //decimal Pstorage = (decimal)(instanceInput.StorageVolumeGB * 0.0000012 * instanceInput.Duration);
            decimal Pall = Pcpu + Pmemory + Pstorage;
            Console.WriteLine($"storageVolumeGB:{storageVolumeGB}, durationDecimal:{durationDecimal}, Pcpu:{Pcpu}, cpuEnergy:{cpuEnergy}, Pmemory:{Pmemory}, Pstorage:{Pstorage:F10}, Pall:{Pall}");
            double vcpu_utilized = cpuCores / instanceInput.CPUCoresAllocated;
            Console.WriteLine($"Vcpu utilized:{vcpu_utilized}");
            decimal vcpu_ratio = (decimal)(vcpu_utilized / cpuCores);
            decimal E = Pall * vcpu_ratio;
            Console.WriteLine($"E for instance {instanceIndex}:{E}, vcpu_ratio:{vcpu_ratio}");
            decimal O = E * (decimal)I; //gCO2e/kWh
            Console.WriteLine($"O for instance {instanceIndex}:{O}");
            decimal TE = 105; //in kgCO2e
            if (string.Equals(providerName, "azure", StringComparison.OrdinalIgnoreCase))
            {
                if (!string.IsNullOrWhiteSpace(instance.Mid))
                {
                    TE = (decimal)await GetBoaviztaEmbeddedEmission(instance.Mid, providerName);
                    //Console.WriteLine($"TE value in azure if loop:{TE}");
                }

            }
            else
            {
                TE = (decimal)await GetBoaviztaEmbeddedEmission(instance.InstanceClass, providerName);
                Console.WriteLine($"TE value in else loop of azure:{TE}");
            }
            Console.WriteLine($"TE for instance {instanceIndex} ={TE}");
            decimal M = TE * 1000 * ((decimal)(instanceInput.Duration * 60 * 60) / (6 * 365 * 24 * 60 * 60)) * vcpu_ratio;
            Console.WriteLine($"M for instance {instanceIndex}={M}");

            // storing results for each instance starts
            decimal instanceSCI = (O + M) / (functionalUnit * (decimal)instanceInput.Duration);

instanceResults.Add(new {
    tier= instanceInput.tier,
    InstanceTypeId = instanceInput.InstanceTypeId,
    InstanceTypeName = instance.InstanceClass, // or CPUModelName
    RegionId = instanceInput.RegionId,
    RegionName = gridEmission.Region, // or Location
    CPUUtilization = instanceInput.CPUUtilization,
    MemoryUtilization = instanceInput.memoryUtilization,
    Duration = instanceInput.Duration,
    OperationalEnergy = E,
    OperationalEmissions = O,
    EmbodiedEmissions = M,
    SCI = instanceSCI
});



            // storing results for each instance ends

            totalE += E;
            totalM += M;
            totalO += O;
            Console.WriteLine($"calculating values upto instance {instanceIndex} :Total E:{totalE}  TotalM: {totalM}  TotalO {totalO}");
            instanceIndex++;
        }
        decimal SCI = (totalO + totalM) / (functionalUnit * (decimal)dur);
        var response = new
        {
            duration = dur,
            TotalOperationalEnergy = totalE,
            TotalOperationalEmissions = totalO,
            TotalEmbodiedEmissions = totalM,
            GridEmissionFactorUsed = gridemm,
            SCI = SCI,
            InstanceResults = instanceResults   
        };
        //Console.WriteLine("final values::",JsonConvert.SerializeObject(response));
        Console.WriteLine($"SCI Result: {JsonConvert.SerializeObject(response)}");
        return Ok(response);
    }
    private double InterpolateTdpCoeff(double[] xAxis, double[] yAxis, double x)
    {
        for (int i = 0; i < xAxis.Length - 1; i++)
        {
            if (xAxis[i] <= x && x <= xAxis[i + 1])
            {
                double x0 = xAxis[i], x1 = xAxis[i + 1];
                double y0 = yAxis[i], y1 = yAxis[i + 1];
                return y0 + ((x - x0) * (y1 - y0)) / (x1 - x0);
            }
        }
        return (x < xAxis[0]) ? yAxis[0] : yAxis[^1];
    }
    private async Task<double> GetBoaviztaEmbeddedEmission(string instanceType, string provider)
    {
        try
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"https://api.boavizta.org/v1/cloud/instance?provider={provider}&instance_type={instanceType.ToLower()}&verbose=false&criteria=gwp";
            var response = await client.GetAsync(url);
           // Console.WriteLine($"resonse status code from fetching function:{response.StatusCode}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine($"Boavizta 404 for instanceType={instanceType}, provider={provider}. Using TE=10.");
                return 10;
            }
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Boavizta error {response.StatusCode} for instanceType={instanceType}. Using TE=10.");
                return 10;
            }
            var content = await response.Content.ReadAsStringAsync();
            dynamic data = JsonConvert.DeserializeObject(content);
            double? te = data?.impacts?.gwp?.embedded?.value;
            if (te == null ||te==0)
            {
                Console.WriteLine($"TE data missing from Boavizta response. Using TE=10.");
                return 10;
            }

            //Console.WriteLine($"teeeeee{te}");
            return te.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception calling Boavizta: {ex.Message}. Using TE=10.");
            return 10;
        }
    }
}
public class SCIRequest
{
    public string ApplicationName { get; set; }
    public int CloudProvider { get; set; }
    public string WorkloadSize { get; set; }
    public List<InstanceInput> Instances { get; set; }
}
public class InstanceInput
{
    public int InstanceTypeId { get; set; }
    public int RegionId { get; set; }
    public int tier { get; set; }
    public double CPUUtilization { get; set; }
    public double memoryUtilization { get; set; }
    public double Duration { get; set; }
    public double CPUCoresAllocated { get; set; }
    public double StorageVolumeGB { get; set; }
    public string memoryUnit {get; set;}
}