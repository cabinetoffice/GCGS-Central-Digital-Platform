using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using CO.CDP.UserManagement.Api.Models;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.UserManagement.Api.Controllers;

/// <summary>
/// Controller for managing application permissions.
/// </summary>
[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationPermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public ApplicationPermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    /// <summary>
    /// Gets all permissions for an application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of permissions.</returns>
    [HttpGet("{id:int}/permissions")]
    [ProducesResponseType(typeof(IEnumerable<PermissionResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PermissionResponse>>> GetPermissions(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var permissions = await _permissionService.GetByApplicationIdAsync(id, cancellationToken);
            return Ok(permissions.ToResponses());
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new permission for an application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="request">The create permission request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created permission.</returns>
    [HttpPost("{id:int}/permissions")]
    [ProducesResponseType(typeof(PermissionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PermissionResponse>> CreatePermission(
        int id,
        [FromBody] CreatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var permission = await _permissionService.CreateAsync(
                id,
                request.Name,
                request.Description,
                request.IsActive,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetPermissions),
                new { id },
                permission.ToResponse());
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
    /// Deletes a permission from an application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="permissionId">The permission identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:int}/permissions/{permissionId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePermission(
        int id,
        int permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _permissionService.DeleteAsync(permissionId, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Updates an existing permission.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="permissionId">The permission identifier.</param>
    /// <param name="request">The update permission request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated permission.</returns>
    [HttpPut("{id:int}/permissions/{permissionId:int}")]
    [ProducesResponseType(typeof(PermissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PermissionResponse>> UpdatePermission(
        int id,
        int permissionId,
        [FromBody] UpdatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var permission = await _permissionService.UpdateAsync(
                permissionId,
                request.Name,
                request.Description,
                request.IsActive,
                cancellationToken);

            return Ok(permission.ToResponse());
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
}
