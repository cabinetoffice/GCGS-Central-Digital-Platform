using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication.Authorization;

public class ChannelAuthorizationHandler
    : AuthorizationHandler<ChannelAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ChannelAuthorizationRequirement requirement)
    {
        if (requirement.Channels.Length == 0)
        {
            return Task.CompletedTask;
        }

        var requestChannel = context.User.FindFirstValue(ClaimType.Channel);
        if (string.IsNullOrWhiteSpace(requestChannel))
        {
            return Task.CompletedTask;
        }

        var hasOneLoginChannelClaim = requirement.Channels.Contains(AuthenticationChannel.OneLogin)
                                        && requestChannel == Channel.OneLogin;

        var hasOrganisationKeyChannelClaim = requirement.Channels.Contains(AuthenticationChannel.OrganisationKey)
                                        && requestChannel == Channel.OrganisationKey;

        var hasServiceKeyChannelClaim = requirement.Channels.Contains(AuthenticationChannel.ServiceKey)
                                        && requestChannel == Channel.ServiceKey;

        if (hasOneLoginChannelClaim || hasOrganisationKeyChannelClaim || hasServiceKeyChannelClaim)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}