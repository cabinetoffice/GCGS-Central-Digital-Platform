using CO.CDP.Tenant.WebApiClient;
using Microsoft.AspNetCore.Authorization;

namespace CO.CDP.OrganisationApp.Authorization;

/// <summary>
/// Represents an authorization requirement that enforces the user must have a specific party role.
/// This requirement checks if the user's organisation has the required party role (approved, not pending).
/// </summary>
public class PartyRoleAuthorizationRequirement(PartyRole requiredRole) : IAuthorizationRequirement
{
    public PartyRole RequiredRole { get; } = requiredRole;
}