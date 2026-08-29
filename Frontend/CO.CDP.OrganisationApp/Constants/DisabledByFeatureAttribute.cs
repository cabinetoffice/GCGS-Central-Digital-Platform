using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.FeatureManagement;

namespace CO.CDP.OrganisationApp.Constants;

/// <summary>
/// Page filter that returns 404 when the named feature flag is enabled.
/// Inverse of [FeatureGate]: use this to disable legacy pages that have been
/// superseded by a separate service when a migration feature flag is on.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class DisabledByFeatureAttribute(string featureName) : Attribute, IAsyncPageFilter
{
    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var featureManager = context.HttpContext.RequestServices.GetRequiredService<IFeatureManager>();

        if (await featureManager.IsEnabledAsync(featureName))
        {
            context.Result = new NotFoundResult();
            return;
        }

        await next();
    }

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;
}
