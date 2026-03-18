using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

public class UmOrganisationSyncRepository(
    IOrganisationRepository organisationRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    ISlugGeneratorService slugGeneratorService,
    IUnitOfWork unitOfWork) : IUmOrganisationSyncRepository
{
    private const string SystemUser = "system:org-sync";

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

    public async Task EnsureFounderOwnerCreatedAsync(
        Guid cdpOrganisationGuid,
        Guid cdpPersonGuid,
        string userPrincipalId,
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

        if (existingMembership is null)
        {
            membershipRepository.Add(new UserOrganisationMembership
            {
                UserPrincipalId = userPrincipalId,
                CdpPersonId = cdpPersonGuid,
                OrganisationId = organisation.Id,
                OrganisationRoleId = (int)OrganisationRole.Owner,
                IsActive = true,
                JoinedAt = DateTimeOffset.UtcNow,
                CreatedBy = SystemUser
            });

            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
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
