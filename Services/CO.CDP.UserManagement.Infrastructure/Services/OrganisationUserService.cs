using CO.CDP.UserManagement.Core.Entities;
using CO.CDP.UserManagement.Core.Exceptions;
using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace CO.CDP.UserManagement.Infrastructure.Services;

/// <summary>
/// Service for managing organisation user memberships.
/// </summary>
public class OrganisationUserService : IOrganisationUserService
{
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IUserOrganisationMembershipRepository _membershipRepository;
    private readonly IUserApplicationAssignmentRepository _assignmentRepository;
    private readonly ICdpMembershipSyncService _membershipSyncService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrganisationUserService> _logger;

    public OrganisationUserService(
        IOrganisationRepository organisationRepository,
        IUserOrganisationMembershipRepository membershipRepository,
        IUserApplicationAssignmentRepository assignmentRepository,
        ICdpMembershipSyncService membershipSyncService,
        IUnitOfWork unitOfWork,
        ILogger<OrganisationUserService> logger)
    {
        _organisationRepository = organisationRepository;
        _membershipRepository = membershipRepository;
        _assignmentRepository = assignmentRepository;
        _membershipSyncService = membershipSyncService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<UserOrganisationMembership>> GetOrganisationUsersAsync(
        Guid cdpOrganisationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting users for CDP organisation ID: {CdpOrganisationId}", cdpOrganisationId);

        var organisation = await _organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);
        }

        _logger.LogDebug("Resolved organisation {OrganisationId} for CDP organisation ID: {CdpOrganisationId}", organisation.Id, cdpOrganisationId);
        var memberships = (await _membershipRepository.GetByOrganisationIdAsync(organisation.Id, cancellationToken)).ToList();
        _logger.LogDebug("Retrieved {MembershipCount} memberships for organisation {OrganisationId}", memberships.Count, organisation.Id);

        var membershipIds = memberships.Select(membership => membership.Id).ToArray();
        var assignments = await _assignmentRepository.GetByMembershipIdsAsync(membershipIds, cancellationToken);
        var assignmentsByMembership = assignments.ToLookup(assignment => assignment.UserOrganisationMembershipId);

        foreach (var membership in memberships)
        {
            var membershipAssignments = assignmentsByMembership[membership.Id].ToArray();
            _logger.LogDebug("Membership {MembershipId} has {AssignmentCount} assignments", membership.Id, membershipAssignments.Length);
            foreach (var assignment in membershipAssignments)
            {
                membership.ApplicationAssignments.Add(assignment);
            }
        }

        return memberships;
    }

    public async Task<UserOrganisationMembership?> GetOrganisationUserAsync(
        Guid cdpOrganisationId,
        string userPrincipalId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting membership for user {UserPrincipalId} in CDP organisation ID: {CdpOrganisationId}",
            userPrincipalId, cdpOrganisationId);

        var organisation = await _organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);
        }

        var membership = await _membershipRepository.GetByUserAndOrganisationAsync(
            userPrincipalId, organisation.Id, cancellationToken);

        if (membership == null)
        {
            return null;
        }

        var assignments = await _assignmentRepository.GetByMembershipIdAsync(membership.Id, cancellationToken);
        foreach (var assignment in assignments)
        {
            membership.ApplicationAssignments.Add(assignment);
        }

        return membership;
    }

    public async Task<UserOrganisationMembership?> GetOrganisationUserByPersonIdAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting membership for CDP person {CdpPersonId} in CDP organisation ID: {CdpOrganisationId}",
            cdpPersonId, cdpOrganisationId);

        var organisation = await _organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);
        }

        var membership = await _membershipRepository.GetByPersonIdAndOrganisationAsync(
            cdpPersonId, organisation.Id, cancellationToken);

        if (membership == null)
        {
            return null;
        }

        var assignments = await _assignmentRepository.GetByMembershipIdAsync(membership.Id, cancellationToken);
        foreach (var assignment in assignments)
        {
            membership.ApplicationAssignments.Add(assignment);
        }

        return membership;
    }

    public async Task<UserOrganisationMembership> UpdateOrganisationRoleAsync(
        Guid cdpOrganisationId,
        Guid cdpPersonId,
        OrganisationRole organisationRole,
        CancellationToken cancellationToken = default)
    {
        var organisation = await _organisationRepository.GetByCdpGuidAsync(cdpOrganisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), cdpOrganisationId);
        }

        var membership = await _membershipRepository.GetByPersonIdAndOrganisationAsync(
            cdpPersonId,
            organisation.Id,
            cancellationToken);
        if (membership == null)
        {
            throw new EntityNotFoundException(nameof(UserOrganisationMembership), cdpPersonId);
        }

        membership.OrganisationRole = organisationRole;
        _membershipRepository.Update(membership);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _membershipSyncService.SyncMembershipRoleChangedAsync(membership, cancellationToken);

        return membership;
    }
}
