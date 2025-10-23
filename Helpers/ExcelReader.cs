using ClosedXML.Excel;
using SCIMetricAPI.Models;

namespace SCIMetricAPI.Helpers
{
    public static class ExcelReader
    {
        public static byte[] WriteResultsToExcel(List<SciExcelModel> inputModels, List<SciResultModel> results)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("SCI Results");

            // Write headers
            worksheet.Cell(1, 1).Value = "App name";
            worksheet.Cell(1, 2).Value = "Processor_name";
            worksheet.Cell(1, 3).Value = "Cpu_utilization %";
            worksheet.Cell(1, 4).Value = "memory_utilization mb";
            worksheet.Cell(1, 5).Value = "storage_type";
            worksheet.Cell(1, 6).Value = "Storage_available/used";
            worksheet.Cell(1, 7).Value = "total_vcpu";
            worksheet.Cell(1, 8).Value = "vcpu_used";
            worksheet.Cell(1, 9).Value = "ram_capacity";
            worksheet.Cell(1, 10).Value = "region";
            worksheet.Cell(1, 11).Value = "duration";
            worksheet.Cell(1, 12).Value = "E(kwh)";
            worksheet.Cell(1, 13).Value = "OperationalEmission(gCo2e)";
            worksheet.Cell(1, 14).Value = "EmbodiedEmission(gco2e)";
            worksheet.Cell(1, 15).Value = "Gridemission";
            worksheet.Cell(1, 16).Value = "SCI score";

            for (int i = 0; i < inputModels.Count; i++)
            {
                var model = inputModels[i];
                var result = results[i];
                int row = i + 2;

                worksheet.Cell(row, 1).Value = model.AppName;
                worksheet.Cell(row, 2).Value = model.ProcessorName;
                worksheet.Cell(row, 3).Value = model.CpuUtilization;
                worksheet.Cell(row, 4).Value = model.MemoryUtilization;
                worksheet.Cell(row, 5).Value = model.StorageType;
                worksheet.Cell(row, 6).Value = model.StorageUsedGB;
                worksheet.Cell(row, 7).Value = model.TotalVcpu;
                worksheet.Cell(row, 8).Value = model.VcpuUsed;
                worksheet.Cell(row, 9).Value = model.RamCapacityGB;
                worksheet.Cell(row, 10).Value = model.region;
                worksheet.Cell(row, 11).Value = model.duration;
                worksheet.Cell(row, 12).Value = result.TotalOperationalEnergy;
                worksheet.Cell(row, 13).Value = result.TotalOperationalEmissions;
                worksheet.Cell(row, 14).Value = result.TotalEmbodiedEmissions;
                worksheet.Cell(row, 15).Value = result.GridEmissionFactorUsed;
                worksheet.Cell(row, 16).Value = result.SCIvalue;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public static List<SciExcelModel> ReadExcel(Stream fileStream)
        {
            var result = new List<SciExcelModel>();
            using var workbook = new XLWorkbook(fileStream);
            var worksheet = workbook.Worksheet(1);
            var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Skip header

            foreach (var row in rows)
            {
                try
                {
                    var model = new SciExcelModel
                    {
                        AppName = row.Cell(1).GetString(),
                        ProcessorName = row.Cell(2).GetString(),
                        CpuUtilization = row.Cell(3).GetValue<double>(),
                        memoryUtilization_unit = row.Cell(4).GetString(), // Assuming this is the unit, adjust as necessary
                        MemoryUtilization = row.Cell(5).GetValue<double>(),
                        StorageType = row.Cell(6).GetString(),
                        StorageUsedGB = row.Cell(7).GetValue<double>(),
                        TotalVcpu = row.Cell(8).GetValue<int>(),
                        VcpuUsed = row.Cell(9).GetValue<int>(),
                        RamCapacityGB = row.Cell(10).GetValue<double>(),
                        region = row.Cell(11).GetString(),
                        duration_unit=row.Cell(12).GetString(), // Assuming this is the unit, adjust as necessary
                        duration = row.Cell(13).GetValue<int>()
                    };

                    result.Add(model);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ExcelReader] Skipping row {row.RowNumber()} due to error: {ex.Message}");
                    continue;
                }
            }

            return result;
        }
    }
}
