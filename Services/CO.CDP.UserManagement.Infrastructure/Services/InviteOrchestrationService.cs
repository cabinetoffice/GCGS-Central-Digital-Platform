using CO.CDP.Organisation.WebApiClient;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Container for invite orchestration repositories.
/// </summary>
public sealed class InviteOrchestrationServiceRepositories
{
    public InviteOrchestrationServiceRepositories(
        IOrganisationRepository organisationRepository,
        IInviteRoleMappingRepository inviteRoleMappingRepository,
        IUserOrganisationMembershipRepository membershipRepository)
    {
        OrganisationRepository = organisationRepository;
        InviteRoleMappingRepository = inviteRoleMappingRepository;
        MembershipRepository = membershipRepository;
    }

    public IOrganisationRepository OrganisationRepository { get; }
    public IInviteRoleMappingRepository InviteRoleMappingRepository { get; }
    public IUserOrganisationMembershipRepository MembershipRepository { get; }
}

/// <summary>
/// Service for orchestrating user invites via CDP bridge.
/// </summary>
public class InviteOrchestrationService : IInviteOrchestrationService
{
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IInviteRoleMappingRepository _inviteRoleMappingRepository;
    private readonly IUserOrganisationMembershipRepository _membershipRepository;
    private readonly IOrganisationClient _organisationClient;
    private readonly IPersonLookupService _personLookupService;
    private readonly ICdpMembershipSyncService _membershipSyncService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InviteOrchestrationService> _logger;

    public InviteOrchestrationService(
        InviteOrchestrationServiceRepositories repositories,
        IOrganisationClient organisationClient,
        IPersonLookupService personLookupService,
        ICdpMembershipSyncService membershipSyncService,
        IUnitOfWork unitOfWork,
        ILogger<InviteOrchestrationService> logger)
    {
        _organisationRepository = repositories.OrganisationRepository;
        _inviteRoleMappingRepository = repositories.InviteRoleMappingRepository;
        _membershipRepository = repositories.MembershipRepository;
        _organisationClient = organisationClient;
        _personLookupService = personLookupService;
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

        var cdpScopes = MapOrganisationRoleToCdpScopes(request.OrganisationRole);

        var cdpInviteRequest = new InvitePersonToOrganisation(
            email: request.Email,
            firstName: request.FirstName,
            lastName: request.LastName,
            scopes: cdpScopes
        );

        var personInvite = await CreatePersonInviteAsync(
            organisation,
            cdpInviteRequest,
            cancellationToken);

        var inviteRoleMapping = new InviteRoleMapping
        {
            CdpPersonInviteGuid = personInvite.Id,
            OrganisationId = organisation.Id,
            OrganisationRole = request.OrganisationRole,
            CreatedBy = inviterPrincipalId ?? "system"
        };

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

        mapping.OrganisationRole = organisationRole;
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
            OrganisationRole = mapping.OrganisationRole,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            InvitedBy = mapping.CreatedBy
        };

        _membershipRepository.Add(membership);
        _inviteRoleMappingRepository.Remove(mapping);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _membershipSyncService.SyncMembershipCreatedAsync(membership, cancellationToken);

        _logger.LogInformation(
            "Accepted invite {MappingId}, created membership {MembershipId}",
            inviteRoleMappingId, membership.Id);
    }

    private static List<string> MapOrganisationRoleToCdpScopes(OrganisationRole organisationRole)
    {
        return organisationRole switch
        {
            OrganisationRole.Owner => new List<string> { "ADMIN" },
            OrganisationRole.Admin => new List<string> { "ADMIN" },
            _ => new List<string> { "VIEWER" }
        };
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

    private async Task<PersonInviteModel> CreatePersonInviteAsync(
        Core.Entities.Organisation organisation,
        InvitePersonToOrganisation cdpInviteRequest,
        CancellationToken cancellationToken)
    {
        try
        {
            var personInvite = await _organisationClient.CreatePersonInviteForServiceAsync(
                organisation.CdpOrganisationGuid,
                cdpInviteRequest,
                cancellationToken);

            _logger.LogInformation(
                "Created CDP invite {CdpInviteGuid} in organisation {OrganisationId}",
                personInvite.Id, organisation.Id);

            return personInvite;
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
