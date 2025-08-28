using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class CustomAuthorizationMiddlewareResultHandler(IServiceScopeFactory serviceScopeFactory)
    : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    private static readonly HashSet<string> AllowedOrigins =
    [
        "buyer-view",
        "overview"
    ];

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
                    var origin = GetSecureOrigin(context);
                    var redirectUrl = $"/organisation/{organisationId}/not-signed-memorandum?origin={Uri.EscapeDataString(origin)}";
                    context.Response.Redirect(redirectUrl);
                    return;
                }
            }

            context.Response.Redirect("/page-not-found");
            return;
        }

        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    private string GetSecureOrigin(HttpContext context)
    {
        var requestedOrigin = context.Request.Query["origin"].ToString();

        if (!string.IsNullOrEmpty(requestedOrigin) && AllowedOrigins.Contains(requestedOrigin))
        {
            return requestedOrigin;
        }

        return "overview";
    }

    private Guid? TryGetOrganisationId()
    {
        try
        {
            using var serviceScope = serviceScopeFactory.CreateScope();
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
