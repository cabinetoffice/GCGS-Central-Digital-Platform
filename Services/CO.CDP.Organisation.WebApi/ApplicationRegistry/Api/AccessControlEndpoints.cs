using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class AccessControlEndpoints
{
    public static void UseAccessControlEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/reports/{reportId:guid}/acl", async (
            Guid reportId,
            IAccessControlRepository repo) =>
        {
            var entries = await repo.GetAclEntriesAsync(reportId);
            return Results.Ok(entries.Select(e => new AccessControlEntryDto(
                e.Id, e.ReportId, e.UserPrincipal, e.OrganisationId,
                e.GrantedBy, e.GrantedAt, e.RevokedAt)));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Access Control");

        app.MapPost("/api/v1/reports/{reportId:guid}/acl", async (
            Guid reportId,
            GrantAccess command,
            IAccessControlRepository repo) =>
        {
            var entry = new AccessControlEntry
            {
                ReportId = reportId,
                UserPrincipal = command.UserPrincipal,
                OrganisationId = command.OrganisationId,
                GrantedBy = Guid.Empty,
                GrantedAt = DateTimeOffset.UtcNow
            };

            var created = await repo.GrantAccessAsync(entry);
            return Results.Created($"/api/v1/reports/{reportId}/acl/{created.Id}", new AccessControlEntryDto(
                created.Id, created.ReportId, created.UserPrincipal, created.OrganisationId,
                created.GrantedBy, created.GrantedAt, created.RevokedAt));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Access Control");

        app.MapDelete("/api/v1/reports/{reportId:guid}/acl/{entryId:guid}", async (
            Guid reportId,
            Guid entryId,
            IAccessControlRepository repo) =>
        {
            await repo.RevokeAccessAsync(entryId);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Access Control");

        app.MapGet("/api/v1/organisations/{orgId:guid}/users", async (
            Guid orgId,
            IOrganisationRepository orgRepo) =>
        {
            var members = await orgRepo.GetMembersAsync(orgId);
            return Results.Ok(members.Select(m => new { m.UserPrincipalId, m.OrganisationRole }));
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("Access Control");

        app.MapGet("/api/v1/reports", () =>
        {
            return Results.Ok(new List<object>());
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Access Control");
    }
}
