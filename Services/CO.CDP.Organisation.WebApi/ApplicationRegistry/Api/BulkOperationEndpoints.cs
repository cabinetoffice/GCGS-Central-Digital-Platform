using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Model;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.UseCase;
using CO.CDP.Organisation.WebApi.ApplicationRegistry.Authorization;
using CO.CDP.ApplicationRegistry.Persistence.Repositories;
using CO.CDP.ApplicationRegistry.Persistence.Entities;

namespace CO.CDP.Organisation.WebApi.ApplicationRegistry.Api;

public static class BulkOperationEndpoints
{
    private const int MaxBatchSize = 1000;

    public static void UseBulkOperationEndpoints(this WebApplication app)
    {
        app.MapPost("/api/bulk/organisations", async (
            BulkCreateOrganisations command,
            IUseCase<CreateOrganisation, OrganisationDto> useCase) =>
        {
            var items = command.Organisations.ToList();
            if (items.Count > MaxBatchSize)
                return Results.BadRequest($"Batch size exceeds maximum of {MaxBatchSize}.");

            var errors = new List<BulkItemError>();
            int succeeded = 0;

            for (int i = 0; i < items.Count; i++)
            {
                try
                {
                    await useCase.Execute(items[i]);
                    succeeded++;
                }
                catch (Exception ex)
                {
                    errors.Add(new BulkItemError(i, ex.Message));
                }
            }

            return Results.Ok(new BulkOperationResult(items.Count, succeeded, errors.Count, errors));
        })
        .RequireAuthorization(AuthorizationPolicies.PlatformAdmin)
        .WithTags("Bulk Operations");

        app.MapPost("/api/bulk/organisations/{orgId:guid}/members", async (
            Guid orgId,
            BulkAddMembers command,
            IUseCase<(Guid, AddMember), bool> useCase) =>
        {
            var items = command.Members.ToList();
            if (items.Count > MaxBatchSize)
                return Results.BadRequest($"Batch size exceeds maximum of {MaxBatchSize}.");

            var errors = new List<BulkItemError>();
            int succeeded = 0;

            for (int i = 0; i < items.Count; i++)
            {
                try
                {
                    await useCase.Execute((orgId, items[i]));
                    succeeded++;
                }
                catch (Exception ex)
                {
                    errors.Add(new BulkItemError(i, ex.Message));
                }
            }

            return Results.Ok(new BulkOperationResult(items.Count, succeeded, errors.Count, errors));
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("Bulk Operations");

        app.MapPost("/api/bulk/organisations/{orgId:guid}/applications/{appId:guid}/users", async (
            Guid orgId,
            Guid appId,
            BulkAssignUsers command,
            IUserAssignmentRepository repo) =>
        {
            var items = command.Assignments.ToList();
            if (items.Count > MaxBatchSize)
                return Results.BadRequest($"Batch size exceeds maximum of {MaxBatchSize}.");

            var errors = new List<BulkItemError>();
            int succeeded = 0;

            for (int i = 0; i < items.Count; i++)
            {
                try
                {
                    var assignment = new UserApplicationAssignment
                    {
                        UserPrincipalId = items[i].UserPrincipalId,
                        ApplicationId = appId,
                        OrganisationId = orgId,
                        AssignedBy = "system"
                    };

                    if (items[i].RoleIds != null)
                    {
                        foreach (var roleId in items[i].RoleIds!)
                        {
                            assignment.RoleAssignments.Add(new UserRoleAssignment { RoleId = roleId });
                        }
                    }

                    await repo.CreateAssignmentAsync(assignment);
                    succeeded++;
                }
                catch (Exception ex)
                {
                    errors.Add(new BulkItemError(i, ex.Message));
                }
            }

            return Results.Ok(new BulkOperationResult(items.Count, succeeded, errors.Count, errors));
        })
        .RequireAuthorization(AuthorizationPolicies.OrgAdmin)
        .WithTags("Bulk Operations");
    }
}
