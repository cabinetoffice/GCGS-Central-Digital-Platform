using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

public class OrganizationRoleRequirement : IAuthorizationRequirement
{
    public string Role { get; }

    public OrganizationRoleRequirement(string role)
    {
        Role = role;
    }
}