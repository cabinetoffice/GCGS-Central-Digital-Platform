using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Services;

/// <summary>
/// Service for managing application permissions.
/// </summary>
public class PermissionService : IPermissionService
{
    private readonly IPermissionRepository _permissionRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PermissionService> _logger;

    public PermissionService(
        IPermissionRepository permissionRepository,
        IApplicationRepository applicationRepository,
        IUnitOfWork unitOfWork,
        ILogger<PermissionService> logger)
    {
        _permissionRepository = permissionRepository;
        _applicationRepository = applicationRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApplicationPermission?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting permission with ID: {PermissionId}", id);
        return await _permissionRepository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<IEnumerable<ApplicationPermission>> GetByApplicationIdAsync(int applicationId, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting permissions for application ID: {ApplicationId}", applicationId);

        // Verify application exists
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application == null)
        {
            throw new EntityNotFoundException(nameof(Application), applicationId);
        }

        return await _permissionRepository.GetByApplicationIdAsync(applicationId, cancellationToken);
    }

    public async Task<ApplicationPermission> CreateAsync(
        int applicationId,
        string name,
        string? description,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating permission '{PermissionName}' for application ID: {ApplicationId}", name, applicationId);

        // Verify application exists
        var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
        if (application == null)
        {
            throw new EntityNotFoundException(nameof(Application), applicationId);
        }

        // Check for duplicate permission name within the application
        var existingPermission = await _permissionRepository.GetByNameAsync(applicationId, name, cancellationToken);
        if (existingPermission != null)
        {
            throw new DuplicateEntityException(nameof(ApplicationPermission), nameof(ApplicationPermission.Name), name);
        }

        var permission = new ApplicationPermission
        {
            ApplicationId = applicationId,
            Name = name,
            Description = description,
            IsActive = isActive
        };

        _permissionRepository.Add(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission '{PermissionName}' created with ID: {PermissionId}", name, permission.Id);
        return permission;
    }

    public async Task<ApplicationPermission> UpdateAsync(
        int id,
        string name,
        string? description,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating permission with ID: {PermissionId}", id);

        var permission = await _permissionRepository.GetByIdAsync(id, cancellationToken);
        if (permission == null)
        {
            throw new EntityNotFoundException(nameof(ApplicationPermission), id);
        }

        // Check for duplicate permission name within the application (excluding current permission)
        var existingPermission = await _permissionRepository.GetByNameAsync(permission.ApplicationId, name, cancellationToken);
        if (existingPermission != null && existingPermission.Id != id)
        {
            throw new DuplicateEntityException(nameof(ApplicationPermission), nameof(ApplicationPermission.Name), name);
        }

        permission.Name = name;
        permission.Description = description;
        permission.IsActive = isActive;

        _permissionRepository.Update(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission with ID: {PermissionId} updated successfully", id);
        return permission;
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting permission with ID: {PermissionId}", id);

        var permission = await _permissionRepository.GetByIdAsync(id, cancellationToken);
        if (permission == null)
        {
            throw new EntityNotFoundException(nameof(ApplicationPermission), id);
        }

        _permissionRepository.Remove(permission);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Permission with ID: {PermissionId} deleted successfully", id);
    }
}
