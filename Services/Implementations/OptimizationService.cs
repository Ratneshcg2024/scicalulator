
using SCIMetricAPI.DTOs;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SCIMetricAPI.Services.Implementations
{
    public class OptimizationService : IOptimizationService
    {
        private readonly IInstanceOptimizationService _optimization;
        private readonly IRecommendationRepository _recommendationRepo;
        private readonly ILogger<OptimizationService> _logger;

        public OptimizationService(
            IInstanceOptimizationService optimization,
            IRecommendationRepository recommendationRepo,
            ILogger<OptimizationService> logger)
        {
            _optimization = optimization;
            _recommendationRepo = recommendationRepo;
            _logger = logger;
        }

        public async Task<OptimizationResponse> GenerateOptimizationAsync(OptimizationRequest request)
        {
            var resp = new OptimizationResponse();

            // ---------- Right-size ----------
            if (IsYes(request.Recommendations?.RightSizeInstances) && request.InstanceContext != null)
            {
                var options = await _optimization.GetRightSizeSuggestionsAsync(request.InstanceContext, 5);
                resp.RightSizeOptions = options;
                _logger.LogInformation("[RS] Received {Count} right-size options", options.Count);

                if (options.Any())
                {
                    var best = options.First();
                    _logger.LogInformation("[RS] Best option: {@Best}", best);

                    resp.RightSizeSentence =
                        $"Your current instance type is {best.CurrentInstanceTypeName} with {best.CurrentvCPUs} vCPUs and {best.CurrentRAM_GB} GB RAM. " +
                        $"If your workload doesn’t fully utilize these resources, consider scaling down to {best.RecommendedInstanceTypeName}. " +
                        $"This will help reduce emissions and achieve a cost savings of {best.HourlyCostSavings:0.00} {best.Currency} per hour.";
                }
            }

            // ---------- Region optimization ----------
            if (IsYes(request.Recommendations?.ChangeHostingRegion) && request.RegionContext != null)
            {
                var providerId = MapProvider(request.RegionContext.CloudProvider);
                var regions = await _optimization.GetRegionOptimizationAsync(request.RegionContext.CurrentRegionId, providerId);
                resp.RegionOptimizationOptions = regions;
                _logger.LogInformation("[Region] Received {Count} region optimization options", regions.Count);

                if (regions.Any())
                {
                    var names = string.Join(", ", regions.Select(r => $"{r.RecommendedRegionName} ({r.Country})"));
                    resp.RegionChangeSentence = $"Top regions for emission savings: {names}.";
                }
            }

            // ---------- Enriched recommendations ----------
            var yesLabels = CollectYesLabels(request.Recommendations);
            _logger.LogInformation("[Enrich] YesLabels={Labels}", string.Join(",", yesLabels));

            var rows = await _recommendationRepo.GetByLabelsAsync(yesLabels);

            foreach (var row in rows)
            {
                var dto = new RecommendationDetailDto
                {
                    Label = row.RecommenderLabel,
                    Description = row.Description,
                    // Recommendation = NormalizeBullets(row.Recommendation),
                    Recommendation = row.Recommendation,
                    ConfigurationTemplate = row.ConfigTemplate
                };

                _logger.LogInformation("[Fill] BEFORE: Label={Label}, Template={Template}", dto.Label, dto.ConfigurationTemplate);

                dto.ConfigurationTemplate = await FillPlaceholdersAsync(dto.ConfigurationTemplate, row.RecommenderLabel, resp, request);

                _logger.LogInformation("[Fill] AFTER : Label={Label}, Template={Template}", dto.Label, dto.ConfigurationTemplate);

                resp.EnrichedRecommendations.Add(dto);
            }

            return resp;
        }

        private static bool IsYes(string s) =>
            !string.IsNullOrWhiteSpace(s) &&
            s.Equals("Yes", StringComparison.OrdinalIgnoreCase);

        private static int MapProvider(string provider) =>
            provider?.Equals("Azure", StringComparison.OrdinalIgnoreCase) == true ? 2 :
            provider?.Equals("AWS", StringComparison.OrdinalIgnoreCase) == true ? 1 : 0;

        private static IEnumerable<string> CollectYesLabels(RecommendationResponse flags)
        {
            var yes = new List<string>();
            void Try(string label, string val) { if (string.Equals(val, "Yes", StringComparison.OrdinalIgnoreCase)) yes.Add(label); }

            Try(nameof(flags.SwitchToRenewablePower), flags.SwitchToRenewablePower);
            Try(nameof(flags.ChangeHostingRegion), flags.ChangeHostingRegion);
            Try(nameof(flags.ScheduleWorkloads), flags.ScheduleWorkloads);
            Try(nameof(flags.RightSizeInstances), flags.RightSizeInstances);
            Try(nameof(flags.ConsolidateWorkloads), flags.ConsolidateWorkloads);
            Try(nameof(flags.SoftwareEfficiency), flags.SoftwareEfficiency);
            Try(nameof(flags.ExtendHardwareLife), flags.ExtendHardwareLife);
            Try(nameof(flags.ReduceStorageFootprint), flags.ReduceStorageFootprint);
            Try(nameof(flags.ShutdownPolicies), flags.ShutdownPolicies);
            Try(nameof(flags.MoveToServerless), flags.MoveToServerless);

            return yes;
        }

        // private static string NormalizeBullets(string s)
        // {
        //     if (string.IsNullOrWhiteSpace(s)) return s;
        //     var items = s.Split('~', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
        //     return string.Join(Environment.NewLine, items.Select(i => "• " + i));
        // }

        /// <summary>
        /// Fills config templates with values and appends multi-suggestion tables where applicable.
        /// Uses named-token replacement first, then falls back to bare {} (in order).
        /// </summary>
        private async Task<string> FillPlaceholdersAsync(string template, string label, OptimizationResponse resp, OptimizationRequest request)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                _logger.LogWarning("[Fill] Empty template for label={Label}", label);
                return template;
            }

            _logger.LogInformation("[Fill] Enter label={Label}", label);

            // ---- RightSizeInstances: fill header, then append all options as a table ----
            if (label.Equals("RightSizeInstances", StringComparison.OrdinalIgnoreCase) && resp.RightSizeOptions.Any())
            {
                var best = resp.RightSizeOptions.First();
                _logger.LogInformation("[Fill-RS] Using best={@Best}", best);

                // Named placeholders
                template = Replace(template, "CurrentType", best.CurrentInstanceTypeName);
                template = Replace(template, "vCPUs", best.CurrentvCPUs.ToString());
                template = Replace(template, "RAM", best.CurrentRAM_GB.ToString());
                template = Replace(template, "RecommendedType", best.RecommendedInstanceTypeName);
                template = Replace(template, "vCPUs", best.RecommendedvCPUs.ToString());
                template = Replace(template, "RAM", best.RecommendedRAM_GB.ToString());
                template = Replace(template, "CostSavings", $"{best.HourlyCostSavings:0.00} {best.Currency}/hour");

                // Fallback for bare {}
                template = ReplaceBareBraces(template,
                    best.CurrentInstanceTypeName,
                    best.CurrentvCPUs.ToString(),
                    best.CurrentRAM_GB.ToString(),
                    best.RecommendedInstanceTypeName,
                    best.RecommendedvCPUs.ToString(),
                    best.RecommendedRAM_GB.ToString(),
                    $"{best.HourlyCostSavings:0.00} {best.Currency}/hour");

                // Append multi-suggestion block (table-like with tabs)
                var sb = new StringBuilder();
                sb.AppendLine(template);
                sb.AppendLine();
                sb.AppendLine("Suggested right-size options:");
                sb.AppendLine("Instance Type\tHourly Cost Savings (USD)");

                foreach (var opt in resp.RightSizeOptions)
                {
                    sb.AppendLine($"{opt.RecommendedInstanceTypeName}\t{opt.HourlyCostSavings:0.00}");
                }

                return sb.ToString();
            }

            // ---- ChangeHostingRegion: fill header with FIRST region, then append all regions with savings (KgCO2e) ----
            if (label.Equals("ChangeHostingRegion", StringComparison.OrdinalIgnoreCase) && resp.RegionOptimizationOptions.Any())
            {
                var bestRegion = resp.RegionOptimizationOptions.First();
                _logger.LogInformation("[Fill-Region] Using best={@BestRegion}, Energy={Energy}", bestRegion, request.OperationalEnergyKwh);

                // Compute savings for header (first region)
                var currentEmissionHdr = bestRegion.CurrentGridFactor_gCO2ePerKWh * request.OperationalEnergyKwh;
                var recommendedEmissionHdr = bestRegion.RecommendedGridFactor_gCO2ePerKWh * request.OperationalEnergyKwh;
                var savingHdrKgCO2e = currentEmissionHdr - recommendedEmissionHdr; // grams -> kg

                // Named placeholders
                template = Replace(template, "CurrentRegion", bestRegion.CurrentRegionName);
                template = Replace(template, "RecommendedRegion", bestRegion.RecommendedRegionName);
                template = Replace(template, "EmissionSavings", $"{savingHdrKgCO2e:0.00}");

                // Fallback for bare {}
                template = ReplaceBareBraces(template,
                    bestRegion.CurrentRegionName,
                    bestRegion.RecommendedRegionName,
                    $"{savingHdrKgCO2e:0.00}");

                // Append multi-suggestion block (table-like with tabs)
                var sb = new StringBuilder();
                sb.AppendLine(template);
                sb.AppendLine();
                sb.AppendLine("Switch to any of the following regions to bring about emission savings:");
                sb.AppendLine("Region\t Hourly Savings (gCO2e)");

                foreach (var r in resp.RegionOptimizationOptions)
                {
                    var currentEmission = r.CurrentGridFactor_gCO2ePerKWh * request.OperationalEnergyKwh;
                    var recommendedEmission = r.RecommendedGridFactor_gCO2ePerKWh * request.OperationalEnergyKwh;
                    var savingKg = currentEmission - recommendedEmission; // grams -> kg

                    sb.AppendLine($"{r.RecommendedRegionName}\t{savingKg:0.00}");
                }

                return sb.ToString();
            }

            // ---- ShutdownPolicies: fill tokens (no table append) ----
            if (label.Equals("ShutdownPolicies", StringComparison.OrdinalIgnoreCase))
            {
                var dailyConsumption = request.OperationalEnergyKwh * 24;
                var hourlySaving = request.OperationalEnergyKwh;

                _logger.LogInformation("[Fill-Shutdown] daily={Daily}, hourly={Hourly}", dailyConsumption, hourlySaving);

                template = Replace(template, "DailyConsumption", $"{dailyConsumption:0.00}");
                template = Replace(template, "HourlySaving", $"{hourlySaving:0.00}");

                // Fallback for bare {}
                template = ReplaceBareBraces(template,
                    $"{dailyConsumption:0.00}",
                    $"{hourlySaving:0.00}");

                return template;
            }

            // For all other labels, just return the original template
            _logger.LogInformation("[Fill] No placeholder logic for label={Label}. Returning template unchanged.", label);
            return template;
        }

        /// <summary>
        /// Named placeholder replacement: replaces {Token} (case-insensitive) with value.
        /// </summary>
        private static string Replace(string src, string token, string value)
        {
            return Regex.Replace(
                src ?? string.Empty,
                @"\{\s*" + Regex.Escape(token) + @"\s*\}",
                value ?? string.Empty,
                RegexOptions.IgnoreCase
            );
        }

        /// <summary>
        /// Fallback for templates that use bare {} with no token names.
        /// Replaces one {} at a time, in order, with supplied values (count = 1 per value).
        /// </summary>
        private static string ReplaceBareBraces(string src, params string[] values)
        {
            var result = src ?? string.Empty;
            if (values == null || values.Length == 0) return result;

            var regex = new Regex(@"\{\s*\}");

            foreach (var v in values)
            {
                result = regex.Replace(result, v ?? string.Empty, 1);
            }
            return result;
        }
    }
}
