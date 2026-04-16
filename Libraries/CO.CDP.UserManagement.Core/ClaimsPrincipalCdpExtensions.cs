using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Shared.Enums;
using System.Security.Claims;

namespace CO.CDP.UserManagement.Core;

/// <summary>
/// Shared extension methods for reading CDP-specific claims from a <see cref="ClaimsPrincipal"/>.
/// Used by both the App and Infrastructure implementations of <see cref="Interfaces.ICurrentUserService"/>
/// to avoid duplicating claims-parsing logic.
/// </summary>
public static class ClaimsPrincipalCdpExtensions
{
    public static UserClaims? GetCdpClaims(this ClaimsPrincipal? principal)
    {
        var json = principal?.FindFirst("cdp_claims")?.Value;
        return JsonHelper.TryDeserialize<UserClaims>(json);
    }

    public static OrganisationRole? GetOrganisationRole(this ClaimsPrincipal? principal, Guid organisationId) =>
        principal?.GetCdpClaims()?.GetOrganisationRole(organisationId);
}
