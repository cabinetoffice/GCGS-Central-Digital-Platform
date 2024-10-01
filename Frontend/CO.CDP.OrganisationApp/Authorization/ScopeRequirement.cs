using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class ScopeRequirement : IAuthorizationRequirement
{
    public string Scope { get; }

    public ScopeRequirement(string scope)
    {
        Scope = scope;
    }
}