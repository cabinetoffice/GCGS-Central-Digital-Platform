using Microsoft.AspNetCore.Mvc;
using healthCheck.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace healthCheck.Controllers;

[Route("/")]
[ApiController]
public class CompositeMonitoringController : ControllerBase
{
    private readonly ElastiCacheHealthCheck _elastiCacheHealthCheck;
    private readonly SqsHealthCheck _sqsHealthCheck;

    public CompositeMonitoringController(
        ElastiCacheHealthCheck elastiCacheHealthCheck,
        SqsHealthCheck sqsHealthCheck)
    {
        _elastiCacheHealthCheck = elastiCacheHealthCheck;
        _sqsHealthCheck = sqsHealthCheck;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var elastiCacheResult = await _elastiCacheHealthCheck.CheckHealthAsync(new HealthCheckContext());
        var sqsResult = await _sqsHealthCheck.CheckHealthAsync(new HealthCheckContext());

        var response = new
        {
            status = "Monitoring",
            description = "Detailed status of all components",
            checks = new[]
            {
                new
                {
                    name = "ElastiCache",
                    status = elastiCacheResult.Status.ToString(),
                    description = elastiCacheResult.Description,
                    data = elastiCacheResult.Data
                },
                new
                {
                    name = "SQS",
                    status = sqsResult.Status.ToString(),
                    description = sqsResult.Description,
                    data = sqsResult.Data
                }
            }
        };

        return Ok(response);
    }
}
