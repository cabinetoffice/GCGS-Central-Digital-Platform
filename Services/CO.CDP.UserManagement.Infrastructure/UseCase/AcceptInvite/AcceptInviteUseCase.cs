using CO.CDP.Functional;
using CO.CDP.MQ;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.UseCase;
using CO.CDP.UserManagement.Infrastructure.Events;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Requests;
using Microsoft.Extensions.Logging;
using CorePartyRole = CO.CDP.UserManagement.Core.Constants.PartyRole;

namespace CO.CDP.UserManagement.Infrastructure.UseCase.AcceptInvite;

public record AcceptInviteCommand(
    Guid CdpOrganisationId,
    int InviteRoleMappingId,
    AcceptOrganisationInviteRequest Request);

public class AcceptInviteUseCase(
    IOrganisationRepository organisationRepository,
    IInviteRoleMappingRepository inviteRoleMappingRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IUserApplicationAssignmentRepository assignmentRepository,
    IOrganisationApplicationRepository organisationApplicationRepository,
    IRoleRepository roleRepository,
    IRoleMappingService roleMappingService,
    IOrganisationApiAdapter organisationApiAdapter,
    IUnitOfWork unitOfWork,
    IPublisher publisher,
    ILogger<AcceptInviteUseCase> logger) : IUseCase<AcceptInviteCommand>
{
    private const string SystemAssignedBy = "system:default-app-assignment";

    public async Task Execute(AcceptInviteCommand command, CancellationToken ct = default)
    {
        var organisation = await organisationRepository.GetByCdpGuidAsync(command.CdpOrganisationId, ct)
                           ?? throw new EntityNotFoundException(nameof(Organisation), command.CdpOrganisationId);

        var mapping = await inviteRoleMappingRepository.GetByIdAsync(command.InviteRoleMappingId, ct);
        if (mapping?.OrganisationId != organisation.Id)
            throw new EntityNotFoundException(nameof(InviteRoleMapping), command.InviteRoleMappingId);

        if (await membershipRepository.GetByUserAndOrganisationAsync(
                command.Request.UserPrincipalId, mapping.OrganisationId, ct) != null)
            throw new DuplicateEntityException(
                nameof(UserOrganisationMembership),
                nameof(UserOrganisationMembership.UserPrincipalId),
                command.Request.UserPrincipalId);

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            var membership = new UserOrganisationMembership
            {
                UserPrincipalId = command.Request.UserPrincipalId,
                CdpPersonId = command.Request.CdpPersonId,
                OrganisationId = mapping.OrganisationId,
                IsActive = true,
                JoinedAt = DateTimeOffset.UtcNow,
                InvitedBy = mapping.CreatedBy
            };
            await roleMappingService.ApplyRoleDefinitionAsync(membership, mapping.OrganisationRole, ct);

            membershipRepository.Add(membership);
            inviteRoleMappingRepository.Remove(mapping);

            // Flush membership to DB so its generated Id is available for default app assignments.
            await unitOfWork.SaveChangesAsync(ct);

            await AssignDefaultApplicationsAsync(membership, organisation, ct);
            await unitOfWork.SaveChangesAsync(ct);

            await Option.From(membership.CdpPersonId).TapAsync(async personId =>
            {
                var scopes = await roleMappingService.GetOrganisationInformationScopesAsync(membership.Id, ct);
                await publisher.Publish(new PersonScopesUpdated
                {
                    OrganisationId = organisation.CdpOrganisationGuid.ToString(),
                    PersonId = personId.ToString(),
                    Scopes = scopes.ToList()
                });
            });

            logger.LogInformation(
                "Accepted invite {MappingId}, created membership {MembershipId} in organisation {CdpOrganisationId}",
                command.InviteRoleMappingId, membership.Id, command.CdpOrganisationId);
        }, ct);
    }

    private async Task AssignDefaultApplicationsAsync(
        UserOrganisationMembership membership, Organisation organisation, CancellationToken ct)
    {
        if (!await roleMappingService.ShouldAutoAssignDefaultApplicationsAsync(membership, ct))
            return;

        var defaultOrgApps = (await organisationApplicationRepository
            .GetDefaultEnabledByOrganisationIdAsync(membership.OrganisationId, ct)).ToList();

        if (defaultOrgApps.Count == 0)
            return;

        var partyRoles = await organisationApiAdapter.GetPartyRolesAsync(organisation.CdpOrganisationGuid, ct);

        foreach (var orgApp in defaultOrgApps)
            await EnsureDefaultAssignedAsync(membership, partyRoles, orgApp, ct);
    }

    private async Task EnsureDefaultAssignedAsync(
        UserOrganisationMembership membership,
        ISet<CorePartyRole> partyRoles,
        OrganisationApplication orgApp,
        CancellationToken ct)
    {
        var existingAssignment = await assignmentRepository
            .GetByMembershipAndApplicationAsync(membership.Id, orgApp.Id, ct);

        var defaultRoles = DefaultApplicationRoleSelector.SelectFor(
            orgApp,
            await roleRepository.GetByApplicationIdAsync(orgApp.ApplicationId, ct),
            partyRoles,
            await roleMappingService.GetInviteScopesAsync(membership.OrganisationRole, ct));

        if (existingAssignment is null)
            AddDefaultAssignment(membership.Id, orgApp.Id, defaultRoles);
        else if (!AssignmentMatchesDesired(existingAssignment, defaultRoles))
            UpdateDefaultAssignment(existingAssignment, defaultRoles);
    }

    private void AddDefaultAssignment(int membershipId, int orgAppId, IReadOnlyList<ApplicationRole> roles) =>
        assignmentRepository.Add(new UserApplicationAssignment
        {
            UserOrganisationMembershipId = membershipId,
            OrganisationApplicationId = orgAppId,
            IsActive = true,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedBy = SystemAssignedBy,
            CreatedBy = SystemAssignedBy,
            Roles = roles.ToList()
        });

    private void UpdateDefaultAssignment(UserApplicationAssignment existing, IReadOnlyList<ApplicationRole> roles)
    {
        existing.IsActive = true;
        existing.IsDeleted = false;
        existing.RevokedAt = null;
        existing.RevokedBy = null;
        existing.DeletedAt = null;
        existing.DeletedBy = null;
        existing.ModifiedBy = SystemAssignedBy;
        existing.Roles.Clear();
        foreach (var role in roles)
            existing.Roles.Add(role);
        assignmentRepository.Update(existing);
    }

    private static bool AssignmentMatchesDesired(
        UserApplicationAssignment assignment, IReadOnlyList<ApplicationRole> desiredRoles) =>
        assignment.IsActive &&
        !assignment.IsDeleted &&
        assignment.Roles.Select(r => r.Id).OrderBy(id => id)
            .SequenceEqual(desiredRoles.Select(r => r.Id).OrderBy(id => id));
}