using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Service for orchestrating user invites via CDP bridge.
/// </summary>
public class InviteOrchestrationService : IInviteOrchestrationService
{
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IInviteRoleMappingRepository _inviteRoleMappingRepository;
    private readonly IUserOrganisationMembershipRepository _membershipRepository;
    private readonly IOrganisationApiAdapter _organisationApiAdapter;
    private readonly IPersonLookupService _personLookupService;
    private readonly IUserAssignmentService _userAssignmentService;
    private readonly IRoleMappingService _roleMappingService;
    private readonly ICdpMembershipSyncService _membershipSyncService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InviteOrchestrationService> _logger;

    public InviteOrchestrationService(
        IOrganisationRepository organisationRepository,
        IInviteRoleMappingRepository inviteRoleMappingRepository,
        IUserOrganisationMembershipRepository membershipRepository,
        IOrganisationApiAdapter organisationApiAdapter,
        IPersonLookupService personLookupService,
        IUserAssignmentService userAssignmentService,
        IRoleMappingService roleMappingService,
        ICdpMembershipSyncService membershipSyncService,
        IUnitOfWork unitOfWork,
        ILogger<InviteOrchestrationService> logger)
    {
        _organisationRepository = organisationRepository;
        _inviteRoleMappingRepository = inviteRoleMappingRepository;
        _membershipRepository = membershipRepository;
        _organisationApiAdapter = organisationApiAdapter;
        _personLookupService = personLookupService;
        _userAssignmentService = userAssignmentService;
        _roleMappingService = roleMappingService;
        _membershipSyncService = membershipSyncService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<InviteRoleMapping> InviteUserAsync(
        Guid cdpOrganisationId,
        InviteUserRequest request,
        string? inviterPrincipalId,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var organisation = await GetOrganisationAsync(cdpOrganisationId, cancellationToken);
        await EnsureMemberDoesNotAlreadyExistAsync(organisation, request, cancellationToken);

        var cdpScopes = await _roleMappingService.GetInviteScopesAsync(request.OrganisationRole, cancellationToken);

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
        await _roleMappingService.ApplyRoleDefinitionAsync(inviteRoleMapping, request.OrganisationRole, cancellationToken);

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

        _inviteRoleMappingRepository.Add(inviteRoleMapping);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created InviteRoleMapping {MappingId} for CDP invite {CdpInviteGuid}",
            inviteRoleMapping.Id, inviteRoleMapping.CdpPersonInviteGuid);

        return inviteRoleMapping;
    }

    private async Task EnsureMemberDoesNotAlreadyExistAsync(
        Core.Entities.Organisation organisation,
        InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email;
        var personDetails = string.IsNullOrWhiteSpace(email)
            ? null
            : await _personLookupService.GetPersonDetailsByEmailAsync(email, cancellationToken);

        var memberExists = personDetails != null &&
                           await _membershipRepository.ExistsByPersonIdAndOrganisationAsync(
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

    public async Task RemoveInviteAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        CancellationToken cancellationToken = default)
    {
        var mapping = await GetInviteRoleMappingAsync(cdpOrganisationId, inviteRoleMappingId, cancellationToken);

        _inviteRoleMappingRepository.Remove(mapping);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
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

        await _roleMappingService.ApplyRoleDefinitionAsync(mapping, organisationRole, cancellationToken);
        _inviteRoleMappingRepository.Update(mapping);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Changed role to {Role} for InviteRoleMapping {MappingId}",
            organisationRole, inviteRoleMappingId);
    }

    public async Task AcceptInviteAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        AcceptOrganisationInviteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var mapping = await GetInviteRoleMappingAsync(cdpOrganisationId, inviteRoleMappingId, cancellationToken);

        var existingMembership = await _membershipRepository.GetByUserAndOrganisationAsync(
            request.UserPrincipalId,
            mapping.OrganisationId,
            cancellationToken);
        if (existingMembership != null)
        {
            throw new DuplicateEntityException(
                nameof(UserOrganisationMembership),
                nameof(UserOrganisationMembership.UserPrincipalId),
                request.UserPrincipalId);
        }

        var membership = new UserOrganisationMembership
        {
            UserPrincipalId = request.UserPrincipalId,
            CdpPersonId = request.CdpPersonId,
            OrganisationId = mapping.OrganisationId,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            InvitedBy = mapping.CreatedBy
        };
        await _roleMappingService.ApplyRoleDefinitionAsync(membership, mapping.OrganisationRole, cancellationToken);

        _membershipRepository.Add(membership);
        _inviteRoleMappingRepository.Remove(mapping);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _userAssignmentService.AssignDefaultApplicationsAsync(membership, cancellationToken);
        await _membershipSyncService.SyncMembershipAccessChangedAsync(membership.Id, cancellationToken);

        _logger.LogInformation(
            "Accepted invite {MappingId}, created membership {MembershipId}",
            inviteRoleMappingId, membership.Id);
    }

    private async Task<CO.CDP.UserManagement.Core.Entities.Organisation> GetOrganisationAsync(Guid cdpOrganisationId,
        CancellationToken cancellationToken)
    {
        var organisation = await _organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(CO.CDP.UserManagement.Core.Entities.Organisation), cdpOrganisationId);
        }

        return organisation;
    }

    private async Task<InviteRoleMapping> GetInviteRoleMappingAsync(
        Guid cdpOrganisationId,
        int inviteRoleMappingId,
        CancellationToken cancellationToken)
    {
        var organisation = await GetOrganisationAsync(cdpOrganisationId, cancellationToken);
        var mapping = await _inviteRoleMappingRepository.GetByIdAsync(inviteRoleMappingId, cancellationToken);

        if (mapping == null || mapping.OrganisationId != organisation.Id)
        {
            throw new EntityNotFoundException(nameof(InviteRoleMapping), inviteRoleMappingId);
        }

        return mapping;
    }

    private async Task<Guid> CreatePersonInviteAsync(
        Core.Entities.Organisation organisation,
        string email,
        string firstName,
        string lastName,
        IReadOnlyList<string> scopes,
        CancellationToken cancellationToken)
    {
        try
        {
            var inviteId = await _organisationApiAdapter.CreatePersonInviteAsync(
                organisation.CdpOrganisationGuid,
                email,
                firstName,
                lastName,
                scopes,
                cancellationToken);

            _logger.LogInformation(
                "Created CDP invite {CdpInviteGuid} in organisation {OrganisationId}",
                inviteId, organisation.Id);

            return inviteId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to create CDP invite in organisation {OrganisationId}, aborting",
                organisation.Id);
            throw new Core.Exceptions.InvalidOperationException(
                $"Failed to create CDP invite for organisation {organisation.Id} (CDP {organisation.CdpOrganisationGuid}).",
                ex);
        }
    }
}
