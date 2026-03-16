using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Infrastructure.Repositories;

public class UmOrganisationSyncRepository(
    IOrganisationRepository organisationRepository,
    ISlugGeneratorService slugGeneratorService,
    IUnitOfWork unitOfWork) : IUmOrganisationSyncRepository
{
    private const string SystemUser = "system:org-sync";

    public async Task EnsureCreatedAsync(
        Guid cdpGuid, string name, CancellationToken cancellationToken = default)
    {
        var existing = await organisationRepository.GetByCdpGuidAsync(cdpGuid, cancellationToken);
        if (existing is not null) return;

        var slug = await ResolveUniqueSlugAsync(name, cancellationToken: cancellationToken);
        organisationRepository.Add(BuildOrganisation(cdpGuid, name, slug));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureNameSyncedAsync(
        Guid cdpGuid, string name, CancellationToken cancellationToken = default)
    {
        var existing = await organisationRepository.GetByCdpGuidAsync(cdpGuid, cancellationToken);
        await (existing is null
            ? EnsureCreatedAsync(cdpGuid, name, cancellationToken)
            : UpdateNameIfChangedAsync(existing, name, cancellationToken));
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
