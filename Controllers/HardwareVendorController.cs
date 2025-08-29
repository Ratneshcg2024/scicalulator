// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using SCIMetricAPI.Data;
// using SCIMetricAPI.Models;

// [Route("api/hardwarevendor")]
// [ApiController]
// public class HardwareVendorController : ControllerBase
// {
//     private readonly ApplicationDbContext _context;
    
//     public HardwareVendorController(ApplicationDbContext context)
//     {
//         _context = context;
//     }

//     [HttpGet]
//     public async Task<ActionResult<IEnumerable<HardwareVendor>>> GetHardwareVendor()
//     {
//         return await _context.HardwareVendor.ToListAsync();
//     }
// }
