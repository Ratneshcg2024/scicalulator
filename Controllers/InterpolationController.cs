using Microsoft.AspNetCore.Mvc;
using SCIMetricAPI.Services.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class InterpolationController : ControllerBase
{
    private readonly ITdpCoefficientRepository _tdpCoefficientRepository;

    public InterpolationController(ITdpCoefficientRepository tdpCoefficientRepository)
    {
        _tdpCoefficientRepository = tdpCoefficientRepository;
    }

    [HttpGet("tdp")]
    public IActionResult GetTdpCoefficient([FromQuery] double cpuUtilization)
    {
        double result = _tdpCoefficientRepository.GetTdpCoefficient(cpuUtilization);
        return Ok(new { TdpCoefficient = result });
    }
}
