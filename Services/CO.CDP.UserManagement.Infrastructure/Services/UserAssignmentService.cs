using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using Microsoft.Extensions.Logging;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Service for managing user assignments to applications with roles.
/// </summary>
public class UserAssignmentService : IUserAssignmentService
{
    private readonly IUserApplicationAssignmentRepository _assignmentRepository;
    private readonly IUserOrganisationMembershipRepository _membershipRepository;
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IOrganisationApplicationRepository _organisationApplicationRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleMappingService _roleMappingService;
    private readonly IOrganisationApiAdapter _organisationApiAdapter;
    private readonly ICdpMembershipSyncService _membershipSyncService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserAssignmentService> _logger;
    private const string SystemAssignedBy = "system:default-app-assignment";

    public UserAssignmentService(
        IUserApplicationAssignmentRepository assignmentRepository,
        IUserOrganisationMembershipRepository membershipRepository,
        IOrganisationRepository organisationRepository,
        IOrganisationApplicationRepository organisationApplicationRepository,
        IRoleRepository roleRepository,
        IRoleMappingService roleMappingService,
        IOrganisationApiAdapter organisationApiAdapter,
        ICdpMembershipSyncService membershipSyncService,
        IUnitOfWork unitOfWork,
        ILogger<UserAssignmentService> logger)
    {
        _assignmentRepository = assignmentRepository;
        _membershipRepository = membershipRepository;
        _organisationRepository = organisationRepository;
        _organisationApplicationRepository = organisationApplicationRepository;
        _roleRepository = roleRepository;
        _roleMappingService = roleMappingService;
        _organisationApiAdapter = organisationApiAdapter;
        _membershipSyncService = membershipSyncService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<UserApplicationAssignment>> GetUserAssignmentsAsync(
        string userId,
        int organisationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting assignments for user: {UserId} in organisation ID: {OrganisationId}",
            userId, organisationId);

        var membership = await ResolveMembershipAsync(userId, organisationId, cancellationToken);

        if (membership == null)
        {
            throw new EntityNotFoundException(
                nameof(UserOrganisationMembership),
                $"User {userId} in Organisation {organisationId}");
        }

        return await _assignmentRepository.GetByMembershipIdAsync(membership.Id, cancellationToken);
    }

    public async Task<UserApplicationAssignment> AssignUserAsync(
        string userId,
        int organisationId,
        int applicationId,
        IEnumerable<int> roleIds,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning user {UserId} to application ID: {ApplicationId} in organisation ID: {OrganisationId}",
            userId, applicationId, organisationId);

        // Get user's membership in the organisation
        var membership = await ResolveMembershipAsync(userId, organisationId, cancellationToken);

