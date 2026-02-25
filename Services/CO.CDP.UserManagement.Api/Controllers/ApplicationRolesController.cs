using CO.CDP.UserManagement.Shared.Requests;
using CO.CDP.UserManagement.Shared.Responses;
using CO.CDP.UserManagement.Api.Models;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.UserManagement.Api.Controllers;

/// <summary>
/// Controller for managing application roles.
/// </summary>
[ApiController]
[Route("api/applications")]
[Authorize]
public class ApplicationRolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public ApplicationRolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    /// <summary>
    /// Gets all roles for an application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Collection of roles.</returns>
    [HttpGet("{id:int}/roles")]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<RoleResponse>>> GetRoles(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var roles = await _roleService.GetByApplicationIdAsync(id, cancellationToken);
            return Ok(roles.ToResponses());
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Creates a new role for an application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="request">The create role request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created role.</returns>
    [HttpPost("{id:int}/roles")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> CreateRole(
        int id,
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.CreateAsync(
                id,
                request.Name,
                request.Description,
                request.IsActive,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetRoles),
                new { id },
                role.ToResponse());
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
    /// Updates an existing role.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="request">The update role request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated role.</returns>
    [HttpPut("{id:int}/roles/{roleId:int}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> UpdateRole(
        int id,
        int roleId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.UpdateAsync(
                roleId,
                request.Name,
                request.Description,
                request.IsActive,
                cancellationToken);

            return Ok(role.ToResponse());
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
    /// Deletes a role from an application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:int}/roles/{roleId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(
        int id,
        int roleId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _roleService.DeleteAsync(roleId, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Assigns permissions to a role.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="request">The assign permissions request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated role with permissions.</returns>
    [HttpPost("{id:int}/roles/{roleId:int}/permissions")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> AssignPermissions(
        int id,
        int roleId,
        [FromBody] AssignPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.AssignPermissionsAsync(
                roleId,
                request.PermissionIds,
                cancellationToken);

            return Ok(role.ToResponse());
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
    /// Removes a permission from a role.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="roleId">The role identifier.</param>
    /// <param name="permissionId">The permission identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated role.</returns>
    [HttpDelete("{id:int}/roles/{roleId:int}/permissions/{permissionId:int}")]
    [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RoleResponse>> RemovePermission(
        int id,
        int roleId,
        int permissionId,
        CancellationToken cancellationToken)
    {
        try
        {
            var role = await _roleService.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
            {
                return NotFound(new ErrorResponse { Message = $"Role with ID {roleId} not found." });
            }

            var permissionsToKeep = role.Permissions
                .Where(p => p.Id != permissionId)
                .Select(p => p.Id);

            var updatedRole = await _roleService.AssignPermissionsAsync(
                roleId,
                permissionsToKeep,
                cancellationToken);

            return Ok(updatedRole.ToResponse());
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }
}
