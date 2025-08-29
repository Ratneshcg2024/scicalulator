using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;

[Route("api/instances")]
[ApiController]
public class InstanceTypeController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public InstanceTypeController(ApplicationDbContext context)
    {
        _context = context;
    }

     [HttpGet("{cloudProviderId}")]
    public async Task<ActionResult<IEnumerable<InstanceType>>> GetInstancesByProvider(int cloudProviderId)
    {
        return await _context.InstanceTypes.Where(i => i.CloudProviderId == cloudProviderId).ToListAsync();
    }

     [HttpGet("cpucores/{id}")]
    public async Task<ActionResult<IEnumerable<InstanceType>>> getmaxcpucores(int id)
    {
        var instance =await _context.InstanceTypes.FindAsync(id);
        if (instance==null){
            return NotFound();
        }
        return Ok(instance.CPUCoresAvailable);
        //return await _context.InstanceTypes.Where(i => i.CloudProviderId == cloudProviderId).ToListAsync();
    }
    
    
}
