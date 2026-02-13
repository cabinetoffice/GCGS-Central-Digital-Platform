using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PersistencePerson = CO.CDP.OrganisationInformation.Persistence.Person;

namespace CO.CDP.UserManagement.Infrastructure.Services;

public class OrganisationPersonsSyncService(
    OrganisationInformationContext orgContext,
    IUserOrganisationMembershipRepository membershipRepository,
    IUnitOfWork unitOfWork,
    ILogger<OrganisationPersonsSyncService> logger) : IOrganisationPersonsSyncService
{
    private const string AdminScope = "ADMIN";
    private const string SystemSyncUser = "system:org-sync";

    public async Task SyncOrganisationMembershipsAsync(
        Guid organisationCdpGuid,
        int organisationId,
        CancellationToken cancellationToken = default)
    {
        if (organisationCdpGuid == Guid.Empty)
        {
            logger.LogError("Invalid organisation GUID provided for membership sync");
            throw new ArgumentException("Organisation CDP GUID cannot be empty.", nameof(organisationCdpGuid));
        }

        if (organisationId <= 0)
        {
            logger.LogError("Invalid organisation ID {OrgId} provided for membership sync", organisationId);
            throw new ArgumentOutOfRangeException(nameof(organisationId), organisationId, "Organisation ID must be positive.");
        }

        logger.LogInformation("Starting membership sync for organisation {CdpGuid} (ID: {OrgId})",
            organisationCdpGuid, organisationId);

        var organisationPersons = await GetOrganisationPersonsAsync(organisationCdpGuid, cancellationToken);

        if (!organisationPersons.Any())
        {
            logger.LogInformation("No persons found for organisation {CdpGuid}", organisationCdpGuid);
            return;
        }

        logger.LogInformation("Found {Count} persons for organisation {CdpGuid}",
            organisationPersons.Count, organisationCdpGuid);

        var existingMembers = await membershipRepository.GetByOrganisationIdAsync(organisationId, cancellationToken);
        var hasExistingMembers = existingMembers.Any();

        var results = await Task.WhenAll(
            organisationPersons.Select(op => ProcessPersonAsync(op, organisationId, cancellationToken)));

        var validResults = results
            .Where(r => r.Membership != null && r.Person != null)
            .ToList();

        var membershipsToCreate = AssignRoles(validResults, hasExistingMembers)
            .ToList();

        var skipped = results.Count(r => r.Membership == null);

        if (membershipsToCreate.Any())
        {
            foreach (var membership in membershipsToCreate)
            {
                membershipRepository.Add(membership);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation(
                "Successfully synced {CreatedCount} memberships for organisation {OrgId} ({SkippedCount} skipped)",
                membershipsToCreate.Count, organisationId, skipped);
        }
        else
        {
            logger.LogInformation(
                "No new memberships created for organisation {OrgId} ({SkippedCount} skipped)",
                organisationId, skipped);
        }
    }

    private async Task<List<OrganisationPerson>> GetOrganisationPersonsAsync(
        Guid organisationCdpGuid,
        CancellationToken cancellationToken)
    {
        var organisation = await orgContext.Organisations
            .Include(o => o.OrganisationPersons)
                .ThenInclude(op => op.Person)
            .FirstOrDefaultAsync(o => o.Guid == organisationCdpGuid, cancellationToken);

        return organisation?.OrganisationPersons.ToList() ?? [];
    }

    private async Task<MembershipResult> ProcessPersonAsync(
        OrganisationPerson orgPerson,
        int organisationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var person = orgPerson.Person;

            if (string.IsNullOrWhiteSpace(person.UserUrn))
            {
                logger.LogWarning("Person {PersonId} has no UserUrn, skipping membership creation",
                    person.Guid);
                return MembershipResult.Skipped();
            }

            var existingMembership = await membershipRepository
                .GetByPersonIdAndOrganisationAsync(person.Guid, organisationId, cancellationToken);

            if (existingMembership != null)
            {
                logger.LogDebug("Membership already exists for person {PersonId} in organisation {OrgId}",
                    person.Guid, organisationId);
                return MembershipResult.Skipped();
            }

            var membership = CreateMembership(person, organisationId, OrganisationRole.Member);

            return MembershipResult.Created(membership, person, orgPerson.Scopes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create membership for person in organisation {OrgId}",
                organisationId);
            return MembershipResult.Skipped();
        }
    }

    private IEnumerable<UserOrganisationMembership> AssignRoles(
        List<MembershipResult> results,
        bool hasExistingMembers)
    {
        var firstAdminProcessed = false;

        foreach (var result in results)
        {
            var hasAdminScope = result.Scopes.Contains(AdminScope);

            var role = DetermineRole(hasAdminScope, hasExistingMembers, firstAdminProcessed);

            if (hasAdminScope && role == OrganisationRole.Owner)
            {
                firstAdminProcessed = true;
            }

            logger.LogDebug("Assigned role {Role} to person {PersonId}",
                role, result.Person!.Guid);

            var membership = result.Membership!;
            membership.OrganisationRole = role;
            yield return membership;
        }
    }

    private static OrganisationRole DetermineRole(
        bool hasAdminScope,
        bool hasExistingMembers,
        bool firstAdminProcessed) =>
        hasAdminScope switch
        {
            false => OrganisationRole.Member,
            true when !hasExistingMembers && !firstAdminProcessed => OrganisationRole.Owner,
            true => OrganisationRole.Admin
        };

    private static UserOrganisationMembership CreateMembership(
        PersistencePerson person,
        int organisationId,
        OrganisationRole role) =>
        new()
        {
            UserPrincipalId = person.UserUrn,
            CdpPersonId = person.Guid,
            OrganisationId = organisationId,
            OrganisationRole = role,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            InvitedBy = null,
            CreatedBy = SystemSyncUser
        };

    private sealed record MembershipResult(
        UserOrganisationMembership? Membership,
        PersistencePerson? Person,
        List<string> Scopes)
    {
        public static MembershipResult Created(
            UserOrganisationMembership membership,
            PersistencePerson person,
            List<string> scopes) =>
            new(membership, person, scopes);

        public static MembershipResult Skipped() => new(null, null, []);
    }
}
