using CO.CDP.Organisation.WebApiClient;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using Microsoft.Extensions.Logging;
using InvalidOperationException = CO.CDP.UserManagement.Core.Exceptions.InvalidOperationException;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Service for orchestrating user invites with the CDP organisation service.
/// </summary>
public class InviteOrchestrationService : IInviteOrchestrationService
{
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IPendingOrganisationInviteRepository _pendingInviteRepository;
    private readonly IOrganisationClient _organisationClient;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<InviteOrchestrationService> _logger;

    public InviteOrchestrationService(
        IOrganisationRepository organisationRepository,
        IPendingOrganisationInviteRepository pendingInviteRepository,
        IOrganisationClient organisationClient,
        IUnitOfWork unitOfWork,
        ILogger<InviteOrchestrationService> logger)
    {
        _organisationRepository = organisationRepository;
        _pendingInviteRepository = pendingInviteRepository;
        _organisationClient = organisationClient;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PendingOrganisationInvite> InviteUserAsync(
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
            throw new DuplicateEntityException(nameof(PendingOrganisationInvite), nameof(PendingOrganisationInvite.Email), request.Email);
        }

        var inviteRequest = new InvitePersonToOrganisation(
            request.Email,
            request.FirstName,
            request.LastName,
            Array.Empty<string>());

        await _organisationClient.CreatePersonInviteAsync(cdpOrganisationId, inviteRequest, cancellationToken);

        var personInvite = await GetPersonInviteAsync(cdpOrganisationId, request.Email, cancellationToken);
        if (personInvite == null)
        {
            throw new InvalidOperationException($"Unable to locate person invite for email '{request.Email}'.");
        }

        var pendingInvite = new PendingOrganisationInvite
        {
            Email = request.Email,
            OrganisationId = organisation.Id,
            OrganisationRole = request.OrganisationRole,
            CdpPersonInviteGuid = personInvite.Id,
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

        await _organisationClient.RemovePersonInviteFromOrganisationAsync(
            cdpOrganisationId,
            pendingInvite.CdpPersonInviteGuid,
            cancellationToken);

        _pendingInviteRepository.Remove(pendingInvite);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task ResendInviteAsync(
        Guid cdpOrganisationId,
        int pendingInviteId,
        CancellationToken cancellationToken = default)
    {
        var pendingInvite = await GetPendingInviteAsync(cdpOrganisationId, pendingInviteId, cancellationToken);

        var currentInvite = await GetPersonInviteAsync(cdpOrganisationId, pendingInvite.Email, cancellationToken);
        if (currentInvite == null)
        {
            throw new InvalidOperationException($"Unable to locate person invite for email '{pendingInvite.Email}'.");
        }

        var inviteRequest = new InvitePersonToOrganisation(
            pendingInvite.Email,
            currentInvite.FirstName,
            currentInvite.LastName,
            Array.Empty<string>());

        await _organisationClient.CreatePersonInviteAsync(cdpOrganisationId, inviteRequest, cancellationToken);

        var personInvite = await GetPersonInviteAsync(cdpOrganisationId, pendingInvite.Email, cancellationToken);
        if (personInvite == null)
        {
            throw new InvalidOperationException($"Unable to locate person invite for email '{pendingInvite.Email}'.");
        }

        pendingInvite.CdpPersonInviteGuid = personInvite.Id;
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

    private async Task<Core.Entities.Organisation> GetOrganisationAsync(Guid cdpOrganisationId, CancellationToken cancellationToken)
    {
        var organisation = await _organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);
        }

        return organisation;
    }

    private async Task<PendingOrganisationInvite> GetPendingInviteAsync(
        Guid cdpOrganisationId,
        int pendingInviteId,
        CancellationToken cancellationToken)
    {
        var organisation = await GetOrganisationAsync(cdpOrganisationId, cancellationToken);
        var pendingInvite = await _pendingInviteRepository.GetByIdAsync(pendingInviteId, cancellationToken);
        if (pendingInvite == null || pendingInvite.OrganisationId != organisation.Id)
        {
            throw new EntityNotFoundException(nameof(PendingOrganisationInvite), pendingInviteId);
        }

        return pendingInvite;
    }

    private async Task<PersonInviteModel?> GetPersonInviteAsync(
        Guid cdpOrganisationId,
        string email,
        CancellationToken cancellationToken)
    {
        var invites = await _organisationClient.GetOrganisationPersonInvitesAsync(cdpOrganisationId, cancellationToken);
        return invites
            .FirstOrDefault(invite => invite.ExpiresOn == null &&
                                      string.Equals(invite.Email, email, StringComparison.OrdinalIgnoreCase));
    }
}
