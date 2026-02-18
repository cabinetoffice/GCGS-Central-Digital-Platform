using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace CO.CDP.UserManagement.Api.FeatureFlags;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class RequireFeatureFlagAttribute(string featureFlagName) : Attribute, IAsyncActionFilter
{
    public string FeatureFlagName { get; } = featureFlagName;

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var configuration = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
        if (!configuration.GetValue(FeatureFlagName, false))
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }
}
