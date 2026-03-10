using CO.CDP.UserManagement.Core.Interfaces;
using CO.CDP.UserManagement.Core.Models;

namespace CO.CDP.UserManagement.Infrastructure.Services;

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
        var memberships = await _membershipRepository.GetByUserPrincipalIdAsync(userPrincipalId, cancellationToken);
        var assignments = await _assignmentRepository.GetAssignmentsForClaimsAsync(userPrincipalId, cancellationToken);
        var assignmentsByOrg = assignments
            .GroupBy(a => a.UserOrganisationMembership.OrganisationId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var organisationMembershipClaims = memberships
            .Select(membership =>
            {
                assignmentsByOrg.TryGetValue(membership.OrganisationId, out var orgAssignments);
                orgAssignments ??= [];

                return new OrganisationMembershipClaim
                {
                    OrganisationId = membership.Organisation.CdpOrganisationGuid,
                    OrganisationName = membership.Organisation.Name,
                    OrganisationRole = membership.OrganisationRole.ToString(),
                    Applications = orgAssignments.Select(assignment => new ApplicationAssignmentClaim
                    {
                        ApplicationId = assignment.OrganisationApplication.Application.Guid,
                        ApplicationName = assignment.OrganisationApplication.Application.Name,
                        ClientId = assignment.OrganisationApplication.Application.ClientId,
                        Roles = assignment.Roles.Select(r => r.Name).ToList(),
                        Permissions = assignment.Roles
                            .SelectMany(r => r.Permissions)
                            .Select(p => p.Name)
                            .Distinct()
                            .ToList()
                    }).ToList()
                };
            })
            .ToList();

        return new UserClaims
        {
            UserPrincipalId = userPrincipalId,
            Organisations = organisationMembershipClaims
        };
    }
}
