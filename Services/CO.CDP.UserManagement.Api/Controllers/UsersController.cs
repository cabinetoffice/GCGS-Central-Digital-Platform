using CO.CDP.UserManagement.Api.Authorization;
using CO.CDP.UserManagement.Api.Models;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Shared.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.Api.Controllers;

/// <summary>
/// Controller for managing user operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IPersonLookupService _personLookupService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IPersonLookupService personLookupService,
        ILogger<UsersController> logger)
    {
        _personLookupService = personLookupService;
        _logger = logger;
    }

    /// <summary>
    /// Looks up user details from the CDP Person service.
    /// </summary>
    /// <param name="userPrincipalId">The user principal identifier (OneLogin 'sub' claim).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The user's details if found.</returns>
    [HttpGet("{userPrincipalId}", Name = "LookupUser")]
    [ProducesResponseType(typeof(PersonDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<PersonDetailsResponse>> LookupUser(
        string userPrincipalId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userPrincipalId))
        {
            return BadRequest(new ErrorResponse { Message = "User principal ID is required." });
        }

        try
        {
            var personDetails = await _personLookupService.GetPersonDetailsAsync(userPrincipalId, cancellationToken);

            if (personDetails == null)
            {
                return NotFound(new ErrorResponse
                {
                    Message = $"User with principal ID '{userPrincipalId}' not found."
                });
            }

            return Ok(personDetails.ToResponse());
        }
        catch (PersonLookupException ex)
        {
            _logger.LogError(ex, "Error looking up user with principal ID: {UserPrincipalId}", userPrincipalId);
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new ErrorResponse { Message = "User lookup service is currently unavailable." });
        }
    }
}
