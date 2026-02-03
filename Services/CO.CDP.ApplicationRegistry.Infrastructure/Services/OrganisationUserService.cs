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
        int organisationId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting users for organisation ID: {OrganisationId}", organisationId);

        var organisation = await _organisationRepository.GetByIdAsync(organisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), organisationId);
        }

        var memberships = (await _membershipRepository.GetByOrganisationIdAsync(organisationId, cancellationToken)).ToList();

        foreach (var membership in memberships)
        {
            var assignments = await _assignmentRepository.GetByMembershipIdAsync(membership.Id, cancellationToken);
            foreach (var assignment in assignments)
            {
                membership.ApplicationAssignments.Add(assignment);
            }
        }

        return memberships;
    }

    public async Task<UserOrganisationMembership?> GetOrganisationUserAsync(
        int organisationId,
        string userPrincipalId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting membership for user {UserPrincipalId} in organisation ID: {OrganisationId}",
            userPrincipalId, organisationId);

        var organisation = await _organisationRepository.GetByIdAsync(organisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), organisationId);
        }

        var membership = await _membershipRepository.GetByUserAndOrganisationAsync(
            userPrincipalId, organisationId, cancellationToken);

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
        int organisationId,
        Guid cdpPersonId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Getting membership for CDP person {CdpPersonId} in organisation ID: {OrganisationId}",
            cdpPersonId, organisationId);

        var organisation = await _organisationRepository.GetByIdAsync(organisationId, cancellationToken);
        if (organisation == null)
        {
            throw new EntityNotFoundException(nameof(Organisation), organisationId);
        }

        var membership = await _membershipRepository.GetByPersonIdAndOrganisationAsync(
            cdpPersonId, organisationId, cancellationToken);

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
