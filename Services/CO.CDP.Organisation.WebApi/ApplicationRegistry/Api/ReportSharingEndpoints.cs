using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class ReportSharingEndpoints
{
    public static void UseReportSharingEndpoints(this WebApplication app)
    {
        app.MapGet("/api/v1/reports/{reportId:guid}/categories", async (
            Guid reportId,
            ICategoryRepository repo) =>
        {
            var assignments = await repo.GetReportCategoriesAsync(reportId);
            return Results.Ok(assignments.Select(a => new ReportCategoryAssignmentDto(
                a.Id, a.ReportId, a.CategoryId, a.Category.Name, a.AssignedBy, a.AssignedAt)));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Report Sharing");

        app.MapPost("/api/v1/reports/{reportId:guid}/categories", async (
            Guid reportId,
            AssignReportToCategory command,
            ICategoryRepository repo) =>
        {
            var assignment = new ReportCategoryAssignment
            {
                ReportId = reportId,
                CategoryId = command.CategoryId,
                AssignedBy = Guid.Empty,
                AssignedAt = DateTimeOffset.UtcNow
            };

            await repo.AssignReportToCategoryAsync(assignment);
            return Results.Created();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Report Sharing");

        app.MapDelete("/api/v1/reports/{reportId:guid}/categories/{categoryId:guid}", async (
            Guid reportId,
            Guid categoryId,
            ICategoryRepository repo) =>
        {
            await repo.RemoveReportFromCategoryAsync(reportId, categoryId);
            return Results.NoContent();
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Report Sharing");
    }
}
