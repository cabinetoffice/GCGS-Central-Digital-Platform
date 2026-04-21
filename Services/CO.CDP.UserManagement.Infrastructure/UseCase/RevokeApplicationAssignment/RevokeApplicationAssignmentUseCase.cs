using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.UseCase;
using CO.CDP.UserManagement.Infrastructure.Events;
using Microsoft.Extensions.Logging;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.UserManagement.Infrastructure.UseCase.RevokeApplicationAssignment;

public record RevokeApplicationAssignmentCommand(string UserId, int OrganisationId, int AssignmentId);

public class RevokeApplicationAssignmentUseCase(
    IUserOrganisationMembershipRepository membershipRepository,
    IUserApplicationAssignmentRepository assignmentRepository,
    IOrganisationRepository organisationRepository,
    IRoleMappingService roleMappingService,
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<RevokeApplicationAssignmentUseCase> logger) : IUseCase<RevokeApplicationAssignmentCommand>
{
    public async Task Execute(RevokeApplicationAssignmentCommand command, CancellationToken ct = default)
    {
        var (membership, assignment) =
            await GetAssignmentForUserAsync(command.UserId, command.OrganisationId, command.AssignmentId, ct);

        if (assignment.OrganisationApplication.Application.IsEnabledByDefault)
            throw new SystemInvalidOperationException(
                $"Application {assignment.OrganisationApplication.ApplicationId} is enabled by default and user access cannot be revoked");

        assignment.IsActive = false;
        assignment.RevokedAt = DateTimeOffset.UtcNow;
        assignmentRepository.Update(assignment);

        await unitOfWork.SaveChangesAsync(ct);

        await PublishScopesAsync(membership, command.OrganisationId, ct);

        logger.LogInformation(
            "Assignment {AssignmentId} revoked for user {UserId} in organisation {OrganisationId}",
            command.AssignmentId, command.UserId, command.OrganisationId);
    }

    private async Task<(UserOrganisationMembership, UserApplicationAssignment)> GetAssignmentForUserAsync(
        string userId, int organisationId, int assignmentId, CancellationToken ct)
    {
        bool isCdpGuid = Guid.TryParse(userId, out var cdpPersonId);
        var membership = (isCdpGuid
                             ? await membershipRepository.GetByPersonIdAndOrganisationAsync(cdpPersonId, organisationId,
                                 ct)
                             : await membershipRepository.GetByUserAndOrganisationAsync(userId, organisationId, ct))
                         ?? throw new EntityNotFoundException(
                             nameof(UserOrganisationMembership),
                             $"User {userId} in Organisation {organisationId}");

        var assignment = (await assignmentRepository.GetByMembershipIdAsync(membership.Id, ct))
                         .SingleOrDefault(a => a.Id == assignmentId)
                         ?? throw new EntityNotFoundException(nameof(UserApplicationAssignment), assignmentId);

        return (membership, assignment);
    }

    private async Task PublishScopesAsync(
        UserOrganisationMembership membership, int organisationId, CancellationToken ct)
    {
        if (!membership.CdpPersonId.HasValue)
            return;

        var organisation = await organisationRepository.GetByIdAsync(organisationId, ct)
                           ?? throw new EntityNotFoundException(nameof(Organisation), organisationId);

        var scopes = await roleMappingService.GetOrganisationInformationScopesAsync(membership.Id, ct);
        await publisher.Publish(new PersonScopesUpdated
        {
            OrganisationId = organisation.CdpOrganisationGuid.ToString(),
            PersonId = membership.CdpPersonId.Value.ToString(),
            Scopes = scopes.ToList()
        });
    }
}