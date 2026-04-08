using CO.CDP.Authentication.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication;

public class ClaimService(
    IHttpContextAccessor httpContextAccessor,
    IOrganisationRepository organisationRepository) : IClaimService
{
    public string? GetUserUrn()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst(ClaimType.Subject)?.Value;
    }

    public IEnumerable<string> GetUserRoles()
    {
        var roles = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimType.Roles)?.Value;
        return string.IsNullOrWhiteSpace(roles) ? [] : roles.Split(",", StringSplitOptions.RemoveEmptyEntries);
    }

    public Guid? GetOrganisationId()
    {
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimType.OrganisationId)?.Value, out Guid result))
        {
            return result;
        }
        return null;
    }

    public async Task<bool> HaveAccessToOrganisation(Guid organisationId, string[]? scopes = null, string[]? personScopes = null)
    {
        var userUrn = GetUserUrn();
        if (string.IsNullOrWhiteSpace(userUrn)) return false;

        if (personScopes != null && GetUserRoles().Intersect(personScopes).Any())
        {
            return true;
        }

        var organisationPerson = await organisationRepository.FindOrganisationPerson(organisationId, userUrn);
        if (organisationPerson == null) return false;

        if (scopes == null) return true;

        return organisationPerson.Scopes.Intersect(scopes).Any();
    }

    public string? GetChannel()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst(ClaimType.Channel)?.Value;
    }

    public UserClaims? GetApplicationClaims()
    {
        var cdpClaimsJson = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimType.CdpClaims)?.Value;
        if (string.IsNullOrWhiteSpace(cdpClaimsJson)) return null;
        try
        {
            return JsonSerializer.Deserialize<UserClaims>(cdpClaimsJson);
        }
        catch
        {
            return null;
        }
    }

    public bool HasApplicationRole(Guid organisationId, string clientId, string roleName)
    {
        var claims = GetApplicationClaims();
        if (claims == null) return false;

        return claims.Organisations
            .Where(o => o.OrganisationId == organisationId)
            .SelectMany(o => o.Applications)
            .Where(a => a.ClientId == clientId)
            .SelectMany(a => a.Roles)
            .Any(r => r == roleName);
    }

    public bool HasApplicationPermission(Guid organisationId, string clientId, string permissionName)
    {
        var claims = GetApplicationClaims();
        if (claims == null) return false;

        return claims.Organisations
            .Where(o => o.OrganisationId == organisationId)
            .SelectMany(o => o.Applications)
            .Where(a => a.ClientId == clientId)
            .SelectMany(a => a.Permissions)
            .Any(p => p == permissionName);
    }
}
