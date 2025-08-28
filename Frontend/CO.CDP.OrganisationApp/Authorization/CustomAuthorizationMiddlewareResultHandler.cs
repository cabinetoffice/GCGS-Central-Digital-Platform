using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class CustomAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public CustomAuthorizationMiddlewareResultHandler(IServiceScopeFactory serviceScopeFactory)
    {
        this._serviceScopeFactory = serviceScopeFactory;
    }

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        if (authorizeResult.Forbidden)
        {
            if (policy.Requirements.Any(r => r is BuyerMouRequirement))
            {
                var organisationId = TryGetOrganisationId();
                if (organisationId.HasValue)
                {
                    var origin = context.Request.Query["origin"].ToString();
                    var redirectUrl = string.IsNullOrEmpty(origin) 
                        ? $"/organisation/{organisationId}/not-signed-memorandum"
                        : $"/organisation/{organisationId}/not-signed-memorandum?origin={Uri.EscapeDataString(origin)}";
                    context.Response.Redirect(redirectUrl);
                    return;
                }
            }

            context.Response.Redirect("/page-not-found");
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    private Guid? TryGetOrganisationId()
    {
        try
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var userInfoService = serviceScope.ServiceProvider.GetRequiredService<IUserInfoService>();
            userInfoService.GetUserInfo().Wait();
            return userInfoService.GetOrganisationId();
        }
        catch
        {
            return null;
        }
    }
}
