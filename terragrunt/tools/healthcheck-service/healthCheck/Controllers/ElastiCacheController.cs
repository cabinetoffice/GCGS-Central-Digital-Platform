using System.Reflection;
using healthCheck.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace healthCheck.Controllers;

[Route("elasticache")]
public class ElastiCacheController : ControllerBase
{
    private readonly ElastiCacheHealthCheck _healthCheck;

    public ElastiCacheController(ElastiCacheHealthCheck healthCheck)
    {
        _healthCheck = healthCheck;
    }

    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        var result = await _healthCheck.CheckHealthAsync(new HealthCheckContext());

        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion ?? "Unknown";

        return Ok(new
        {
            check_version = version,
            status = result.Status.ToString(),
            description = result.Description,
            data = result.Data
        });
    }
}
