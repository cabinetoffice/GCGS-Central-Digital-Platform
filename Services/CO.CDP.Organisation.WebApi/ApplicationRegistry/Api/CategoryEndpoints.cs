using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class CategoryEndpoints
{
    public static void UseCategoryEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/categories", async (ICategoryRepository repo) =>
        {
            var categories = await repo.GetAllAsync();
            return Results.Ok(categories.Select(c => new CategoryDto(
                c.Id, c.Name, c.Description, c.TaxonomyType, c.IsActive)));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Categories");

        app.MapPost("/api/v1/categories", async (
            CreateCategory command,
            ICategoryRepository repo) =>
        {
            var category = new ReportCategory
            {
                Name = command.Name,
                Description = command.Description,
                TaxonomyType = command.TaxonomyType
            };

            var created = await repo.CreateAsync(category);
            return Results.Created($"/api/v1/categories/{created.Id}", new CategoryDto(
                created.Id, created.Name, created.Description, created.TaxonomyType, created.IsActive));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Categories");

        app.MapPut("/api/v1/categories/{categoryId:guid}", async (
            Guid categoryId,
            UpdateCategory command,
            ICategoryRepository repo) =>
        {
            var category = await repo.GetByIdAsync(categoryId);
            if (category == null) return Results.NotFound();

            if (command.Name != null) category.Name = command.Name;
            if (command.Description != null) category.Description = command.Description;
            if (command.TaxonomyType != null) category.TaxonomyType = command.TaxonomyType;
            if (command.IsActive.HasValue) category.IsActive = command.IsActive.Value;

            await repo.UpdateAsync(category);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Categories");

        app.MapDelete("/api/v1/categories/{categoryId:guid}", async (
            Guid categoryId,
            ICategoryRepository repo) =>
        {
            var category = await repo.GetByIdAsync(categoryId);
            if (category == null) return Results.NotFound();

            category.IsActive = false;
            await repo.UpdateAsync(category);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Categories");

        app.MapGet("/api/v1/categories/{categoryId:guid}/permissions", async (
            Guid categoryId,
            ICategoryRepository repo) =>
        {
            var perms = await repo.GetPermissionsAsync(categoryId);
            return Results.Ok(perms.Select(p => new CategoryPermissionDto(
                p.Id, p.OrganisationTypeId, p.PermissionLevel, p.GrantedBy, p.GrantedAt)));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Categories");

        app.MapPut("/api/v1/categories/{categoryId:guid}/permissions", async (
            Guid categoryId,
            SetCategoryPermissions command,
            ICategoryRepository repo) =>
        {
            var permissions = command.Permissions.Select(p => new CategoryPermission
            {
                CategoryId = categoryId,
                OrganisationTypeId = p.OrganisationTypeId,
                PermissionLevel = p.PermissionLevel,
                GrantedBy = Guid.Empty,
                GrantedAt = DateTimeOffset.UtcNow
            }).ToList();

            await repo.SetPermissionsAsync(categoryId, permissions);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Categories");

        app.MapDelete("/api/v1/categories/{categoryId:guid}/permissions", async (
            Guid categoryId,
            ICategoryRepository repo) =>
        {
            await repo.ClearPermissionsAsync(categoryId);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Categories");
    }
}
