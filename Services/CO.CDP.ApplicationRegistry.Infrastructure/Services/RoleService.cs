using CO.CDP.ApplicationRegistry.Core.Entities;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Services;

/// <summary>
/// Service for managing application roles and their permissions.
/// </summary>
public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoleService> _logger;

    public RoleService(
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<RoleService> logger)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApplicationRole?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting role with ID: {RoleId}", id);
        return await _roleRepository.GetByIdWithPermissionsAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<ApplicationRole>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting roles for application ID: {ApplicationId}", applicationId);

        // Verify application exists
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application == null)
        {
            throw new EntityNotFoundException(nameof(Application), applicationId);
        }

        return await _roleRepository.GetByApplicationIdAsync(applicationId, cancellationToken);
    }

    public async Task<ApplicationRole> CreateAsync(
        int applicationId,
        string name,
        string? description,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating role '{RoleName}' for application ID: {ApplicationId}", name, applicationId);

        // Verify application exists
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application == null)
        {
            throw new EntityNotFoundException(nameof(Application), applicationId);
        }

        // Check for duplicate role name within the application
        var existingRole = await _roleRepository.GetByNameAsync(applicationId, name, cancellationToken);
        if (existingRole != null)
        {
            throw new DuplicateEntityException(nameof(ApplicationRole), nameof(ApplicationRole.Name), name);
        }

        var role = new ApplicationRole
        {
            ApplicationId = applicationId,
            Name = name,
            Description = description,
            IsActive = isActive
        };

        _roleRepository.Add(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role '{RoleName}' created with ID: {RoleId}", name, role.Id);
        return role;
    }

    public async Task<ApplicationRole> UpdateAsync(
        int id,
        string name,
        string? description,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating role with ID: {RoleId}", id);

        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role == null)
        {
            throw new EntityNotFoundException(nameof(ApplicationRole), id);
        }

        // Check for duplicate role name within the application (excluding current role)
        var existingRole = await _roleRepository.GetByNameAsync(role.ApplicationId, name, cancellationToken);
        if (existingRole != null && existingRole.Id != id)
        {
            throw new DuplicateEntityException(nameof(ApplicationRole), nameof(ApplicationRole.Name), name);
        }

        role.Name = name;
        role.Description = description;
        role.IsActive = isActive;

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role with ID: {RoleId} updated successfully", id);
        return role;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting role with ID: {RoleId}", id);

        var role = await _roleRepository.GetByIdAsync(id, cancellationToken);
        if (role == null)
        {
            throw new EntityNotFoundException(nameof(ApplicationRole), id);
        }

        _roleRepository.Remove(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Role with ID: {RoleId} deleted successfully", id);
    }

    public async Task<ApplicationRole> AssignPermissionsAsync(
        int roleId,
        IEnumerable<int> permissionIds,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning permissions to role ID: {RoleId}", roleId);

        var role = await _roleRepository.GetByIdWithPermissionsAsync(roleId, cancellationToken);
        if (role == null)
        {
            throw new EntityNotFoundException(nameof(ApplicationRole), roleId);
        }

        var permissionIdsList = permissionIds.ToList();

        // Verify all permissions exist and belong to the same application
        var permissions = new List<ApplicationPermission>();
        foreach (var permissionId in permissionIdsList)
        {
            var permission = await _permissionRepository.GetByIdAsync(permissionId, cancellationToken);
            if (permission == null)
            {
                throw new EntityNotFoundException(nameof(ApplicationPermission), permissionId);
            }

            if (permission.ApplicationId != role.ApplicationId)
            {
                throw new SystemInvalidOperationException(
                    $"Permission {permissionId} does not belong to application {role.ApplicationId}");
            }

            permissions.Add(permission);
        }

        // Clear existing permissions and assign new ones
        role.Permissions.Clear();
        foreach (var permission in permissions)
        {
            role.Permissions.Add(permission);
        }

        _roleRepository.Update(role);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permissions assigned to role ID: {RoleId} successfully", roleId);
        return role;
    }
}
