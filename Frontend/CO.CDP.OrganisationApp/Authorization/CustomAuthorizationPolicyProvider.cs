using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CO.CDP.OrganisationApp.Authorization;

public class CustomAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    const string ORG_POLICY_PREFIX = "OrgScope_";
    const string POLICY_PREFIX = "PersonScope_";
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        var role = extractRoleFromPolicyName(policyName);

        if (role != null)
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new ScopeRequirement(role))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    private static string? extractRoleFromPolicyName(string policyName)
    {
        if (policyName.StartsWith(ORG_POLICY_PREFIX))
        {
            return policyName.Substring(ORG_POLICY_PREFIX.Length);
        }

        if (policyName.StartsWith(POLICY_PREFIX))
        {
            return policyName.Substring(POLICY_PREFIX.Length);
        }

        return null;
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();
}