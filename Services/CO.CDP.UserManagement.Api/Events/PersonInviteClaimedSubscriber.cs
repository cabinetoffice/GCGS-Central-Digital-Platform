using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;

namespace CO.CDP.UserManagement.Api.Events;

public class PersonInviteClaimedSubscriber(
    IPendingOrganisationInviteRepository pendingInviteRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IOrganisationRepository organisationRepository,
    IUnitOfWork unitOfWork,
    ILogger<PersonInviteClaimedSubscriber> logger)
    : ISubscriber<PersonInviteClaimed>
{
    public async Task Handle(PersonInviteClaimed @event)
    {
        var pendingInvite = await pendingInviteRepository.GetByCdpPersonInviteGuidAsync(@event.PersonInviteGuid);
        if (pendingInvite == null)
        {
            logger.LogWarning(
                "Pending invite not found for person invite GUID {PersonInviteGuid}",
                @event.PersonInviteGuid);
            return;
        }

        var organisation = await organisationRepository.GetByCdpGuidAsync(@event.OrganisationGuid);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), @event.OrganisationGuid);
        }

        var membership = new UserOrganisationMembership
        {
            UserPrincipalId = @event.UserUrn,
            CdpPersonId = @event.PersonGuid,
            OrganisationId = organisation.Id,
            OrganisationRole = pendingInvite.OrganisationRole,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            InvitedBy = pendingInvite.InvitedBy
        };

        membershipRepository.Add(membership);
        pendingInviteRepository.Remove(pendingInvite);
        await unitOfWork.SaveChangesAsync();
    }
}
