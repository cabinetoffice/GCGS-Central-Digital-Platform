using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Api.Models;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreEntities = CO.CDP.UserManagement.Core.Entities;

namespace CO.CDP.UserManagement.Api.Controllers;

/// <summary>
/// Controller for managing organisation user invites.
/// </summary>
[ApiController]
[Route("api/organisations/{cdpOrganisationId:guid}/invites")]
[Authorize(Policy = PolicyNames.OrganisationAdmin)]
public class OrganisationInvitesController : ControllerBase
{
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IPendingOrganisationInviteRepository _pendingInviteRepository;
    private readonly IInviteOrchestrationService _inviteOrchestrationService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<OrganisationInvitesController> _logger;

    public OrganisationInvitesController(
        IOrganisationRepository organisationRepository,
        IPendingOrganisationInviteRepository pendingInviteRepository,
        IInviteOrchestrationService inviteOrchestrationService,
        ICurrentUserService currentUserService,
        ILogger<OrganisationInvitesController> logger)
    {
        _organisationRepository = organisationRepository;
        _pendingInviteRepository = pendingInviteRepository;
        _inviteOrchestrationService = inviteOrchestrationService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Gets pending invites for an organisation.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PendingOrganisationInviteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PendingOrganisationInviteResponse>>> GetInvites(
        Guid cdpOrganisationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var organisation = await _organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken);
            if (organisation == null)
            {
                throw new EntityNotFoundException(nameof(CoreEntities.Organisation), cdpOrganisationId);
            }

            var pendingInvites = (await _pendingInviteRepository.GetByOrganisationIdAsync(
                organisation.Id, cancellationToken)).ToList();
            var responses = pendingInvites
                .Select(invite => invite.ToResponse())
                .ToList();

            return Ok(responses);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new invite for an organisation.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PendingOrganisationInviteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PendingOrganisationInviteResponse>> InviteUser(
        Guid cdpOrganisationId,
        [FromBody] InviteUserRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var inviter = _currentUserService.GetUserPrincipalId();
            var pendingInvite = await _inviteOrchestrationService.InviteUserAsync(
                cdpOrganisationId,
                request,
                inviter,
                cancellationToken);

            var response = pendingInvite.ToResponse();

            return CreatedAtAction(nameof(GetInvites), new { cdpOrganisationId }, response);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (DuplicateEntityException ex)
        {
            return Conflict(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Resends a pending invite.
    /// </summary>
    [HttpPost("{pendingInviteId:int}/resend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ResendInvite(
        Guid cdpOrganisationId,
        int pendingInviteId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _inviteOrchestrationService.ResendInviteAsync(cdpOrganisationId, pendingInviteId, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Changes the organisation role for a pending invite.
    /// </summary>
    [HttpPut("{pendingInviteId:int}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ChangeInviteRole(
        Guid cdpOrganisationId,
        int pendingInviteId,
        [FromBody] ChangeOrganisationRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _inviteOrchestrationService.ChangeInviteRoleAsync(
                cdpOrganisationId,
                pendingInviteId,
                request.OrganisationRole,
                cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }
}
