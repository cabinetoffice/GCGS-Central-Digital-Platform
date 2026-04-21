using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.UseCase;
using CO.CDP.UserManagement.Infrastructure.Events;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.UserManagement.Infrastructure.UseCase.UpdateApplicationAssignment;

public record UpdateApplicationAssignmentCommand(
    string UserId,
    int OrganisationId,
    int AssignmentId,
    IEnumerable<int> RoleIds);

public class UpdateApplicationAssignmentUseCase(
    IUserOrganisationMembershipRepository membershipRepository,
    IUserApplicationAssignmentRepository assignmentRepository,
    IOrganisationRepository organisationRepository,
    IRoleMappingService roleMappingService,
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<UpdateApplicationAssignmentUseCase> logger)
    : IUseCase<UpdateApplicationAssignmentCommand, UserApplicationAssignment>
{
    public async Task<UserApplicationAssignment> Execute(UpdateApplicationAssignmentCommand command,
        CancellationToken ct = default)
    {
        var (membership, assignment) =
            await GetAssignmentForUserAsync(command.UserId, command.OrganisationId, command.AssignmentId, ct);

        if (!assignment.IsActive)
            throw new SystemInvalidOperationException($"Assignment {command.AssignmentId} is not active");

        var roles = await GetValidatedRolesAsync(
            command.OrganisationId,
            membership.OrganisationRole,
            assignment.OrganisationApplication.ApplicationId,
            command.RoleIds,
            ct);

        assignment.Roles.Clear();
        foreach (var role in roles)
            assignment.Roles.Add(role);

        assignmentRepository.Update(assignment);
        await unitOfWork.SaveChangesAsync(ct);

        await PublishScopesAsync(membership, command.OrganisationId, ct);

        logger.LogInformation(
            "Assignment {AssignmentId} updated for user {UserId} in organisation {OrganisationId}",
            command.AssignmentId, command.UserId, command.OrganisationId);

        return assignment;
    }

    private async Task<(UserOrganisationMembership, UserApplicationAssignment)> GetAssignmentForUserAsync(
        string userId, int organisationId, int assignmentId, CancellationToken ct)
    {
        var membership = (Guid.TryParse(userId, out var cdpPersonId)
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

    private async Task<IReadOnlyList<ApplicationRole>> GetValidatedRolesAsync(
        int organisationId,
        OrganisationRole orgRole,
        int applicationId,
        IEnumerable<int> roleIds,
        CancellationToken ct)
    {
        var roles = await roleMappingService.GetAssignableRolesAsync(organisationId, orgRole, roleIds, ct);

        var invalid = roles.FirstOrDefault(r => r.ApplicationId != applicationId || !r.IsActive);
        if (invalid != null)
            throw invalid.ApplicationId != applicationId
                ? new SystemInvalidOperationException(
                    $"Role {invalid.Id} does not belong to application {applicationId}")
                : new SystemInvalidOperationException($"Role {invalid.Id} is not active");

        return roles;
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