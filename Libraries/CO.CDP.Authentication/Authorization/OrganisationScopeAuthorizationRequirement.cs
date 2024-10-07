using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.Authentication.Authorization;

public class OrganisationScopeAuthorizationRequirement(
    string[]? organisationPersonScopes = null,
    OrganisationIdLocation organisationIdLocation = OrganisationIdLocation.None,
    string[]? personScopes = null) : IAuthorizationRequirement
{
    public string[] OrganisationPersonScopes { get; private set; } = organisationPersonScopes ?? [];

    public OrganisationIdLocation OrganisationIdLocation { get; private set; } = organisationIdLocation;

    public string[] PersonScopes { get; private set; } = personScopes ?? [];
}