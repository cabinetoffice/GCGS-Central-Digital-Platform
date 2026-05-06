using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Claims;

public class GetClaimsTreeUseCase : IUseCase<string, ClaimsTree>
{
    private readonly IOrganisationRepository _organisationRepository;
    private readonly IUserAssignmentRepository _userAssignmentRepository;

    public GetClaimsTreeUseCase(
        IOrganisationRepository organisationRepository,
        IUserAssignmentRepository userAssignmentRepository)
    {
        _organisationRepository = organisationRepository;
        _userAssignmentRepository = userAssignmentRepository;
    }

    public async Task<ClaimsTree> Execute(string userPrincipalId)
    {
        var allOrgs = await _organisationRepository.GetAllAsync();
        var orgClaims = new List<OrganisationClaims>();

        foreach (var org in allOrgs)
        {
            var member = await _organisationRepository.GetMemberAsync(org.Id, userPrincipalId);
            if (member == null) continue;

            var orgApps = await _organisationRepository.GetOrganisationApplicationsAsync(org.Id);
            var appClaims = new List<ApplicationClaims>();

            foreach (var orgApp in orgApps)
            {
                var assignment = await _userAssignmentRepository.GetAssignmentAsync(
                    org.Id, orgApp.ApplicationId, userPrincipalId);

                if (assignment == null) continue;

                var roles = assignment.RoleAssignments.Select(ra => ra.Role.Name).ToList();
                var permissions = assignment.RoleAssignments
                    .SelectMany(ra => ra.Role.RolePermissions)
                    .Select(rp => rp.Permission.Name)
                    .Distinct()
                    .ToList();

                appClaims.Add(new ApplicationClaims(
                    orgApp.ApplicationId,
                    orgApp.Application.Name,
                    roles,
                    permissions));
            }

            orgClaims.Add(new OrganisationClaims(
                org.Id,
                org.Name,
                member.OrganisationRole,
                appClaims));
        }

        return new ClaimsTree(userPrincipalId, orgClaims);
    }
}
