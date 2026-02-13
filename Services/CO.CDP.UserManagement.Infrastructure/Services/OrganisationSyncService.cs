using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Infrastructure.Services;

public class OrganisationSyncService(
    IOrganisationRepository organisationRepository,
    ISlugGeneratorService slugGeneratorService,
    IOrganisationPersonsSyncService personsSyncService,
    IUnitOfWork unitOfWork,
    ILogger<OrganisationSyncService> logger) : IOrganisationSyncService
{
    private const string SystemSyncUser = "system:org-sync";
    private const int MaxSlugAttempts = 10;

    public async Task SyncRegisteredAsync(string id, string name, CancellationToken cancellationToken = default)
    {
        var (cdpGuid, validatedName) = ParseEvent(id, name, "OrganisationRegistered");

        var existing = await organisationRepository.GetByCdpGuidAsync(cdpGuid, cancellationToken);
        if (existing != null)
        {
            logger.LogInformation("Organisation {CdpGuid} already exists in UserManagement (Id: {Id}), skipping",
                cdpGuid, existing.Id);
            return;
        }

        await CreateOrganisationWithRetry(cdpGuid, validatedName, OrganisationSyncOrigin.Registered, cancellationToken);
    }

    public async Task SyncUpdatedAsync(string id, string name, CancellationToken cancellationToken = default)
    {
        var (cdpGuid, validatedName) = ParseEvent(id, name, "OrganisationUpdated");

        var existing = await organisationRepository.GetByCdpGuidAsync(cdpGuid, cancellationToken);

        if (existing == null)
        {
            logger.LogWarning(
                "Organisation {CdpGuid} not found in UserManagement during update event - creating it (out-of-order delivery)",
                cdpGuid);

            await CreateOrganisationWithRetry(cdpGuid, validatedName, OrganisationSyncOrigin.UpdateOutOfOrder, cancellationToken);
            return;
        }

        if (existing.Name != validatedName)
        {
            await UpdateOrganisationWithRetry(cdpGuid, existing, validatedName, cancellationToken);
        }
        else
        {
            logger.LogDebug("Organisation {CdpGuid} name unchanged, skipping update", cdpGuid);
        }
    }

    private (Guid CdpGuid, string Name) ParseEvent(string id, string name, string eventName)
    {
        var cdpGuid = ParseGuid(id, eventName);

        if (string.IsNullOrWhiteSpace(name))
        {
            logger.LogError("Invalid organisation name for {EventName} event: {Name}", eventName, name);
            throw new ArgumentException($"Invalid organisation name: {name}");
        }

        return (cdpGuid, name);
    }

    private Guid ParseGuid(string id, string eventName)
    {
        if (!Guid.TryParse(id, out var cdpGuid))
        {
            logger.LogError("Invalid GUID format for {EventName} event: {Id}", eventName, id);
            throw new ArgumentException($"Invalid GUID format: {id}");
        }

        return cdpGuid;
    }

    private async Task CreateOrganisationWithRetry(
        Guid cdpGuid,
        string name,
        OrganisationSyncOrigin origin,
        CancellationToken cancellationToken)
    {
        var baseSlug = ValidateSlug(slugGeneratorService.GenerateSlug(name));
        await TryWithSlugCandidates(
            baseSlug,
            slug => TryCreateOrganisation(cdpGuid, name, slug, origin, cancellationToken),
            $"Failed to create organisation {cdpGuid} after {MaxSlugAttempts} attempts. Base slug: '{baseSlug}'",
            cancellationToken);
    }

    private async Task<OrganisationSyncAttemptResult> TryCreateOrganisation(
        Guid cdpGuid,
        string name,
        string slug,
        OrganisationSyncOrigin origin,
        CancellationToken cancellationToken)
    {
        var organisation = BuildOrganisation(cdpGuid, name, slug);

        organisationRepository.Add(organisation);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);

            LogCreateSuccess(origin, cdpGuid, slug, organisation.Id);

            await SyncMemberships(cdpGuid, organisation.Id, cancellationToken);

            return OrganisationSyncAttemptResult.Success(slug);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Slug collision for '{Slug}', retrying", slug);
            return OrganisationSyncAttemptResult.Retry(slug);
        }
    }

    private void LogCreateSuccess(OrganisationSyncOrigin origin, Guid cdpGuid, string slug, int organisationId)
    {
        switch (origin)
        {
            case OrganisationSyncOrigin.Registered:
                logger.LogInformation(
                    "Synced organisation {CdpGuid} to UserManagement with slug '{Slug}' (Id: {Id})",
                    cdpGuid, slug, organisationId);
                break;
            case OrganisationSyncOrigin.UpdateOutOfOrder:
                logger.LogInformation(
                    "Created organisation {CdpGuid} in UserManagement with slug '{Slug}' (Id: {Id}) from update event",
                    cdpGuid, slug, organisationId);
                break;
        }
    }

    private async Task UpdateOrganisationWithRetry(
        Guid cdpGuid,
        CoreOrganisation existing,
        string newName,
        CancellationToken cancellationToken)
    {
        var baseSlug = ValidateSlug(slugGeneratorService.GenerateSlug(newName));
        await TryWithSlugCandidates(
            baseSlug,
            slug => TryUpdateOrganisation(cdpGuid, existing, newName, slug, cancellationToken),
            $"Failed to update organisation {cdpGuid} after {MaxSlugAttempts} attempts. Base slug: '{baseSlug}'",
            cancellationToken);
    }

    private async Task<OrganisationSyncAttemptResult> TryUpdateOrganisation(
        Guid cdpGuid,
        CoreOrganisation existing,
        string newName,
        string slug,
        CancellationToken cancellationToken)
    {
        existing.Name = newName;
        existing.Slug = slug;
        existing.ModifiedBy = SystemSyncUser;

        organisationRepository.Update(existing);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Updated organisation {CdpGuid} in UserManagement: Name='{Name}', Slug='{Slug}' (Id: {Id})",
                cdpGuid, existing.Name, existing.Slug, existing.Id);
            return OrganisationSyncAttemptResult.Success(slug);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Slug collision for '{Slug}' during update, retrying", slug);
            return OrganisationSyncAttemptResult.Retry(slug);
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505";

    private string ValidateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            logger.LogError("Slug generator produced an empty slug.");
            throw new InvalidOperationException("Slug generator produced an empty slug.");
        }

        return slug;
    }

    private CoreOrganisation BuildOrganisation(Guid cdpGuid, string name, string slug) =>
        new()
        {
            CdpOrganisationGuid = cdpGuid,
            Name = name,
            Slug = slug,
            IsActive = true,
            CreatedBy = SystemSyncUser
        };

    private async Task SyncMemberships(Guid cdpGuid, int organisationId, CancellationToken cancellationToken)
    {
        try
        {
            await personsSyncService.SyncOrganisationMembershipsAsync(
                cdpGuid,
                organisationId,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync memberships for organisation {OrgId}", organisationId);
        }
    }

    private static async Task TryWithSlugCandidates(
        string baseSlug,
        Func<string, Task<OrganisationSyncAttemptResult>> attempt,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        var succeeded = await BuildSlugCandidates(baseSlug)
            .ToAsyncEnumerable()
            .AnyAwaitAsync(async slug =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await attempt(slug);
                return result.Succeeded;
            });

        if (!succeeded)
        {
            cancellationToken.ThrowIfCancellationRequested();
            throw new InvalidOperationException(errorMessage);
        }
    }

    private static IEnumerable<string> BuildSlugCandidates(string baseSlug) =>
        Enumerable.Range(0, MaxSlugAttempts)
            .Select(attempt => attempt == 0 ? baseSlug : $"{baseSlug}-{attempt}");

    private enum OrganisationSyncOrigin
    {
        Registered,
        UpdateOutOfOrder
    }

    private sealed record OrganisationSyncAttemptResult(bool Succeeded, string Slug)
    {
        public static OrganisationSyncAttemptResult Success(string slug) => new(true, slug);

        public static OrganisationSyncAttemptResult Retry(string slug) => new(false, slug);
    }
}
