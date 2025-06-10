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
        if (context.User.HasClaim(c => c.Type == ClaimType.ApiKeyScope))
        {
            var apiKeyScopesClaim = context.User.FindFirst(c => c.Type == ClaimType.ApiKeyScope);
            if (apiKeyScopesClaim != null && !string.IsNullOrEmpty(apiKeyScopesClaim.Value))
            {
                var userScopes = apiKeyScopesClaim.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (requirement.RequiredScopes.Any(requiredScope => userScopes.Contains(requiredScope)))
                {
                    context.Succeed(requirement);
                }
            }
        }
        return Task.CompletedTask;
    }
}