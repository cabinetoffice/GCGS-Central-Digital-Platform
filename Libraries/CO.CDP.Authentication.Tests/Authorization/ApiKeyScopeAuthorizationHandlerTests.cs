using CO.CDP.Authentication.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication.Tests.Authorization;

public class ApiKeyScopeAuthorizationHandlerTests
{
    private readonly ApiKeyScopeAuthorizationHandler _handler = new();

    private static ClaimsPrincipal CreateUserWithChannel(string? channel, string? scopeClaimValue)
    {
        var claims = new List<Claim>();
        if (channel != null)
        {
            claims.Add(new Claim(ClaimType.Channel, channel));
        }

        if (scopeClaimValue != null)
        {
            claims.Add(new Claim(ClaimType.ApiKeyScope, scopeClaimValue));
        }

        var identity = new ClaimsIdentity(claims, "testAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenRequirementIsEmptyAndChannelMissing_SucceedsAndDoesNotAddClaim()
    {
        var user = CreateUserWithChannel(null, "read:data");
        var requirement = new ApiKeyScopeAuthorizationRequirement(Array.Empty<string>());
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        user.HasClaim("privileged_api_access", "true").Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenNoUserApiKeyScopeAndChannelMissing_SucceedsAndDoesNotAddClaim()
    {
        var user = CreateUserWithChannel(null, null);
        var requirement = new ApiKeyScopeAuthorizationRequirement(new[] { "read:data" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        user.HasClaim("privileged_api_access", "true").Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenChannelIsNotApiKey_SucceedsAndDoesNotAddClaim()
    {
        var user = CreateUserWithChannel(Channel.OneLogin, "read:data");
        var requirement = new ApiKeyScopeAuthorizationRequirement(new[] { "read:data" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        user.HasClaim("privileged_api_access", "true").Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenRequirementIsEmpty_ShouldSucceed()
    {
        var user = CreateUserWithChannel(Channel.ServiceKey, "read:data");
        var requirement = new ApiKeyScopeAuthorizationRequirement(Array.Empty<string>());
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasServiceKey_ShouldSucceed()
    {
        var user = CreateUserWithChannel(Channel.ServiceKey, null);
        var requirement = new ApiKeyScopeAuthorizationRequirement(new[] { "read:data" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Theory]
    [InlineData(Channel.ServiceKey, "read:data", new[] { "read:data" }, true)]
    [InlineData(Channel.OrganisationKey, "read:data write:data", new[] { "read:data" },
        false)]
    [InlineData(Channel.ServiceKey, "read:data", new[] { "read:data", "admin:data" }, true)]
    public async Task HandleRequirementAsync_WhenApiKeyChannelAndMatchingScope_AddsEnhancedApiAccessClaim(
        string channel, string? userScopesClaim, string[] requiredScopes, bool shouldHaveClaim)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimType.Channel, channel),
            new Claim(ClaimType.ApiKeyScope, userScopesClaim ?? "")
        }, "test"));
        var requirement = new ApiKeyScopeAuthorizationRequirement(requiredScopes);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        user.HasClaim("privileged_api_access", "true").Should().Be(shouldHaveClaim,
            $"user with channel '{channel}' and scopes '{userScopesClaim}' and requirement '{string.Join(",", requiredScopes)}'");
    }

    [Theory]
    [InlineData(Channel.ServiceKey, "read:other", new[] { "read:data" }, false)]
    [InlineData(Channel.OrganisationKey, "read:data", new[] { "write:data" }, false)]
    [InlineData(Channel.ServiceKey, "", new[] { "read:data" }, false)]
    [InlineData(Channel.OrganisationKey, null, new[] { "read:data" }, false)]
    [InlineData(Channel.ServiceKey, "read:data", new string[] { }, false)]
    [InlineData(Channel.OneLogin, "read:data", new[] { "read:data" }, false)]
    public async Task HandleRequirementAsync_WhenNoEnhancedAccessConditions_DoesNotAddEnhancedApiAccessClaim(
        string channel, string? userScopesClaim, string[] requiredScopes, bool shouldHaveClaim)
    {
        var claims = new List<Claim> { new Claim(ClaimType.Channel, channel) };
        if (userScopesClaim != null)
        {
            claims.Add(new Claim(ClaimType.ApiKeyScope, userScopesClaim));
        }

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
        var requirement = new ApiKeyScopeAuthorizationRequirement(requiredScopes);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        var shouldSucceed = channel == Channel.OneLogin || !requiredScopes.Any();
        context.HasSucceeded.Should().Be(shouldSucceed);
        user.HasClaim("privileged_api_access", "true").Should().Be(shouldHaveClaim,
            $"user with channel '{channel}' and scopes '{userScopesClaim}' and requirement '{string.Join(",", requiredScopes)}'");
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenChannelClaimIsMissing_DoesNotAddEnhancedApiAccessClaim()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimType.ApiKeyScope, "read:data")
        }, "test"));
        var requirement = new ApiKeyScopeAuthorizationRequirement(new[] { "read:data" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        user.HasClaim("privileged_api_access", "true").Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenRequiredScopesIsEmpty_SucceedsAndDoesNotAddEnhancedClaim()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimType.Channel, Channel.ServiceKey),
            new Claim(ClaimType.ApiKeyScope, "read:data")
        }, "test"));
        var requirement = new ApiKeyScopeAuthorizationRequirement(Array.Empty<string>()); // No required scopes
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        user.HasClaim("privileged_api_access", "true").Should().BeFalse();
    }

    [Theory]
    [InlineData("read:data write:data", new[] { "read:data", "write:data" }, true)] // User has all required scopes
    [InlineData("read:data write:data admin:data", new[] { "read:data", "write:data" },
        true)] // User has more than required
    [InlineData("read:data", new[] { "read:data", "write:data" }, true)] // User has some required scopes
    [InlineData("write:data", new[] { "read:data", "write:data" }, true)] // User has different required scope
    [InlineData("admin:data", new[] { "read:data", "write:data" }, false)] // User has none of the required scopes
    public async Task HandleRequirementAsync_ServiceKeyWithMultipleRequiredScopes_HandlesCorrectly(
        string userScopesClaim, string[] requiredScopes, bool shouldHaveEnhancedClaim)
    {
        var user = CreateUserWithChannel(Channel.ServiceKey, userScopesClaim);
        var requirement = new ApiKeyScopeAuthorizationRequirement(requiredScopes);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(shouldHaveEnhancedClaim);
        user.HasClaim("privileged_api_access", "true").Should().Be(shouldHaveEnhancedClaim);
    }

    [Theory]
    [InlineData("", new[] { "read:data" }, false)] // Empty string
    [InlineData("   ", new[] { "read:data" }, false)] // Only whitespace
    public async Task HandleRequirementAsync_ServiceKeyWithWhitespaceInScopes_HandlesCorrectly(
        string userScopesClaim, string[] requiredScopes, bool shouldHaveEnhancedClaim)
    {
        var user = CreateUserWithChannel(Channel.ServiceKey, userScopesClaim);
        var requirement = new ApiKeyScopeAuthorizationRequirement(requiredScopes);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(shouldHaveEnhancedClaim);
        user.HasClaim("privileged_api_access", "true").Should().Be(shouldHaveEnhancedClaim);
    }

    [Fact]
    public async Task HandleRequirementAsync_OrganisationKeyWithMatchingScopes_SucceedsButDoesNotAddEnhancedClaim()
    {
        var user = CreateUserWithChannel(Channel.OrganisationKey, "read:data write:data");
        var requirement = new ApiKeyScopeAuthorizationRequirement(new[] { "read:data" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        user.HasClaim("privileged_api_access", "true").Should()
            .BeFalse("OrganisationKey should not receive enhanced access claim");
    }

    [Theory]
    [InlineData("READ:DATA", new[] { "read:data" }, false)] // Case sensitivity test
    [InlineData("read:data", new[] { "READ:DATA" }, false)] // Case sensitivity test
    [InlineData("Read:Data", new[] { "read:data" }, false)] // Mixed case
    public async Task HandleRequirementAsync_ServiceKeyWithCaseSensitiveScopes_HandlesCorrectly(
        string userScopesClaim, string[] requiredScopes, bool shouldHaveEnhancedClaim)
    {
        var user = CreateUserWithChannel(Channel.ServiceKey, userScopesClaim);
        var requirement = new ApiKeyScopeAuthorizationRequirement(requiredScopes);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(shouldHaveEnhancedClaim);
        user.HasClaim("privileged_api_access", "true").Should().Be(shouldHaveEnhancedClaim);
    }

    [Fact]
    public async Task HandleRequirementAsync_ServiceKeyWithSpecialCharactersInScopes_HandlesCorrectly()
    {
        var user = CreateUserWithChannel(Channel.ServiceKey, "read:data-123 write:data_456 admin:data.789");
        var requirement = new ApiKeyScopeAuthorizationRequirement(new[] { "read:data-123" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        user.HasClaim("privileged_api_access", "true").Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithMultipleRequirements_HandlesCorrectly()
    {
        var user = CreateUserWithChannel(Channel.ServiceKey, "read:data");
        var requirement1 = new ApiKeyScopeAuthorizationRequirement(new[] { "read:data" });
        var requirement2 = new ApiKeyScopeAuthorizationRequirement(new[] { "write:data" });
        var context = new AuthorizationHandlerContext(new[] { requirement1, requirement2 }, user, null);

        await _handler.HandleAsync(context);

        // Multiple requirements must all be satisfied.
        context.HasSucceeded.Should().BeFalse();
        user.HasClaim("privileged_api_access", "true").Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNonAuthenticatedIdentity_SucceedsWhenNotApiKey()
    {
        var identity = new ClaimsIdentity(); // Not authenticated
        var user = new ClaimsPrincipal(identity);
        var requirement = new ApiKeyScopeAuthorizationRequirement(new[] { "read:data" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        user.HasClaim("privileged_api_access", "true").Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ServiceKeyWithEmptyStringScopeElements_DoesNotMatch()
    {
        var user = CreateUserWithChannel(Channel.ServiceKey,
            "read:data  write:data"); // Double space creates empty element
        var requirement = new ApiKeyScopeAuthorizationRequirement(new[] { "" }); // Empty required scope
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        user.HasClaim("privileged_api_access", "true").Should().BeFalse();
    }

    [Theory]
    [InlineData("unknown:channel")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task HandleRequirementAsync_WithInvalidChannelValues_SucceedsAndDoesNotAddClaim(string invalidChannel)
    {
        var user = CreateUserWithChannel(invalidChannel, "read:data");
        var requirement = new ApiKeyScopeAuthorizationRequirement(new[] { "read:data" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
        user.HasClaim("privileged_api_access", "true").Should().BeFalse();
    }
}
