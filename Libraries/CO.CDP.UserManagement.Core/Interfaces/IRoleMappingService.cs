using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Shared.Enums;

namespace CO.CDP.UserManagement.Core.Interfaces;

public interface IRoleMappingService
{
    Task ApplyRoleDefinitionAsync(
        UserOrganisationMembership membership,
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default);

    Task ApplyRoleDefinitionAsync(
        InviteRoleMapping mapping,
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetInviteScopesAsync(
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default);

    Task<bool> ShouldAutoAssignDefaultApplicationsAsync(
        UserOrganisationMembership membership,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ApplicationRole>> GetAssignableRolesAsync(
        int organisationId,
        OrganisationRole organisationRole,
        IEnumerable<int> roleIds,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetOrganisationInformationScopesAsync(
        int membershipId,
        CancellationToken cancellationToken = default);

}
