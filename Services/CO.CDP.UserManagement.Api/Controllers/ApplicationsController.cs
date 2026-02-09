using CO.CDP.ApplicationRegistry.Shared.Requests;
using CO.CDP.ApplicationRegistry.Shared.Responses;
using CO.CDP.UserManagement.Api.Models;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.UserManagement.Api.Controllers;

/// <summary>
/// Controller for managing applications.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ApplicationsController : ControllerBase
{
    private readonly IApplicationService _applicationService;
    private readonly IPermissionService _permissionService;
    private readonly IRoleService _roleService;
    private readonly ILogger<ApplicationsController> _logger;

    public ApplicationsController(
        IApplicationService applicationService,
        IPermissionService permissionService,
        IRoleService roleService,
        ILogger<ApplicationsController> logger)
    {
        _applicationService = applicationService;
        _permissionService = permissionService;
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all applications.
    /// </summary>
    /// <returns>Collection of applications.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ApplicationResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ApplicationResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var applications = await _applicationService.GetAllAsync(cancellationToken);
        return Ok(applications.Select(a => a.ToResponse()));
    }

    /// <summary>
    /// Gets an application by ID.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The application if found.</returns>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var application = await _applicationService.GetByIdAsync(id, cancellationToken);
        if (application == null)
        {
            return NotFound(new ErrorResponse { Message = $"Application with ID {id} not found." });
        }

        return Ok(application.ToResponse());
    }

    /// <summary>
    /// Creates a new application.
    /// </summary>
    /// <param name="request">The create application request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created application.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApplicationResponse>> Create(
        [FromBody] CreateApplicationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var application = await _applicationService.CreateAsync(
                request.Name,
                request.ClientId,
                request.Description,
                request.IsActive,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = application.Id },
                application.ToResponse());
        }
        catch (DuplicateEntityException ex)
        {
            return BadRequest(new ErrorResponse { Message = ex.Message, Code = "DUPLICATE_ENTITY" });
        }
    }

    /// <summary>
    /// Updates an existing application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="request">The update application request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated application.</returns>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApplicationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApplicationResponse>> Update(
        int id,
        [FromBody] UpdateApplicationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var application = await _applicationService.UpdateAsync(
                id,
                request.Name,
                request.Description,
                request.IsActive,
                cancellationToken);

            return Ok(application.ToResponse());
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    /// <summary>
    /// Deletes an application.
    /// </summary>
    /// <param name="id">The application identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>No content.</returns>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        try
        {
            await _applicationService.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (EntityNotFoundException ex)
        {
            return NotFound(new ErrorResponse { Message = ex.Message });
        }
    }

    // ====== PERMISSION ROUTES ======

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

    // ====== ROLE ROUTES ======

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

            // Remove the permission from the role
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
