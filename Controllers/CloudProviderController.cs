using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;

[Route("api/cloudproviders")]
[ApiController]
public class CloudProviderController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    
    public CloudProviderController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CloudProvider>>> GetCloudProviders()
    {
        return await _context.CloudProviders.ToListAsync();
    }
}
