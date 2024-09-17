using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class OrganizationScopeRequirement : IAuthorizationRequirement
{
    public string Scope { get; }

    public OrganizationScopeRequirement(string scope)
    {
        Scope = scope;
    }
}