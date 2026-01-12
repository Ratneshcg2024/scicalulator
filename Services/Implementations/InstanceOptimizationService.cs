
using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.DTOs;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCIMetricAPI.Services.Implementations
{
    public class InstanceOptimizationService : IInstanceOptimizationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInstanceTypeLookupService _instanceTypes;
        private readonly IAzureRetailPriceService _azurePrice;

        private const decimal MaxTargetUtilization = 0.70m;
        private const int DefaultMonthHours = 730;

        public InstanceOptimizationService(
            ApplicationDbContext context,
            IInstanceTypeLookupService instanceTypes,
            IAzureRetailPriceService azurePrice)
        {
            _context = context;
            _instanceTypes = instanceTypes;
            _azurePrice = azurePrice;
        }

        // public async Task<List<RightSizeResult>> GetRightSizeSuggestionsAsync(InstanceContext telemetry, int topN = 5)
        // {
        //     var current = await _instanceTypes.GetByIdAsync(telemetry.InstanceTypeId);
        //     var currentPrice = await ResolveLinuxPriceOrDbAsync(current.InstanceClass, telemetry.Region, current.InstanceCost ?? 0m);

        //     // Memory-led rule: keep steady utilization ≤70%
        //     var usedRamGb = current.MemoryAvailable * (decimal)(telemetry.MemoryUtilization / 100.0);
        //     var minTargetRamGb = usedRamGb / MaxTargetUtilization;

        //     var candidatesRaw = await _instanceTypes.GetByFamilyAsync(current.CloudProviderId, current.InstanceFamily);

        //     var valid = new List<(InstanceType inst, decimal price)>();
        //     foreach (var c in candidatesRaw)
        //     {
        //         if (c.Id == current.Id) continue;

        //         var candidatePrice = await ResolveLinuxPriceOrDbAsync(c.InstanceClass, telemetry.Region, c.InstanceCost ?? 0m);
        //         if (candidatePrice < currentPrice && c.MemoryAvailable >= minTargetRamGb)
        //         {
        //             valid.Add((c, candidatePrice));
        //         }
        //     }

        //     var topCandidates = valid.OrderBy(x => x.price).Take(topN).ToList();
        //     var results = new List<RightSizeResult>();
        //     foreach (var candidate in topCandidates)
        //     {
        //         var hourlySavings = currentPrice - candidate.price;
        //         var monthlySavings = hourlySavings * DefaultMonthHours;

        //         results.Add(new RightSizeResult
        //         {
        //             CurrentInstanceTypeName = current.InstanceClass,
        //             CurrentvCPUs = current.CPUCoresAvailable,
        //             CurrentRAM_GB = (int)Math.Round((double)current.MemoryAvailable),
        //             CurrentHourlyCost = currentPrice,
        //             Currency = "USD",
        //             FamilyKey = current.InstanceFamily,

        //             RecommendedInstanceTypeName = candidate.inst.InstanceClass,
        //             RecommendedvCPUs = candidate.inst.CPUCoresAvailable,
        //             RecommendedRAM_GB = (int)Math.Round((double)candidate.inst.MemoryAvailable),
        //             RecommendedHourlyCost = candidate.price,

        //             HourlyCostSavings = hourlySavings,
        //             MonthlyCostSavings = monthlySavings
        //         });
        //     }
        //     return results;
        // }

public async Task<List<RightSizeResult>> GetRightSizeSuggestionsAsync(InstanceContext telemetry, int topN = 5)
        {
            if (telemetry == null) return new List<RightSizeResult>();

            // 1) Current instance + current hourly price
            var current = await _instanceTypes.GetByIdAsync(telemetry.InstanceTypeId).ConfigureAwait(false);
            if (current == null) return new List<RightSizeResult>();

            var currentPrice = await ResolveHourlyPriceAsync(
                providerId: current.CloudProviderId,
                instanceClass: current.InstanceClass,
                region: telemetry.Region,
                dbFallback: current.InstanceCost ?? 0m
            ).ConfigureAwait(false);

            // 2) Compute RAM requirement to keep utilization <= 70%
            var utilization = ((decimal)telemetry.MemoryUtilization) / 100m;
            var usedRamGb = current.MemoryAvailable * utilization;
            var minTargetRamGb = (MaxTargetUtilization > 0m)
                ? (usedRamGb / MaxTargetUtilization)
                : usedRamGb;

            // 3) Same-family candidates (same provider)
            var candidatesRaw = await _instanceTypes
                .GetByFamilyAsync(current.CloudProviderId, current.InstanceFamily)
                .ConfigureAwait(false);

            var valid = new List<(InstanceType inst, decimal price)>();

            // 4) Evaluate candidates with provider-aware pricing
            foreach (var c in candidatesRaw)
            {
                if (c.Id == current.Id) continue;

                var candidatePrice = await ResolveHourlyPriceAsync(
                    providerId: c.CloudProviderId,
                    instanceClass: c.InstanceClass,
                    region: telemetry.Region,
                    dbFallback: c.InstanceCost ?? 0m
                ).ConfigureAwait(false);

                if (candidatePrice < currentPrice && c.MemoryAvailable >= minTargetRamGb)
                {
                    valid.Add((c, candidatePrice));
                }
            }

            // 5) Rank and shape results
            var topCandidates = valid.OrderBy(x => x.price).Take(topN).ToList();

            var results = new List<RightSizeResult>(topCandidates.Count);
            foreach (var candidate in topCandidates)
            {
                var hourlySavings = currentPrice - candidate.price;
                var monthlySavings = hourlySavings * DefaultMonthHours;

                results.Add(new RightSizeResult
                {
                    CurrentInstanceTypeName = current.InstanceClass,
                    CurrentvCPUs = current.CPUCoresAvailable,
                    CurrentRAM_GB = (int)Math.Round((double)current.MemoryAvailable),
                    CurrentHourlyCost = currentPrice,
                    Currency = "USD",
                    FamilyKey = current.InstanceFamily,

                    RecommendedInstanceTypeName = candidate.inst.InstanceClass,
                    RecommendedvCPUs = candidate.inst.CPUCoresAvailable,
                    RecommendedRAM_GB = (int)Math.Round((double)candidate.inst.MemoryAvailable),
                    RecommendedHourlyCost = candidate.price,

                    HourlyCostSavings = hourlySavings,
                    MonthlyCostSavings = monthlySavings
                });
            }

            return results;
        }

private async Task<decimal> ResolveHourlyPriceAsync(int providerId, string instanceClass, string region, decimal dbFallback)
        {
            if (providerId == 2) // Azure
            {
                var dto = await _azurePrice.GetVmPaygPricesAsync(instanceClass, region).ConfigureAwait(false);
                var apiPrice = dto?.LinuxPaygUsdPerHour;
                if (apiPrice.HasValue && apiPrice.Value > 0)
                    return (decimal)apiPrice.Value;

                // DB fallback when API is missing/zero
                return dbFallback;
            }

            // AWS (1) and GCP (3) → DB price only
            return dbFallback;
        }

        // public async Task<List<RegionOptimizationResult>> GetRegionOptimizationAsync(int currentRegionId, int cloudProviderId)
        // {
        //     var currentRegion = await _context.GridEmissions.FirstOrDefaultAsync(g => g.Id == currentRegionId);
        //     if (currentRegion == null) return new List<RegionOptimizationResult>();

        //     var allRegions = await _context.GridEmissions
        //         .Where(g => g.ProviderId == cloudProviderId && g.Id != currentRegionId)
        //         .OrderBy(g => g.CO2e)
        //         .ToListAsync();

        //     // Group by country and pick best in each country, then take top 3 by CO2e
        //     var grouped = allRegions
        //         .GroupBy(r => r.Country)
        //         .Select(g => g.First())
        //         .OrderBy(r => r.CO2e)
        //         .Take(3)
        //         .ToList();

        //     var results = new List<RegionOptimizationResult>();
        //     foreach (var region in grouped)
        //     {
        //         decimal reductionPercent = (decimal)(currentRegion.CO2e > 0
        //             ? ((currentRegion.CO2e - region.CO2e) / currentRegion.CO2e) * 100
        //             : 0);

        //         results.Add(new RegionOptimizationResult
        //         {
        //             CurrentRegionName = currentRegion.Region,
        //             RecommendedRegionName = region.Region,
        //             Country = region.Country,
        //             CurrentGridFactor_gCO2ePerKWh = (decimal)currentRegion.CO2e,
        //             RecommendedGridFactor_gCO2ePerKWh = (decimal)region.CO2e,
        //             EmissionReductionPercentage = reductionPercent
        //         });
        //     }
        //     return results;
        // }

public async Task<List<RegionOptimizationResult>> GetRegionOptimizationAsync(
            int currentRegionId,
            int cloudProviderId)
        {
            int topN = 3;
            // Load current region
            var currentRegion = await _context.GridEmissions
                .FirstOrDefaultAsync(g => g.Id == currentRegionId)
                .ConfigureAwait(false);

            if (currentRegion == null || topN <= 0)
                return new List<RegionOptimizationResult>();

            // Normalize current country once
            var currentCountry = (currentRegion.Country ?? string.Empty).Trim();
            var currentCo2 = (decimal)currentRegion.CO2e;

            // Fetch candidate regions for the same provider, excluding current
            var allCandidates = await _context.GridEmissions
                .Where(g => g.ProviderId == cloudProviderId && g.Id != currentRegionId)
                .ToListAsync()
                .ConfigureAwait(false);

            // Split into same-country and other-country lists
            var sameCountry = allCandidates
                .Where(r => StringEqualsOrdinalIgnoreCase(r.Country, currentCountry))
                .OrderBy(r => r.CO2e) // ascending CO2e
                .Take(topN)
                .ToList();

            // If not enough, fill from other countries by lowest CO2e
            if (sameCountry.Count < topN)
            {
                var remaining = topN - sameCountry.Count;

                var others = allCandidates
                    .Where(r => !StringEqualsOrdinalIgnoreCase(r.Country, currentCountry))
                    .OrderBy(r => r.CO2e)
                    .Take(remaining)
                    .ToList();

                sameCountry.AddRange(others);
            }

            // Map to results
            var results = new List<RegionOptimizationResult>(sameCountry.Count);
            foreach (var region in sameCountry)
            {
                var candidateCo2 = (decimal)region.CO2e;
                var reductionPercent = currentCo2 > 0m
                    ? ((currentCo2 - candidateCo2) / currentCo2) * 100m
                    : 0m;

                results.Add(new RegionOptimizationResult
                {
                    CurrentRegionName = currentRegion.Region,
                    RecommendedRegionName = region.Region,
                    Country = region.Country,
                    CurrentGridFactor_gCO2ePerKWh = currentCo2,
                    RecommendedGridFactor_gCO2ePerKWh = candidateCo2,
                    EmissionReductionPercentage = reductionPercent
                });
            }

            return results;
        }

private static bool StringEqualsOrdinalIgnoreCase(string? a, string? b)
        {
          var sa = (a ?? string.Empty).Trim();
            var sb = (b ?? string.Empty).Trim();
            return sa.Equals(sb, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<decimal> ResolveLinuxPriceOrDbAsync(string armSkuName, string region, decimal dbFallback)
        {
            var dto = await _azurePrice.GetVmPaygPricesAsync(armSkuName, region);
            var apiPrice = dto.LinuxPaygUsdPerHour;
            return (apiPrice.HasValue && apiPrice.Value > 0) ? (decimal)apiPrice.Value : dbFallback;
        }
    }
}
