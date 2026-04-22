using CO.CDP.Functional;
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

        await Option
            .From(
                await membershipRepository.GetByPersonIdAndOrganisationAsync(command.CdpPersonId, organisation.Id, ct))
            .Where(m => m.IsActive)
            .TapAsync(async membership =>
            {
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

                await Option.From(membership.CdpPersonId).TapAsync(personId =>
                    publisher.Publish(new PersonRemovedFromOrganisation
                    {
                        OrganisationId = command.CdpOrganisationId.ToString(),
                        PersonId = personId.ToString()
                    }));

                await unitOfWork.SaveChangesAsync(ct);

                logger.LogInformation(
                    "User {CdpPersonId} removed from organisation {CdpOrganisationId} by {ActingUser}",
                    command.CdpPersonId, command.CdpOrganisationId, command.ActingUserId);
            });
    }

    private async Task GuardLastOwnerAsync(
        UserOrganisationMembership membership, int organisationId, Guid cdpOrganisationId, CancellationToken ct)
    {
        var isLastOwner = membership.OrganisationRole == OrganisationRole.Owner
                          && await membershipRepository.CountActiveOwnersByOrganisationIdAsync(organisationId, ct) <= 1;
        if (isLastOwner)
            throw new LastOwnerRemovalException(cdpOrganisationId);
    }
}