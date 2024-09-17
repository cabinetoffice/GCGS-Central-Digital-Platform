using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CO.CDP.OrganisationApp.Authorization;

public class CustomAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    const string POLICY_PREFIX = "OrgScope_";
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(POLICY_PREFIX))
        {
            var role = policyName.Substring(POLICY_PREFIX.Length);

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new OrganizationScopeRequirement(role))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();
}