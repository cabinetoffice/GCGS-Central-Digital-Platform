using CO.CDP.Authentication.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication.Tests.Authorization;

public class ApiKeyScopeAuthorizationHandlerTests
{
    private readonly ApiKeyScopeAuthorizationHandler _handler = new();

    private static ClaimsPrincipal CreateUser(string? scopeClaimValue)
    {
        var claims = new List<Claim>();
        if (scopeClaimValue != null)
        {
            claims.Add(new Claim(ClaimType.ApiKeyScope, scopeClaimValue));
        }
        var identity = new ClaimsIdentity(claims, "ApiKey");
        return new ClaimsPrincipal(identity);
    }

    [Theory]
    [InlineData("read:data", new[] { "read:data" }, true)] // User has the exact required scope
    [InlineData("read:data write:data", new[] { "read:data" }, true)] // User has required scope among others
    [InlineData("read:data", new[] { "read:data", "admin:data" }, true)] // User has one of the required scopes
    [InlineData("read:other write:another", new[] { "read:data" }, false)] // User has scopes, but not the required one
    [InlineData("read:data", new[] { "write:data" }, false)] // User has a scope, but not the one required
    [InlineData("", new[] { "read:data" }, false)] // User has empty scope claim
    [InlineData(null, new[] { "read:data" }, false)] // User has no scope claim
    [InlineData("read:data", new string[] { }, false)] // Requirement has no scopes (handler expects at least one required scope to match)
    [InlineData("read:data", new[] { "read:data", "read:another" }, true)] // User has one of the required scopes
    [InlineData("read:another read:data", new[] { "read:data", "admin:data" }, true)] // User has one of the required scopes (order doesn't matter)
    [InlineData("read:data", new[] { "Read:Data" }, false)] // Case sensitivity: required scope has different case
    [InlineData("Read:Data", new[] { "read:data" }, false)] // Case sensitivity: user scope has different case
    [InlineData("read:data read:data", new[] { "read:data" }, true)] // Duplicate scope in user claim, matches requirement
    [InlineData("read:data", new[] { "read:data", "read:data" }, true)] // Duplicate scope in requirement, matches user claim
    [InlineData(" read:data ", new[] { "read:data" }, true)] // User scope claim with leading/trailing spaces around the whole string
    [InlineData("read:data", new[] { " read:data " }, false)] // Required scope has leading/trailing spaces (treated as a different scope string)
    [InlineData("scope1  scope2", new[] { "scope1" }, true)] // Multiple spaces between scopes in user claim
    [InlineData("scope1  scope2", new[] { "scope2" }, true)] // Multiple spaces between scopes in user claim
    public async Task HandleRequirementAsync_ShouldSucceedOrNot_BasedOnScopes(
        string? userScopesClaim, string[] requiredScopes, bool shouldSucceed)
    {
        var user = CreateUser(userScopesClaim);
        var requirement = new ApiKeyScopeAuthorizationRequirement(requiredScopes);
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(shouldSucceed);
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenRequirementIsEmpty_ShouldNotSucceed()
    {
        var user = CreateUser("read:data");
        var requirement = new ApiKeyScopeAuthorizationRequirement(Array.Empty<string>());
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_WhenUserHasNoApiKeyScopeClaim_ShouldNotSucceed()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity());
        var requirement = new ApiKeyScopeAuthorizationRequirement(new[] { "read:data" });
        var context = new AuthorizationHandlerContext(new[] { requirement }, user, null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }
}

