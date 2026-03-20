using CO.CDP.OrganisationInformation.Persistence.Constants;
using CO.CDP.UserManagement.Core.Constants;
using CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Infrastructure.Services;

internal static class DefaultApplicationRoleSelector
{
    private const string FindATenderClientId = "find-a-tender";

    internal static IReadOnlyList<ApplicationRole> SelectFor(
        OrganisationApplication organisationApplication,
        IEnumerable<ApplicationRole> applicationRoles,
        IEnumerable<PartyRole> organisationPartyRoles,
        IEnumerable<string> organisationInformationScopes) =>
        organisationApplication.Application.ClientId.Equals(FindATenderClientId, StringComparison.Ordinal)
            ? applicationRoles
                .Where(IsSyncableRole)
                .Where(MatchesDefaultOrganisationInformationScopes(organisationInformationScopes))
                .Where(MatchesAnyRequiredPartyRole(ExpandPartyRoles(organisationPartyRoles)))
                .ToList()
            : [];

    private static bool IsSyncableRole(ApplicationRole role) =>
        role.IsActive &&
        !role.IsDeleted &&
        role.SyncToOrganisationInformation &&
        role.OrganisationInformationScopes.Count != 0;

    private static Func<ApplicationRole, bool> MatchesDefaultOrganisationInformationScopes(
        IEnumerable<string> organisationInformationScopes)
    {
        var defaultApplicationScopes = ExpandDefaultApplicationScopes(organisationInformationScopes);
        return role => role.OrganisationInformationScopes.Any(defaultApplicationScopes.Contains);
    }

    private static Func<ApplicationRole, bool> MatchesAnyRequiredPartyRole(ISet<PartyRole> organisationPartyRoles) =>
        role => role.RequiredPartyRoles.Count == 0 || role.RequiredPartyRoles.Any(organisationPartyRoles.Contains);

    private static ISet<string> ExpandDefaultApplicationScopes(IEnumerable<string> organisationInformationScopes) =>
        organisationInformationScopes
            .SelectMany(MapToDefaultApplicationScopes)
            .ToHashSet(StringComparer.Ordinal);

    private static IEnumerable<string> MapToDefaultApplicationScopes(string scope) =>
        scope switch
        {
            OrganisationPersonScopes.Admin => [OrganisationPersonScopes.Admin, OrganisationPersonScopes.Editor],
            OrganisationPersonScopes.Editor => [OrganisationPersonScopes.Editor],
            OrganisationPersonScopes.Viewer => [OrganisationPersonScopes.Viewer],
            _ => Array.Empty<string>()
        };

    private static ISet<PartyRole> ExpandPartyRoles(IEnumerable<PartyRole> organisationPartyRoles)
    {
        var roles = organisationPartyRoles.ToHashSet();
        return roles.Contains(PartyRole.Supplier) || roles.Contains(PartyRole.Tenderer)
            ? roles
                .Append(PartyRole.Supplier)
                .Append(PartyRole.Tenderer)
                .ToHashSet()
            : roles;
    }
}
