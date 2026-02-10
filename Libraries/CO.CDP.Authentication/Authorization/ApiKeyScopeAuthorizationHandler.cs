using Microsoft.AspNetCore.Authorization;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication.Authorization;

/// <summary>
/// Handles the <see cref="ApiKeyScopeAuthorizationRequirement"/> for API key scope authorization.
/// This handler grants standard API access to service keys and checks for specific scopes to grant privileged access.
/// </summary>
public class ApiKeyScopeAuthorizationHandler : AuthorizationHandler<ApiKeyScopeAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyScopeAuthorizationRequirement requirement)
    {
        var channelClaimValue = context.User.FindFirst(c => c.Type == ClaimType.Channel)?.Value;
        var apiKeyChannels = new[] { Channel.ServiceKey, Channel.OrganisationKey };

        if (channelClaimValue == null || !apiKeyChannels.Contains(channelClaimValue))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (!requirement.RequiredScopes.Any())
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var userApiKeyScopesClaim = context.User.FindFirst(c => c.Type == ClaimType.ApiKeyScope)?.Value;
        if (!string.IsNullOrEmpty(userApiKeyScopesClaim))
        {
            var userScopes = userApiKeyScopesClaim.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (requirement.RequiredScopes.Any(rs => userScopes.Contains(rs)))
            {
                context.Succeed(requirement);
                if (channelClaimValue == Channel.ServiceKey && context.User.Identity is System.Security.Claims.ClaimsIdentity claimsIdentity)
                {
                    claimsIdentity.AddClaim(new System.Security.Claims.Claim("privileged_api_access", "true"));
                }
            }
        }
        return Task.CompletedTask;
    }
}
