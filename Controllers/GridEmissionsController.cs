using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;

[Route("api/gridemissions")]
[ApiController]
public class GridEmissionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public GridEmissionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("{providerId}")]
    public async Task<ActionResult<IEnumerable<GridEmission>>> GetEmissionsByProvider(int providerId)
    {
        return await _context.GridEmissions.AsNoTracking().Where(g => g.ProviderId == providerId).ToListAsync();
    }
}
