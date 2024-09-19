using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        var requestChannel = context.User.FindFirstValue("channel");
        if (string.IsNullOrWhiteSpace(requestChannel))
        {
            return Task.CompletedTask;
        }

        var hasOneLoginChannelClaim = requirement.Channels.Contains(AuthenticationChannel.OneLogin)
                                        && requestChannel == "one-login";

        var hasOrganisationKeyChannelClaim = requirement.Channels.Contains(AuthenticationChannel.OrganisationKey)
                                        && requestChannel == "organisation-key";

        var hasServiceKeyChannelClaim = requirement.Channels.Contains(AuthenticationChannel.ServiceKey)
                                        && requestChannel == "service-key";

        if (hasOneLoginChannelClaim || hasOrganisationKeyChannelClaim || hasServiceKeyChannelClaim)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}