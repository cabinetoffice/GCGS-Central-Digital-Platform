using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using CO.CDP.ApplicationRegistry.Persistence.Entities;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class UserAssignmentEndpoints
{
    public static void UseUserAssignmentEndpoints(this WebApplication app)
    {
        app.MapGet("/api/organisations/{orgId:guid}/applications/{appId:guid}/users", async (
            Guid orgId,
            Guid appId,
            IUserAssignmentRepository repo) =>
        {
            var assignments = await repo.GetAssignmentsAsync(orgId, appId);
            return Results.Ok(assignments.Select(a => new UserAssignmentDto(
                a.Id, a.UserPrincipalId, a.ApplicationId, a.OrganisationId,
                a.AssignedAt, a.AssignedBy, a.IsActive,
                a.RoleAssignments.Select(ra => new RoleDto(
                    ra.Role.Id, ra.Role.ApplicationId, ra.Role.Name,
                    ra.Role.Description, ra.Role.IsActive,
                    ra.Role.RolePermissions.Select(rp => new PermissionDto(
                        rp.Permission.Id, rp.Permission.ApplicationId,
                        rp.Permission.Name, rp.Permission.Description)))))));
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("User Assignment");

        app.MapPost("/api/organisations/{orgId:guid}/applications/{appId:guid}/users", async (
            Guid orgId,
            Guid appId,
            CreateUserAssignment command,
            IUserAssignmentRepository repo) =>
        {
            var assignment = new UserApplicationAssignment
            {
                UserPrincipalId = command.UserPrincipalId,
                ApplicationId = appId,
                OrganisationId = orgId,
                AssignedBy = "system",
                AssignedAt = DateTimeOffset.UtcNow
            };

            if (command.RoleIds != null)
            {
                foreach (var roleId in command.RoleIds)
                {
                    assignment.RoleAssignments.Add(new UserRoleAssignment
                    {
                        RoleId = roleId
                    });
                }
            }

            var created = await repo.CreateAssignmentAsync(assignment);
            return Results.Created($"/api/organisations/{orgId}/applications/{appId}/users/{command.UserPrincipalId}", created);
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("User Assignment");

        app.MapPut("/api/organisations/{orgId:guid}/applications/{appId:guid}/users/{userId}", async (
            Guid orgId,
            Guid appId,
            string userId,
            UpdateUserAssignment command,
            IUserAssignmentRepository repo) =>
        {
            var assignment = await repo.GetAssignmentAsync(orgId, appId, userId);
            if (assignment == null) return Results.NotFound();

            assignment.RoleAssignments.Clear();
            foreach (var roleId in command.RoleIds)
            {
                assignment.RoleAssignments.Add(new UserRoleAssignment
                {
                    UserApplicationAssignmentId = assignment.Id,
                    RoleId = roleId
                });
            }

            await repo.UpdateAssignmentAsync(assignment);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("User Assignment");

        app.MapDelete("/api/organisations/{orgId:guid}/applications/{appId:guid}/users/{userId}", async (
            Guid orgId,
            Guid appId,
            string userId,
            IUserAssignmentRepository repo) =>
        {
            await repo.RevokeAssignmentAsync(orgId, appId, userId);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("User Assignment");
    }
}
