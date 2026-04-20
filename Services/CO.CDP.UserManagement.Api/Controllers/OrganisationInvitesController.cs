using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoreEntities = CO.CDP.UserManagement.Core.Entities;
using IUmOrganisationRepository = CO.CDP.UserManagement.Core.Interfaces.IOrganisationRepository;

namespace CO.CDP.UserManagement.Api.Controllers;

/// <summary>
/// Controller for managing organisation user invites.
/// </summary>
[ApiController]
[Route("api/organisations/{cdpOrganisationId:guid}/invites")]
[Authorize(Policy = PolicyNames.OrganisationAdmin)]
public class OrganisationInvitesController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IInviteOrchestrationService _inviteOrchestrationService;
    private readonly IInviteRoleMappingRepository _inviteRoleMappingRepository;
    private readonly IOrganisationApiAdapter _organisationApiAdapter;
    private readonly IUmOrganisationRepository _organisationRepository;

    public OrganisationInvitesController(
        IUmOrganisationRepository organisationRepository,
        IInviteRoleMappingRepository inviteRoleMappingRepository,
        IOrganisationApiAdapter organisationApiAdapter,
        IInviteOrchestrationService inviteOrchestrationService,
        ICurrentUserService currentUserService)
    {
        _organisationRepository = organisationRepository;
        _inviteRoleMappingRepository = inviteRoleMappingRepository;
        _organisationApiAdapter = organisationApiAdapter;
        _inviteOrchestrationService = inviteOrchestrationService;
        _currentUserService = currentUserService;
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

            var mappings = (await _inviteRoleMappingRepository.GetByOrganisationIdAsync(
                organisation.Id, cancellationToken)).ToList();

            if (!mappings.Any())
            {
                return Ok(Enumerable.Empty<PendingOrganisationInviteResponse>());
            }

            var oiInvites = await _organisationApiAdapter.GetOrganisationPersonInvitesAsync(
                cdpOrganisationId, cancellationToken);

            var oiInviteById = oiInvites.ToDictionary(i => i.Id);

            var responses = from mapping in mappings
                where oiInviteById.ContainsKey(mapping.CdpPersonInviteGuid)
                let oiInvite = oiInviteById[mapping.CdpPersonInviteGuid]
                select new PendingOrganisationInviteResponse
                {
                    PendingInviteId = mapping.Id,
                    OrganisationId = mapping.OrganisationId,
                    CdpPersonInviteGuid = oiInvite.Id,
                    Email = oiInvite.Email,
                    FirstName = oiInvite.FirstName,
                    LastName = oiInvite.LastName,
                    OrganisationRole = mapping.OrganisationRole,
                    Status = UserStatus.Pending,
                    InvitedBy = mapping.CreatedBy,
                    ExpiresOn = oiInvite.ExpiresOn,
                    CreatedAt = oiInvite.CreatedOn ?? mapping.CreatedAt,
                    ApplicationAssignments = mapping.ApplicationAssignments.Select(a =>
                        new InviteApplicationAssignmentResponse
                        {
                            OrganisationApplicationId = a.OrganisationApplicationId,
                            ApplicationId = a.OrganisationApplication?.ApplicationId,
                            ApplicationName = a.OrganisationApplication?.Application?.Name ?? string.Empty,
                            ApplicationRoleId = a.ApplicationRoleId
                        })
                };

            return Ok(responses.ToList());
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
            var mapping = await _inviteOrchestrationService.InviteUserAsync(
                cdpOrganisationId,
                request,
                inviter,
                cancellationToken);

            var response = new PendingOrganisationInviteResponse
            {
                PendingInviteId = mapping.Id,
                OrganisationId = mapping.OrganisationId,
                CdpPersonInviteGuid = mapping.CdpPersonInviteGuid,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                OrganisationRole = mapping.OrganisationRole,
                Status = UserStatus.Pending,
                InvitedBy = mapping.CreatedBy,
                CreatedAt = mapping.CreatedAt
            };

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
        catch (PersonLookupException ex)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Changes the organisation role for a pending invite.
    /// </summary>
    [HttpPut("{inviteId:int}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ChangeInviteRole(
        Guid cdpOrganisationId,
        int inviteId,
        [FromBody] ChangeOrganisationRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _inviteOrchestrationService.ChangeInviteRoleAsync(
                cdpOrganisationId,
                inviteId,
                request.OrganisationRole,
                cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Removes a pending invite.
    /// </summary>
    [HttpDelete("{inviteId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveInvite(
        Guid cdpOrganisationId,
        int inviteId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _inviteOrchestrationService.RemoveInviteAsync(cdpOrganisationId, inviteId, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Resends a pending invite, extending its lifespan and re-sending the notification email.
    /// </summary>
    [HttpPost("{inviteId:int}/resend")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ResendInvite(
        Guid cdpOrganisationId,
        int inviteId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _inviteOrchestrationService.ResendInviteAsync(cdpOrganisationId, inviteId, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }
}