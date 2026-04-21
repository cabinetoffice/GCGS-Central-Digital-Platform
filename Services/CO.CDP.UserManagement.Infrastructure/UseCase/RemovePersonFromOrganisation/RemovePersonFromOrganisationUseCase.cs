using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.UseCase;
using CO.CDP.UserManagement.Infrastructure.Events;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.UseCase.RemovePersonFromOrganisation;

public record RemovePersonFromOrganisationCommand(Guid CdpOrganisationId, Guid CdpPersonId, string ActingUserId);

public class RemovePersonFromOrganisationUseCase(
    IOrganisationRepository organisationRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IUserApplicationAssignmentRepository assignmentRepository,
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<RemovePersonFromOrganisationUseCase> logger) : IUseCase<RemovePersonFromOrganisationCommand>
{
    public async Task Execute(RemovePersonFromOrganisationCommand command, CancellationToken ct = default)
    {
        var organisation = await organisationRepository.GetByCdpGuidAsync(command.CdpOrganisationId, ct)
                           ?? throw new EntityNotFoundException(nameof(Organisation), command.CdpOrganisationId);

        var membership = await membershipRepository.GetByPersonIdAndOrganisationAsync(
            command.CdpPersonId, organisation.Id, ct);

        if (membership == null)
            return;

        if (!membership.IsActive)
            return;

        await GuardLastOwnerAsync(membership, organisation.Id, command.CdpOrganisationId, ct);

        var now = DateTimeOffset.UtcNow;

        membership.IsActive = false;
        membership.IsDeleted = true;
        membership.DeletedAt = now;
        membership.DeletedBy = command.ActingUserId;
        membershipRepository.Update(membership);

        var assignments = (await assignmentRepository.GetByMembershipIdAsync(membership.Id, ct))
            .Where(a => a.IsActive)
            .ToList();

        foreach (var assignment in assignments)
        {
            assignment.IsActive = false;
            assignment.RevokedAt = now;
            assignment.RevokedBy = command.ActingUserId;
            assignmentRepository.Update(assignment);
        }

        // Publish event and then persist all changes atomically.
        // The publisher's DatabaseOutboxMessageRepository.SaveAsync calls SaveChangesAsync internally
        // (which may use the same or a different DbContext scope depending on DI configuration).
        // The explicit unitOfWork.SaveChangesAsync ensures membership/assignment changes are always persisted.
        if (membership.CdpPersonId.HasValue)
        {
            await publisher.Publish(new PersonRemovedFromOrganisation
            {
                OrganisationId = command.CdpOrganisationId.ToString(),
                PersonId = membership.CdpPersonId.Value.ToString()
            });
        }

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "User {CdpPersonId} removed from organisation {CdpOrganisationId} by {ActingUser}",
            command.CdpPersonId, command.CdpOrganisationId, command.ActingUserId);
    }

    private async Task GuardLastOwnerAsync(
        UserOrganisationMembership membership, int organisationId, Guid cdpOrganisationId, CancellationToken ct)
    {
        if (membership.OrganisationRole != OrganisationRole.Owner)
            return;
        if (await membershipRepository.CountActiveOwnersByOrganisationIdAsync(organisationId, ct) > 1)
            return;
        throw new LastOwnerRemovalException(cdpOrganisationId);
    }
}