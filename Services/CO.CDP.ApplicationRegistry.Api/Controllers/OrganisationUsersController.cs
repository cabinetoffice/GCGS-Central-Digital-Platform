using CO.CDP.ApplicationRegistry.Api.Authorization;
using CO.CDP.ApplicationRegistry.Api.Models;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Core.Models;
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
    private readonly IPersonLookupService _personLookupService;
    private readonly ILogger<OrganisationUsersController> _logger;

    public OrganisationUsersController(
        IOrganisationUserService organisationUserService,
        IPersonLookupService personLookupService,
        ILogger<OrganisationUsersController> logger)
    {
        _organisationUserService = organisationUserService;
        _personLookupService = personLookupService;
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
            var memberships = await _organisationUserService.GetOrganisationUsersAsync(orgId, cancellationToken);
            var responses = new List<OrganisationUserResponse>();

            foreach (var membership in memberships)
            {
                var personDetails = await GetPersonDetailsAsync(membership.UserPrincipalId, cancellationToken);
                responses.Add(membership.ToResponse(includeAssignments: false, personDetails: personDetails));
            }

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

            var personDetails = await GetPersonDetailsAsync(membership.UserPrincipalId, cancellationToken);
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

            var personDetails = await GetPersonDetailsAsync(membership.UserPrincipalId, cancellationToken);
            return Ok(membership.ToResponse(includeAssignments: true, personDetails: personDetails));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    private async Task<PersonDetails?> GetPersonDetailsAsync(string userPrincipalId, CancellationToken cancellationToken)
    {
        try
        {
            return await _personLookupService.GetPersonDetailsAsync(userPrincipalId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to lookup person details for UserPrincipalId: {UserPrincipalId}. Person details will be null.",
                userPrincipalId);
            return null;
        }
    }
}
