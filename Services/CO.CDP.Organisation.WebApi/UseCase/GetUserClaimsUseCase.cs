using CO.CDP.Organisation.WebApi.DapsRegistry;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Organisation.WebApi.UseCase;

/// <summary>
/// Builds cdp_claims for the Authority. Organisations and the person's organisation role come
/// from CDP; application roles and permissions come from the DAPS Application Registry (the
/// source of truth for applications), matched to CDP organisations by their CDP organisation id.
/// If the registry is unavailable the organisations are still returned, with no applications.
/// </summary>
public class GetUserClaimsUseCase(OrganisationInformationContext db, IDapsRegistryClaimsClient registry)
    : IUseCase<string, UserClaimsResponse?>
{
    public async Task<UserClaimsResponse?> Execute(string userUrn)
    {
        var person = await db.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserUrn == userUrn);

        if (person == null) return null;

        var orgPersons = await db.Set<OrganisationPerson>()
            .AsNoTracking()
            .Include(op => op.Organisation)
            .Where(op => op.PersonId == person.Id)
            .ToListAsync();

        var registryClaims = await registry.GetClaimsAsync(userUrn);

        return new UserClaimsResponse
        {
            UserPrincipalId = userUrn,
            Organisations = orgPersons
                .Select(op => (op.Organisation!.Guid, op.Organisation.Name, (IEnumerable<string>)op.Scopes))
                .Select(org => ComposeOrganisation(org.Guid, org.Name, org.Item3, registryClaims))
                .ToList()
        };
    }

    /// <summary>One CDP organisation with the user's DAPS application roles for it.</summary>
    public static OrganisationMembershipClaim ComposeOrganisation(Guid organisationId, string organisationName,
        IEnumerable<string> scopes, IReadOnlyList<DapsOrganisationClaims>? registryClaims) =>
        new()
        {
            OrganisationId = organisationId,
            OrganisationName = organisationName,
            OrganisationRole = SelectOrganisationRole(scopes),
            Applications = (registryClaims ?? [])
                .Where(o => Guid.TryParse(o.CdpOrganisationId, out var cdpId) && cdpId == organisationId)
                .SelectMany(o => o.Applications)
                .GroupBy(a => a.Discriminator, StringComparer.Ordinal)
                .Select(g => new ApplicationAssignmentClaim
                {
                    ApplicationId = ToGuid(g.First().ApplicationId),
                    ApplicationName = g.First().Name,
                    ClientId = g.Key,
                    Roles = g.SelectMany(a => a.Roles).Distinct().ToList(),
                    Permissions = g.SelectMany(a => a.Permissions).Distinct().ToList()
                })
                .ToList()
        };

    /// <summary>
    /// DAPS ids are 24-hex Mongo ObjectIds; cdp_claims carries a GUID, so left-pad to 32 hex
    /// digits (stable and reversible). Anything else maps to a name-based GUID.
    /// </summary>
    public static Guid ToGuid(string registryId)
    {
        if (Guid.TryParse(registryId, out var guid)) return guid;
        if (registryId.Length == 24 && registryId.All(Uri.IsHexDigit))
        {
            return Guid.ParseExact(registryId.PadLeft(32, '0'), "N");
        }

        return new Guid(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes("daps:" + registryId)));
    }

    /// <summary>Highest-privilege organisation scopes first, so a person who is an admin is always reported as one.</summary>
    private static readonly string[] OrganisationRolePrecedence =
    [
        CO.CDP.OrganisationInformation.Persistence.Constants.OrganisationPersonScopes.Admin,
        CO.CDP.OrganisationInformation.Persistence.Constants.OrganisationPersonScopes.Editor,
        CO.CDP.OrganisationInformation.Persistence.Constants.OrganisationPersonScopes.Viewer,
        CO.CDP.OrganisationInformation.Persistence.Constants.OrganisationPersonScopes.Responder
    ];

    /// <summary>
    /// cdp_claims carries a single organisationRole per organisation. Scopes are stored in
    /// insertion order (e.g. ["EDITOR","ADMIN"]), so taking the first one could hide ADMIN;
    /// pick the highest-precedence scope instead, falling back to the first unknown one.
    /// </summary>
    public static string SelectOrganisationRole(IEnumerable<string> scopes)
    {
        var list = scopes.ToList();
        return OrganisationRolePrecedence.FirstOrDefault(role => list.Contains(role, StringComparer.OrdinalIgnoreCase))
               ?? list.FirstOrDefault()
               ?? "";
    }
}
