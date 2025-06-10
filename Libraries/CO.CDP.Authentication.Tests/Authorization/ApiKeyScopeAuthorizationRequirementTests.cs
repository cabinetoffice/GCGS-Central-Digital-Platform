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
    public void Constructor_WithNullScopes_ShouldThrowArgumentNullException()
    {
        Action act = () => new ApiKeyScopeAuthorizationRequirement(null!);

        act.Should().Throw<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'requiredScopes')");
    }

    [Fact]
    public void Constructor_WithEmptyScopes_ShouldSetEmptyRequiredScopes()
    {
        var scopes = Array.Empty<string>();
        var requirement = new ApiKeyScopeAuthorizationRequirement(scopes);

        requirement.RequiredScopes.Should().BeEmpty();
    }
}

