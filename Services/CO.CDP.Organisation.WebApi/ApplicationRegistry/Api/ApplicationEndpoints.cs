using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class ApplicationEndpoints
{
    public static void UseApplicationEndpoints(this WebApplication app)
    {
        app.MapGet("/api/applications", async (
            IUseCase<bool, IEnumerable<ApplicationDto>> useCase) =>
        {
            var result = await useCase.Execute(true);
            return Results.Ok(result);
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application")
        .WithSummary("[Platform Admin] List all applications")
        .WithDescription("Returns all registered applications in the Application Registry. Requires Platform Admin credentials (client_credentials grant with the platform_admin scope).");

        app.MapPost("/api/applications", async (
            CreateApplication command,
            IUseCase<CreateApplication, ApplicationDto> useCase) =>
        {
            try
            {
                var result = await useCase.Execute(command);
                return Results.Created($"/api/applications/{result.Id}", result);
            }
            catch (MongoWriteException ex) when (ex.WriteError.Code == 11000)
            {
                return Results.Conflict(new
                {
                    error = "An application with this ClientId already exists."
                });
            }
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application")
        .WithSummary("[Platform Admin] Register a new application")
        .WithDescription("Creates a new application entry in the Application Registry. The application record defines the client system and is the root for permissions and roles. Requires Platform Admin credentials.");

        app.MapGet("/api/applications/{appId:guid}", async (
            Guid appId,
            IUseCase<Guid, ApplicationDto?> useCase) =>
        {
            var result = await useCase.Execute(appId);
            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser)
        .WithTags("Application")
        .WithSummary("Get application by ID")
        .WithDescription("Returns the details of a specific application by its unique identifier. Accessible to any authenticated CDP user (required by the role-assignment UI).");

        app.MapPut("/api/applications/{appId:guid}", async (
            Guid appId,
            UpdateApplication command,
            IUseCase<(Guid, UpdateApplication), bool> useCase) =>
        {
            await useCase.Execute((appId, command));
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application")
        .WithSummary("[Platform Admin] Update an application")
        .WithDescription("Updates the name, description, or active status of an existing application. Requires Platform Admin credentials.");

        app.MapDelete("/api/applications/{appId:guid}", async (
            Guid appId,
            IUseCase<(Guid, UpdateApplication), bool> useCase) =>
        {
            await useCase.Execute((appId, new UpdateApplication(null, null, null, false)));
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application")
        .WithSummary("[Platform Admin] Deactivate an application")
        .WithDescription("Soft-deletes (deactivates) an application by setting its active flag to false. The record is retained for audit purposes. Requires Platform Admin credentials.");

        // Permissions
        app.MapGet("/api/applications/{appId:guid}/permissions", async (
            Guid appId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            var perms = await repo.GetPermissionsAsync(appId);
            return Results.Ok(perms.Select(p => new PermissionDto(p.Id, p.ApplicationId, p.Name, p.Description)));
        })
        .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser)
        .WithTags("Application - Permissions")
        .WithSummary("List permissions for an application")
        .WithDescription("Returns all permissions defined for the specified application. Accessible to any authenticated CDP user (required by the role-assignment UI).");

        app.MapPost("/api/applications/{appId:guid}/permissions", async (
            Guid appId,
            CreatePermission command,
            IUseCase<(Guid, CreatePermission), PermissionDto> useCase) =>
        {
            var result = await useCase.Execute((appId, command));
            return Results.Created($"/api/applications/{appId}/permissions/{result.Id}", result);
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application - Permissions")
        .WithSummary("[Platform Admin] Add a permission to an application")
        .WithDescription("Creates a new fine-grained permission for the specified application. Permissions are subsequently assigned to roles. Requires Platform Admin credentials.");

        app.MapPut("/api/applications/{appId:guid}/permissions/{permId:guid}", async (
            Guid appId,
            Guid permId,
            UpdatePermission command,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            var perm = await repo.GetPermissionByIdAsync(permId);
            if (perm == null) return Results.NotFound();

            if (command.Name != null) perm.Name = command.Name;
            if (command.Description != null) perm.Description = command.Description;
            await repo.UpdatePermissionAsync(perm);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application - Permissions")
        .WithSummary("[Platform Admin] Update a permission")
        .WithDescription("Updates the name or description of an existing permission on the specified application. Requires Platform Admin credentials.");

        app.MapDelete("/api/applications/{appId:guid}/permissions/{permId:guid}", async (
            Guid appId,
            Guid permId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            await repo.DeletePermissionAsync(permId);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application - Permissions")
        .WithSummary("[Platform Admin] Delete a permission")
        .WithDescription("Permanently removes a permission from the specified application. All role-permission associations for this permission are also removed. Requires Platform Admin credentials.");

        // Roles
        app.MapGet("/api/applications/{appId:guid}/roles", async (
            Guid appId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            var roles = await repo.GetRolesAsync(appId);
            return Results.Ok(roles.Select(r => new RoleDto(
                r.Id, r.ApplicationId, r.Name, r.Description, r.IsActive,
                r.RolePermissions.Select(rp => new PermissionDto(
                    rp.Permission.Id, rp.Permission.ApplicationId,
                    rp.Permission.Name, rp.Permission.Description)))));
        })
        .RequireAuthorization(AuthorizationPolicies.AuthenticatedUser)
        .WithTags("Application - Roles")
        .WithSummary("List roles for an application")
        .WithDescription("Returns all roles defined for the specified application, including the permissions assigned to each role. Accessible to any authenticated CDP user (required by the role-assignment UI).");

        app.MapPost("/api/applications/{appId:guid}/roles", async (
            Guid appId,
            CreateRole command,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            var role = new CO.CDP.ApplicationRegistry.Persistence.Entities.ApplicationRole
            {
                ApplicationId = appId,
                Name = command.Name,
                Description = command.Description
            };
            var created = await repo.CreateRoleAsync(role);
            return Results.Created($"/api/applications/{appId}/roles/{created.Id}", new RoleDto(
                created.Id, created.ApplicationId, created.Name, created.Description,
                created.IsActive, Enumerable.Empty<PermissionDto>()));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application - Roles")
        .WithSummary("[Platform Admin] Create a role for an application")
        .WithDescription("Creates a new role for the specified application. Use the set-permissions endpoint to assign permissions to the role after creation. Requires Platform Admin credentials.");

        app.MapPut("/api/applications/{appId:guid}/roles/{roleId:guid}", async (
            Guid appId,
            Guid roleId,
            UpdateRole command,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            var role = await repo.GetRoleByIdAsync(roleId);
            if (role == null) return Results.NotFound();

            if (command.Name != null) role.Name = command.Name;
            if (command.Description != null) role.Description = command.Description;
            if (command.IsActive.HasValue) role.IsActive = command.IsActive.Value;
            await repo.UpdateRoleAsync(role);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application - Roles")
        .WithSummary("[Platform Admin] Update a role")
        .WithDescription("Updates the name, description, or active status of an existing role. Requires Platform Admin credentials.");

        app.MapDelete("/api/applications/{appId:guid}/roles/{roleId:guid}", async (
            Guid appId,
            Guid roleId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            await repo.DeleteRoleAsync(roleId);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application - Roles")
        .WithSummary("[Platform Admin] Delete a role")
        .WithDescription("Permanently removes a role from the specified application. All role-permission associations for this role are also removed. Requires Platform Admin credentials.");

        app.MapPut("/api/applications/{appId:guid}/roles/{roleId:guid}/permissions", async (
            Guid appId,
            Guid roleId,
            SetRolePermissions command,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            await repo.SetRolePermissionsAsync(roleId, command.PermissionIds);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application - Roles")
        .WithSummary("[Platform Admin] Set permissions on a role")
        .WithDescription("Replaces the full set of permissions assigned to a role. Supply the complete desired list of permission IDs; any existing assignments not in the list will be removed. Requires Platform Admin credentials.");
    }
}
