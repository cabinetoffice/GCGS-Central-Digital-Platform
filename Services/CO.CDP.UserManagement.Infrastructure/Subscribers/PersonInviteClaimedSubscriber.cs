using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Events;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.Subscribers;

/// <summary>
/// Handles PersonInviteClaimed events to create memberships and assignments.
/// </summary>
public class PersonInviteClaimedSubscriber(
    IInviteRoleMappingRepository inviteRoleMappingRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IUserApplicationAssignmentRepository assignmentRepository,
    ICdpMembershipSyncService membershipSyncService,
    IUnitOfWork unitOfWork,
    ILogger<PersonInviteClaimedSubscriber> logger) : ISubscriber<PersonInviteClaimed>
{
    public async Task Handle(PersonInviteClaimed @event)
    {
        var mapping = await inviteRoleMappingRepository.GetByCdpPersonInviteGuidAsync(@event.PersonInviteGuid);

        if (mapping == null)
        {
            logger.LogInformation("InviteRoleMapping not found for CDP invite {InviteGuid}", @event.PersonInviteGuid);
        }
        else
        {
            var organisation = mapping.Organisation;

            if (organisation.CdpOrganisationGuid != @event.OrganisationGuid)
            {
                logger.LogError(
                    "Organisation GUID mismatch for invite {InviteGuid}. Expected {ExpectedGuid}, received {ActualGuid}",
                    mapping.CdpPersonInviteGuid,
                    organisation.CdpOrganisationGuid,
                    @event.OrganisationGuid);
            }
            else
            {
                var existingMembership = await membershipRepository.GetByUserAndOrganisationAsync(
                    @event.UserUrn,
                    organisation.Id,
                    CancellationToken.None);

                if (existingMembership == null)
                {
                    var membership = new UserOrganisationMembership
                    {
                        UserPrincipalId = @event.UserUrn,
                        CdpPersonId = @event.PersonGuid,
                        OrganisationId = organisation.Id,
                        OrganisationRole = mapping.OrganisationRole,
                        IsActive = true,
                        JoinedAt = DateTimeOffset.UtcNow,
                        InvitedBy = mapping.CreatedBy
                    };

                    membershipRepository.Add(membership);

                    var assignments = BuildAssignments(mapping, membership);
                    foreach (var assignment in assignments)
                    {
                        assignmentRepository.Add(assignment);
                    }

                    inviteRoleMappingRepository.Remove(mapping);
                    await unitOfWork.SaveChangesAsync();

                    await membershipSyncService.SyncMembershipCreatedAsync(membership, CancellationToken.None);
                }
                else
                {
                    inviteRoleMappingRepository.Remove(mapping);
                    await unitOfWork.SaveChangesAsync();

                    logger.LogInformation(
                        "Membership already exists for user {UserUrn} in organisation {OrganisationId}, mapping removed",
                        @event.UserUrn,
                        organisation.Id);
                }
            }
        }
    }

    private IEnumerable<UserApplicationAssignment> BuildAssignments(
        InviteRoleMapping mapping,
        UserOrganisationMembership membership)
    {
        var assignments = mapping.ApplicationAssignments
            .Where(a => a.OrganisationApplication.IsActive &&
                        a.ApplicationRole.IsActive)
            .Select(a => new UserApplicationAssignment
            {
                UserOrganisationMembership = membership,
                OrganisationApplicationId = a.OrganisationApplicationId,
                IsActive = true,
                AssignedAt = DateTimeOffset.UtcNow,
                Roles = new List<ApplicationRole> { a.ApplicationRole }
            })
            .ToList();

        var skippedAssignments = mapping.ApplicationAssignments.Count - assignments.Count;
        if (skippedAssignments > 0)
        {
            logger.LogWarning("Skipped {SkippedCount} invite assignments for invite {InviteGuid}",
                skippedAssignments,
                mapping.CdpPersonInviteGuid);
        }

        return assignments;
    }
}
