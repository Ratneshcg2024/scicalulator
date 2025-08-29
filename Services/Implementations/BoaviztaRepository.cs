using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
public class BoaviztaRepository : IBoaviztaRepository
{
    private readonly HttpClient _httpClient;

    public BoaviztaRepository(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<decimal> GetEmbeddedEmissionsAsync(ServerHardwareSpec spec)
    {
        // var payload = new
        // {
        //     model = new { type = "rack" },
        //     configuration = new
        //     {
        //         cpu = new { units = spec.CpuUnits, core_units = spec.CoreUnits},
        //         ram = new[] {
        //             new { units = spec.RamUnits, capacity = spec.RamCapacity}
        //         },
        //         disk = new[] {
        //             new { units = spec.DiskUnits, type = spec.DiskType, capacity = spec.DiskCapacity}
        //         },
        //         power_supply = new { units = spec.PowerSupplyUnits, unit_weight = spec.PowerSupplyWeight }
        //     }
        // };
         var payload = new
        {
            model = new { type = "rack" },
            configuration = new
            {
                cpu = new { units = 1, core_units = spec.CoreUnits},
                ram = new[] {
                    new { units = 1, capacity = spec.RamCapacity}
                },
                disk = new[] {
                    new { units = 1, type = spec.DiskType, capacity = spec.DiskCapacity}
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
}
