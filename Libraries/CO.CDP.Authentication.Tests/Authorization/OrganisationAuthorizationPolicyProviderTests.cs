using CO.CDP.Authentication.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;

namespace CO.CDP.Authentication.Tests.Authorization;

public class OrganisationAuthorizationPolicyProviderTests
{
    private readonly OrganisationAuthorizationPolicyProvider _policyProvider;
    private readonly AuthorizationPolicy fallbackPolicy;

    public OrganisationAuthorizationPolicyProviderTests()
    {
        fallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        var options = Options.Create(new AuthorizationOptions { FallbackPolicy = fallbackPolicy });

        _policyProvider = new OrganisationAuthorizationPolicyProvider(options);
    }

    [Fact]
    public async Task GetDefaultPolicyAsync_ShouldReturnDefaultPolicy()
    {
        var policy = await _policyProvider.GetDefaultPolicyAsync();

        policy.AuthenticationSchemes.Should().Contain(Extensions.JwtBearerOrApiKeyScheme);
        policy.Requirements.Should().ContainSingle(r => r is DenyAnonymousAuthorizationRequirement);
    }

    [Fact]
    public async Task GetFallbackPolicyAsync_ShouldReturnFallbackPolicyFromFallbackPolicyProvider()
    {
        var result = await _policyProvider.GetFallbackPolicyAsync();
        result.Should().Be(fallbackPolicy);
    }

    [Theory]
    [InlineData("Org_Channels$OneLogin|ServiceKey;OrgScopes$scope1|scope2;OrgIdLoc$Header;", true)]
    [InlineData("Org_Channels$InvalidChannel;OrgScopes$scope1;", true)]
    [InlineData("Org_Channels$OneLogin|ServiceKey;PersonScopes$scope1|scope2;", true)]
    [InlineData("InvalidPolicy", false)]
    public async Task GetPolicyAsync_ShouldReturnCorrectPolicyBasedOnPolicyName(string policyName, bool isValidPolicy)
    {
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        if (isValidPolicy)
        {
            policy.Should().NotBeNull();
            policy!.AuthenticationSchemes.Should().Contain(Extensions.JwtBearerOrApiKeyScheme);
            policy.Requirements.Should().ContainSingle(r => r is ChannelAuthorizationRequirement);

            if (policyName.Contains("OrgScopes") && policyName.Contains("OneLogin"))
            {
                policy.Requirements.Should().Contain(r => r is OrganisationScopeAuthorizationRequirement);
            }

            if (policyName.Contains("PersonScopes") && policyName.Contains("OneLogin"))
            {
                policy.Requirements.Should().Contain(r => r is OrganisationScopeAuthorizationRequirement);
            }
        }
        else
        {
            policy.Should().BeNull();
        }
    }
}