// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using SCIMetricAPI.Data;
// using SCIMetricAPI.Models;

// [Route("api/cpuinfo")]
// [ApiController]
// public class CPUInfoController : ControllerBase
// {
//     private readonly ApplicationDbContext _context;

//     public CPUInfoController(ApplicationDbContext context)
//     {
//         _context = context;
//     }

//      [HttpGet("{hardwarevendorid}")]
//     public async Task<ActionResult<IEnumerable<CpuInfo>>> GetcpuInfobyVendorId(int hardwarevendorid)
//     {
//         return await _context.CpuInfo.Where(i => i.HId == hardwarevendorid).ToListAsync();
//     }    
    
// }
