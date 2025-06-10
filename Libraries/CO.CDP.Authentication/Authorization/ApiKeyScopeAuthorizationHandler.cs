using Microsoft.AspNetCore.Authorization;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication.Authorization;

/// <summary>
/// Handles the <see cref="ApiKeyScopeAuthorizationRequirement"/> for API key scope authorization.
/// This handler checks if the authenticated API key possesses any of the required scopes.
/// </summary>
public class ApiKeyScopeAuthorizationHandler : AuthorizationHandler<ApiKeyScopeAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyScopeAuthorizationRequirement requirement)
    {
        var channelClaimValue = context.User.FindFirst(c => c.Type == ClaimType.Channel)?.Value;

        var apiKeyChannels = new[] { AuthenticationChannel.ServiceKey.ToString(), AuthenticationChannel.OrganisationKey.ToString() };

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

        var apiKeyScopesClaimValue = context.User.FindFirst(c => c.Type == ClaimType.ApiKeyScope)?.Value;

        if (!string.IsNullOrEmpty(apiKeyScopesClaimValue))
        {
            var userScopes = apiKeyScopesClaimValue.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (requirement.RequiredScopes.Any(userScopes.Contains))
            {
                context.Succeed(requirement);
            }
        }
        return Task.CompletedTask;
    }
}