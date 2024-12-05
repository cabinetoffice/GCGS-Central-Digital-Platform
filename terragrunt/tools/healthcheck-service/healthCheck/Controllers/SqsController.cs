using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using healthCheck.HealthChecks;

namespace healthCheck.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SqsController : ControllerBase
    {
        private readonly SqsHealthCheck _sqsHealthCheck;

        public SqsController(SqsHealthCheck sqsHealthCheck)
        {
            _sqsHealthCheck = sqsHealthCheck;
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestQueueHealth()
        {
            var healthCheckResult = await _sqsHealthCheck.CheckHealthAsync(new HealthCheckContext());

            var version = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
                .InformationalVersion ?? "Unknown";

            var result = new
            {
                check_version = version,
                status = healthCheckResult.Status.ToString(),
                description = healthCheckResult.Description,
                data = healthCheckResult.Data
            };

            return Ok(result);
        }
    }
}
