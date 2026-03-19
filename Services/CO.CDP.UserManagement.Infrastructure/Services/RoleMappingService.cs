using CO.CDP.OrganisationInformation.Persistence.Constants;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Infrastructure.Services;

public class RoleMappingService(
    IUserOrganisationMembershipRepository membershipRepository,
    IUserApplicationAssignmentRepository assignmentRepository,
    IOrganisationRepository organisationRepository,
    IRoleRepository roleRepository,
    IOrganisationApiAdapter organisationApiAdapter) : IRoleMappingService
{
    public async Task ApplyRoleDefinitionAsync(
        UserOrganisationMembership membership,
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default)
    {
        var definition = await GetRoleAsync(organisationRole, cancellationToken);
        membership.OrganisationRoleId = definition.Id;
        membership.OrganisationRoleEntity = definition;
    }

    public async Task ApplyRoleDefinitionAsync(
        InviteRoleMapping mapping,
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default)
    {
        var definition = await GetRoleAsync(organisationRole, cancellationToken);
        mapping.OrganisationRoleId = definition.Id;
        mapping.OrganisationRoleEntity = definition;
    }

    public async Task<IReadOnlyList<string>> GetInviteScopesAsync(
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default)
    {
        var definition = await GetRoleAsync(organisationRole, cancellationToken);
        return definition.OrganisationInformationScopes
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    public async Task<bool> ShouldAutoAssignDefaultApplicationsAsync(
        UserOrganisationMembership membership,
        CancellationToken cancellationToken = default)
    {
        var definition = await GetRoleForMembershipAsync(membership, cancellationToken);
        return definition.AutoAssignDefaultApplications;
    }

    public async Task<IReadOnlyList<ApplicationRole>> GetAssignableRolesAsync(
        int organisationId,
        OrganisationRole organisationRole,
        IEnumerable<int> roleIds,
        CancellationToken cancellationToken = default)
    {
        var requestedRoleIds = roleIds.Distinct().ToArray();
        if (requestedRoleIds.Length == 0)
        {
            return [];
        }

        var organisation = await organisationRepository.GetByIdAsync(organisationId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Organisation), organisationId);

        var organisationPartyRoles = await organisationApiAdapter.GetPartyRolesAsync(organisation.CdpOrganisationGuid, cancellationToken);

        var roles = await roleRepository.GetByIdsAsync(requestedRoleIds, cancellationToken);

        if (roles.Count != requestedRoleIds.Length)
        {
            var foundIds = roles.Select(r => r.Id).ToHashSet();
            var missingRoleId = requestedRoleIds.First(id => !foundIds.Contains(id));
            throw new EntityNotFoundException(nameof(ApplicationRole), missingRoleId);
        }

        return roles
            .Where(role => IsAllowedForPartyRoles(role, organisationPartyRoles))
            .ToList();
    }

    public async Task<IReadOnlyList<string>> GetOrganisationInformationScopesAsync(
        int membershipId,
        CancellationToken cancellationToken = default)
    {
        var membership = await LoadMembershipWithOrgAsync(membershipId, cancellationToken);
        var applicationRoleScopes = await LoadSyncableApplicationRoleScopesAsync(membershipId, cancellationToken);

        return BuildOrganisationInformationScopes(membership, applicationRoleScopes);
    }

    public async Task<bool> ShouldSyncToOrganisationInformationAsync(
        int membershipId,
        CancellationToken cancellationToken = default)
    {
        var membership = await membershipRepository.GetWithOrganisationAndRoleAsync(membershipId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(UserOrganisationMembership), membershipId);

        return membership.OrganisationRoleEntity.SyncToOrganisationInformation;
    }

    private static IReadOnlyList<string> BuildOrganisationInformationScopes(
        UserOrganisationMembership membership,
        IEnumerable<string> applicationRoleScopes) =>
        membership.OrganisationRoleEntity.OrganisationInformationScopes
            .Concat(applicationRoleScopes)
            .Concat(ResponderScope(membership.OrganisationRole))
            .Distinct(StringComparer.Ordinal)
            .ToList();

    private static IEnumerable<string> ResponderScope(OrganisationRole organisationRole) =>
        organisationRole != OrganisationRole.Agent
            ? [OrganisationPersonScopes.Responder]
            : [];

    private async Task<UserOrganisationMembership> LoadMembershipWithOrgAsync(
        int membershipId,
        CancellationToken cancellationToken) =>
        await membershipRepository.GetWithOrganisationAndRoleAsync(membershipId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(UserOrganisationMembership), membershipId);

    private async Task<IEnumerable<string>> LoadSyncableApplicationRoleScopesAsync(
        int membershipId,
        CancellationToken cancellationToken)
    {
        var assignments = await assignmentRepository.GetActiveForSyncAsync(membershipId, cancellationToken);

        return assignments
            .SelectMany(a => a.Roles)
            .Where(r => !r.IsDeleted && r.SyncToOrganisationInformation)
            .SelectMany(r => r.OrganisationInformationScopes);
    }

    private Task<OrganisationRoleEntity> GetRoleForMembershipAsync(
        UserOrganisationMembership membership,
        CancellationToken cancellationToken) =>
        GetRoleAsync(membership.OrganisationRole, cancellationToken);

    private async Task<OrganisationRoleEntity> GetRoleAsync(
        OrganisationRole organisationRole,
        CancellationToken cancellationToken) =>
        await membershipRepository.GetOrganisationRoleAsync(organisationRole, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(OrganisationRoleEntity), organisationRole);

    private static bool IsAllowedForPartyRoles(ApplicationRole role, ISet<CorePartyRole> organisationPartyRoles) =>
        role.RequiredPartyRoles.Count == 0 ||
        role.RequiredPartyRoles.Any(organisationPartyRoles.Contains);
}
