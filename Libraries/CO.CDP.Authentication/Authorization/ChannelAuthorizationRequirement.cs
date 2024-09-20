using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.Authorization;

public class ChannelAuthorizationRequirement(AuthenticationChannel[] channels) : IAuthorizationRequirement
{
    public AuthenticationChannel[] Channels { get; private set; } = channels;
}