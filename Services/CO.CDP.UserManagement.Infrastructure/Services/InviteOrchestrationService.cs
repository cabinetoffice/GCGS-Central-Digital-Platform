using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using Microsoft.Extensions.Logging;
using InvalidOperationException = CO.CDP.UserManagement.Core.Exceptions.InvalidOperationException;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Orchestrates invite lifecycle. Write operations that touch OI
/// delegate to <see cref="IAtomicMembershipSync"/> for atomic consistency.
/// </summary>
public class InviteOrchestrationService(
    IOrganisationRepository organisationRepository,
    IInviteRoleMappingRepository inviteRoleMappingRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IOrganisationApiAdapter organisationApiAdapter,
    IPersonApiAdapter personApiAdapter,
    IRoleMappingService roleMappingService,
    IAtomicMembershipSync atomicMembershipSync,
    IUnitOfWork unitOfWork,
    ILogger<InviteOrchestrationService> logger) : IInviteOrchestrationService
{
    public async Task<InviteRoleMapping> InviteUserAsync(
        Guid cdpOrganisationId,
        InviteUserRequest request,
        string? inviterPrincipalId,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var organisation = await GetOrganisationAsync(cdpOrganisationId, cancellationToken);
        await EnsureMemberDoesNotAlreadyExistAsync(organisation, request, cancellationToken);

        var cdpScopes = await roleMappingService.GetInviteScopesAsync(request.OrganisationRole, cancellationToken);

        var cdpInviteGuid = await CreatePersonInviteAsync(
            organisation,
            request.Email,
            request.FirstName,
            request.LastName,
            cdpScopes,
            cancellationToken);

        var inviteRoleMapping = new InviteRoleMapping
        {
            CdpPersonInviteGuid = cdpInviteGuid,
            OrganisationId = organisation.Id,
            CreatedBy = inviterPrincipalId ?? "system"
        };
        await roleMappingService.ApplyRoleDefinitionAsync(inviteRoleMapping, request.OrganisationRole,
            cancellationToken);

        if (request.ApplicationAssignments != null && request.ApplicationAssignments.Any())
        {
            foreach (var appAssignment in request.ApplicationAssignments)
            {
                foreach (var roleId in appAssignment.ApplicationRoleIds)
                {
                    inviteRoleMapping.ApplicationAssignments.Add(new InviteRoleApplicationAssignment
                    {
                        OrganisationApplicationId = appAssignment.OrganisationApplicationId,
                        ApplicationRoleId = roleId,
                        CreatedBy = inviterPrincipalId ?? "system"
                    });
                }
            }
        }

        inviteRoleMappingRepository.Add(inviteRoleMapping);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Created InviteRoleMapping {MappingId} for CDP invite {CdpInviteGuid}",
            inviteRoleMapping.Id, inviteRoleMapping.CdpPersonInviteGuid);

        return inviteRoleMapping;
    }

    public async Task RemoveInviteAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        CancellationToken cancellationToken = default)
    {
        var mapping = await GetInviteRoleMappingAsync(cdpOrganisationId, inviteRoleMappingId, cancellationToken);

        inviteRoleMappingRepository.Remove(mapping);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Removed InviteRoleMapping {MappingId} for organisation {OrganisationId}",
            inviteRoleMappingId, mapping.OrganisationId);
    }

    public async Task ChangeInviteRoleAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default)
    {
        var mapping = await GetInviteRoleMappingAsync(cdpOrganisationId, inviteRoleMappingId, cancellationToken);

        await roleMappingService.ApplyRoleDefinitionAsync(mapping, organisationRole, cancellationToken);
        inviteRoleMappingRepository.Update(mapping);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation(
            "Changed role to {Role} for InviteRoleMapping {MappingId}",
            organisationRole, inviteRoleMappingId);
    }

    public Task AcceptInviteAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        AcceptOrganisationInviteRequest request,
        CancellationToken cancellationToken = default) =>
        atomicMembershipSync.AcceptInviteAsync(cdpOrganisationId, inviteRoleMappingId, request, cancellationToken);

    public async Task ResendInviteAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        CancellationToken cancellationToken = default)
    {
        var organisation = await GetOrganisationAsync(cdpOrganisationId, cancellationToken);
        var mapping = await inviteRoleMappingRepository.GetByIdAsync(inviteRoleMappingId, cancellationToken);

        if (mapping == null || mapping.OrganisationId != organisation.Id)
            throw new EntityNotFoundException(nameof(InviteRoleMapping), inviteRoleMappingId);

        await organisationApiAdapter.ResendPersonInviteAsync(organisation.CdpOrganisationGuid,
            mapping.CdpPersonInviteGuid, cancellationToken);

        logger.LogInformation(
            "Resent invite for InviteRoleMapping {MappingId} (CDP invite {CdpInviteGuid}) in organisation {OrganisationId}",
            inviteRoleMappingId, mapping.CdpPersonInviteGuid, organisation.Id);
    }

    private async Task EnsureMemberDoesNotAlreadyExistAsync(
        Organisation organisation,
        InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email;
        var personDetails = string.IsNullOrWhiteSpace(email)
            ? null
            : await personApiAdapter.GetPersonDetailsByEmailAsync(email, cancellationToken);

        var memberExists = personDetails != null &&
                           await membershipRepository.ExistsByPersonIdAndOrganisationAsync(
                               personDetails.CdpPersonId,
                               organisation.Id,
                               cancellationToken);

        if (memberExists)
        {
            throw new DuplicateEntityException(
                nameof(UserOrganisationMembership),
                nameof(UserOrganisationMembership.CdpPersonId),
                personDetails!.CdpPersonId);
        }
    }

    private async Task<Organisation> GetOrganisationAsync(Guid cdpOrganisationId,
        CancellationToken cancellationToken) =>
        await organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken)
        ?? throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);

    private async Task<InviteRoleMapping> GetInviteRoleMappingAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        CancellationToken cancellationToken)
    {
        var organisation = await GetOrganisationAsync(cdpOrganisationId, cancellationToken);
        var mapping = await inviteRoleMappingRepository.GetByIdAsync(inviteRoleMappingId, cancellationToken);

        if (mapping == null || mapping.OrganisationId != organisation.Id)
        {
            throw new EntityNotFoundException(nameof(InviteRoleMapping), inviteRoleMappingId);
        }

        return mapping;
    }

    private async Task<Guid> CreatePersonInviteAsync(
        Organisation organisation,
        string email,
        string firstName,
        string lastName,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken)
    {
        try
        {
            var inviteId = await organisationApiAdapter.CreatePersonInviteAsync(
                organisation.CdpOrganisationGuid,
                email,
                firstName,
                lastName,
                scopes,
                cancellationToken);

            logger.LogInformation(
                "Created CDP invite {CdpInviteGuid} in organisation {OrganisationId}",
                inviteId, organisation.Id);

            return inviteId;
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "Failed to create CDP invite in organisation {OrganisationId}, aborting",
                organisation.Id);
            throw new InvalidOperationException(
                $"Failed to create CDP invite for organisation {organisation.Id} (CDP {organisation.CdpOrganisationGuid}).",
                ex);
        }
    }
}