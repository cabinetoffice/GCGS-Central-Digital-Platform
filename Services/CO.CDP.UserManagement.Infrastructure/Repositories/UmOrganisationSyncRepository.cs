using CO.CDP.Functional;
using CO.CDP.OrganisationInformation.Persistence.Constants;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

public class UmOrganisationSyncRepository(
    IOrganisationRepository organisationRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IApplicationRepository applicationRepository,
    IOrganisationApplicationRepository organisationApplicationRepository,
    IUserApplicationAssignmentRepository userApplicationAssignmentRepository,
    IRoleRepository roleRepository,
    ISlugGeneratorService slugGeneratorService) : IUmOrganisationSyncRepository
{
    private const string SystemUser = "system:org-sync";
    private static readonly IReadOnlyList<string> FounderOrganisationInformationScopes = [OrganisationPersonScopes.Admin];

    public async Task<Result<string, Unit>> EnsureCreatedAsync(
        Guid cdpGuid, string name, CancellationToken cancellationToken = default)
    {
        var existing = await organisationRepository.GetByCdpGuidAsync(cdpGuid, cancellationToken);
        if (existing is null)
        {
            var slug = await ResolveUniqueSlugAsync(name, cancellationToken: cancellationToken);
            organisationRepository.Add(BuildOrganisation(cdpGuid, name, slug));
        }
        return Result<string, Unit>.Success(Unit.Value);
    }

    public async Task<Result<string, Unit>> EnsureNameSyncedAsync(
        Guid cdpGuid, string name, CancellationToken cancellationToken = default)
    {
        var existing = await organisationRepository.GetByCdpGuidAsync(cdpGuid, cancellationToken);
        return existing is null
            ? await EnsureCreatedAsync(cdpGuid, name, cancellationToken)
            : await UpdateNameIfChangedAsync(existing, name, cancellationToken);
    }

    public async Task<Result<string, Unit>> EnsureActiveApplicationsEnabledAsync(
        Guid cdpGuid,
        CancellationToken cancellationToken = default)
    {
        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpGuid, cancellationToken);
        return organisation is null
            ? Result<string, Unit>.Failure($"Cannot enable applications: UM organisation '{cdpGuid}' does not exist.")
            : await EnableActiveApplicationsAsync(organisation.Id, cancellationToken);
    }

    public async Task<Result<string, Unit>> EnsureFounderOwnerCreatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        string userPrincipalId,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken = default)
    {
        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpOrganisationGuid, cancellationToken);
        return organisation is null
            ? Result<string, Unit>.Failure($"Cannot create founder membership: UM organisation '{cdpOrganisationGuid}' does not exist.")
            : await EnsureFounderMembershipTrackedAsync(
                organisation, cdpPersonGuid, userPrincipalId, organisationPartyRoles, cancellationToken);
    }

    public async Task<Result<string, Unit>> EnsureMemberCreatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        string userPrincipalId,
        IReadOnlyList<string> inviteScopes,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken = default)
    {
        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpOrganisationGuid, cancellationToken);
        return organisation is null
            ? Result<string, Unit>.Failure($"Cannot create member membership: UM organisation '{cdpOrganisationGuid}' does not exist.")
            : await EnsureMemberMembershipTrackedAsync(
                organisation, cdpPersonGuid, userPrincipalId, inviteScopes, organisationPartyRoles, cancellationToken);
    }

    private async Task<Result<string, Unit>> EnsureFounderMembershipTrackedAsync(
        CoreOrganisation organisation,
        Guid cdpPersonGuid,
        string userPrincipalId,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken)
    {
        var membership = await ResolveOrCreateMembershipAsync(
            organisation.Id, cdpPersonGuid, userPrincipalId, OrganisationRole.Owner, cancellationToken);
        await TrackDefaultApplicationAssignmentsAsync(
            membership, organisationPartyRoles, FounderOrganisationInformationScopes, cancellationToken);
        return Result<string, Unit>.Success(Unit.Value);
    }

    private async Task<Result<string, Unit>> EnsureMemberMembershipTrackedAsync(
        CoreOrganisation organisation,
        Guid cdpPersonGuid,
        string userPrincipalId,
        IReadOnlyList<string> inviteScopes,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken)
    {
        var organisationRole = ResolveOrganisationRole(inviteScopes);
        var roleEntity = await membershipRepository.GetOrganisationRoleAsync(organisationRole, cancellationToken);
        return roleEntity is null
            ? Result<string, Unit>.Failure($"Organisation role '{organisationRole}' not found in User Management.")
            : await TrackMemberMembershipAsync(
                organisation.Id, cdpPersonGuid, userPrincipalId,
                organisationRole, organisationPartyRoles,
                roleEntity.OrganisationInformationScopes, cancellationToken);
    }

    private async Task<Result<string, Unit>> TrackMemberMembershipAsync(
        int organisationId,
        Guid cdpPersonGuid,
        string userPrincipalId,
        OrganisationRole organisationRole,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        IReadOnlyList<string> organisationInformationScopes,
        CancellationToken cancellationToken)
    {
        var membership = await ResolveOrCreateMembershipAsync(
            organisationId, cdpPersonGuid, userPrincipalId, organisationRole, cancellationToken);
        await TrackDefaultApplicationAssignmentsAsync(
            membership, organisationPartyRoles, organisationInformationScopes, cancellationToken);
        return Result<string, Unit>.Success(Unit.Value);
    }

    private Task<Result<string, Unit>> UpdateNameIfChangedAsync(
        CoreOrganisation organisation, string name, CancellationToken cancellationToken) =>
        organisation.Name == name
            ? Task.FromResult(Result<string, Unit>.Success(Unit.Value))
            : ApplyNameUpdateAsync(organisation, name, cancellationToken);

    private async Task<Result<string, Unit>> ApplyNameUpdateAsync(
        CoreOrganisation organisation, string name, CancellationToken cancellationToken)
    {
        var slug = await ResolveUniqueSlugAsync(name, organisation.Id, cancellationToken);
        organisation.Name = name;
        organisation.Slug = slug;
        organisation.ModifiedBy = SystemUser;
        organisationRepository.Update(organisation);
        return Result<string, Unit>.Success(Unit.Value);
    }

    private async Task<Result<string, Unit>> EnableActiveApplicationsAsync(
        int organisationId, CancellationToken cancellationToken)
    {
        var activeApplications = await applicationRepository.FindAsync(
            application => application.IsActive && !application.IsDeleted,
            cancellationToken);

        foreach (var application in activeApplications)
            await TrackApplicationEnabledAsync(organisationId, application, cancellationToken);

        return Result<string, Unit>.Success(Unit.Value);
    }

    private async Task TrackApplicationEnabledAsync(
        int organisationId,
        Application application,
        CancellationToken cancellationToken)
    {
        var existing = await organisationApplicationRepository.GetByOrganisationAndApplicationAsync(
            organisationId, application.Id, cancellationToken);

        switch (existing)
        {
            case null:
                organisationApplicationRepository.Add(new OrganisationApplication
                {
                    OrganisationId = organisationId,
                    ApplicationId = application.Id,
                    IsActive = true,
                    EnabledAt = DateTimeOffset.UtcNow,
                    EnabledBy = SystemUser,
                    CreatedBy = SystemUser
                });
                break;
            case { IsActive: true, IsDeleted: false }:
                break;
            default:
                existing.IsActive = true;
                existing.IsDeleted = false;
                existing.EnabledAt = DateTimeOffset.UtcNow;
                existing.EnabledBy = SystemUser;
                existing.DisabledAt = null;
                existing.DisabledBy = null;
                existing.DeletedAt = null;
                existing.DeletedBy = null;
                existing.ModifiedBy = SystemUser;
                organisationApplicationRepository.Update(existing);
                break;
        }
    }

    private async Task TrackDefaultApplicationAssignmentsAsync(
        UserOrganisationMembership membership,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        IReadOnlyList<string> organisationInformationScopes,
        CancellationToken cancellationToken)
    {
        var defaultApps = await organisationApplicationRepository.GetDefaultEnabledByOrganisationIdAsync(
            membership.OrganisationId, cancellationToken);

        foreach (var orgApp in defaultApps)
            await TrackDefaultApplicationAssignmentAsync(
                membership, organisationPartyRoles, organisationInformationScopes, orgApp, cancellationToken);
    }

    private async Task TrackDefaultApplicationAssignmentAsync(
        UserOrganisationMembership membership,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        IReadOnlyList<string> organisationInformationScopes,
        OrganisationApplication organisationApplication,
        CancellationToken cancellationToken)
    {
        var existing = await userApplicationAssignmentRepository.GetByMembershipAndApplicationAsync(
            membership.Id, organisationApplication.Id, cancellationToken);
        var defaultRoles = await GetDefaultRolesAsync(
            organisationApplication, organisationPartyRoles, organisationInformationScopes, cancellationToken);

        switch (existing)
        {
            case null:
                userApplicationAssignmentRepository.Add(new UserApplicationAssignment
                {
                    UserOrganisationMembershipId = membership.Id,
                    OrganisationApplicationId = organisationApplication.Id,
                    IsActive = true,
                    AssignedAt = DateTimeOffset.UtcNow,
                    AssignedBy = SystemUser,
                    CreatedBy = SystemUser,
                    Roles = defaultRoles.ToList()
                });
                break;
            case var a when AssignmentMatches(a, defaultRoles):
                break;
            default:
                existing.IsActive = true;
                existing.IsDeleted = false;
                existing.AssignedAt = DateTimeOffset.UtcNow;
                existing.AssignedBy = SystemUser;
                existing.RevokedAt = null;
                existing.RevokedBy = null;
                existing.DeletedAt = null;
                existing.DeletedBy = null;
                existing.ModifiedBy = SystemUser;
                SyncRoles(existing, defaultRoles);
                userApplicationAssignmentRepository.Update(existing);
                break;
        }
    }

    private async Task<UserOrganisationMembership> ResolveOrCreateMembershipAsync(
        int organisationId,
        Guid cdpPersonGuid,
        string userPrincipalId,
        OrganisationRole role,
        CancellationToken cancellationToken) =>
        await membershipRepository.GetByPersonIdAndOrganisationAsync(cdpPersonGuid, organisationId, cancellationToken)
            ?? await membershipRepository.GetByUserAndOrganisationAsync(userPrincipalId, organisationId, cancellationToken)
            ?? TrackNewMembership(organisationId, cdpPersonGuid, userPrincipalId, role);

    private UserOrganisationMembership TrackNewMembership(
        int organisationId,
        Guid cdpPersonGuid,
        string userPrincipalId,
        OrganisationRole role)
    {
        var membership = new UserOrganisationMembership
        {
            UserPrincipalId = userPrincipalId,
            CdpPersonId = cdpPersonGuid,
            OrganisationId = organisationId,
            OrganisationRoleId = (int)role,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            CreatedBy = SystemUser
        };
        membershipRepository.Add(membership);
        return membership;
    }

    private async Task<IReadOnlyList<ApplicationRole>> GetDefaultRolesAsync(
        OrganisationApplication organisationApplication,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        IReadOnlyList<string> organisationInformationScopes,
        CancellationToken cancellationToken) =>
        DefaultApplicationRoleSelector.SelectFor(
            organisationApplication,
            await roleRepository.GetByApplicationIdAsync(organisationApplication.ApplicationId, cancellationToken),
            organisationPartyRoles,
            organisationInformationScopes);

    private static bool AssignmentMatches(
        UserApplicationAssignment assignment,
        IReadOnlyList<ApplicationRole> desiredRoles) =>
        assignment.IsActive &&
        !assignment.IsDeleted &&
        assignment.Roles.Select(r => r.Id).OrderBy(id => id)
            .SequenceEqual(desiredRoles.Select(r => r.Id).OrderBy(id => id));

    private static void SyncRoles(
        UserApplicationAssignment assignment,
        IEnumerable<ApplicationRole> desiredRoles)
    {
        assignment.Roles.Clear();
        foreach (var role in desiredRoles)
            assignment.Roles.Add(role);
    }

    private static OrganisationRole ResolveOrganisationRole(IReadOnlyList<string> inviteScopes) =>
        inviteScopes.Contains(OrganisationPersonScopes.Admin, StringComparer.Ordinal)
            ? OrganisationRole.Admin
            : OrganisationRole.Member;

    private async Task<string> ResolveUniqueSlugAsync(
        string name, int? excludeId = null, CancellationToken cancellationToken = default) =>
        await BuildSlugCandidates(slugGeneratorService.GenerateSlug(name))
            .ToAsyncEnumerable()
            .FirstOrDefaultAwaitAsync(
                async slug => !await organisationRepository.SlugExistsAsync(slug, excludeId, cancellationToken),
                cancellationToken)
        ?? throw new InvalidOperationException($"Could not generate a unique slug for '{name}'.");

    private static IEnumerable<string> BuildSlugCandidates(string baseSlug) =>
        Enumerable.Range(0, 10).Select(i => i == 0 ? baseSlug : $"{baseSlug}-{i}");

    private static CoreOrganisation BuildOrganisation(Guid cdpGuid, string name, string slug) =>
        new()
        {
            CdpOrganisationGuid = cdpGuid,
            Name = name,
            Slug = slug,
            IsActive = true,
            CreatedBy = SystemUser,
        };
}
