using CO.CDP.ApplicationRegistry.Api.Models;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.ApplicationRegistry.Api.Controllers;

/// <summary>
/// Controller for managing organisation-application relationships.
/// </summary>
[ApiController]
[Route("api/organisations/{orgId:int}/applications")]
[Authorize]
public class OrganisationApplicationsController : ControllerBase
{
    private readonly IOrganisationApplicationService _organisationApplicationService;
    private readonly ILogger<OrganisationApplicationsController> _logger;

    public OrganisationApplicationsController(
        IOrganisationApplicationService organisationApplicationService,
        ILogger<OrganisationApplicationsController> logger)
    {
        _organisationApplicationService = organisationApplicationService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all applications enabled for an organisation.
    /// </summary>
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of organisation applications.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<OrganisationApplicationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<OrganisationApplicationResponse>>> GetApplications(
        int orgId,
        CancellationToken cancellationToken)
    {
        try
        {
            var orgApps = await _organisationApplicationService.GetByOrganisationIdAsync(orgId, cancellationToken);
            return Ok(orgApps.ToResponses());
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Gets all applications accessible by a user.
    /// </summary>
    /// <param name="orgId">The organisation identifier (not used, included for route consistency).</param>
    /// <param name="userId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of applications the user has access to.</returns>
    [HttpGet("users/{userId}")]
    [ProducesResponseType(typeof(IEnumerable<OrganisationApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<OrganisationApplicationResponse>>> GetApplicationsByUser(
        int orgId,
        string userId,
        CancellationToken cancellationToken)
    {
        var orgApps = await _organisationApplicationService.GetApplicationsByUserAsync(userId, cancellationToken);
        return Ok(orgApps.ToResponses());
    }

    /// <summary>
    /// Enables an application for an organisation.
    /// </summary>
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="request">The enable application request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created organisation application relationship.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(OrganisationApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrganisationApplicationResponse>> EnableApplication(
        int orgId,
        [FromBody] EnableApplicationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var orgApp = await _organisationApplicationService.EnableApplicationAsync(
                orgId,
                request.ApplicationId,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetApplications),
                new { orgId },
                orgApp.ToResponse());
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (DuplicateEntityException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message, Code = "DUPLICATE_ENTITY" });
        }
    }

    /// <summary>
    /// Disables an application for an organisation.
    /// </summary>
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="applicationId">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{applicationId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableApplication(
        int orgId,
        int applicationId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _organisationApplicationService.DisableApplicationAsync(orgId, applicationId, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }
}
