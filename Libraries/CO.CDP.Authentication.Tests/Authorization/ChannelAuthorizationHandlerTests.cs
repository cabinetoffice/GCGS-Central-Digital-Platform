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
    [InlineData(AuthenticationChannel.OneLogin, "one-login", true)]
    [InlineData(AuthenticationChannel.OrganisationKey, "organisation-key", true)]
    [InlineData(AuthenticationChannel.ServiceKey, "service-key", true)]
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
        var context = CreateAuthorizationHandlerContext([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey], ["service-key"]);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldNotSucceed_WhenChannelsArrayIsEmpty()
    {
        var context = CreateAuthorizationHandlerContext([], ["service-key"]);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ShouldSucceed_WhenMultipleClaimsAndValidClaimExists()
    {
        var context = CreateAuthorizationHandlerContext([AuthenticationChannel.OneLogin, AuthenticationChannel.ServiceKey], ["one-login", "invalid-channel"]);

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
                identity.AddClaim(new Claim("channel", channel));
            }
        });

        var requirement = new ChannelAuthorizationRequirement(channels);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        return new AuthorizationHandlerContext([requirement], claimsPrincipal, new object());
    }
}
