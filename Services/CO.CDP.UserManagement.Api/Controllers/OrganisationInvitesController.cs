using CO.CDP.OrganisationInformation.Persistence;
using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    private readonly IUmOrganisationRepository _organisationRepository;
    private readonly IInviteRoleMappingRepository _inviteRoleMappingRepository;
    private readonly OrganisationInformationContext _organisationInformationContext;
    private readonly IInviteOrchestrationService _inviteOrchestrationService;
    private readonly ICurrentUserService _currentUserService;

    public OrganisationInvitesController(
        IUmOrganisationRepository organisationRepository,
        IInviteRoleMappingRepository inviteRoleMappingRepository,
        OrganisationInformationContext organisationInformationContext,
        IInviteOrchestrationService inviteOrchestrationService,
        ICurrentUserService currentUserService)
    {
        _organisationRepository = organisationRepository;
        _inviteRoleMappingRepository = inviteRoleMappingRepository;
        _organisationInformationContext = organisationInformationContext;
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

            // Get InviteRoleMappings from UserManagement DB
            var mappings = (await _inviteRoleMappingRepository.GetByOrganisationIdAsync(
                organisation.Id, cancellationToken)).ToList();

            if (!mappings.Any())
            {
                return Ok(Enumerable.Empty<PendingOrganisationInviteResponse>());
            }

            // Get CDP PersonInvites via OrganisationInformationContext
            var cdpInviteGuids = mappings.Select(m => m.CdpPersonInviteGuid).ToList();
            var cdpInvites = await _organisationInformationContext.PersonInvites
                .Where(pi => cdpInviteGuids.Contains(pi.Guid))
                .ToListAsync(cancellationToken);

            // Join and map to response
            var responses = from mapping in mappings
                            join cdpInvite in cdpInvites on mapping.CdpPersonInviteGuid equals cdpInvite.Guid
                            select new PendingOrganisationInviteResponse
                            {
                                PendingInviteId = mapping.Id,
                                OrganisationId = mapping.OrganisationId,
                                CdpPersonInviteGuid = cdpInvite.Guid,
                                Email = cdpInvite.Email,
                                FirstName = cdpInvite.FirstName,
                                LastName = cdpInvite.LastName,
                                OrganisationRole = mapping.OrganisationRole,
                                Status = UserStatus.Pending,
                                InvitedBy = mapping.CreatedBy,
                                ExpiresOn = cdpInvite.ExpiresOn,
                                CreatedAt = cdpInvite.CreatedOn
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

            // Get CDP invite details for response
            var cdpInvite = await _organisationInformationContext.PersonInvites
                .FirstOrDefaultAsync(pi => pi.Guid == mapping.CdpPersonInviteGuid, cancellationToken);

            var response = new PendingOrganisationInviteResponse
            {
                PendingInviteId = mapping.Id,
                OrganisationId = mapping.OrganisationId,
                CdpPersonInviteGuid = mapping.CdpPersonInviteGuid,
                Email = cdpInvite?.Email ?? request.Email,
                FirstName = cdpInvite?.FirstName ?? request.FirstName,
                LastName = cdpInvite?.LastName ?? request.LastName,
                OrganisationRole = mapping.OrganisationRole,
                Status = UserStatus.Pending,
                InvitedBy = mapping.CreatedBy,
                ExpiresOn = cdpInvite?.ExpiresOn,
                CreatedAt = cdpInvite?.CreatedOn ?? mapping.CreatedAt
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
}
