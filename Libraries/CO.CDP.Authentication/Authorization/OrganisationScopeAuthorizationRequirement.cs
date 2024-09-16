using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.Authorization;

public class OrganisationScopeAuthorizationRequirement(
    string[]? scopes = null,
    OrganisationIdLocation organisationIdLocation = OrganisationIdLocation.None) : IAuthorizationRequirement
{
    public string[] Scopes { get; private set; } = scopes ?? [];

    public OrganisationIdLocation OrganisationIdLocation { get; private set; } = organisationIdLocation;
}