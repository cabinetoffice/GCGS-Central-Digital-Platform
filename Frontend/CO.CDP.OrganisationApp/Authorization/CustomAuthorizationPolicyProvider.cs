using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CO.CDP.OrganisationApp.Authorization;

public class CustomAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    const string POLICY_PREFIX = "OrgRolePolicy_";
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(POLICY_PREFIX))
        {
            var role = policyName.Substring(POLICY_PREFIX.Length);

            // Create a dynamic policy with the custom role requirement
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new OrganizationRoleRequirement(role))
                .Build();

            return Task.FromResult(policy);
        }

        // Fall back to the default provider for other policies
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();
}