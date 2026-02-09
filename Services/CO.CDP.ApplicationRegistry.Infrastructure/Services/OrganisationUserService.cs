using CO.CDP.ApplicationRegistry.Core.Entities;
using CO.CDP.ApplicationRegistry.Core.Exceptions;
using CO.CDP.ApplicationRegistry.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Services;

/// <summary>
/// Service for managing organisation user memberships.
/// </summary>
public class OrganisationUserService : IOrganisationUserService
{
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IUserOrganisationMembershipRepository _membershipRepository;
    private readonly IUserApplicationAssignmentRepository _assignmentRepository;
    private readonly ILogger<OrganisationUserService> _logger;

    public OrganisationUserService(
        IOrganisationRepository organisationRepository,
        IUserOrganisationMembershipRepository membershipRepository,
        IUserApplicationAssignmentRepository assignmentRepository,
        ILogger<OrganisationUserService> logger)
    {
        _organisationRepository = organisationRepository;
        _membershipRepository = membershipRepository;
        _assignmentRepository = assignmentRepository;
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

        foreach (var membership in memberships)
        {
            var assignments = await _assignmentRepository.GetByMembershipIdAsync(membership.Id, cancellationToken);
            _logger.LogDebug("Membership {MembershipId} has {AssignmentCount} assignments", membership.Id, assignments.Count());
            foreach (var assignment in assignments)
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
}
