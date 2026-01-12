// using ClosedXML.Excel;
// using SCIMetricAPI.Models;

// namespace SCIMetricAPI.Helpers
// {
//     public static class ExcelReader
//     {
//         public static byte[] WriteResultsToExcel(List<SciExcelModel> inputModels, List<SciResultModel> results)
//         {
//             using var workbook = new XLWorkbook();
//             var worksheet = workbook.Worksheets.Add("SCI Results");

//             // Write headers
//             worksheet.Cell(1, 1).Value = "App name";
//             worksheet.Cell(1, 2).Value = "Processor_name";
//             worksheet.Cell(1, 3).Value = "Cpu_utilization %";
//             worksheet.Cell(1, 4).Value = "memory_utilization mb";
//             worksheet.Cell(1, 5).Value = "storage_type";
//             worksheet.Cell(1, 6).Value = "Storage_available/used";
//             worksheet.Cell(1, 7).Value = "total_vcpu";
//             worksheet.Cell(1, 8).Value = "vcpu_used";
//             worksheet.Cell(1, 9).Value = "ram_capacity";
//             worksheet.Cell(1, 10).Value = "region";
//             worksheet.Cell(1, 11).Value = "duration";
//             worksheet.Cell(1, 12).Value = "E(kwh)";
//             worksheet.Cell(1, 13).Value = "OperationalEmission(gCo2e)";
//             worksheet.Cell(1, 14).Value = "EmbodiedEmission(gco2e)";
//             worksheet.Cell(1, 15).Value = "Gridemission";
//             worksheet.Cell(1, 16).Value = "SCI score";

//             for (int i = 0; i < inputModels.Count; i++)
//             {
//                 var model = inputModels[i];
//                 var result = results[i];
//                 int row = i + 2;

//                 worksheet.Cell(row, 1).Value = model.AppName;
//                 worksheet.Cell(row, 2).Value = model.ProcessorName;
//                 worksheet.Cell(row, 3).Value = model.CpuUtilization;
//                 worksheet.Cell(row, 4).Value = model.MemoryUtilization;
//                 worksheet.Cell(row, 5).Value = model.StorageType;
//                 worksheet.Cell(row, 6).Value = model.StorageUsedGB;
//                 worksheet.Cell(row, 7).Value = model.TotalVcpu;
//                 worksheet.Cell(row, 8).Value = model.VcpuUsed;
//                 worksheet.Cell(row, 9).Value = model.RamCapacityGB;
//                 worksheet.Cell(row, 10).Value = model.region;
//                 worksheet.Cell(row, 11).Value = model.duration;
//                 worksheet.Cell(row, 12).Value = result.TotalOperationalEnergy;
//                 worksheet.Cell(row, 13).Value = result.TotalOperationalEmissions;
//                 worksheet.Cell(row, 14).Value = result.TotalEmbodiedEmissions;
//                 worksheet.Cell(row, 15).Value = result.GridEmissionFactorUsed;
//                 worksheet.Cell(row, 16).Value = result.SCIvalue;
//             }

//             using var stream = new MemoryStream();
//             workbook.SaveAs(stream);
//             return stream.ToArray();
//         }

//         public static List<SciExcelModel> ReadExcel(Stream fileStream)
//         {
//             var result = new List<SciExcelModel>();
//             using var workbook = new XLWorkbook(fileStream);
//             var worksheet = workbook.Worksheet(1);
//             var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

//             foreach (var row in rows)
//             {
//                 try
//                 {
//                     var model = new SciExcelModel
//                     {
//                         AppName = row.Cell(1).GetString(),
//                         ProcessorName = row.Cell(2).GetString(),
//                         CpuUtilization = row.Cell(3).GetValue<double>(),
//                         memoryUtilization_unit = row.Cell(4).GetString(), // Assuming this is the unit, adjust as necessary
//                         MemoryUtilization = row.Cell(5).GetValue<double>(),
//                         StorageType = row.Cell(6).GetString(),
//                         StorageUsedGB = row.Cell(7).GetValue<double>(),
//                         TotalVcpu = row.Cell(8).GetValue<int>(),
//                         VcpuUsed = row.Cell(9).GetValue<int>(),
//                         RamCapacityGB = row.Cell(10).GetValue<double>(),
//                         region = row.Cell(11).GetString(),
//                         duration_unit=row.Cell(12).GetString(), // Assuming this is the unit, adjust as necessary
//                         duration = row.Cell(13).GetValue<int>()
//                     };

