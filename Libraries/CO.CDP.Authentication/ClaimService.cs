using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.AspNetCore.Http;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.Authentication;

public class ClaimService(
    IHttpContextAccessor httpContextAccessor,
    ITenantRepository tenantRepository) : IClaimService
{
    public string? GetUserUrn()
    {
        return httpContextAccessor.HttpContext?.User?.FindFirst(ClaimType.Subject)?.Value;
    }

    public async Task<bool> HaveAccessToOrganisation(Guid oragnisationId)
    {
        var userUrn = GetUserUrn();
        if (string.IsNullOrEmpty(userUrn)) return false;

        var tenantlookup = await tenantRepository.LookupTenant(userUrn);
        if (tenantlookup == null) return false;

        return tenantlookup.Tenants.SelectMany(t => t.Organisations).Any(o => o.Id == oragnisationId);
    }

    public Guid? GetOrganisationId()
    {
        if (Guid.TryParse(httpContextAccessor.HttpContext?.User?.FindFirst(ClaimType.OrganisationId)?.Value, out Guid result))
        {
            return result;
        }
        return null;
    }
}