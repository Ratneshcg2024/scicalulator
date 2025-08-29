using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCIMetricAPI.Data;
using SCIMetricAPI.Models;

[Route("api/countrygridemissions")]
[ApiController]
public class CountryGridEmissionController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CountryGridEmissionController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<CountryEmission>>> GetCountryemission()
    {
        return await _context.CountryGridEmissions.ToListAsync();
    }
}
