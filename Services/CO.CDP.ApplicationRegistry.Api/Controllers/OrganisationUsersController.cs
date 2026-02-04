using CO.CDP.ApplicationRegistry.Api.Authorization;
using CO.CDP.ApplicationRegistry.Api.Models;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Core.Models;
using CO.CDP.Person.WebApiClient;
using CO.CDP.ApplicationRegistry.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.ApplicationRegistry.Api.Controllers;

/// <summary>
/// Controller for managing organisation users.
/// </summary>
[ApiController]
[Route("api/organisations/{orgId:int}/users")]
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
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organisation users.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrganisationUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrganisationUserResponse>>> GetUsers(
        int orgId,
        CancellationToken cancellationToken)
    {
        try
        {
            var memberships = (await _organisationUserService.GetOrganisationUsersAsync(orgId, cancellationToken)).ToList();
            var personIds = memberships.Where(m => m.CdpPersonId.HasValue)
                .Select(m => m.CdpPersonId!.Value.ToString())
                .Distinct()
                .ToList();
            var personsById = personIds.Count == 0
                ? new Dictionary<string, CO.CDP.Person.WebApiClient.BulkLookupPersonResult>()
                : await _personClient.BulkLookupPersonAsync(new BulkLookupPerson(personIds), cancellationToken);

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
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organisation users without person enrichment.</returns>
    [HttpGet("service")]
    [Authorize(Policy = PolicyNames.ServiceAccount)]
    [ProducesResponseType(typeof(IEnumerable<OrganisationUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrganisationUserResponse>>> GetUsersForService(
        int orgId,
        CancellationToken cancellationToken)
    {
        try
        {
            var memberships = await _organisationUserService.GetOrganisationUsersAsync(orgId, cancellationToken);
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
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="cdpPersonId">The CDP person identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation user if found.</returns>
    [HttpGet("{cdpPersonId:guid}")]
    [ProducesResponseType(typeof(OrganisationUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationUserResponse>> GetUserByPersonId(
        int orgId,
        Guid cdpPersonId,
        CancellationToken cancellationToken)
    {
        try
        {
            var membership = await _organisationUserService.GetOrganisationUserByPersonIdAsync(orgId, cdpPersonId, cancellationToken);
            if (membership == null)
            {
                return NotFound(new ErrorResponse { Message = $"User with CDP Person ID {cdpPersonId} not found in organisation {orgId}." });
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
    /// Gets a specific user in an organisation.
    /// </summary>
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="userId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The organisation user if found.</returns>
    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(OrganisationUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationUserResponse>> GetUser(
        int orgId,
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var membership = await _organisationUserService.GetOrganisationUserAsync(orgId, userId, cancellationToken);
            if (membership == null)
            {
                return NotFound(new ErrorResponse { Message = $"User {userId} not found in organisation {orgId}." });
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
