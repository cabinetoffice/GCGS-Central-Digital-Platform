using CO.CDP.ApplicationRegistry.Api.Authorization;
using CO.CDP.ApplicationRegistry.Api.Models;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
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
    private readonly ILogger<OrganisationUsersController> _logger;

    public OrganisationUsersController(
        IOrganisationUserService organisationUserService,
        ILogger<OrganisationUsersController> logger)
    {
        _organisationUserService = organisationUserService;
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
            return Ok(memberships.ToResponses());
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

            return Ok(membership.ToResponse(includeAssignments: true));
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }
}
