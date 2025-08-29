using Microsoft.AspNetCore.Mvc;
using SCIMetricAPI.Helpers;
using SCIMetricAPI.Models;
using SCIMetricAPI.Services.Interfaces;

namespace SCIMetricAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExcelSCIController : ControllerBase
    {
        private readonly ISciCalculatorService _sciCalculatorService;

        public ExcelSCIController(ISciCalculatorService sciCalculatorService)
        {
            _sciCalculatorService = sciCalculatorService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            using var stream = file.OpenReadStream();
            var models = ExcelReader.ReadExcel(stream);

            var results = models.Select(model => _sciCalculatorService.CalculateSCI(model)).ToList();

            return Ok(results);
        }
        // Upload Excel file and return results in a new Excel file
        [HttpPost("upload-with-results")]
        public async Task<IActionResult> UploadExcelWithResults(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Invalid file.");

            using var stream = file.OpenReadStream();
            var models = ExcelReader.ReadExcel(stream);
            var results = models.Select(model => _sciCalculatorService.CalculateSCI(model)).ToList();

            var excelBytes = ExcelReader.WriteResultsToExcel(models, results);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SCI_Results.xlsx");
        }

       // uplaod multiple processors
        // [HttpPost("upload-multi-app")]
        // public async Task<IActionResult> UploadMultiAppExcel(IFormFile file)
        // {
        //     if (file == null || file.Length == 0)
        //         return BadRequest("Invalid file.");

        //     using var stream = file.OpenReadStream();
        //     var models = ExcelReader.ReadExcel(stream);

        //     var results = models.Select(model => _sciCalculatorService.CalculateSCI(model)).ToList();

        //     var excelBytes = ExcelReader.WriteResultsToExcel(models, results);

        //     return File(excelBytes, 
        //                 "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
        //                 "SCI_Results_MultiApp.xlsx");
        // }
        [HttpPost("upload-multi-app")]
public async Task<IActionResult> UploadMultiAppExcel(IFormFile file)
{
    if (file == null || file.Length == 0)
        return BadRequest("Invalid file.");

    using var stream = file.OpenReadStream();
    var models = ExcelReader.ReadExcel(stream);

    var results = models.Select(model => _sciCalculatorService.CalculateSCI(model)).ToList();

    var excelBytes = ExcelReader.WriteResultsToExcel(models, results);

    // Ensure the directory exists
    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "files");
    if (!Directory.Exists(folderPath))
        Directory.CreateDirectory(folderPath);

    // Save the file
    var fileName = $"SCI_Results_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
    var filePath = Path.Combine(folderPath, fileName);
    await System.IO.File.WriteAllBytesAsync(filePath, excelBytes);

    // Build the public URL
    var fileUrl = $"{Request.Scheme}://{Request.Host}/files/{fileName}";
    Console.WriteLine($"Saved Excel file at: {filePath}");
    Console.WriteLine($"Accessible via: {fileUrl}");


    return Ok(new { downloadUrl = fileUrl });
}



    }
}
