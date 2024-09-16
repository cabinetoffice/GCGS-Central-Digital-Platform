using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.Authorization;

public class OrganisationAuthorizationRequirement(
    AuthenticationChannel[] channels,
    string[]? scopes = null,
    OrganisationIdLocation organisationIdLocation = OrganisationIdLocation.None) : IAuthorizationRequirement
{
    public AuthenticationChannel[] Channels { get; private set; } = channels;

    public string[] Scopes { get; private set; } = scopes ?? [];

    public OrganisationIdLocation OrganisationIdLocation { get; private set; } = organisationIdLocation;
}