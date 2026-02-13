using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Infrastructure.Subscribers;

/// <summary>
/// Handles OrganisationUpdated events from CDP to keep UserManagement organisations in sync.
/// </summary>
public class OrganisationUpdatedSubscriber(
    IOrganisationRepository organisationRepository,
    ISlugGeneratorService slugGeneratorService,
    IUnitOfWork unitOfWork,
    ILogger<OrganisationUpdatedSubscriber> logger)
    : ISubscriber<OrganisationUpdated>
{
    public async Task Handle(OrganisationUpdated @event)
    {
        if (!Guid.TryParse(@event.Id, out var cdpGuid))
        {
            logger.LogError("Invalid GUID format for OrganisationUpdated event: {Id}", @event.Id);
            throw new ArgumentException($"Invalid GUID format: {@event.Id}");
        }

        var existing = await organisationRepository.GetByCdpGuidAsync(cdpGuid);

        if (existing == null)
        {
            // Out-of-order delivery: OrganisationUpdated arrived before OrganisationRegistered
            logger.LogWarning(
                "Organisation {CdpGuid} not found in UserManagement during update event - creating it (out-of-order delivery)",
                cdpGuid);

            await CreateOrganisationWithRetry(cdpGuid, @event.Name);
            return;
        }

        // Update if name changed
        if (existing.Name != @event.Name)
        {
            await UpdateOrganisationWithRetry(cdpGuid, existing, @event.Name);
        }
        else
        {
            logger.LogDebug("Organisation {CdpGuid} name unchanged, skipping update", cdpGuid);
        }
    }

    private async Task CreateOrganisationWithRetry(Guid cdpGuid, string name)
    {
        var baseSlug = slugGeneratorService.GenerateSlug(name);
        const int maxAttempts = 10;

        var succeeded = await Enumerable.Range(0, maxAttempts)
            .Select(attempt => attempt == 0 ? baseSlug : $"{baseSlug}-{attempt}")
            .ToAsyncEnumerable()
            .AnyAwaitAsync(async slug => await TryCreateOrganisation(cdpGuid, name, slug));

        if (!succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create organisation {cdpGuid} after {maxAttempts} attempts. Base slug: '{baseSlug}'");
        }
    }

    private async Task<bool> TryCreateOrganisation(Guid cdpGuid, string name, string slug)
    {
        var organisation = new CoreOrganisation
        {
            CdpOrganisationGuid = cdpGuid,
            Name = name,
            Slug = slug,
            IsActive = true,
            CreatedBy = "system:org-sync"
        };

        organisationRepository.Add(organisation);

        try
        {
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation(
                "Created organisation {CdpGuid} in UserManagement with slug '{Slug}' (Id: {Id}) from update event",
                cdpGuid, slug, organisation.Id);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Slug collision for '{Slug}', retrying", slug);
            return false;
        }
    }

    private async Task UpdateOrganisationWithRetry(Guid cdpGuid, CoreOrganisation existing, string newName)
    {
        var baseSlug = slugGeneratorService.GenerateSlug(newName);
        const int maxAttempts = 10;

        var succeeded = await Enumerable.Range(0, maxAttempts)
            .Select(attempt => attempt == 0 ? baseSlug : $"{baseSlug}-{attempt}")
            .ToAsyncEnumerable()
            .AnyAwaitAsync(async slug => await TryUpdateOrganisation(cdpGuid, existing, newName, slug));

        if (!succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to update organisation {cdpGuid} after {maxAttempts} attempts. Base slug: '{baseSlug}'");
        }
    }

    private async Task<bool> TryUpdateOrganisation(Guid cdpGuid, CoreOrganisation existing, string newName, string slug)
    {
        existing.Name = newName;
        existing.Slug = slug;
        existing.ModifiedBy = "system:org-sync";

        organisationRepository.Update(existing);

        try
        {
            await unitOfWork.SaveChangesAsync();
            logger.LogInformation(
                "Updated organisation {CdpGuid} in UserManagement: Name='{Name}', Slug='{Slug}' (Id: {Id})",
                cdpGuid, existing.Name, existing.Slug, existing.Id);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Slug collision for '{Slug}' during update, retrying", slug);
            return false;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505";
}