        if (membership == null)
        {
            throw new EntityNotFoundException(
                nameof(UserOrganisationMembership),
                $"User {userId} in Organisation {organisationId}");
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
                $"User {userId}, Application {applicationId}",
                $"{userId}-{applicationId}");
        }

        var roles = await _roleMappingService.GetAssignableRolesAsync(
            organisationId,
            membership.OrganisationRole,
            roleIds,
            cancellationToken);

        foreach (var role in roles)
        {
            if (role.ApplicationId != applicationId)
            {
                throw new SystemInvalidOperationException(
                    $"Role {role.Id} does not belong to application {applicationId}");
            }

            if (!role.IsActive)
            {
                throw new SystemInvalidOperationException($"Role {role.Id} is not active");
            }
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
            await _membershipSyncService.SyncMembershipAccessChangedAsync(membership.Id, cancellationToken);

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
        await _membershipSyncService.SyncMembershipAccessChangedAsync(membership.Id, cancellationToken);

        _logger.LogInformation("User assigned to application with assignment ID: {AssignmentId}", assignment.Id);
        return assignment;
    }

    public async Task<UserApplicationAssignment> UpdateAssignmentAsync(
        string userId,
        int organisationId,
        int assignmentId,
        IEnumerable<int> roleIds,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating assignment ID: {AssignmentId} for user {UserId} in organisation ID: {OrganisationId}",
            assignmentId, userId, organisationId);

        var assignment = await GetAssignmentForUserAsync(userId, organisationId, assignmentId, cancellationToken);

        if (!assignment.IsActive)
        {
            throw new SystemInvalidOperationException($"Assignment {assignmentId} is not active");
        }

        // Get the application ID from the organisation application
        var organisationApplication = assignment.OrganisationApplication;
        var applicationId = organisationApplication.ApplicationId;

        var membership = assignment.UserOrganisationMembership;
        var roles = await _roleMappingService.GetAssignableRolesAsync(
            organisationId,
            membership.OrganisationRole,
            roleIds,
            cancellationToken);

        foreach (var role in roles)
        {
            if (role.ApplicationId != applicationId)
            {
                throw new SystemInvalidOperationException(
                    $"Role {role.Id} does not belong to application {applicationId}");
            }

            if (!role.IsActive)
            {
                throw new SystemInvalidOperationException($"Role {role.Id} is not active");
            }
        }

        // Update roles
        assignment.Roles.Clear();
        foreach (var role in roles)
        {
            assignment.Roles.Add(role);
        }

        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _membershipSyncService.SyncMembershipAccessChangedAsync(membership.Id, cancellationToken);

        _logger.LogInformation("Assignment ID: {AssignmentId} updated successfully", assignmentId);
        return assignment;
    }

    public async Task RevokeAssignmentAsync(string userId, int organisationId, int assignmentId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Revoking assignment ID: {AssignmentId} for user {UserId} in organisation ID: {OrganisationId}",
            assignmentId, userId, organisationId);

        var assignment = await GetAssignmentForUserAsync(userId, organisationId, assignmentId, cancellationToken);

        if (assignment.OrganisationApplication.Application.IsEnabledByDefault)
        {
            throw new SystemInvalidOperationException(
                $"Application {assignment.OrganisationApplication.ApplicationId} is enabled by default and user access cannot be revoked");
        }

        assignment.IsActive = false;
        assignment.RevokedAt = DateTimeOffset.UtcNow;

        _assignmentRepository.Update(assignment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _membershipSyncService.SyncMembershipAccessChangedAsync(assignment.UserOrganisationMembershipId, cancellationToken);

        _logger.LogInformation("Assignment ID: {AssignmentId} revoked successfully", assignmentId);
    }

    public async Task AssignDefaultApplicationsAsync(
        UserOrganisationMembership membership,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Assigning default applications for membership ID: {MembershipId} in organisation ID: {OrganisationId}",
            membership.Id, membership.OrganisationId);

        if (!await _roleMappingService.ShouldAutoAssignDefaultApplicationsAsync(membership, cancellationToken))
        {
            _logger.LogInformation(
                "Skipping default application assignment for membership {MembershipId} because organisation role {OrganisationRole} is not eligible",
                membership.Id,
                membership.OrganisationRole);
            return;
        }

        var defaultOrganisationApplications = (await _organisationApplicationRepository.GetDefaultEnabledByOrganisationIdAsync(
            membership.OrganisationId,
            cancellationToken)).ToList();

        if (defaultOrganisationApplications.Count == 0)
        {
            return;
        }

        var organisationPartyRoles = await GetOrganisationPartyRolesAsync(membership.OrganisationId, cancellationToken);
        var changes = await defaultOrganisationApplications
            .ToAsyncEnumerable()
            .SelectAwait(async organisationApplication => await EnsureDefaultApplicationAssignedAsync(
                membership,
                organisationPartyRoles,
                organisationApplication,
                cancellationToken))
            .ToListAsync(cancellationToken);

        if (changes.Any(changed => changed))
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }

    private async Task<UserOrganisationMembership?> ResolveMembershipAsync(
        string userId,
        int organisationId,
        CancellationToken cancellationToken)
    {
        if (Guid.TryParse(userId, out var cdpPersonId))
        {
            return await _membershipRepository.GetByPersonIdAndOrganisationAsync(cdpPersonId, organisationId, cancellationToken);
        }

        return await _membershipRepository.GetByUserAndOrganisationAsync(userId, organisationId, cancellationToken);
    }

    private async Task<UserApplicationAssignment> GetAssignmentForUserAsync(
        string userId,
        int organisationId,
        int assignmentId,
        CancellationToken cancellationToken)
    {
        var membership = await ResolveMembershipAsync(userId, organisationId, cancellationToken);
        if (membership == null)
        {
            throw new EntityNotFoundException(
                nameof(UserOrganisationMembership),
                $"User {userId} in Organisation {organisationId}");
        }

        var assignment = (await _assignmentRepository.GetByMembershipIdAsync(membership.Id, cancellationToken))
            .SingleOrDefault(a => a.Id == assignmentId);

        if (assignment == null)
        {
            throw new EntityNotFoundException(nameof(UserApplicationAssignment), assignmentId);
        }

        return assignment;
    }

    private async Task<ISet<CO.CDP.UserManagement.Core.Constants.PartyRole>> GetOrganisationPartyRolesAsync(
        int organisationId,
        CancellationToken cancellationToken)
    {
        var organisation = await _organisationRepository.GetByIdAsync(organisationId, cancellationToken)
            ?? throw new EntityNotFoundException(nameof(Organisation), organisationId);

        return await _organisationApiAdapter.GetPartyRolesAsync(organisation.CdpOrganisationGuid, cancellationToken);
    }

    private async Task<IReadOnlyList<ApplicationRole>> GetDefaultRolesAsync(
        UserOrganisationMembership membership,
        OrganisationApplication organisationApplication,
        ISet<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        CancellationToken cancellationToken)
    {
        var organisationInformationScopes = await _roleMappingService.GetInviteScopesAsync(
            membership.OrganisationRole,
            cancellationToken);

        return DefaultApplicationRoleSelector.SelectFor(
            organisationApplication,
            await _roleRepository.GetByApplicationIdAsync(organisationApplication.ApplicationId, cancellationToken),
            organisationPartyRoles,
            organisationInformationScopes);
    }

    private async Task<bool> EnsureDefaultApplicationAssignedAsync(
        UserOrganisationMembership membership,
        ISet<CO.CDP.UserManagement.Core.Constants.PartyRole> organisationPartyRoles,
        OrganisationApplication organisationApplication,
        CancellationToken cancellationToken)
    {
        var existingAssignment = await _assignmentRepository.GetByMembershipAndApplicationAsync(
            membership.Id,
            organisationApplication.Id,
            cancellationToken);
        var defaultRoles = await GetDefaultRolesAsync(
            membership,
            organisationApplication,
            organisationPartyRoles,
            cancellationToken);

        if (existingAssignment is null)
        {
            _assignmentRepository.Add(new UserApplicationAssignment
            {
                UserOrganisationMembershipId = membership.Id,
                OrganisationApplicationId = organisationApplication.Id,
                IsActive = true,
                AssignedAt = DateTimeOffset.UtcNow,
                AssignedBy = SystemAssignedBy,
                CreatedBy = SystemAssignedBy,
                Roles = defaultRoles.ToList()
            });
            return true;
        }

        if (AssignmentMatches(existingAssignment, defaultRoles))
        {
            return false;
        }

        existingAssignment.IsActive = true;
        existingAssignment.IsDeleted = false;
        existingAssignment.RevokedAt = null;
        existingAssignment.RevokedBy = null;
        existingAssignment.DeletedAt = null;
        existingAssignment.DeletedBy = null;
        existingAssignment.ModifiedBy = SystemAssignedBy;
        SyncRoles(existingAssignment, defaultRoles);

        _assignmentRepository.Update(existingAssignment);
        return true;
    }

    private static bool AssignmentMatches(
        UserApplicationAssignment assignment,
        IReadOnlyList<ApplicationRole> desiredRoles) =>
        assignment.IsActive &&
        !assignment.IsDeleted &&
        assignment.Roles.Select(role => role.Id).OrderBy(id => id)
            .SequenceEqual(desiredRoles.Select(role => role.Id).OrderBy(id => id));

    private static void SyncRoles(
        UserApplicationAssignment assignment,
        IEnumerable<ApplicationRole> desiredRoles)
    {
        assignment.Roles.Clear();
        foreach (var role in desiredRoles)
        {
            assignment.Roles.Add(role);
        }
    }
}
