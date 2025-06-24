using CO.CDP.Authentication.Authorization;
using FluentAssertions;

namespace CO.CDP.Authentication.Tests.Authorization;

public class ApiKeyScopeAuthorizationRequirementTests
{
    [Fact]
    public void Constructor_WithValidScopes_ShouldSetRequiredScopes()
    {
        var scopes = new[] { "scope1", "scope2" };
        var requirement = new ApiKeyScopeAuthorizationRequirement(scopes);

        requirement.RequiredScopes.Should().BeEquivalentTo(scopes);
    }

    [Fact]
    public void Constructor_WithNullScopes_ShouldSetEmptyRequiredScopes()
    {
        var requirement = new ApiKeyScopeAuthorizationRequirement(null!);

        requirement.RequiredScopes.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_WithEmptyScopes_ShouldSetEmptyRequiredScopes()
    {
        var scopes = Array.Empty<string>();
        var requirement = new ApiKeyScopeAuthorizationRequirement(scopes);

        requirement.RequiredScopes.Should().BeEmpty();
    }
}