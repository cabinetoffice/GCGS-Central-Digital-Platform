using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace CO.CDP.OrganisationApp.Authorization;

public class CustomAuthorizationPolicyProvider : IAuthorizationPolicyProvider
{
    const string OrgPolicyPrefix = "OrgScope_";
    const string PolicyPrefix = "PersonScope_";
    const string PartyRolePolicyPrefix = "PartyRole_";
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public CustomAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PartyRolePolicyPrefix))
        {
            var roleString = policyName.Substring(PartyRolePolicyPrefix.Length);
            if (Enum.TryParse<PartyRole>(roleString, out var partyRole))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new PartyRoleAuthorizationRequirement(partyRole))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        if (policyName == PolicyNames.PartyRole.BuyerWithSignedMou)
        {
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new BuyerMouRequirement())
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        var role = ExtractRoleFromPolicyName(policyName);
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

    private static string? ExtractRoleFromPolicyName(string policyName)
    {
        if (policyName.StartsWith(OrgPolicyPrefix))
        {
            return policyName.Substring(OrgPolicyPrefix.Length);
        }

        if (policyName.StartsWith(PolicyPrefix))
        {
            return policyName.Substring(PolicyPrefix.Length);
        }

        return null;
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallbackPolicyProvider.GetFallbackPolicyAsync();
}