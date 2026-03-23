using CO.CDP.UserManagement.Api.Models;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Responses;
using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.Api.Controllers;

/// <summary>
/// Controller for managing organisation users.
/// </summary>
[ApiController]
[Route("api/organisations/{cdpOrganisationId:guid}/users")]
[Authorize(Policy = PolicyNames.OrganisationMember)]
public class OrganisationUsersController : ControllerBase
{
    private readonly IOrganisationUserService _organisationUserService;
    private readonly IPersonApiAdapter _personApiAdapter;
    private readonly IOrganisationApiAdapter _organisationApiAdapter;
    private readonly ILogger<OrganisationUsersController> _logger;

    public OrganisationUsersController(
        IOrganisationUserService organisationUserService,
        IPersonApiAdapter personLookupService,
        IOrganisationApiAdapter organisationApiAdapter,
        ILogger<OrganisationUsersController> logger)
    {
        _organisationUserService = organisationUserService;
        _personApiAdapter = personLookupService;
        _organisationApiAdapter = organisationApiAdapter;
        _logger = logger;
    }

    /// <summary>
    /// Gets all users in an organisation.
    /// </summary>
    /// <param name="cdpOrganisationId">The CDP organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organisation users.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrganisationUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrganisationUserResponse>>> GetUsers(
        Guid cdpOrganisationId,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Getting organisation users for CDP organisation {CdpOrganisationId}", cdpOrganisationId);
            var memberships = (await _organisationUserService.GetOrganisationUsersAsync(cdpOrganisationId, cancellationToken)).ToArray();
            _logger.LogInformation("Retrieved {MembershipCount} memberships for CDP organisation {CdpOrganisationId}", memberships.Length, cdpOrganisationId);

            var oiPersons = await _organisationApiAdapter.GetOrganisationPersonsAsync(cdpOrganisationId, cancellationToken);
            var oiPersonById = oiPersons.ToDictionary(p => p.Id);

            var responses = memberships
                .Select(membership => membership.ToResponse(
                    includeAssignments: true,
                    personDetails: membership.CdpPersonId.HasValue &&
                                   oiPersonById.TryGetValue(membership.CdpPersonId.Value, out var oiPerson)
                        ? new PersonDetails
                        {
                            CdpPersonId = oiPerson.Id,
                            FirstName = oiPerson.FirstName,
                            LastName = oiPerson.LastName,
                            Email = oiPerson.Email
                        }
                        : null))
                .ToArray();

            LogScopeMismatches(cdpOrganisationId, memberships, oiPersonById);

            return Ok(responses);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets all users in an organisation for service-to-service consumers.
    /// </summary>
    /// <param name="cdpOrganisationId">The CDP organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organisation users without person enrichment.</returns>
    [HttpGet("service")]
    [Authorize(Policy = PolicyNames.ServiceAccount)]
    [ProducesResponseType(typeof(IEnumerable<OrganisationUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrganisationUserResponse>>> GetUsersForService(
        Guid cdpOrganisationId,
        CancellationToken cancellationToken)
    {
        try
        {
            var memberships = await _organisationUserService.GetOrganisationUsersAsync(cdpOrganisationId, cancellationToken);
            var responses = memberships.Select(membership =>
                membership.ToResponse(includeAssignments: true, personDetails: null));
            return Ok(responses);
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific user in an organisation by CDP person ID.
    /// </summary>
    /// <param name="cdpOrganisationId">The CDP organisation identifier.</param>
    /// <param name="cdpPersonId">The CDP person identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation user if found.</returns>
    [HttpGet("{cdpPersonId:guid}")]
    [ProducesResponseType(typeof(OrganisationUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationUserResponse>> GetUserByPersonId(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        CancellationToken cancellationToken)
    {
        try
        {
            var membership = await _organisationUserService.GetOrganisationUserByPersonIdAsync(cdpOrganisationId, cdpPersonId, cancellationToken);
            if (membership == null)
            {
                return NotFound(new ErrorResponse { Message = $"User with CDP Person ID {cdpPersonId} not found in organisation {cdpOrganisationId}." });
            }

            var personDetails = await GetPersonDetailsAsync(membership.UserPrincipalId, cancellationToken);
            return Ok(membership.ToResponse(includeAssignments: true, personDetails: personDetails));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Updates the organisation role for a user by CDP person ID.
    /// </summary>
    [HttpPut("{cdpPersonId:guid}/role")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdateRole(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        [FromBody] ChangeOrganisationRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _organisationUserService.UpdateOrganisationRoleAsync(
                cdpOrganisationId,
                cdpPersonId,
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
    /// Removes a user from an organisation (soft-delete). Idempotent — returns 204 if already removed.
    /// </summary>
    [HttpDelete("{cdpPersonId:guid}")]
    [Authorize(Policy = PolicyNames.OrganisationAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult> RemoveUser(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _organisationUserService.RemoveUserFromOrganisationAsync(
                cdpOrganisationId, cdpPersonId, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (LastOwnerRemovalException ex)
        {
            return Conflict(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets a specific user in an organisation.
    /// </summary>
    /// <param name="cdpOrganisationId">The CDP organisation identifier.</param>
    /// <param name="userId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation user if found.</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(OrganisationUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationUserResponse>> GetUser(
        Guid cdpOrganisationId,
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var membership = await _organisationUserService.GetOrganisationUserAsync(cdpOrganisationId, userId, cancellationToken);
            if (membership == null)
            {
                return NotFound(new ErrorResponse { Message = $"User {userId} not found in organisation {cdpOrganisationId}." });
            }

            var personDetails = await GetPersonDetailsAsync(membership.UserPrincipalId, cancellationToken);
            return Ok(membership.ToResponse(includeAssignments: true, personDetails: personDetails));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    private void LogScopeMismatches(
        Guid cdpOrganisationId,
        IEnumerable<UserOrganisationMembership> memberships,
        IDictionary<Guid, OiOrganisationPerson> oiPersonById)
    {
        foreach (var membership in memberships.Where(m => m.CdpPersonId.HasValue))
        {
            if (!oiPersonById.TryGetValue(membership.CdpPersonId!.Value, out var oiPerson))
            {
                _logger.LogWarning(
                    "Organisation {CdpOrganisationId}: UM member {CdpPersonId} (role={UmRole}) has no matching OI person record",
                    cdpOrganisationId, membership.CdpPersonId, membership.OrganisationRole);
                continue;
            }

            var hasAdminScope = oiPerson.Scopes.Contains("ADMIN", StringComparer.OrdinalIgnoreCase);
            var isUmAdmin = membership.OrganisationRole >= OrganisationRole.Admin;

            if (hasAdminScope != isUmAdmin)
            {
                _logger.LogWarning(
                    "Organisation {CdpOrganisationId}: scope/role mismatch for person {Email} — OI scopes [{OiScopes}] vs UM role {UmRole}",
                    cdpOrganisationId, oiPerson.Email, string.Join(", ", oiPerson.Scopes), membership.OrganisationRole);
            }
        }
    }

    private async Task<PersonDetails?> GetPersonDetailsAsync(
        string userPrincipalId,
        CancellationToken cancellationToken)
    {
        return await _personApiAdapter.GetPersonDetailsAsync(userPrincipalId, cancellationToken);
    }
}
