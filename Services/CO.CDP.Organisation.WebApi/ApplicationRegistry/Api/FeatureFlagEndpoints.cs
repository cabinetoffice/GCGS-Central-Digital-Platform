using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class FeatureFlagEndpoints
{
    public static void UseFeatureFlagEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/feature-flags/{tileId}", async (
            string tileId,
            IFeatureFlagRepository repo) =>
        {
            var flag = await repo.GetByTileIdAsync(tileId);
            if (flag == null) return Results.NotFound();

            return Results.Ok(new FeatureFlagDto(
                flag.TileId, flag.Enabled, flag.Reason, flag.UpdatedBy, flag.UpdatedAt));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Feature Flags");

        app.MapPut("/api/v1/feature-flags/{tileId}", async (
            string tileId,
            UpdateFeatureFlag command,
            IFeatureFlagRepository repo) =>
        {
            var flag = new FeatureFlag
            {
                TileId = tileId,
                Enabled = command.Enabled,
                Reason = command.Reason,
                UpdatedBy = Guid.Empty // Would be resolved from JWT in production
            };

            var result = await repo.UpsertAsync(flag);
            return Results.Ok(new FeatureFlagDto(
                result.TileId, result.Enabled, result.Reason, result.UpdatedBy, result.UpdatedAt));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Feature Flags");

        app.MapGet("/api/v1/feature-flags/{tileId}/scope", async (
            string tileId,
            IFeatureFlagRepository repo) =>
        {
            var scopes = await repo.GetScopesAsync(tileId);
            return Results.Ok(new FeatureFlagScopeDto(scopes.Select(s => s.OrganisationTypeId)));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Feature Flags");

        app.MapPut("/api/v1/feature-flags/{tileId}/scope", async (
            string tileId,
            FeatureFlagScopeDto command,
            IFeatureFlagRepository repo) =>
        {
            await repo.SetScopesAsync(tileId, command.OrganisationTypeIds, Guid.Empty);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Feature Flags");

        app.MapDelete("/api/v1/feature-flags/{tileId}/scope", async (
            string tileId,
            IFeatureFlagRepository repo) =>
        {
            await repo.ClearScopesAsync(tileId);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Feature Flags");
    }
}
