using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Services;

/// <summary>
/// Service for managing user assignments to applications with roles.
/// </summary>
public class UserAssignmentService : IUserAssignmentService
{
    private readonly IUserApplicationAssignmentRepository _assignmentRepository;
    private readonly IUserOrganisationMembershipRepository _membershipRepository;
    private readonly IOrganisationApplicationRepository _organisationApplicationRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserAssignmentService> _logger;

    public UserAssignmentService(
        IUserApplicationAssignmentRepository assignmentRepository,
        IUserOrganisationMembershipRepository membershipRepository,
        IOrganisationApplicationRepository organisationApplicationRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserAssignmentService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _membershipRepository = membershipRepository;
        _organisationApplicationRepository = organisationApplicationRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<UserApplicationAssignment>> GetUserAssignmentsAsync(
        string userPrincipalId,
        int organisationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting assignments for user: {UserPrincipalId} in organisation ID: {OrganisationId}",
            userPrincipalId, organisationId);

        // Get user's membership in the organisation
        var membership = await _membershipRepository.GetByUserAndOrganisationAsync(
            userPrincipalId, organisationId, cancellationToken);

        if (membership == null)
        {
            throw new EntityNotFoundException(
                nameof(UserOrganisationMembership),
                $"User {userPrincipalId} in Organisation {organisationId}");
        }

        return await _assignmentRepository.GetByMembershipIdAsync(membership.Id, cancellationToken);
    }

    public async Task<UserApplicationAssignment> AssignUserAsync(
        string userPrincipalId,
        int organisationId,
        int applicationId,
        IEnumerable<int> roleIds,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning user {UserPrincipalId} to application ID: {ApplicationId} in organisation ID: {OrganisationId}",
            userPrincipalId, applicationId, organisationId);

        // Get user's membership in the organisation
        var membership = await _membershipRepository.GetByUserAndOrganisationAsync(
            userPrincipalId, organisationId, cancellationToken);

        if (membership == null)
        {
            throw new EntityNotFoundException(
                nameof(UserOrganisationMembership),
                $"User {userPrincipalId} in Organisation {organisationId}");
        }

        // Get organisation-application relationship
        var organisationApplication = await _organisationApplicationRepository
            .GetByOrganisationAndApplicationAsync(organisationId, applicationId, cancellationToken);

        if (organisationApplication == null)
        {
            throw new EntityNotFoundException(
                nameof(OrganisationApplication),
                $"Organisation {organisationId} and Application {applicationId}");
        }

        if (!organisationApplication.IsActive)
        {
            throw new SystemInvalidOperationException(
                $"Application {applicationId} is not active for organisation {organisationId}");
        }

        // Check if assignment already exists
        var existingAssignment = await _assignmentRepository
            .GetByMembershipAndApplicationAsync(membership.Id, organisationApplication.Id, cancellationToken);

        if (existingAssignment != null && existingAssignment.IsActive)
        {
            throw new DuplicateEntityException(
                nameof(UserApplicationAssignment),
                $"User {userPrincipalId}, Application {applicationId}",
                $"{userPrincipalId}-{applicationId}");
        }

        var roleIdsList = roleIds.ToList();

        // Verify all roles exist and belong to the application
        var roles = new List<ApplicationRole>();
        foreach (var roleId in roleIdsList)
        {
            var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
            {
                throw new EntityNotFoundException(nameof(ApplicationRole), roleId);
            }

            if (role.ApplicationId != applicationId)
            {
                throw new SystemInvalidOperationException(
                    $"Role {roleId} does not belong to application {applicationId}");
            }

            if (!role.IsActive)
            {
                throw new SystemInvalidOperationException($"Role {roleId} is not active");
            }

            roles.Add(role);
        }

        // If assignment exists but inactive, reactivate it
        if (existingAssignment != null)
        {
            existingAssignment.IsActive = true;
            existingAssignment.AssignedAt = DateTimeOffset.UtcNow;
            existingAssignment.RevokedAt = null;
            existingAssignment.RevokedBy = null;
            existingAssignment.Roles.Clear();
            foreach (var role in roles)
            {
                existingAssignment.Roles.Add(role);
            }

            _assignmentRepository.Update(existingAssignment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User assignment reactivated with ID: {AssignmentId}", existingAssignment.Id);
            return existingAssignment;
        }

        // Create new assignment
        var assignment = new UserApplicationAssignment
        {
            UserOrganisationMembershipId = membership.Id,
            OrganisationApplicationId = organisationApplication.Id,
            IsActive = true,
            AssignedAt = DateTimeOffset.UtcNow
        };

        foreach (var role in roles)
        {
            assignment.Roles.Add(role);
        }

        _assignmentRepository.Add(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User assigned to application with assignment ID: {AssignmentId}", assignment.Id);
        return assignment;
    }

    public async Task<UserApplicationAssignment> UpdateAssignmentAsync(
        int assignmentId,
        IEnumerable<int> roleIds,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating assignment ID: {AssignmentId}", assignmentId);

        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new EntityNotFoundException(nameof(UserApplicationAssignment), assignmentId);
        }

        if (!assignment.IsActive)
        {
            throw new SystemInvalidOperationException($"Assignment {assignmentId} is not active");
        }

        var roleIdsList = roleIds.ToList();

        // Get the application ID from the organisation application
        var organisationApplication = assignment.OrganisationApplication;
        var applicationId = organisationApplication.ApplicationId;

        // Verify all roles exist and belong to the application
        var roles = new List<ApplicationRole>();
        foreach (var roleId in roleIdsList)
        {
            var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
            if (role == null)
            {
                throw new EntityNotFoundException(nameof(ApplicationRole), roleId);
            }

            if (role.ApplicationId != applicationId)
            {
                throw new SystemInvalidOperationException(
                    $"Role {roleId} does not belong to application {applicationId}");
            }

            if (!role.IsActive)
            {
                throw new SystemInvalidOperationException($"Role {roleId} is not active");
            }

            roles.Add(role);
        }

        // Update roles
        assignment.Roles.Clear();
        foreach (var role in roles)
        {
            assignment.Roles.Add(role);
        }

        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assignment ID: {AssignmentId} updated successfully", assignmentId);
        return assignment;
    }

    public async Task RevokeAssignmentAsync(int assignmentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Revoking assignment ID: {AssignmentId}", assignmentId);

        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId, cancellationToken);
        if (assignment == null)
        {
            throw new EntityNotFoundException(nameof(UserApplicationAssignment), assignmentId);
        }

        assignment.IsActive = false;
        assignment.RevokedAt = DateTimeOffset.UtcNow;

        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Assignment ID: {AssignmentId} revoked successfully", assignmentId);
    }
}
