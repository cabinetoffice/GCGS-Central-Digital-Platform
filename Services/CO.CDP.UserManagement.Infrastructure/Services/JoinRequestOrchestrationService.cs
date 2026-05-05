using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Orchestrates approval and rejection of organisation join requests.
/// Reads/writes join request state via the OI API (<see cref="IOrganisationApiAdapter"/>)
/// and creates a <see cref="UserOrganisationMembership"/> on approval.
/// </summary>
/// <remarks>
/// Cross-service write on approve (OI update → UM membership) shares the same risk profile as
/// <see cref="InviteOrchestrationService"/>. An outbox/queue pattern will make this atomic when
/// that infrastructure lands; <see cref="IOrganisationApiAdapter"/> is the swap point.
/// </remarks>
public class JoinRequestOrchestrationService(
    IOrganisationRepository organisationRepository,
    IOrganisationApiAdapter organisationApiAdapter,
    IPersonApiAdapter personApiAdapter,
    IUserOrganisationMembershipRepository membershipRepository,
    IRoleMappingService roleMappingService,
    IUnitOfWork unitOfWork,
    ILogger<JoinRequestOrchestrationService> logger) : IJoinRequestOrchestrationService
{
    public async Task ApproveJoinRequestAsync(
        Guid cdpOrganisationId,
        Guid joinRequestId,
        Guid requestingPersonId,
        string reviewerPrincipalId,
        CancellationToken cancellationToken = default)
    {
        var organisation = await GetOrganisationAsync(cdpOrganisationId, cancellationToken);
        var reviewerPersonId = await ResolvePersonIdAsync(reviewerPrincipalId, cancellationToken);

        await organisationApiAdapter.UpdateJoinRequestAsync(
            cdpOrganisationId, joinRequestId, "Accepted", reviewerPersonId, cancellationToken);

        logger.LogInformation(
            "Approved join request {JoinRequestId} for person {PersonId} in organisation {CdpOrganisationId}",
            joinRequestId, requestingPersonId, cdpOrganisationId);

        if (await membershipRepository.ExistsByPersonIdAndOrganisationAsync(
                requestingPersonId, organisation.Id, cancellationToken))
        {
            logger.LogWarning(
                "Membership already exists for person {PersonId} in organisation {OrganisationId} — skipping creation",
                requestingPersonId, organisation.Id);
            return;
        }

        var membership = new UserOrganisationMembership
        {
            CdpPersonId = requestingPersonId,
            UserPrincipalId = string.Empty,
            OrganisationId = organisation.Id,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            InvitedBy = reviewerPersonId.ToString()
        };
        await roleMappingService.ApplyRoleDefinitionAsync(membership, OrganisationRole.Member, cancellationToken);

        membershipRepository.Add(membership);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created membership {MembershipId} for person {PersonId} in organisation {CdpOrganisationId}",
            membership.Id, requestingPersonId, cdpOrganisationId);
    }

    public async Task RejectJoinRequestAsync(
        Guid cdpOrganisationId,
        Guid joinRequestId,
        Guid requestingPersonId,
        string reviewerPrincipalId,
        CancellationToken cancellationToken = default)
    {
        var reviewerPersonId = await ResolvePersonIdAsync(reviewerPrincipalId, cancellationToken);

        await organisationApiAdapter.UpdateJoinRequestAsync(
            cdpOrganisationId, joinRequestId, "Rejected", reviewerPersonId, cancellationToken);

        logger.LogInformation(
            "Rejected join request {JoinRequestId} for person {PersonId} in organisation {CdpOrganisationId}",
            joinRequestId, requestingPersonId, cdpOrganisationId);
    }

    private async Task<Organisation>
        GetOrganisationAsync(Guid cdpOrganisationId, CancellationToken cancellationToken) =>
        await organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken)
        ?? throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);

    /// <summary>
    /// Resolves the OI person GUID from a user principal ID (URN/sub).
    /// The OI API's UpdateJoinRequest requires an OI person Guid for ReviewedBy.
    /// </summary>
    private async Task<Guid> ResolvePersonIdAsync(string userPrincipalId, CancellationToken cancellationToken)
    {
        var details = await personApiAdapter.GetPersonDetailsAsync(userPrincipalId, cancellationToken);
        if (details == null)
            throw new EntityNotFoundException("Person", userPrincipalId);
        return details.CdpPersonId;
    }
}