//                     result.Add(model);
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine($"[ExcelReader] Skipping row {row.RowNumber()} due to error: {ex.Message}");
//                     continue;
//                 }
//             }

//             return result;
//         }
//     }
// }


using ClosedXML.Excel;
using SCIMetricAPI.Models;
using SCIMetricAPI.DTOs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SCIMetricAPI.Helpers
{
    public static class ExcelReader
    {
        // -----------------------
        // READ: unchanged (with your columns + defaulting vCPU used)
        // -----------------------
        public static List<SciExcelModel> ReadExcel(Stream fileStream)
        {
            var result = new List<SciExcelModel>();
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1);

            foreach (var row in rows)
            {
                try
                {
                    var model = new SciExcelModel
                    {
                        AppName                = row.Cell(1).GetString(),
                        AppCriticality         = row.Cell(2).GetString(),
                        DeploymentType         = row.Cell(3).GetString(),
                        CloudProvider          = row.Cell(4).GetString(),
                        ProcessorName          = row.Cell(5).GetString(),
                        InstanceCount          = SafeParseInt(row.Cell(6).GetString()),
                        AppCountPerInstance    = SafeParseInt(row.Cell(7).GetString()),
                        duration_unit          = row.Cell(8).GetString(),
                        duration               = SafeParseDouble(row.Cell(9).GetString()),
                        CpuUtilization         = SafeParseDouble(row.Cell(10).GetString()),
                        memoryUtilization_unit = row.Cell(11).GetString(),
                        MemoryUtilization      = SafeParseDouble(row.Cell(12).GetString()),
                        StorageType            = row.Cell(13).GetString(),
                        StorageUsedGB          = SafeParseDouble(row.Cell(14).GetString()),
                        TotalVcpu              = SafeParseInt(row.Cell(15).GetString()),
                        VcpuUsed               = SafeParseInt(row.Cell(16).GetString()) == 0 ? SafeParseInt(row.Cell(15).GetString()) : SafeParseInt(row.Cell(16).GetString()),
                        RamCapacityGB          = SafeParseDouble(row.Cell(17).GetString()),
                        region                 = row.Cell(18).GetString()
                    };
                    Console.WriteLine($"[ExcelReader] Row {row.RowNumber()} Parsed:");
                    Console.WriteLine($"AppName={model.AppName}, Criticality={model.AppCriticality}, Deployment={model.DeploymentType}, Provider={model.CloudProvider}, AppCountPerInstance={model.AppCountPerInstance}, Processor={model.ProcessorName}, CPU={model.CpuUtilization}, Mem={model.MemoryUtilization} {model.memoryUtilization_unit}, Storage={model.StorageUsedGB}, vCPU={model.VcpuUsed}/{model.TotalVcpu}, RAM={model.RamCapacityGB}, Region={model.region}, Duration={model.duration} {model.duration_unit}");
                    result.Add(model);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ExcelReader] Skipping row {row.RowNumber()} due to error: {ex.Message}");
                }
            }
            return result;
        }

        // -----------------------
        // WRITE: multi-sheet (SCI + Recommendations + Details)
        // -----------------------
 

   public static byte[] WriteResultsAndRecommendationsToExcel(
            List<SciExcelModel> inputModels,
            List<SciResultModel> results,
            List<AppRecommendationAggregate> aggregates)
        {
            using var workbook = new XLWorkbook();

            // -----------------------------------------------------------------
            // SHEET 1: SCI Results
            // -----------------------------------------------------------------
            var wsResults = workbook.Worksheets.Add("SCI Results");
            string[] resultsHeaders = {
                "App name","App Criticality","DeploymentType","CloudProvider","Processor_name",
                "duration","duration_unit","Cpu_utilization %","memory_utilization","memory_unit",
                "storage_type","Storage_used GB","total_vcpu","vcpu_used","ram_capacity in GB",
                "region","E(kWh)","Operational Emissions(gCO₂e)","Embodied Emissions(gCO₂e)",
                "GridEmissions","Software Carbon Intensity"
            };
            for (int i = 0; i < resultsHeaders.Length; i++)
                wsResults.Cell(1, i + 1).Value = resultsHeaders[i];
            ApplyHeaderStyle(1, 1, resultsHeaders.Length, wsResults);

            int resRow = 2;
            for (int i = 0; i < results.Count; i++)
            {
                var model = inputModels[i];
                var r = results[i];

                wsResults.Cell(resRow, 1).Value  = model.AppName;
                wsResults.Cell(resRow, 2).Value  = model.AppCriticality;
                wsResults.Cell(resRow, 3).Value  = model.DeploymentType;
                wsResults.Cell(resRow, 4).Value  = model.CloudProvider;
                wsResults.Cell(resRow, 5).Value  = model.ProcessorName;
                wsResults.Cell(resRow, 6).Value  = model.duration;
                wsResults.Cell(resRow, 7).Value  = model.duration_unit;
                wsResults.Cell(resRow, 8).Value  = model.CpuUtilization;
                wsResults.Cell(resRow, 9).Value  = model.MemoryUtilization;
                wsResults.Cell(resRow,10).Value  = model.memoryUtilization_unit;
                wsResults.Cell(resRow,11).Value  = model.StorageType;
                wsResults.Cell(resRow,12).Value  = model.StorageUsedGB;
                wsResults.Cell(resRow,13).Value  = r.TotalVcpu != 0 ? r.TotalVcpu : model.TotalVcpu;
                wsResults.Cell(resRow,14).Value  = r.VcpuUsed != 0 ? r.VcpuUsed : model.VcpuUsed;
                wsResults.Cell(resRow,15).Value  = r.RamCapacityGB != 0 ? r.RamCapacityGB : model.RamCapacityGB;
                wsResults.Cell(resRow,16).Value  = model.region;
                wsResults.Cell(resRow,17).Value  = r.TotalOperationalEnergy;
                wsResults.Cell(resRow,18).Value  = r.TotalOperationalEmissions;
                wsResults.Cell(resRow,19).Value  = r.TotalEmbodiedEmissions;
                wsResults.Cell(resRow,20).Value  = r.GridEmissionFactorUsed;
                wsResults.Cell(resRow,21).Value  = r.SCIvalue;
                resRow++;
            }

            // Highlight last 5 metric columns
            wsResults.Range(1, 17, resRow - 1, 21).Style.Fill.BackgroundColor = XLColor.YellowGreen;

            // 🔸 UNIFORM ALIGNMENT: left + top across all data columns
            ApplyUniformAlignment(wsResults, 2, resRow - 1, 1, resultsHeaders.Length, wrapColumns: Array.Empty<int>());

            wsResults.Columns().AdjustToContents();

            // -----------------------------------------------------------------
            // SHEET 2: Recommendations (only "Yes")
            // -----------------------------------------------------------------
            var wsRecs = workbook.Worksheets.Add("Recommendations");
            string[] recHeaders = {
                "App name","DeploymentType","CloudProvider","Region","SCI",
                "Recommendation","Recommendation Details"
            };
            for (int i = 0; i < recHeaders.Length; i++)
                wsRecs.Cell(1, i + 1).Value = recHeaders[i];
            ApplyHeaderStyle(1, 1, recHeaders.Length, wsRecs);

            static string GetFlag(AppRecommendationAggregate a, string label)
            {
                var prop = typeof(RecommendationResponse).GetProperty(label);
                return (string)prop?.GetValue(a.Flags);
            }
            static string GetDescription(AppRecommendationAggregate a, string label)
            {
                var desc = a.Optimization?.EnrichedRecommendations?.FirstOrDefault(x => x.Label == label)?.Description;
                return string.IsNullOrWhiteSpace(desc) ? label : desc;
            }
            static string GetDbRecommendationBulleted(AppRecommendationAggregate a, string label)
            {
                var rec = a.Optimization?.EnrichedRecommendations?.FirstOrDefault(x => x.Label == label)?.Recommendation;
                return NormalizeBullets(rec ?? string.Empty);
            }

            int rr = 2;
            foreach (var a in aggregates)
            {
                void EmitIfYes(string label)
                {
                    var yesNo = GetFlag(a, label);
                    if (!string.Equals(yesNo, "Yes", StringComparison.OrdinalIgnoreCase)) return;

                    wsRecs.Cell(rr, 1).Value = a.AppName;
                    wsRecs.Cell(rr, 2).Value = a.DeploymentType;
                    wsRecs.Cell(rr, 3).Value = a.CloudProvider;
                    wsRecs.Cell(rr, 4).Value = a.Region;
                    wsRecs.Cell(rr, 5).Value = a.SCI;
                    wsRecs.Cell(rr, 6).Value = GetDescription(a, label);
                    wsRecs.Cell(rr, 7).Value = GetDbRecommendationBulleted(a, label);
                    rr++;
                }

                EmitIfYes("RightSizeInstances");
                EmitIfYes("ChangeHostingRegion");
                EmitIfYes("ScheduleWorkloads");
                EmitIfYes("ShutdownPolicies");
                EmitIfYes("SoftwareEfficiency");
                EmitIfYes("ConsolidateWorkloads");
                EmitIfYes("ExtendHardwareLife");
                EmitIfYes("ReduceStorageFootprint");
                EmitIfYes("MoveToServerless");
                EmitIfYes("SwitchToRenewablePower");
            }

            wsRecs.Columns().AdjustToContents();
            const double MAX_TEXT_WIDTH = 60.0;
            CapColumnWidth(wsRecs, 6, MAX_TEXT_WIDTH); // Description
            CapColumnWidth(wsRecs, 7, MAX_TEXT_WIDTH); // Recommendation Details

            // 🔸 UNIFORM ALIGNMENT: left + top across all data columns; and wrap for text columns (6,7)
            ApplyUniformAlignment(wsRecs, 2, rr - 1, 1, recHeaders.Length, wrapColumns: new[] { 6, 7 });

            // -----------------------------------------------------------------
            // SHEET 3: RightSize Options (conditionally)
            // -----------------------------------------------------------------
            var anyRightSizeYesWithOptions =
                aggregates.Any(a =>
                    string.Equals(GetFlag(a, "RightSizeInstances"), "Yes", StringComparison.OrdinalIgnoreCase) &&
                    (a.Optimization?.RightSizeOptions?.Any() ?? false));

            if (anyRightSizeYesWithOptions)
            {
                var wsRs = workbook.Worksheets.Add("RightSize Options");
                string[] rsHeaders = {
                    "App name","Current Instance","Current vCPU","Current RAM (GB)","Current Hourly Cost (USD)",
                    "Recommended Instance","Recommended vCPU","Recommended RAM (GB)",
                    "Hourly Cost (USD)","Hourly Cost Savings (USD)","Monthly Cost Savings (USD)","Family Key",
                    "Recommendation"
                };
                for (int i = 0; i < rsHeaders.Length; i++)
                    wsRs.Cell(1, i + 1).Value = rsHeaders[i];
                ApplyHeaderStyle(1, 1, rsHeaders.Length, wsRs);

                int rrs = 2;
                foreach (var a in aggregates)
                {
                    var yesRightSize = string.Equals(GetFlag(a, "RightSizeInstances"), "Yes", StringComparison.OrdinalIgnoreCase);
                    var list = yesRightSize ? a.Optimization?.RightSizeOptions : null;
                    if (list == null || !list.Any()) continue;

                    foreach (var o in list)
                    {
                        wsRs.Cell(rrs, 1).Value  = a.AppName;
                        wsRs.Cell(rrs, 2).Value  = o.CurrentInstanceTypeName;
                        wsRs.Cell(rrs, 3).Value  = o.CurrentvCPUs;
                        wsRs.Cell(rrs, 4).Value  = o.CurrentRAM_GB;
                        wsRs.Cell(rrs, 5).Value  = o.CurrentHourlyCost;
                        wsRs.Cell(rrs, 6).Value  = o.RecommendedInstanceTypeName;
                        wsRs.Cell(rrs, 7).Value  = o.RecommendedvCPUs;
                        wsRs.Cell(rrs, 8).Value  = o.RecommendedRAM_GB;
                        wsRs.Cell(rrs, 9).Value  = o.RecommendedHourlyCost;
                        wsRs.Cell(rrs,10).Value  = o.HourlyCostSavings;
                        wsRs.Cell(rrs,11).Value  = o.MonthlyCostSavings;
                        wsRs.Cell(rrs,12).Value  = o.FamilyKey;

                        var rsReco = FormatRightSizeRecommendation(
                            o.CurrentInstanceTypeName, o.CurrentvCPUs, o.CurrentRAM_GB,
                            o.RecommendedInstanceTypeName, o.RecommendedvCPUs, o.RecommendedRAM_GB,
                            o.HourlyCostSavings);
                        wsRs.Cell(rrs, 13).Value = rsReco;
                        rrs++;
                    }
                }

                wsRs.Columns().AdjustToContents();
                CapColumnWidth(wsRs, 13, MAX_TEXT_WIDTH);

                // 🔸 UNIFORM ALIGNMENT: left + top across all data columns; and wrap for Recommendation column (13)
                ApplyUniformAlignment(wsRs, 2, rrs - 1, 1, rsHeaders.Length, wrapColumns: new[] { 13 });
            }

            // -----------------------------------------------------------------
            // SHEET 4: Region Options (conditionally)
            // -----------------------------------------------------------------
            var anyRegionYesWithOptions =
                aggregates.Any(a =>
                    string.Equals(GetFlag(a, "ChangeHostingRegion"), "Yes", StringComparison.OrdinalIgnoreCase) &&
                    (a.Optimization?.RegionOptimizationOptions?.Any() ?? false));

            if (anyRegionYesWithOptions)
            {
                var wsReg = workbook.Worksheets.Add("Region Options");
                string[] regHeaders = {
                    "App name","Current Region","Recommended Region","Country",
                    "Current Grid (gCO2e/kWh)","Recommended Grid (gCO2e/kWh)",
                    "Emission Reduction %","Estimated Hourly Savings (gCO2e)","Recommendation"
                };
                for (int i = 0; i < regHeaders.Length; i++)
                    wsReg.Cell(1, i + 1).Value = regHeaders[i];
                ApplyHeaderStyle(1, 1, regHeaders.Length, wsReg);

                int rrg = 2;
                foreach (var a in aggregates)
                {
                    var yesRegion = string.Equals(GetFlag(a, "ChangeHostingRegion"), "Yes", StringComparison.OrdinalIgnoreCase);
                    var list = yesRegion ? a.Optimization?.RegionOptimizationOptions : null;
                    if (list == null || !list.Any()) continue;

                    foreach (var o in list)
                    {
                        double currentGrid = (double)o.CurrentGridFactor_gCO2ePerKWh;
                        double recommendedGrid = (double)o.RecommendedGridFactor_gCO2ePerKWh;
                        double energy = a.OperationalEnergyKwh;
                        double hourlySaving = (currentGrid - recommendedGrid) * energy;

                        wsReg.Cell(rrg, 1).Value = a.AppName;
                        wsReg.Cell(rrg, 2).Value = o.CurrentRegionName;
                        wsReg.Cell(rrg, 3).Value = o.RecommendedRegionName;
                        wsReg.Cell(rrg, 4).Value = o.Country;
                        wsReg.Cell(rrg, 5).Value = o.CurrentGridFactor_gCO2ePerKWh;
                        wsReg.Cell(rrg, 6).Value = o.RecommendedGridFactor_gCO2ePerKWh;
                        wsReg.Cell(rrg, 7).Value = o.EmissionReductionPercentage;
                        wsReg.Cell(rrg, 8).Value = Math.Max(0, hourlySaving);

                        var regReco = FormatRegionRecommendation(
                            o.CurrentRegionName, o.RecommendedRegionName, hourlySaving);
                        wsReg.Cell(rrg, 9).Value = regReco;
                        rrg++;
                    }
                }

                wsReg.Columns().AdjustToContents();
                CapColumnWidth(wsReg, 9, MAX_TEXT_WIDTH);

                // 🔸 UNIFORM ALIGNMENT: left + top across all data columns; and wrap for Recommendation column (9)
                ApplyUniformAlignment(wsReg, 2, rrg - 1, 1, regHeaders.Length, wrapColumns: new[] { 9 });
            }

            // -----------------------------------------------------------------
            // SHEET 5: Shutdown Policies (conditionally)
            // -----------------------------------------------------------------
            var anyShutdownYes =
                aggregates.Any(a =>
                    string.Equals(GetFlag(a, "ShutdownPolicies"), "Yes", StringComparison.OrdinalIgnoreCase));

            if (anyShutdownYes)
            {
                var wsSd = workbook.Worksheets.Add("Shutdown Policies");
                string[] sdHeaders = { "App name","Description","Recommendations","Savings" };
                for (int i = 0; i < sdHeaders.Length; i++)
                    wsSd.Cell(1, i + 1).Value = sdHeaders[i];
                ApplyHeaderStyle(1, 1, sdHeaders.Length, wsSd);

                int rsd = 2;
                foreach (var a in aggregates)
                {
                    var yes = string.Equals(GetFlag(a, "ShutdownPolicies"), "Yes", StringComparison.OrdinalIgnoreCase);
                    if (!yes) continue;

                    var desc = a.Optimization?.EnrichedRecommendations?.FirstOrDefault(x => x.Label == "ShutdownPolicies")?.Description
                               ?? "Design shutdown policies to minimize resource consumption";
                    var rec  = a.Optimization?.EnrichedRecommendations?.FirstOrDefault(x => x.Label == "ShutdownPolicies")?.Recommendation
                               ?? string.Empty;
                    rec = NormalizeBullets(rec);

                    double hourly = a.OperationalEnergyKwh;
                    double daily  = hourly * 24.0;
                    var savingsText = $"Your current power consumption for a day is {daily:0.##} kwh. " +
                                      $"An hour of shutdown can bring a savings of {hourly:0.##} kwh";

                    wsSd.Cell(rsd, 1).Value = a.AppName;
                    wsSd.Cell(rsd, 2).Value = desc;
                    wsSd.Cell(rsd, 3).Value = rec;
                    wsSd.Cell(rsd, 4).Value = savingsText;
                    rsd++;
                }

                wsSd.Columns().AdjustToContents();
                CapColumnWidth(wsSd, 2, MAX_TEXT_WIDTH);
                CapColumnWidth(wsSd, 3, MAX_TEXT_WIDTH);
                CapColumnWidth(wsSd, 4, MAX_TEXT_WIDTH);

                // 🔸 UNIFORM ALIGNMENT: left + top across all data columns; and wrap for text columns 2–4
                ApplyUniformAlignment(wsSd, 2, rsd - 1, 1, sdHeaders.Length, wrapColumns: new[] { 2, 3, 4 });
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        // ============================
        // Styling & Formatting Helpers
        // ============================

        private static void ApplyHeaderStyle(int headerRow, int firstCol, int lastCol, IXLWorksheet ws)
        {
            var rng = ws.Range(headerRow, firstCol, headerRow, lastCol);
            rng.Style.Font.Bold = true;
            rng.Style.Font.FontSize = 12;
            rng.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            rng.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
            rng.Style.Fill.BackgroundColor = XLColor.LightBlue; // unified header color
        }

        /// <summary>
        /// Apply uniform alignment (Left + Top) across a rectangular block, with optional WrapText for specific columns.
        /// </summary>
        private static void ApplyUniformAlignment(IXLWorksheet ws, int firstDataRow, int lastDataRow, int firstCol, int lastCol, int[] wrapColumns)
        {
            var rng = ws.Range(firstDataRow, firstCol, lastDataRow, lastCol);
            var uniform = rng.Style.Alignment;
            uniform.Horizontal = XLAlignmentHorizontalValues.Left;   // <-- change to Center if you prefer middle alignment
            uniform.Vertical   = XLAlignmentVerticalValues.Top;

            if (wrapColumns != null && wrapColumns.Length > 0)
            {
                foreach (var c in wrapColumns)
                {
                    if (c < firstCol || c > lastCol) continue;
                    ws.Range(firstDataRow, c, lastDataRow, c).Style.Alignment.WrapText = true;
                }
            }
        }

        private static void CapColumnWidth(IXLWorksheet ws, int col, double maxWidth)
        {
            ws.Column(col).Width = Math.Min(ws.Column(col).Width, maxWidth);
        }

        private static string NormalizeBullets(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var parts = s.Split('~', StringSplitOptions.RemoveEmptyEntries)
                         .Select(x => x.Trim())
                         .Where(x => !string.IsNullOrWhiteSpace(x))
                         .Select(x => "• " + x);
            return string.Join(Environment.NewLine, parts);
        }

        private static string FormatRightSizeRecommendation(
            string currentType, int currentVcpu, double currentRamGb,
            string recType, int recVcpu, double recRamGb,
            decimal? hourlySaving)
        {
            var savingTxt = hourlySaving.HasValue ? $"{hourlySaving.Value:0.##} USD/hour" : "N/A";
            return $"Your current instance type is {currentType} with {currentVcpu} vCPUs and {currentRamGb:0.##} GB RAM. " +
                   $"Consider scaling down to {recType} with {recVcpu} vCPUs and {recRamGb:0.##} GB RAM. " +
                   $"This will help reduce emissions and achieve a cost savings of {savingTxt}";
        }

        private static string FormatRegionRecommendation(
            string currentRegion, string recommendedRegion, double hourlySavingGco2e)
        {
            return $"Your current hosting region is {currentRegion}, changing to region {recommendedRegion} " +
                   $"can bring about an emission savings of {hourlySavingGco2e:0.##} gCO2e";
        }
        public static byte[] WriteResultsToExcel(List<SciExcelModel> inputModels, List<SciResultModel> results)
        {
            return WriteResultsAndRecommendationsToExcel(inputModels, results, new List<AppRecommendationAggregate>());
        }

        private static double SafeParseDouble(string value) => double.TryParse(value, out var result) ? result : 0;
        private static int SafeParseInt(string value) => int.TryParse(value, out var result) ? result : 0;
    }
}
