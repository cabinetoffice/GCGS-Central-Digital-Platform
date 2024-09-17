using CO.CDP.Authentication.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CO.CDP.Authentication.Tests.Authorization;

public class ChannelAuthorizationHandlerTests
{
    private readonly ChannelAuthorizationHandler _handler;

    public ChannelAuthorizationHandlerTests()
    {
        _handler = new ChannelAuthorizationHandler();
    }

    [Theory]
    [InlineData("one-login", AuthenticationChannel.OneLogin, true)]
    [InlineData("organisation-key", AuthenticationChannel.OrganisationKey, true)]
    [InlineData("service-key", AuthenticationChannel.ServiceKey, true)]
    [InlineData("invalid-channel", AuthenticationChannel.ServiceKey, false)]
    [InlineData(" ", AuthenticationChannel.ServiceKey, false)]
    [InlineData(null, AuthenticationChannel.ServiceKey, false)]
    public async Task HandleRequirementAsync_ShouldMatchExpectedResult(string? claimValue, AuthenticationChannel requiredChannel, bool expectedResult)
    {
        var requirement = CreateRequirement(requiredChannel);
        var user = CreateClaimsPrincipal(claimValue);
        var context = CreateAuthorizationHandlerContext(user, requirement);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(expectedResult);
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenMultipleChannelsAndValidClaimExists()
    {
        var requirement = CreateRequirement(AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey);
        var user = CreateClaimsPrincipal("service-key");
        var context = CreateAuthorizationHandlerContext(user, requirement);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldNotSucceed_WhenChannelsArrayIsEmpty()
    {
        var requirement = CreateRequirement();
        var user = CreateClaimsPrincipal("service-key");
        var context = CreateAuthorizationHandlerContext(user, requirement);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenMultipleClaimsAndValidClaim_xists()
    {
        var requirement = CreateRequirement(AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey);
        var user = CreateClaimsPrincipal("one-login", "invalid-channel");
        var context = CreateAuthorizationHandlerContext(user, requirement);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(ClaimsPrincipal user, ChannelAuthorizationRequirement requirement)
    {
        return new AuthorizationHandlerContext(new[] { requirement }, user, new object());
    }

    private static ChannelAuthorizationRequirement CreateRequirement(params AuthenticationChannel[] channels)
    {
        return new ChannelAuthorizationRequirement(channels);
    }

    private static ClaimsPrincipal CreateClaimsPrincipal(params string?[] channelValue)
    {
        var identity = new ClaimsIdentity();

        Array.ForEach(channelValue, (channel) =>
        {
            if (!string.IsNullOrWhiteSpace(channel))
            {
                identity.AddClaim(new Claim("channel", channel));
            }
        });

        return new ClaimsPrincipal(identity);
    }
}
