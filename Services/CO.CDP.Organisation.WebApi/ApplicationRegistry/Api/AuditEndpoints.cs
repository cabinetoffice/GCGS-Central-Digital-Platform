using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Audit;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class AuditEndpoints
{
    public static void UseAuditEndpoints(this WebApplication app)
    {
        app.MapGet("/api/audit", async (
            [FromQuery] string? entityType,
            [FromQuery] string? action,
            [FromQuery] string? userId,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromQuery] int limit,
            [FromQuery] int offset,
            IUseCase<AuditQuery, IEnumerable<AuditLogDto>> useCase) =>
        {
            var effectiveLimit = limit > 0 ? limit : 100;
            var effectiveOffset = offset > 0 ? offset : 0;

            var result = await useCase.Execute(new AuditQuery(
                entityType, action, userId, from, to, effectiveLimit, effectiveOffset));
            return Results.Ok(result);
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Audit");
    }
}
