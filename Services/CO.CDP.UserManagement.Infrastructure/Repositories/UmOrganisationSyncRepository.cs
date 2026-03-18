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
    ISlugGeneratorService slugGeneratorService,
    IUnitOfWork unitOfWork) : IUmOrganisationSyncRepository
{
    private const string SystemUser = "system:org-sync";
    private static readonly IReadOnlyList<string> FounderOrganisationInformationScopes = [OrganisationPersonScopes.Admin];

    public async Task EnsureCreatedAsync(
        Guid cdpGuid, string name, CancellationToken cancellationToken = default)
    {
        var existing = await organisationRepository.GetByCdpGuidAsync(cdpGuid, cancellationToken);
        if (existing is null)
        {
            var slug = await ResolveUniqueSlugAsync(name, cancellationToken: cancellationToken);
            organisationRepository.Add(BuildOrganisation(cdpGuid, name, slug));
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task EnsureNameSyncedAsync(
        Guid cdpGuid, string name, CancellationToken cancellationToken = default)
    {
        var existing = await organisationRepository.GetByCdpGuidAsync(cdpGuid, cancellationToken);
        await (existing is null
            ? EnsureCreatedAsync(cdpGuid, name, cancellationToken)
            : UpdateNameIfChangedAsync(existing, name, cancellationToken));
    }

    public async Task EnsureActiveApplicationsEnabledAsync(
        Guid cdpGuid,
        CancellationToken cancellationToken = default)
    {
        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpGuid, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Cannot enable applications because User Management organisation '{cdpGuid}' does not exist.");

        var activeApplications = (await applicationRepository.FindAsync(
            application => application.IsActive && !application.IsDeleted,
            cancellationToken)).ToList();

        var changes = await activeApplications
            .ToAsyncEnumerable()
            .SelectAwait(async application => await EnsureApplicationEnabledAsync(
                organisation.Id,
                application,
                cancellationToken))
            .ToListAsync(cancellationToken);

        if (changes.Any(changed => changed))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task EnsureFounderOwnerCreatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        string userPrincipalId,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userPrincipalId);

        var organisation = await organisationRepository.GetByCdpGuidAsync(cdpOrganisationGuid, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Cannot create founder membership because User Management organisation '{cdpOrganisationGuid}' does not exist.");

        var existingMembership = await membershipRepository.GetByPersonIdAndOrganisationAsync(
                                     cdpPersonGuid,
                                     organisation.Id,
                                     cancellationToken)
                                 ?? await membershipRepository.GetByUserAndOrganisationAsync(
                                     userPrincipalId,
                                     organisation.Id,
                                     cancellationToken);

        var membership = existingMembership;

        if (membership is null)
        {
            membership = new UserOrganisationMembership
            {
                UserPrincipalId = userPrincipalId,
                CdpPersonId = cdpPersonGuid,
                OrganisationId = organisation.Id,
                OrganisationRoleId = (int)OrganisationRole.Owner,
                IsActive = true,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedBy = SystemUser
            };

            membershipRepository.Add(membership);

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        await EnsureDefaultApplicationsAssignedAsync(membership, organisationPartyRoles, cancellationToken);
    }

    private Task UpdateNameIfChangedAsync(
        CoreOrganisation organisation, string name, CancellationToken cancellationToken) =>
        organisation.Name == name
            ? Task.CompletedTask
            : ApplyNameUpdateAsync(organisation, name, cancellationToken);

    private async Task ApplyNameUpdateAsync(
        CoreOrganisation organisation, string name, CancellationToken cancellationToken)
    {
        var slug = await ResolveUniqueSlugAsync(name, organisation.Id, cancellationToken);
        organisation.Name = name;
        organisation.Slug = slug;
        organisation.ModifiedBy = SystemUser;
        organisationRepository.Update(organisation);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureDefaultApplicationsAssignedAsync(
        UserOrganisationMembership membership,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken)
    {
        var defaultOrganisationApplications =
            (await organisationApplicationRepository.GetDefaultEnabledByOrganisationIdAsync(
                membership.OrganisationId,
                cancellationToken)).ToList();

        var changes = await defaultOrganisationApplications
            .ToAsyncEnumerable()
            .SelectAwait(async organisationApplication => await EnsureDefaultApplicationAssignedAsync(
                membership,
                organisationPartyRoles,
                organisationApplication,
                cancellationToken))
            .ToListAsync(cancellationToken);

        if (changes.Any(changed => changed))
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<bool> EnsureApplicationEnabledAsync(
        int organisationId,
        Application application,
        CancellationToken cancellationToken)
    {
        var existingRelationship = await organisationApplicationRepository.GetByOrganisationAndApplicationAsync(
            organisationId,
            application.Id,
            cancellationToken);

        if (existingRelationship is null)
        {
            organisationApplicationRepository.Add(new OrganisationApplication
            {
                OrganisationId = organisationId,
                ApplicationId = application.Id,
                IsActive = true,
                EnabledAt = DateTimeOffset.UtcNow,
                EnabledBy = SystemUser,
                CreatedBy = SystemUser
            });
            return true;
        }

        if (existingRelationship.IsActive && !existingRelationship.IsDeleted)
        {
            return false;
        }

        existingRelationship.IsActive = true;
        existingRelationship.IsDeleted = false;
        existingRelationship.EnabledAt = DateTimeOffset.UtcNow;
        existingRelationship.EnabledBy = SystemUser;
        existingRelationship.DisabledAt = null;
        existingRelationship.DisabledBy = null;
        existingRelationship.DeletedAt = null;
        existingRelationship.DeletedBy = null;
        existingRelationship.ModifiedBy = SystemUser;

        organisationApplicationRepository.Update(existingRelationship);
        return true;
    }

    private async Task<bool> EnsureDefaultApplicationAssignedAsync(
        UserOrganisationMembership membership,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        OrganisationApplication organisationApplication,
        CancellationToken cancellationToken)
    {
        var existingAssignment = await userApplicationAssignmentRepository.GetByMembershipAndApplicationAsync(
            membership.Id,
            organisationApplication.Id,
            cancellationToken);
        var defaultRoles = await GetDefaultRolesAsync(
            organisationApplication,
            organisationPartyRoles,
            cancellationToken);

        if (existingAssignment is null)
        {
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
            return true;
        }

        if (AssignmentMatches(existingAssignment, defaultRoles))
        {
            return false;
        }

        existingAssignment.IsActive = true;
        existingAssignment.IsDeleted = false;
        existingAssignment.AssignedAt = DateTimeOffset.UtcNow;
        existingAssignment.AssignedBy = SystemUser;
        existingAssignment.RevokedAt = null;
        existingAssignment.RevokedBy = null;
        existingAssignment.DeletedAt = null;
        existingAssignment.DeletedBy = null;
        existingAssignment.ModifiedBy = SystemUser;
        SyncRoles(existingAssignment, defaultRoles);

        userApplicationAssignmentRepository.Update(existingAssignment);
        return true;
    }

    private async Task<IReadOnlyList<ApplicationRole>> GetDefaultRolesAsync(
        OrganisationApplication organisationApplication,
        IReadOnlyCollection<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken) =>
        DefaultApplicationRoleSelector.SelectFor(
            organisationApplication,
            await roleRepository.GetByApplicationIdAsync(organisationApplication.ApplicationId, cancellationToken),
            organisationPartyRoles,
            FounderOrganisationInformationScopes);

    private static bool AssignmentMatches(
        UserApplicationAssignment assignment,
        IReadOnlyList<ApplicationRole> desiredRoles) =>
        assignment.IsActive &&
        !assignment.IsDeleted &&
        assignment.Roles.Select(role => role.Id).OrderBy(id => id)
            .SequenceEqual(desiredRoles.Select(role => role.Id).OrderBy(id => id));

    private static void SyncRoles(
        UserApplicationAssignment assignment,
        IEnumerable<ApplicationRole> desiredRoles)
    {
        assignment.Roles.Clear();
        foreach (var role in desiredRoles)
        {
            assignment.Roles.Add(role);
        }
    }

    private async Task<string> ResolveUniqueSlugAsync(
        string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var baseSlug = slugGeneratorService.GenerateSlug(name);
        return await BuildSlugCandidates(baseSlug)
            .ToAsyncEnumerable()
            .FirstOrDefaultAwaitAsync(
                async slug => !await organisationRepository.SlugExistsAsync(slug, excludeId, cancellationToken),
                cancellationToken)
            ?? throw new InvalidOperationException($"Could not generate a unique slug for '{name}'.");
    }

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
