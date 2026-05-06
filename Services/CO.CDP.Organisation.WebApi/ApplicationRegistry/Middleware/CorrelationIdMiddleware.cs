namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Middleware;

public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.ContainsKey(CorrelationIdHeader))
        {
            context.Request.Headers.Append(CorrelationIdHeader, Guid.NewGuid().ToString());
        }

        var correlationId = context.Request.Headers[CorrelationIdHeader].ToString();
        context.Response.Headers.Append(CorrelationIdHeader, correlationId);

        using (context.RequestServices.GetRequiredService<ILoggerFactory>()
            .CreateLogger<CorrelationIdMiddleware>()
            .BeginScope(new Dictionary<string, object> { ["CorrelationId"] = correlationId }))
        {
            await _next(context);
        }
    }
}
