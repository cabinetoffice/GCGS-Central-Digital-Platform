using CO.CDP.ApplicationRegistry.Api.Models;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.ApplicationRegistry.Api.Controllers;

/// <summary>
/// Controller for managing user assignments to applications.
/// </summary>
[ApiController]
[Route("api/organisations/{orgId:int}/users/{userId}/assignments")]
[Authorize]
public class UserAssignmentsController : ControllerBase
{
    private readonly IUserAssignmentService _userAssignmentService;
    private readonly ILogger<UserAssignmentsController> _logger;

    public UserAssignmentsController(
        IUserAssignmentService userAssignmentService,
        ILogger<UserAssignmentsController> logger)
    {
        _userAssignmentService = userAssignmentService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all assignments for a user within an organisation.
    /// </summary>
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="userId">The user principal identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of user application assignments.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<UserAssignmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<UserAssignmentResponse>>> GetAssignments(
        int orgId,
        string userId,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignments = await _userAssignmentService.GetUserAssignmentsAsync(userId, orgId, cancellationToken);
            return Ok(assignments.ToResponses());
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Assigns a user to an application with specific roles.
    /// </summary>
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="userId">The user principal identifier.</param>
    /// <param name="request">The assignment request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created assignment.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(UserAssignmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserAssignmentResponse>> AssignUser(
        int orgId,
        string userId,
        [FromBody] AssignUserToApplicationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignment = await _userAssignmentService.AssignUserAsync(
                userId,
                orgId,
                request.ApplicationId,
                request.RoleIds,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetAssignments),
                new { orgId, userId },
                assignment.ToResponse());
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (DuplicateEntityException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message, Code = "DUPLICATE_ENTITY" });
        }
        catch (SystemInvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message, Code = "INVALID_OPERATION" });
        }
    }

    /// <summary>
    /// Updates a user's role assignments for an application.
    /// </summary>
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="userId">The user principal identifier.</param>
    /// <param name="assignmentId">The assignment identifier.</param>
    /// <param name="request">The update assignment request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated assignment.</returns>
    [HttpPut("{assignmentId:int}")]
    [ProducesResponseType(typeof(UserAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserAssignmentResponse>> UpdateAssignment(
        int orgId,
        string userId,
        int assignmentId,
        [FromBody] UpdateAssignmentRolesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignment = await _userAssignmentService.UpdateAssignmentAsync(
                assignmentId,
                request.RoleIds,
                cancellationToken);

            return Ok(assignment.ToResponse());
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
        catch (SystemInvalidOperationException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message, Code = "INVALID_OPERATION" });
        }
    }

    /// <summary>
    /// Revokes a user's assignment to an application.
    /// </summary>
    /// <param name="orgId">The organisation identifier.</param>
    /// <param name="userId">The user principal identifier.</param>
    /// <param name="assignmentId">The assignment identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{assignmentId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeAssignment(
        int orgId,
        string userId,
        int assignmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _userAssignmentService.RevokeAssignmentAsync(assignmentId, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }
}
