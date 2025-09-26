// using System.Net.Http;
// using System.Net.Http.Json;
// using System.Text.Json;
// using SCIMetricAPI.Models;
// using SCIMetricAPI.Services.Interfaces;
// public class BoaviztaRepository : IBoaviztaRepository
// {
//     private readonly HttpClient _httpClient;

//     public BoaviztaRepository(HttpClient httpClient)
//     {
//         _httpClient = httpClient;
//     }

//     public async Task<decimal> GetServerEmbeddedEmissions(ServerHardwareSpec spec)
//     {
//          var payload = new
//         {
//             model = new { type = "rack" },
//             configuration = new
//             {
//                 cpu = new { units = 1, core_units = spec.CoreUnits},
//                 ram = new[] {
//                     new { units = 1, capacity = spec.RamCapacity}
//                 },
//                 disk = new[] {
//                     new { units = 1, type = spec.DiskType, capacity = spec.DiskCapacity}
//                 }
//             }
//         };

//         var response = await _httpClient.PostAsJsonAsync("https://api.boavizta.org/v1/server/?verbose=true", payload);
//         response.EnsureSuccessStatusCode();

//         var json = await response.Content.ReadAsStringAsync();
//         using var doc = JsonDocument.Parse(json);

//         if (doc.RootElement.TryGetProperty("impacts", out var impacts) &&
//             impacts.TryGetProperty("gwp", out var gwp) &&
//             gwp.TryGetProperty("embedded", out var embedded) &&
//             embedded.TryGetProperty("value", out var value))
//         {
//             return value.GetDecimal();
//         }

//         throw new Exception("Embedded GWP value not found in Boavizta response.");
//     }
// }
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Newtonsoft.Json;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;

public class BoaviztaRepository : IBoaviztaRepository
{
    private readonly HttpClient _httpClient;

    public BoaviztaRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> GetServerEmbeddedEmissions(ServerHardwareSpec spec)
    {
        var payload = new
        {
            model = new { type = "rack" },
            configuration = new
            {
                cpu = new { units = 1, core_units = spec.CoreUnits },
                ram = new[] {
                    new { units = 1, capacity = spec.RamCapacity }
                },
                disk = new[] {
                    new { units = 1, type = spec.DiskType, capacity = spec.DiskCapacity }
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync("https://api.boavizta.org/v1/server/?verbose=true", payload);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        if (doc.RootElement.TryGetProperty("impacts", out var impacts) &&
            impacts.TryGetProperty("gwp", out var gwp) &&
            gwp.TryGetProperty("embedded", out var embedded) &&
            embedded.TryGetProperty("value", out var value))
        {
            return value.GetDecimal();
        }

        throw new Exception("Embedded GWP value not found in Boavizta response.");
    }

    public async Task<double> GetCloudEmbeddedEmissions(string provider, string instanceType)
    {
        try
        {
            var url = $"https://api.boavizta.org/v1/cloud/instance?provider={provider}&instance_type={instanceType.ToLower()}&verbose=false&criteria=gwp";
            Console.WriteLine($"Calling Boavizta: {url}");
            var response = await _httpClient.GetAsync(url);

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

            if (te == null || te == 0)
            {
                Console.WriteLine($"TE data missing from Boavizta response. Using TE=10.");
                return 10;
            }

            return te.Value;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception calling Boavizta: {ex.Message}. Using TE=10.");
            return 10;
        }
    }
}
