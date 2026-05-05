using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class ProfileEndpoints
{
    public static void UseProfileEndpoints(this WebApplication app)
    {
        app.MapGet("/api/profile", async (HttpContext context, IOrganisationRepository orgRepo) =>
        {
            var userId = context.User.FindFirst("sub")?.Value ?? "anonymous";
            var allOrgs = await orgRepo.GetAllAsync();
            var myOrgs = new List<ProfileOrganisation>();

            foreach (var org in allOrgs)
            {
                var member = await orgRepo.GetMemberAsync(org.Id, userId);
                if (member != null)
                {
                    myOrgs.Add(new ProfileOrganisation(org.Id, org.Name, member.OrganisationRole));
                }
            }

            return Results.Ok(new ProfileDto(userId, myOrgs));
        })
        .RequireAuthorization()
        .WithTags("Profile");

        app.MapGet("/api/profile/organisations", async (HttpContext context, IOrganisationRepository orgRepo) =>
        {
            var userId = context.User.FindFirst("sub")?.Value ?? "anonymous";
            var allOrgs = await orgRepo.GetAllAsync();
            var myOrgs = new List<ProfileOrganisation>();

            foreach (var org in allOrgs)
            {
                var member = await orgRepo.GetMemberAsync(org.Id, userId);
                if (member != null)
                {
                    myOrgs.Add(new ProfileOrganisation(org.Id, org.Name, member.OrganisationRole));
                }
            }

            return Results.Ok(myOrgs);
        })
        .RequireAuthorization()
        .WithTags("Profile");

        app.MapGet("/api/profile/organisations/{orgId:guid}/permissions", async (
            Guid orgId,
            HttpContext context,
            IOrganisationRepository orgRepo,
            IUserAssignmentRepository assignmentRepo) =>
        {
            var userId = context.User.FindFirst("sub")?.Value ?? "anonymous";
            var orgApps = await orgRepo.GetOrganisationApplicationsAsync(orgId);
            var appClaims = new List<ApplicationClaims>();

            foreach (var orgApp in orgApps)
            {
                var assignment = await assignmentRepo.GetAssignmentAsync(orgId, orgApp.ApplicationId, userId);
                if (assignment == null) continue;

                var roles = assignment.RoleAssignments.Select(ra => ra.Role.Name).ToList();
                var permissions = assignment.RoleAssignments
                    .SelectMany(ra => ra.Role.RolePermissions)
                    .Select(rp => rp.Permission.Name)
                    .Distinct()
                    .ToList();

                appClaims.Add(new ApplicationClaims(
                    orgApp.ApplicationId, orgApp.Application.Name, roles, permissions));
            }

            return Results.Ok(new ProfilePermissions(orgId, appClaims));
        })
        .RequireAuthorization()
        .WithTags("Profile");
    }
}
