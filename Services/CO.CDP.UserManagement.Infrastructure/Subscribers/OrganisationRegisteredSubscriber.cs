using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using CoreOrganisation = CO.CDP.UserManagement.Core.Entities.Organisation;

namespace CO.CDP.UserManagement.Infrastructure.Subscribers;

/// <summary>
/// Handles OrganisationRegistered events from CDP to auto-sync organisations into UserManagement.
/// </summary>
public class OrganisationRegisteredSubscriber(
    IOrganisationRepository organisationRepository,
    ISlugGeneratorService slugGeneratorService,
    IUnitOfWork unitOfWork,
    ILogger<OrganisationRegisteredSubscriber> logger)
    : ISubscriber<OrganisationRegistered>
{
    public async Task Handle(OrganisationRegistered @event)
    {
        if (!Guid.TryParse(@event.Id, out var cdpGuid))
        {
            logger.LogError("Invalid GUID format for OrganisationRegistered event: {Id}", @event.Id);
            throw new ArgumentException($"Invalid GUID format: {@event.Id}");
        }

        // Idempotent check - skip if already synced
        var existing = await organisationRepository.GetByCdpGuidAsync(cdpGuid);
        if (existing != null)
        {
            logger.LogInformation("Organisation {CdpGuid} already exists in UserManagement (Id: {Id}), skipping",
                cdpGuid, existing.Id);
            return;
        }

        var baseSlug = slugGeneratorService.GenerateSlug(@event.Name);
        const int maxAttempts = 10;

        var succeeded = await Enumerable.Range(0, maxAttempts)
            .Select(attempt => (Attempt: attempt, Slug: attempt == 0 ? baseSlug : $"{baseSlug}-{attempt}"))
            .ToAsyncEnumerable()
            .AnyAwaitAsync(async item =>
                await TryCreateOrganisation(cdpGuid, @event.Name, item.Slug, item.Attempt, maxAttempts));

        if (!succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create organisation {cdpGuid} after {maxAttempts} attempts due to slug collisions. Base slug: '{baseSlug}'");
        }
    }

    private async Task<bool> TryCreateOrganisation(Guid cdpGuid, string name, string slug, int attempt, int maxAttempts)
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
            logger.LogInformation("Synced organisation {CdpGuid} to UserManagement with slug '{Slug}' (Id: {Id})",
                cdpGuid, slug, organisation.Id);
            return true;
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            logger.LogWarning(ex, "Slug collision for '{Slug}' (attempt {Attempt}/{MaxAttempts}), retrying",
                slug, attempt + 1, maxAttempts);
            return false;
        }
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505";
}
