using CO.CDP.Authentication.Model;

namespace CO.CDP.Authentication;

public interface IClaimService
{
    string? GetUserUrn();

    IEnumerable<string> GetUserRoles();

    Guid? GetOrganisationId();

    Task<bool> HaveAccessToOrganisation(Guid organisationId, string[] scopes, string[]? personScopes = null);

    string? GetChannel();

    UserClaims? GetApplicationClaims();

    bool HasApplicationRole(Guid organisationId, string clientId, string roleName);

    bool HasApplicationPermission(Guid organisationId, string clientId, string permissionName);
}
