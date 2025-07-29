using CO.CDP.OrganisationApp.Authorization;
using CO.CDP.OrganisationApp.Constants;
using CO.CDP.Tenant.WebApiClient;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.Options;
using Moq;

namespace CO.CDP.OrganisationApp.Tests.Authorization;

public class CustomAuthorizationPolicyProviderTests
{
    private readonly CustomAuthorizationPolicyProvider _policyProvider;

    public CustomAuthorizationPolicyProviderTests()
    {
        var options = new Mock<IOptions<AuthorizationOptions>>();
        options.Setup(o => o.Value).Returns(new AuthorizationOptions());
        _policyProvider = new CustomAuthorizationPolicyProvider(options.Object);
    }

    [Theory]
    [InlineData(PolicyNames.PartyRole.Buyer, PartyRole.Buyer)]
    [InlineData(PolicyNames.PartyRole.Supplier, PartyRole.Supplier)]
    [InlineData(PolicyNames.PartyRole.ProcuringEntity, PartyRole.ProcuringEntity)]
    [InlineData(PolicyNames.PartyRole.Tenderer, PartyRole.Tenderer)]
    [InlineData(PolicyNames.PartyRole.Funder, PartyRole.Funder)]
    [InlineData(PolicyNames.PartyRole.Enquirer, PartyRole.Enquirer)]
    [InlineData(PolicyNames.PartyRole.Payer, PartyRole.Payer)]
    [InlineData(PolicyNames.PartyRole.Payee, PartyRole.Payee)]
    [InlineData(PolicyNames.PartyRole.ReviewBody, PartyRole.ReviewBody)]
    [InlineData(PolicyNames.PartyRole.InterestedParty, PartyRole.InterestedParty)]
    public async Task GetPolicyAsync_ValidPartyRolePolicy_ReturnsCorrectPolicy(string policyName, PartyRole expectedRole)
    {
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        policy.Should().NotBeNull();
        policy!.Requirements.Should().HaveCount(2);
        policy.Requirements.Should().Contain(r => r is DenyAnonymousAuthorizationRequirement);
        policy.Requirements.Should().Contain(r => r is PartyRoleAuthorizationRequirement);

        var requirement = policy.Requirements.OfType<PartyRoleAuthorizationRequirement>().First();
        requirement.RequiredRole.Should().Be(expectedRole);
    }

    [Fact]
    public async Task GetPolicyAsync_BuyerMouPolicy_ReturnsCorrectPolicy()
    {
        var policy = await _policyProvider.GetPolicyAsync(PolicyNames.PartyRole.BuyerWithSignedMou);

        policy.Should().NotBeNull();
        policy!.Requirements.Should().HaveCount(2);
        policy.Requirements.Should().Contain(r => r is DenyAnonymousAuthorizationRequirement);
        policy.Requirements.Should().Contain(r => r is BuyerMouRequirement);
    }

    [Fact]
    public async Task GetPolicyAsync_InvalidPartyRolePolicy_ReturnsNull()
    {
        var policy = await _policyProvider.GetPolicyAsync("PartyRole_InvalidRole");

        policy.Should().BeNull();
    }

    [Theory]
    [InlineData("OrgScope_Admin")]
    [InlineData("PersonScope_SuperAdmin")]
    public async Task GetPolicyAsync_ScopeBasedPolicy_ReturnsCorrectPolicy(string policyName)
    {
        var policy = await _policyProvider.GetPolicyAsync(policyName);

        policy.Should().NotBeNull();
        policy!.Requirements.Should().HaveCount(2);
        policy.Requirements.Should().Contain(r => r is DenyAnonymousAuthorizationRequirement);
        policy.Requirements.Should().Contain(r => r is ScopeRequirement);
    }

    [Fact]
    public async Task GetPolicyAsync_UnknownPolicy_ReturnsNull()
    {
        var policy = await _policyProvider.GetPolicyAsync("UnknownPolicy");

        policy.Should().BeNull();
    }

    [Fact]
    public async Task GetPolicyAsync_EmptyPolicyName_ReturnsNull()
    {
        var policy = await _policyProvider.GetPolicyAsync("");

        policy.Should().BeNull();
    }

    [Fact]
    public async Task GetPolicyAsync_PartyRolePrefixWithoutRole_ReturnsNull()
    {
        var policy = await _policyProvider.GetPolicyAsync("PartyRole_");

        policy.Should().BeNull();
    }

    [Fact]
    public async Task GetDefaultPolicyAsync_ReturnsDefaultPolicy()
    {
        var policy = await _policyProvider.GetDefaultPolicyAsync();

        policy.Should().NotBeNull();
    }

    [Fact]
    public async Task GetFallbackPolicyAsync_ReturnsFallbackPolicy()
    {
        _ = await _policyProvider.GetFallbackPolicyAsync();

        await Task.CompletedTask;
    }
}