using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        .WithTags("Application");

        app.MapPost("/api/applications", async (
            CreateApplication command,
            IUseCase<CreateApplication, ApplicationDto> useCase) =>
        {
            var result = await useCase.Execute(command);
            return Results.Created($"/api/applications/{result.Id}", result);
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application");

        app.MapGet("/api/applications/{appId:guid}", async (
            Guid appId,
            IUseCase<Guid, ApplicationDto?> useCase) =>
        {
            var result = await useCase.Execute(appId);
            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .WithTags("Application");

        app.MapPut("/api/applications/{appId:guid}", async (
            Guid appId,
            UpdateApplication command,
            IUseCase<(Guid, UpdateApplication), bool> useCase) =>
        {
            await useCase.Execute((appId, command));
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application");

        app.MapDelete("/api/applications/{appId:guid}", async (
            Guid appId,
            IUseCase<(Guid, UpdateApplication), bool> useCase) =>
        {
            await useCase.Execute((appId, new UpdateApplication(null, null, null, false)));
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application");

        // Permissions
        app.MapGet("/api/applications/{appId:guid}/permissions", async (
            Guid appId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            var perms = await repo.GetPermissionsAsync(appId);
            return Results.Ok(perms.Select(p => new PermissionDto(p.Id, p.ApplicationId, p.Name, p.Description)));
        })
        .WithTags("Application - Permissions");

        app.MapPost("/api/applications/{appId:guid}/permissions", async (
            Guid appId,
            CreatePermission command,
            IUseCase<(Guid, CreatePermission), PermissionDto> useCase) =>
        {
            var result = await useCase.Execute((appId, command));
            return Results.Created($"/api/applications/{appId}/permissions/{result.Id}", result);
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application - Permissions");

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
        .WithTags("Application - Permissions");

        app.MapDelete("/api/applications/{appId:guid}/permissions/{permId:guid}", async (
            Guid appId,
            Guid permId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            await repo.DeletePermissionAsync(permId);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application - Permissions");

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
        .WithTags("Application - Roles");

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
        .WithTags("Application - Roles");

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
        .WithTags("Application - Roles");

        app.MapDelete("/api/applications/{appId:guid}/roles/{roleId:guid}", async (
            Guid appId,
            Guid roleId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository repo) =>
        {
            await repo.DeleteRoleAsync(roleId);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Application - Roles");

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
        .WithTags("Application - Roles");
    }
}
