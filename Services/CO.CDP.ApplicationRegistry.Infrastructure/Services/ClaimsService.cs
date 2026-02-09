using CO.CDP.ApplicationRegistry.Core.Interfaces;
using CO.CDP.ApplicationRegistry.Core.Models;

namespace CO.CDP.ApplicationRegistry.Infrastructure.Services;

/// <summary>
/// Service for resolving user claims across all organisations and applications.
/// </summary>
public class ClaimsService : IClaimsService
{
    private readonly IUserApplicationAssignmentRepository _assignmentRepository;
    private readonly IUserOrganisationMembershipRepository _membershipRepository;

    public ClaimsService(
        IUserApplicationAssignmentRepository assignmentRepository,
        IUserOrganisationMembershipRepository membershipRepository)
    {
        _assignmentRepository = assignmentRepository;
        _membershipRepository = membershipRepository;
    }

    public async Task<UserClaims> GetUserClaimsAsync(string userPrincipalId, CancellationToken cancellationToken = default)
    {
        // Get all memberships
        var memberships = await _membershipRepository.GetByUserPrincipalIdAsync(userPrincipalId, cancellationToken);

        // Get all assignments with full details
        var assignments = await _assignmentRepository.GetAssignmentsForClaimsAsync(userPrincipalId, cancellationToken);

        // Group assignments by organisation
        var assignmentsByOrg = assignments
            .GroupBy(a => a.UserOrganisationMembership.OrganisationId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var organisationMembershipClaims = new List<OrganisationMembershipClaim>();

        foreach (var membership in memberships)
        {
            var applicationAssignmentClaims = new List<ApplicationAssignmentClaim>();

            if (assignmentsByOrg.TryGetValue(membership.OrganisationId, out var orgAssignments))
            {
                foreach (var assignment in orgAssignments)
                {
                    var roles = assignment.Roles.Select(r => r.Name).ToList();
                    var permissions = assignment.Roles
                        .SelectMany(r => r.Permissions)
                        .Select(p => p.Name)
                        .Distinct()
                        .ToList();

                    applicationAssignmentClaims.Add(new ApplicationAssignmentClaim
                    {
                        ApplicationId = assignment.OrganisationApplication.Application.Id,
                        ApplicationName = assignment.OrganisationApplication.Application.Name,
                        ClientId = assignment.OrganisationApplication.Application.ClientId,
                        Roles = roles,
                        Permissions = permissions
                    });
                }
            }

            organisationMembershipClaims.Add(new OrganisationMembershipClaim
            {
                OrganisationId = membership.Organisation.Id,
                OrganisationName = membership.Organisation.Name,
                OrganisationSlug = membership.Organisation.Slug,
                OrganisationRole = membership.OrganisationRole.ToString(),
                CdpPersonId = membership.CdpPersonId,
                ApplicationAssignments = applicationAssignmentClaims
            });
        }

        return new UserClaims
        {
            UserPrincipalId = userPrincipalId,
            OrganisationMemberships = organisationMembershipClaims
        };
    }
}
