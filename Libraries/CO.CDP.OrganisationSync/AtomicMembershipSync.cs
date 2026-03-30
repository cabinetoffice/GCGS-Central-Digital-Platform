using CO.CDP.Functional;
using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Infrastructure.Services;
using CO.CDP.UserManagement.Shared.Enums;
using CO.CDP.UserManagement.Shared.Requests;
using Microsoft.Extensions.Logging;
using SystemInvalidOperationException = System.InvalidOperationException;

namespace CO.CDP.OrganisationSync;

/// <summary>
/// Atomic bidirectional sync between UM and OI databases.
/// Every cross-DB write is wrapped in <see cref="IAtomicScope.ExecuteAsync"/> so both
/// <c>OrganisationInformationContext</c> and <c>UserManagementDbContext</c> commit
/// or roll back together.
/// </summary>
public sealed class AtomicMembershipSync(
    IAtomicScope atomicScope,
    IOrganisationRepository organisationRepository,
    IUserOrganisationMembershipRepository membershipRepository,
    IUserApplicationAssignmentRepository assignmentRepository,
    IOrganisationApplicationRepository organisationApplicationRepository,
    IRoleRepository roleRepository,
    IInviteRoleMappingRepository inviteRoleMappingRepository,
    IRoleMappingService roleMappingService,
    IOrganisationPersonSyncRepository organisationPersonSyncRepository,
    IOrganisationApiAdapter organisationApiAdapter,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ILogger<AtomicMembershipSync> logger) : IAtomicMembershipSync
{
    private const string SystemAssignedBy = "system:default-app-assignment";

    // ───────────────────────────── public entry points ─────────────────────────────

    public Task RemoveUserFromOrganisationAsync(
        Guid cdpOrganisationId, Guid cdpPersonId, CancellationToken ct = default) =>
        atomicScope.ExecuteAsync(token => RemoveAsync(cdpOrganisationId, cdpPersonId, token), ct);

    public Task<UserOrganisationMembership> UpdateMembershipRoleAsync(
        Guid cdpOrganisationId, Guid cdpPersonId, OrganisationRole newRole, CancellationToken ct = default) =>
        atomicScope.ExecuteAsync(token => UpdateRoleAsync(cdpOrganisationId, cdpPersonId, newRole, token), ct);

    public Task<UserApplicationAssignment> AssignUserToApplicationAsync(
        string userId, int organisationId, int applicationId,
        IEnumerable<int> roleIds, CancellationToken ct = default) =>
        atomicScope.ExecuteAsync(token => AssignAsync(userId, organisationId, applicationId, roleIds, token), ct);

    public Task<UserApplicationAssignment> UpdateApplicationAssignmentAsync(
        string userId, int organisationId, int assignmentId,
        IEnumerable<int> roleIds, CancellationToken ct = default) =>
        atomicScope.ExecuteAsync(token => UpdateAssignmentAsync(userId, organisationId, assignmentId, roleIds, token), ct);

    public Task RevokeApplicationAssignmentAsync(
        string userId, int organisationId, int assignmentId, CancellationToken ct = default) =>
        atomicScope.ExecuteAsync(async token =>
        {
            await RevokeAsync(userId, organisationId, assignmentId, token);
            return Unit.Value;
        }, ct);

    public Task AcceptInviteAsync(
        Guid cdpOrganisationId, int inviteRoleMappingId,
        AcceptOrganisationInviteRequest request, CancellationToken ct = default) =>
        atomicScope.ExecuteAsync(async token =>
        {
            await AcceptAsync(cdpOrganisationId, inviteRoleMappingId, request, token);
            return Unit.Value;
        }, ct);

    // ───────────────────────────── Remove user ─────────────────────────────

    private async Task<Unit> RemoveAsync(
        Guid cdpOrganisationId, Guid cdpPersonId, CancellationToken ct)
    {
        var organisation = await GetOrganisationAsync(cdpOrganisationId, ct);
        var membership = await GetMembershipByPersonIdAsync(cdpPersonId, organisation.Id, ct);

        if (!membership.IsActive)
            return Unit.Value;

        await GuardLastOwnerAsync(membership, organisation.Id, cdpOrganisationId, ct);

        var now = DateTimeOffset.UtcNow;
        var actingUser = currentUserService.GetUserPrincipalId() ?? "unknown";

        membershipRepository.Update(MarkDeleted(membership, now, actingUser));

        (await assignmentRepository.GetByMembershipIdAsync(membership.Id, ct))
            .Where(a => a.IsActive)
            .Select(a => MarkRevoked(a, now, actingUser))
            .ToList()
            .ForEach(assignmentRepository.Update);

        if (membership.CdpPersonId.HasValue)
            await organisationPersonSyncRepository.RemoveAsync(
                organisation.CdpOrganisationGuid, membership.CdpPersonId.Value, ct);

        logger.LogInformation(
            "User {CdpPersonId} removed from organisation {CdpOrganisationId} by {ActingUser}",
            cdpPersonId, cdpOrganisationId, actingUser);

        return Unit.Value;
    }

    // ───────────────────────────── Update role ─────────────────────────────

    private async Task<UserOrganisationMembership> UpdateRoleAsync(
        Guid cdpOrganisationId, Guid cdpPersonId, OrganisationRole newRole, CancellationToken ct)
    {
        var organisation = await GetOrganisationAsync(cdpOrganisationId, ct);
        var membership = await GetMembershipByPersonIdAsync(cdpPersonId, organisation.Id, ct);

        await roleMappingService.ApplyRoleDefinitionAsync(membership, newRole, ct);
        membershipRepository.Update(membership);

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation(
            "Role for user {CdpPersonId} in organisation {CdpOrganisationId} updated to {NewRole}",
            cdpPersonId, cdpOrganisationId, newRole);

        return membership;
    }

    // ───────────────────────────── Assign user to application ─────────────────────────────

    private async Task<UserApplicationAssignment> AssignAsync(
        string userId, int organisationId, int applicationId,
        IEnumerable<int> roleIds, CancellationToken ct)
    {
        var membership = await ResolveMembershipOrThrowAsync(userId, organisationId, ct);
        var organisationApplication = await GetActiveOrganisationAppAsync(organisationId, applicationId, ct);
        var existingAssignment = await assignmentRepository
            .GetByMembershipAndApplicationAsync(membership.Id, organisationApplication.Id, ct);

        if (existingAssignment is { IsActive: true })
            throw new DuplicateEntityException(
                nameof(UserApplicationAssignment),
                $"User {userId}, Application {applicationId}",
                $"{userId}-{applicationId}");

        var roles = await GetValidatedRolesAsync(organisationId, membership.OrganisationRole, applicationId, roleIds, ct);

        var assignment = existingAssignment switch
        {
            not null => ReactivateWith(existingAssignment, roles),
            null => NewAssignment(membership.Id, organisationApplication.Id, roles)
        };

        if (existingAssignment != null)
            assignmentRepository.Update(assignment);
        else
            assignmentRepository.Add(assignment);

        await unitOfWork.SaveChangesAsync(ct);
        await SyncScopesAsync(membership, organisationId, ct);

        logger.LogInformation("User {UserId} assigned to application {ApplicationId} in organisation {OrganisationId}",
            userId, applicationId, organisationId);

        return assignment;
    }

    // ───────────────────────────── Update assignment ─────────────────────────────

    private async Task<UserApplicationAssignment> UpdateAssignmentAsync(
        string userId, int organisationId, int assignmentId,
        IEnumerable<int> roleIds, CancellationToken ct)
    {
        var (membership, assignment) = await GetAssignmentForUserAsync(userId, organisationId, assignmentId, ct);

        if (!assignment.IsActive)
            throw new SystemInvalidOperationException($"Assignment {assignmentId} is not active");

        var roles = await GetValidatedRolesAsync(
            organisationId, membership.OrganisationRole,
            assignment.OrganisationApplication.ApplicationId, roleIds, ct);

        assignmentRepository.Update(WithRoles(assignment, roles));

        await unitOfWork.SaveChangesAsync(ct);
        await SyncScopesAsync(membership, organisationId, ct);

        logger.LogInformation("Assignment {AssignmentId} updated for user {UserId} in organisation {OrganisationId}",
            assignmentId, userId, organisationId);

        return assignment;
    }

    // ───────────────────────────── Revoke assignment ─────────────────────────────

    private async Task RevokeAsync(
        string userId, int organisationId, int assignmentId, CancellationToken ct)
    {
        var (membership, assignment) = await GetAssignmentForUserAsync(userId, organisationId, assignmentId, ct);

        if (assignment.OrganisationApplication.Application.IsEnabledByDefault)
            throw new SystemInvalidOperationException(
                $"Application {assignment.OrganisationApplication.ApplicationId} is enabled by default and user access cannot be revoked");

        assignmentRepository.Update(MarkRevoked(assignment, DateTimeOffset.UtcNow));

        await unitOfWork.SaveChangesAsync(ct);
        await SyncScopesAsync(membership, organisationId, ct);

        logger.LogInformation("Assignment {AssignmentId} revoked for user {UserId} in organisation {OrganisationId}",
            assignmentId, userId, organisationId);
    }

    // ───────────────────────────── Accept invite ─────────────────────────────

    private async Task AcceptAsync(
        Guid cdpOrganisationId, int inviteRoleMappingId,
        AcceptOrganisationInviteRequest request, CancellationToken ct)
    {
        var organisation = await GetOrganisationAsync(cdpOrganisationId, ct);
        var mapping = await GetInviteMappingAsync(inviteRoleMappingId, organisation.Id, ct);

        if (await membershipRepository.GetByUserAndOrganisationAsync(request.UserPrincipalId, mapping.OrganisationId, ct) != null)
            throw new DuplicateEntityException(
                nameof(UserOrganisationMembership),
                nameof(UserOrganisationMembership.UserPrincipalId),
                request.UserPrincipalId);

        var membership = new UserOrganisationMembership
        {
            UserPrincipalId = request.UserPrincipalId,
            CdpPersonId = request.CdpPersonId,
            OrganisationId = mapping.OrganisationId,
            IsActive = true,
            JoinedAt = DateTimeOffset.UtcNow,
            InvitedBy = mapping.CreatedBy
        };
        await roleMappingService.ApplyRoleDefinitionAsync(membership, mapping.OrganisationRole, ct);

        membershipRepository.Add(membership);
        inviteRoleMappingRepository.Remove(mapping);

        // Flush to materialise the membership ID before creating assignments
        await unitOfWork.SaveChangesAsync(ct);

        await AssignDefaultApplicationsAsync(membership, organisation, ct);
        await SyncScopesAsync(membership, organisation, ct);

        logger.LogInformation(
            "Accepted invite {MappingId}, created membership {MembershipId} in organisation {CdpOrganisationId}",
            inviteRoleMappingId, membership.Id, cdpOrganisationId);
    }

    // ───────────────────────────── Default application assignment ─────────────────────────────

    private async Task AssignDefaultApplicationsAsync(
        UserOrganisationMembership membership, Organisation organisation, CancellationToken ct)
    {
        if (!await roleMappingService.ShouldAutoAssignDefaultApplicationsAsync(membership, ct))
            return;

        var defaultOrgApps = (await organisationApplicationRepository
            .GetDefaultEnabledByOrganisationIdAsync(membership.OrganisationId, ct)).ToList();

        if (defaultOrgApps.Count == 0)
            return;

        var partyRoles = await organisationApiAdapter.GetPartyRolesAsync(
            organisation.CdpOrganisationGuid, ct);

        var changes = await defaultOrgApps
            .ToAsyncEnumerable()
            .SelectAwait(async orgApp => await EnsureDefaultAssignedAsync(membership, partyRoles, orgApp, ct))
            .ToListAsync(ct);

        if (changes.Any(changed => changed))
            await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<bool> EnsureDefaultAssignedAsync(
        UserOrganisationMembership membership,
        ISet<CO.CDP.UserManagement.Core.Constants.PartyRole> partyRoles,
        OrganisationApplication orgApp,
        CancellationToken ct)
    {
        var existingAssignment = await assignmentRepository
            .GetByMembershipAndApplicationAsync(membership.Id, orgApp.Id, ct);

        var defaultRoles = DefaultApplicationRoleSelector.SelectFor(
            orgApp,
            await roleRepository.GetByApplicationIdAsync(orgApp.ApplicationId, ct),
            partyRoles,
            await roleMappingService.GetInviteScopesAsync(membership.OrganisationRole, ct));

        return existingAssignment switch
        {
            null => AddDefaultAssignment(membership.Id, orgApp.Id, defaultRoles),
            _ when AssignmentMatchesDesired(existingAssignment, defaultRoles) => false,
            _ => UpdateDefaultAssignment(existingAssignment, defaultRoles)
        };
    }

    private bool AddDefaultAssignment(int membershipId, int orgAppId, IReadOnlyList<ApplicationRole> roles)
    {
        assignmentRepository.Add(new UserApplicationAssignment
        {
            UserOrganisationMembershipId = membershipId,
            OrganisationApplicationId = orgAppId,
            IsActive = true,
            AssignedAt = DateTimeOffset.UtcNow,
            AssignedBy = SystemAssignedBy,
            CreatedBy = SystemAssignedBy,
            Roles = roles.ToList()
        });
        return true;
    }

    private bool UpdateDefaultAssignment(UserApplicationAssignment existing, IReadOnlyList<ApplicationRole> roles)
    {
        existing.IsActive = true;
        existing.IsDeleted = false;
        existing.RevokedAt = null;
        existing.RevokedBy = null;
        existing.DeletedAt = null;
        existing.DeletedBy = null;
        existing.ModifiedBy = SystemAssignedBy;
        WithRoles(existing, roles);
        assignmentRepository.Update(existing);
        return true;
    }

    // ───────────────────────────── OI scope sync ─────────────────────────────

    private async Task SyncScopesAsync(
        UserOrganisationMembership membership, Organisation organisation, CancellationToken ct)
    {
        if (!membership.CdpPersonId.HasValue)
            return;

        var scopes = await roleMappingService.GetOrganisationInformationScopesAsync(membership.Id, ct);
        await organisationPersonSyncRepository.UpsertAsync(
            organisation.CdpOrganisationGuid, membership.CdpPersonId.Value, scopes, ct);
    }

    private async Task SyncScopesAsync(
        UserOrganisationMembership membership, int organisationId, CancellationToken ct) =>
        await SyncScopesAsync(
            membership,
            await organisationRepository.GetByIdAsync(organisationId, ct)
                ?? throw new EntityNotFoundException(nameof(Organisation), organisationId),
            ct);

    // ───────────────────────────── Entity lookups ─────────────────────────────

    private async Task<Organisation> GetOrganisationAsync(Guid cdpOrganisationId, CancellationToken ct) =>
        await organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, ct)
            ?? throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);

    private async Task<UserOrganisationMembership> GetMembershipByPersonIdAsync(
        Guid cdpPersonId, int organisationId, CancellationToken ct) =>
        await membershipRepository.GetByPersonIdAndOrganisationAsync(cdpPersonId, organisationId, ct)
            ?? throw new EntityNotFoundException(nameof(UserOrganisationMembership), cdpPersonId);

    private async Task<UserOrganisationMembership> ResolveMembershipOrThrowAsync(
        string userId, int organisationId, CancellationToken ct) =>
        (Guid.TryParse(userId, out var cdpPersonId)
            ? await membershipRepository.GetByPersonIdAndOrganisationAsync(cdpPersonId, organisationId, ct)
            : await membershipRepository.GetByUserAndOrganisationAsync(userId, organisationId, ct))
        ?? throw new EntityNotFoundException(
            nameof(UserOrganisationMembership),
            $"User {userId} in Organisation {organisationId}");

    private async Task<OrganisationApplication> GetActiveOrganisationAppAsync(
        int organisationId, int applicationId, CancellationToken ct)
    {
        var orgApp = await organisationApplicationRepository
            .GetByOrganisationAndApplicationAsync(organisationId, applicationId, ct)
            ?? throw new EntityNotFoundException(
                nameof(OrganisationApplication),
                $"Organisation {organisationId} and Application {applicationId}");

        return orgApp.IsActive
            ? orgApp
            : throw new SystemInvalidOperationException(
                $"Application {applicationId} is not active for organisation {organisationId}");
    }

    private async Task<InviteRoleMapping> GetInviteMappingAsync(
        int inviteRoleMappingId, int organisationId, CancellationToken ct)
    {
        var mapping = await inviteRoleMappingRepository.GetByIdAsync(inviteRoleMappingId, ct);
        return mapping?.OrganisationId == organisationId
            ? mapping
            : throw new EntityNotFoundException(nameof(InviteRoleMapping), inviteRoleMappingId);
    }

    private async Task<(UserOrganisationMembership Membership, UserApplicationAssignment Assignment)>
        GetAssignmentForUserAsync(string userId, int organisationId, int assignmentId, CancellationToken ct)
    {
        var membership = await ResolveMembershipOrThrowAsync(userId, organisationId, ct);
        var assignment = (await assignmentRepository.GetByMembershipIdAsync(membership.Id, ct))
            .SingleOrDefault(a => a.Id == assignmentId)
            ?? throw new EntityNotFoundException(nameof(UserApplicationAssignment), assignmentId);
        return (membership, assignment);
    }

    // ───────────────────────────── Validation ─────────────────────────────

    private async Task GuardLastOwnerAsync(
        UserOrganisationMembership membership, int organisationId, Guid cdpOrganisationId, CancellationToken ct)
    {
        if (membership.OrganisationRole != OrganisationRole.Owner)
            return;
        if (await membershipRepository.CountActiveOwnersByOrganisationIdAsync(organisationId, ct) > 1)
            return;
        throw new LastOwnerRemovalException(cdpOrganisationId);
    }

    private async Task<IReadOnlyList<ApplicationRole>> GetValidatedRolesAsync(
        int organisationId, OrganisationRole orgRole, int applicationId,
        IEnumerable<int> roleIds, CancellationToken ct)
    {
        var roles = await roleMappingService.GetAssignableRolesAsync(organisationId, orgRole, roleIds, ct);

        var invalid = roles.FirstOrDefault(r => r.ApplicationId != applicationId || !r.IsActive);
        if (invalid != null)
            throw invalid.ApplicationId != applicationId
                ? new SystemInvalidOperationException($"Role {invalid.Id} does not belong to application {applicationId}")
                : new SystemInvalidOperationException($"Role {invalid.Id} is not active");

        return roles;
    }

    // ───────────────────────────── Pure entity transforms ─────────────────────────────

    private static UserOrganisationMembership MarkDeleted(
        UserOrganisationMembership m, DateTimeOffset now, string actingUser)
    {
        m.IsActive = false;
        m.IsDeleted = true;
        m.DeletedAt = now;
        m.DeletedBy = actingUser;
        return m;
    }

    private static UserApplicationAssignment MarkRevoked(
        UserApplicationAssignment a, DateTimeOffset now, string? actingUser = null)
    {
        a.IsActive = false;
        a.RevokedAt = now;
        if (actingUser != null) a.RevokedBy = actingUser;
        return a;
    }

    private static UserApplicationAssignment NewAssignment(
        int membershipId, int orgAppId, IReadOnlyList<ApplicationRole> roles) =>
        new()
        {
            UserOrganisationMembershipId = membershipId,
            OrganisationApplicationId = orgAppId,
            IsActive = true,
            AssignedAt = DateTimeOffset.UtcNow,
            Roles = roles.ToList()
        };

    private static UserApplicationAssignment ReactivateWith(
        UserApplicationAssignment existing, IReadOnlyList<ApplicationRole> roles)
    {
        existing.IsActive = true;
        existing.AssignedAt = DateTimeOffset.UtcNow;
        existing.RevokedAt = null;
        existing.RevokedBy = null;
        return WithRoles(existing, roles);
    }

    private static UserApplicationAssignment WithRoles(
        UserApplicationAssignment assignment, IEnumerable<ApplicationRole> roles)
    {
        assignment.Roles.Clear();
        foreach (var role in roles)
            assignment.Roles.Add(role);
        return assignment;
    }

    private static bool AssignmentMatchesDesired(
        UserApplicationAssignment assignment, IReadOnlyList<ApplicationRole> desiredRoles) =>
        assignment.IsActive &&
        !assignment.IsDeleted &&
        assignment.Roles.Select(r => r.Id).OrderBy(id => id)
            .SequenceEqual(desiredRoles.Select(r => r.Id).OrderBy(id => id));
}
