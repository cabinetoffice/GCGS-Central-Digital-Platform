using CO.CDP.Functional;
using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.UseCase;
using CO.CDP.UserManagement.Infrastructure.Events;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.UserManagement.Infrastructure.UseCase.AssignUserToApplication;

public record AssignUserToApplicationCommand(
    string UserId,
    int OrganisationId,
    int ApplicationId,
    IEnumerable<int> RoleIds);

public class AssignUserToApplicationUseCase(
    IUserOrganisationMembershipRepository membershipRepository,
    IUserApplicationAssignmentRepository assignmentRepository,
    IOrganisationApplicationRepository organisationApplicationRepository,
    IOrganisationRepository organisationRepository,
    IRoleMappingService roleMappingService,
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<AssignUserToApplicationUseCase> logger)
    : IUseCase<AssignUserToApplicationCommand, UserApplicationAssignment>
{
    public async Task<UserApplicationAssignment> Execute(AssignUserToApplicationCommand command,
        CancellationToken ct = default)
    {
        var membership = await ResolveMembershipOrThrowAsync(command.UserId, command.OrganisationId, ct);

        var orgApp = await GetActiveOrganisationAppAsync(command.OrganisationId, command.ApplicationId, ct);

        var existingAssignment = await assignmentRepository
            .GetByMembershipAndApplicationAsync(membership.Id, orgApp.Id, ct);

        if (existingAssignment is { IsActive: true })
            throw new DuplicateEntityException(
                nameof(UserApplicationAssignment),
                $"User {command.UserId}, Application {command.ApplicationId}",
                $"{command.UserId}-{command.ApplicationId}");

        var roles = await GetValidatedRolesAsync(command.OrganisationId, membership.OrganisationRole,
            command.ApplicationId, command.RoleIds, ct);

        return await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var assignment = existingAssignment switch
            {
                not null => ReactiveExistingAssignment(existingAssignment, roles),
                _ => CreateNewAssignment(membership.Id, orgApp.Id, roles)
            };

            await unitOfWork.SaveChangesAsync(ct);

            await PublishScopesAsync(membership, command.OrganisationId, ct);

            logger.LogInformation(
                "User {UserId} assigned to application {ApplicationId} in organisation {OrganisationId}",
                command.UserId, command.ApplicationId, command.OrganisationId);

            return assignment;
        }, ct);
    }

    private async Task<UserOrganisationMembership> ResolveMembershipOrThrowAsync(
        string userId, int organisationId, CancellationToken ct) =>
        (Guid.TryParse(userId, out var cdpPersonId)
            ? await membershipRepository.GetByPersonIdAndOrganisationAsync(cdpPersonId, organisationId, ct)
            : await membershipRepository.GetByUserAndOrganisationAsync(userId, organisationId, ct))
        ?? throw new EntityNotFoundException(
            nameof(UserOrganisationMembership),
            $"User {userId} in Organisation {organisationId}");

    private async Task<OrganisationApplication> GetActiveOrganisationAppAsync(
        int organisationId, int applicationId, CancellationToken ct)
    {
        var orgApp = await organisationApplicationRepository
                         .GetByOrganisationAndApplicationAsync(organisationId, applicationId, ct)
                     ?? throw new EntityNotFoundException(
                         nameof(OrganisationApplication),
                         $"Organisation {organisationId} and Application {applicationId}");

        return orgApp.IsActive
            ? orgApp
            : throw new SystemInvalidOperationException(
                $"Application {applicationId} is not active for organisation {organisationId}");
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

    private UserApplicationAssignment ReactiveExistingAssignment(
        UserApplicationAssignment existing, IReadOnlyList<ApplicationRole> roles)
    {
        existing.IsActive = true;
        existing.AssignedAt = DateTimeOffset.UtcNow;
        existing.RevokedAt = null;
        existing.RevokedBy = null;
        existing.Roles.Clear();
        foreach (var role in roles)
            existing.Roles.Add(role);
        assignmentRepository.Update(existing);
        return existing;
    }

    private UserApplicationAssignment CreateNewAssignment(
        int membershipId, int orgAppId, IReadOnlyList<ApplicationRole> roles)
    {
        var assignment = new UserApplicationAssignment
        {
            UserOrganisationMembershipId = membershipId,
            OrganisationApplicationId = orgAppId,
            IsActive = true,
            AssignedAt = DateTimeOffset.UtcNow,
            Roles = roles.ToList()
        };
        assignmentRepository.Add(assignment);
        return assignment;
    }

    private async Task PublishScopesAsync(
        UserOrganisationMembership membership, int organisationId, CancellationToken ct) =>
        await Option.From(membership.CdpPersonId).TapAsync(async personId =>
        {
            var organisation = await organisationRepository.GetByIdAsync(organisationId, ct)
                               ?? throw new EntityNotFoundException(nameof(Organisation), organisationId);

            var scopes = await roleMappingService.GetOrganisationInformationScopesAsync(membership.Id, ct);
            await publisher.Publish(new PersonScopesUpdated
            {
                OrganisationId = organisation.CdpOrganisationGuid.ToString(),
                PersonId = personId.ToString(),
                Scopes = scopes.ToList()
            });
        });
}