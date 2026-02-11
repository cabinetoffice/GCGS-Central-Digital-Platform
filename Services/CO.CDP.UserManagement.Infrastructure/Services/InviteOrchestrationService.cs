using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using Microsoft.Extensions.Logging;
using CoreEntities = CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Service for orchestrating user invites.
/// </summary>
public class InviteOrchestrationService : IInviteOrchestrationService
{
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IPendingOrganisationInviteRepository _pendingInviteRepository;
    private readonly IUserOrganisationMembershipRepository _membershipRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InviteOrchestrationService> _logger;

    public InviteOrchestrationService(
        IOrganisationRepository organisationRepository,
        IPendingOrganisationInviteRepository pendingInviteRepository,
        IUserOrganisationMembershipRepository membershipRepository,
        IUnitOfWork unitOfWork,
        ILogger<InviteOrchestrationService> logger)
    {
        _organisationRepository = organisationRepository;
        _pendingInviteRepository = pendingInviteRepository;
        _membershipRepository = membershipRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CoreEntities.PendingOrganisationInvite> InviteUserAsync(
        Guid cdpOrganisationId,
        InviteUserRequest request,
        string? inviterPrincipalId,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var organisation = await GetOrganisationAsync(cdpOrganisationId, cancellationToken);

        var existingInvite = await _pendingInviteRepository.GetByEmailAndOrganisationAsync(
            request.Email, organisation.Id, cancellationToken);
        if (existingInvite != null)
        {
            throw new DuplicateEntityException(nameof(CoreEntities.PendingOrganisationInvite),
                nameof(CoreEntities.PendingOrganisationInvite.Email), request.Email);
        }

        var pendingInvite = new CoreEntities.PendingOrganisationInvite
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            OrganisationId = organisation.Id,
            OrganisationRole = request.OrganisationRole,
            CdpPersonInviteGuid = Guid.NewGuid(),
            InvitedBy = inviterPrincipalId
        };

        _pendingInviteRepository.Add(pendingInvite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created pending invite {InviteId} for organisation {OrganisationId} and email {Email}",
            pendingInvite.Id,
            organisation.Id,
            request.Email);

        return pendingInvite;
    }

    public async Task RemoveInviteAsync(
        Guid cdpOrganisationId,
        int pendingInviteId,
        CancellationToken cancellationToken = default)
    {
        var pendingInvite = await GetPendingInviteAsync(cdpOrganisationId, pendingInviteId, cancellationToken);

        _pendingInviteRepository.Remove(pendingInvite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ResendInviteAsync(
        Guid cdpOrganisationId,
        int pendingInviteId,
        CancellationToken cancellationToken = default)
    {
        var pendingInvite = await GetPendingInviteAsync(cdpOrganisationId, pendingInviteId, cancellationToken);
        pendingInvite.CdpPersonInviteGuid = Guid.NewGuid();
        _pendingInviteRepository.Update(pendingInvite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ChangeInviteRoleAsync(
        Guid cdpOrganisationId,
        int pendingInviteId,
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default)
    {
        var pendingInvite = await GetPendingInviteAsync(cdpOrganisationId, pendingInviteId, cancellationToken);
        pendingInvite.OrganisationRole = organisationRole;
        _pendingInviteRepository.Update(pendingInvite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task AcceptInviteAsync(
        Guid cdpOrganisationId,
        int pendingInviteId,
        AcceptOrganisationInviteRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));

        var pendingInvite = await GetPendingInviteAsync(cdpOrganisationId, pendingInviteId, cancellationToken);

        var membership = new CoreEntities.UserOrganisationMembership
        {
            UserPrincipalId = request.UserPrincipalId,
            CdpPersonId = request.CdpPersonId,
            OrganisationId = pendingInvite.OrganisationId,
            OrganisationRole = pendingInvite.OrganisationRole,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            InvitedBy = pendingInvite.InvitedBy
        };

        _membershipRepository.Add(membership);
        _pendingInviteRepository.Remove(pendingInvite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<CoreEntities.Organisation> GetOrganisationAsync(Guid cdpOrganisationId,
        CancellationToken cancellationToken)
    {
        var organisation = await _organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(CoreEntities.Organisation), cdpOrganisationId);
        }

        return organisation;
    }

    private async Task<CoreEntities.PendingOrganisationInvite> GetPendingInviteAsync(
        Guid cdpOrganisationId,
        int pendingInviteId,
        CancellationToken cancellationToken)
    {
        var organisation = await GetOrganisationAsync(cdpOrganisationId, cancellationToken);
        var pendingInvite = await _pendingInviteRepository.GetByIdAsync(pendingInviteId, cancellationToken);
        if (pendingInvite == null || pendingInvite.OrganisationId != organisation.Id)
        {
            throw new EntityNotFoundException(nameof(CoreEntities.PendingOrganisationInvite), pendingInviteId);
        }

        return pendingInvite;
    }
}