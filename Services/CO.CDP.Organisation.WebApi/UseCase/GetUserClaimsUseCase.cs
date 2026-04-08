using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.OrganisationInformation.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Organisation.WebApi.UseCase;

public class GetUserClaimsUseCase(OrganisationInformationContext db)
    : IUseCase<string, UserClaimsResponse?>
{
    public async Task<UserClaimsResponse?> Execute(string userUrn)
    {
        var person = await db.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserUrn == userUrn);

        if (person == null) return null;

        // Get all organisations the person belongs to, with their scopes
        var orgPersons = await db.Set<OrganisationPerson>()
            .AsNoTracking()
            .Include(op => op.Organisation)
            .Where(op => op.PersonId == person.Id)
            .ToListAsync();

        var orgIds = orgPersons.Select(op => op.OrganisationId).ToList();

        // Get all active application assignments for this person across their organisations
        var assignments = await db.UserApplicationAssignments
            .AsNoTracking()
            .Include(a => a.OrganisationApplication!)
                .ThenInclude(oa => oa.Application!)
            .Include(a => a.Roles)
                .ThenInclude(r => r.Permissions)
            .Where(a => a.PersonId == person.Id
                && a.IsActive
                && a.OrganisationApplication!.IsActive
                && orgIds.Contains(a.OrganisationApplication.OrganisationId))
            .ToListAsync();

        // Group assignments by organisation
        var assignmentsByOrg = assignments
            .GroupBy(a => a.OrganisationApplication!.OrganisationId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var organisations = orgPersons.Select(op => new OrganisationMembershipClaim
        {
            OrganisationId = op.Organisation!.Guid,
            OrganisationName = op.Organisation.Name,
            OrganisationRole = op.Scopes.FirstOrDefault() ?? "",
            Applications = assignmentsByOrg.TryGetValue(op.OrganisationId, out var orgAssignments)
                ? orgAssignments.Select(a => new ApplicationAssignmentClaim
                {
                    ApplicationId = a.OrganisationApplication!.Application!.Guid,
                    ApplicationName = a.OrganisationApplication.Application.Name,
                    ClientId = a.OrganisationApplication.Application.ClientId,
                    Roles = a.Roles.Select(r => r.Name).ToList(),
                    Permissions = a.Roles
                        .SelectMany(r => r.Permissions)
                        .Select(p => p.Name)
                        .Distinct()
                        .ToList()
                }).ToList()
                : []
        }).ToList();

        return new UserClaimsResponse
        {
            UserPrincipalId = userUrn,
            Organisations = organisations
        };
    }
}
