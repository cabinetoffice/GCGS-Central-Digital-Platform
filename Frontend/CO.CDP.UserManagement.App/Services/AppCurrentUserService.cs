using CO.CDP.UserManagement.Core;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.App.Services;

/// <summary>
/// Reads current-user information from the HTTP context claims for the App (frontend) project.
/// </summary>
public sealed class AppCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? GetUserPrincipalId() => GetCdpClaims()?.UserPrincipalId;

    public string? GetUserEmail() =>
        httpContextAccessor.HttpContext?.User.FindFirst("email")?.Value;

    public bool IsAuthenticated() =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public string? GetCurrentUserId() => GetUserPrincipalId();

    public UserClaims? GetCdpClaims()
    {
        var json = httpContextAccessor.HttpContext?.User.FindFirst("cdp_claims")?.Value;
        return JsonHelper.TryDeserialize<UserClaims>(json);
    }

    public OrganisationRole? GetOrganisationRole(Guid organisationId) =>
        GetCdpClaims()?.GetOrganisationRole(organisationId);
}
