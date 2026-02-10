using CO.CDP.UserManagement.Api.Models;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;
using CO.CDP.Person.WebApiClient;
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
    private readonly IPersonClient _personClient;
    private readonly ILogger<OrganisationUsersController> _logger;

    public OrganisationUsersController(
        IOrganisationUserService organisationUserService,
        IPersonClient personClient,
        ILogger<OrganisationUsersController> logger)
    {
        _organisationUserService = organisationUserService;
        _personClient = personClient;
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
            var memberships = (await _organisationUserService.GetOrganisationUsersAsync(cdpOrganisationId, cancellationToken)).ToList();
            _logger.LogInformation("Retrieved {MembershipCount} memberships for CDP organisation {CdpOrganisationId}", memberships.Count, cdpOrganisationId);
            var personIds = memberships.Where(m => m.CdpPersonId.HasValue)
                .Select(m => m.CdpPersonId!.Value.ToString())
                .Distinct()
                .ToList();
            _logger.LogInformation("Looking up {PersonIdCount} person IDs for CDP organisation {CdpOrganisationId}", personIds.Count, cdpOrganisationId);
            var personsById = personIds.Count == 0
                ? new Dictionary<string, CO.CDP.Person.WebApiClient.BulkLookupPersonResult>()
                : await _personClient.BulkLookupPersonAsync(new BulkLookupPerson(personIds), cancellationToken);
            _logger.LogInformation("Person lookup returned {PersonCount} records for CDP organisation {CdpOrganisationId}", personsById.Count, cdpOrganisationId);
            _logger.LogDebug("Person lookup request IDs: {PersonIds}", personIds);
            _logger.LogDebug("Person lookup response IDs: {PersonIds}", personsById.Keys);
            var missingPersonIds = personIds.Except(personsById.Keys).ToList();
            if (missingPersonIds.Count > 0)
            {
                _logger.LogWarning("Person lookup missing {MissingCount} IDs for CDP organisation {CdpOrganisationId}: {MissingPersonIds}", missingPersonIds.Count, cdpOrganisationId, missingPersonIds);
            }

            var responses = memberships
                .Select(membership =>
                {
                    PersonDetails? personDetails = null;
                    if (membership.CdpPersonId.HasValue && personsById.TryGetValue(membership.CdpPersonId.Value.ToString(), out var person))
                    {
                        personDetails = new PersonDetails
                        {
                            FirstName = person.FirstName,
                            LastName = person.LastName,
                            Email = person.Email,
                            CdpPersonId = person.Id,
                                                    };
                    }
                    return membership.ToResponse(includeAssignments: false, personDetails: personDetails);
                })
                .ToList();

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

            var personsById = await _personClient.BulkLookupPersonAsync(new BulkLookupPerson([membership.CdpPersonId!.Value.ToString()]), cancellationToken);
            var person = membership.CdpPersonId.HasValue && personsById.TryGetValue(membership.CdpPersonId.Value.ToString(), out var personById)
                ? personById
                : null;
            var personDetails = person == null ? null : new PersonDetails
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                CdpPersonId = person.Id,
                            };

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
        ChangeOrganisationRoleRequest request,
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

            PersonDetails? personDetails = null;
            if (membership.CdpPersonId.HasValue)
            {
                var personsById = await _personClient.BulkLookupPersonAsync(new BulkLookupPerson([membership.CdpPersonId!.Value.ToString()]), cancellationToken);
                var person = membership.CdpPersonId.HasValue && personsById.TryGetValue(membership.CdpPersonId.Value.ToString(), out var personById)
                    ? personById
                    : null;
                if (person != null)
                {
                    personDetails = new PersonDetails
                    {
                        FirstName = person.FirstName,
                        LastName = person.LastName,
                        Email = person.Email,
                        CdpPersonId = person.Id,
                                            };
                }
            }

            return Ok(membership.ToResponse(includeAssignments: true, personDetails: personDetails));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }
}
