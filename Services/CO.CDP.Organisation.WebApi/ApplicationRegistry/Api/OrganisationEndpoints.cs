using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase.Organisation;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class OrganisationEndpoints
{
    public static void UseAppRegistryOrganisationEndpoints(this WebApplication app)
    {
        app.MapGet("/api/organisations", async (
            [FromQuery] string? name,
            [FromQuery] string? type,
            IUseCase<GetOrganisationsQuery, IEnumerable<OrganisationDto>> useCase) =>
        {
            var result = await useCase.Execute(new GetOrganisationsQuery(name, type));
            return Results.Ok(result);
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Organisation");

        app.MapPost("/api/organisations", async (
            CreateOrganisation command,
            IUseCase<CreateOrganisation, OrganisationDto> useCase) =>
        {
            var result = await useCase.Execute(command);
            return Results.Created($"/api/organisations/{result.Id}", result);
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Organisation");

        app.MapGet("/api/organisations/{orgId:guid}", async (
            Guid orgId,
            IUseCase<Guid, OrganisationDto?> useCase) =>
        {
            var result = await useCase.Execute(orgId);
            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .RequireAuthorization(AuthorizationPolicies.OrgMember)
        .WithTags("Organisation");

        app.MapPut("/api/organisations/{orgId:guid}", async (
            Guid orgId,
            UpdateOrganisation command,
            IUseCase<(Guid, UpdateOrganisation), bool> useCase) =>
        {
            await useCase.Execute((orgId, command));
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("Organisation");

        app.MapDelete("/api/organisations/{orgId:guid}", async (
            Guid orgId,
            IUseCase<(Guid, UpdateOrganisation), bool> useCase) =>
        {
            await useCase.Execute((orgId, new UpdateOrganisation(null, null, null, false)));
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Organisation");

        app.MapGet("/api/organisations/slug/{slug}", async (
            string slug,
            IUseCase<string, OrganisationDto?> useCase) =>
        {
            var result = await useCase.Execute(slug);
            return result != null ? Results.Ok(result) : Results.NotFound();
        })
        .WithTags("Organisation");

        // Members
        app.MapGet("/api/organisations/{orgId:guid}/members", async (
            Guid orgId,
            IUseCase<Guid, IEnumerable<MemberDto>> useCase) =>
        {
            var result = await useCase.Execute(orgId);
            return Results.Ok(result);
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("Organisation - Members");

        app.MapPost("/api/organisations/{orgId:guid}/members", async (
            Guid orgId,
            AddMember command,
            IUseCase<(Guid, AddMember), bool> useCase) =>
        {
            await useCase.Execute((orgId, command));
            return Results.Created();
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("Organisation - Members");

        app.MapPut("/api/organisations/{orgId:guid}/members/{userId}", async (
            Guid orgId,
            string userId,
            UpdateMember command,
            IUseCase<(Guid, AddMember), bool> useCase) =>
        {
            // Reuse add member logic to update role
            await useCase.Execute((orgId, new AddMember(userId, command.OrganisationRole)));
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("Organisation - Members");

        app.MapDelete("/api/organisations/{orgId:guid}/members/{userId}", async (
            Guid orgId,
            string userId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IOrganisationRepository repo) =>
        {
            var membership = await repo.GetMemberAsync(orgId, userId);
            if (membership == null)
            {
                return Results.NotFound();
            }
            membership.IsActive = false;
            await repo.UpdateMemberAsync(membership);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("Organisation - Members");

        // Organisation Applications
        app.MapGet("/api/organisations/{orgId:guid}/applications", async (
            Guid orgId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IOrganisationRepository repo) =>
        {
            var apps = await repo.GetOrganisationApplicationsAsync(orgId);
            return Results.Ok(apps.Select(oa => new ApplicationDto(
                oa.Application.Id,
                oa.Application.Name,
                oa.Application.ClientId,
                oa.Application.Description,
                oa.Application.Category,
                oa.Application.IsActive,
                oa.Application.CreatedOn,
                oa.Application.UpdatedOn)));
        })
        .RequireAuthorization(AuthorizationPolicies.OrgMember)
        .WithTags("Organisation - Applications");

        app.MapPost("/api/organisations/{orgId:guid}/applications/{appId:guid}", async (
            Guid orgId,
            Guid appId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IOrganisationRepository orgRepo,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IApplicationRepository  appRepo,
            Microsoft.AspNetCore.Http.IHttpContextAccessor http) =>
        {
            var org = await orgRepo.GetByIdAsync(orgId);
            if (org == null) return Results.NotFound();

            var application = await appRepo.GetByIdAsync(appId);
            if (application == null || !application.IsActive) return Results.NotFound();

            var existing = await orgRepo.GetOrganisationApplicationsAsync(orgId);
            if (existing.Any(oa => oa.ApplicationId == appId)) return Results.Conflict();

            var enabledBy = http.HttpContext?.User?.FindFirst("sub")?.Value ?? "system";
            await orgRepo.EnableApplicationAsync(new CO.CDP.ApplicationRegistry.Persistence.Entities.OrganisationApplication
            {
                OrganisationId = orgId,
                ApplicationId  = appId,
                EnabledAt      = DateTimeOffset.UtcNow,
                EnabledBy      = enabledBy
            });

            return Results.Created($"/api/organisations/{orgId}/applications", null);
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Organisation - Applications");

        app.MapDelete("/api/organisations/{orgId:guid}/applications/{appId:guid}", async (
            Guid orgId,
            Guid appId,
            CO.CDP.ApplicationRegistry.Persistence.Repositories.IOrganisationRepository orgRepo) =>
        {
            var existing = await orgRepo.GetOrganisationApplicationsAsync(orgId);
            if (!existing.Any(oa => oa.ApplicationId == appId)) return Results.NotFound();

            await orgRepo.DisableApplicationAsync(orgId, appId);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Organisation - Applications");
    }
}
