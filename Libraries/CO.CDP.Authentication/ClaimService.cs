using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.AspNetCore.Http;
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

    public async Task<bool> HaveAccessToOrganisation(Guid organisationId, string[]? scopes = null)
    {
        var userUrn = GetUserUrn();
        if (string.IsNullOrWhiteSpace(userUrn)) return false;

        var organisationPerson = await organisationRepository.FindOrganisationPerson(organisationId, userUrn);
        if (organisationPerson == null) return false;

        if (scopes == null) return true;

        return organisationPerson.Scopes.Intersect(scopes).Any();
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