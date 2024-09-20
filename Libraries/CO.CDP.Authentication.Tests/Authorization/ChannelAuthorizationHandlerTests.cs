using CO.CDP.Authentication.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication.Tests.Authorization;

public class ChannelAuthorizationHandlerTests
{
    private readonly ChannelAuthorizationHandler _handler;

    public ChannelAuthorizationHandlerTests()
    {
        _handler = new ChannelAuthorizationHandler();
    }

    [Theory]
    [InlineData(AuthenticationChannel.OneLogin, Channel.OneLogin, true)]
    [InlineData(AuthenticationChannel.OrganisationKey, Channel.OrganisationKey, true)]
    [InlineData(AuthenticationChannel.ServiceKey, Channel.ServiceKey, true)]
    [InlineData(AuthenticationChannel.OrganisationKey, "invalid-channel", false)]
    [InlineData(AuthenticationChannel.ServiceKey, " ", false)]
    [InlineData(AuthenticationChannel.OrganisationKey, null, false)]
    public async Task HandleRequirementAsync_ShouldMatchExpectedResult(AuthenticationChannel requiredChannel, string? claimValue, bool expectedResult)
    {
        var context = CreateAuthorizationHandlerContext([requiredChannel], [claimValue]);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().Be(expectedResult);
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenMultipleChannelsAndValidClaimExists()
    {
        var context = CreateAuthorizationHandlerContext([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey], [Channel.ServiceKey]);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldNotSucceed_WhenChannelsArrayIsEmpty()
    {
        var context = CreateAuthorizationHandlerContext([], [Channel.ServiceKey]);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenMultipleClaimsAndValidClaimExists()
    {
        var context = CreateAuthorizationHandlerContext([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey], [Channel.OneLogin, "invalid-channel"]);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(AuthenticationChannel[] channels, string?[] channelValue)
    {
        var identity = new ClaimsIdentity();

        Array.ForEach(channelValue, (channel) =>
        {
            if (!string.IsNullOrWhiteSpace(channel))
            {
                identity.AddClaim(new Claim(ClaimType.Channel, channel));
            }
        });

        var requirement = new ChannelAuthorizationRequirement(channels);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        return new AuthorizationHandlerContext([requirement], claimsPrincipal, new object());
    }
}
