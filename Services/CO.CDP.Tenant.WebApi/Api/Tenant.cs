using CO.CDP.Common;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.UseCase;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Tenant.WebApi.Api;
internal record UserDetails
{
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

    [EmailAddress, Required(AllowEmptyStrings = true)]
    public required string Email { get; init; }
}

internal record UserInTenants
{
    [Required(AllowEmptyStrings = true)] public required Guid Id { get; init; }
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
}

internal record AssignUserToOrganisation
{
    [Required(AllowEmptyStrings = true)] public required Guid UserId { get; init; }
    [Required(AllowEmptyStrings = true)] public required Guid OrganisationId { get; init; }
}

internal record Receipt
{
    [Required(AllowEmptyStrings = true)] public required string Message { get; init; }
}

internal record Error
{
    [Required(AllowEmptyStrings = true)] public required string Code { get; init; }
    [Required(AllowEmptyStrings = true)] public required string Message { get; init; }
}

internal record ModifyUserPermissions
{
    [Required(AllowEmptyStrings = true)] public required string UserId { get; init; }
    [Required(AllowEmptyStrings = true)] public required string OrganisationId { get; init; }
    [Required] public required List<string> Permissions { get; init; }
    [Required] public required UserPermissionsAction Action { get; init; }

    internal enum UserPermissionsAction
    {
        Add,
        Remove
    }
}

public static class EndpointExtensions
{
    public static void UseTenantEndpoints(this WebApplication app)
    {
        app.MapPost("/tenants", async (RegisterTenant command, IUseCase<RegisterTenant, Model.Tenant> useCase) =>
                await useCase.Execute(command)
                    .AndThen(tenant =>
                        Results.Created(new Uri($"/tenants/{tenant.Id}", UriKind.Relative), tenant)
                    ))
            .Produces<Model.Tenant>(StatusCodes.Status201Created, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreateTenant";
                operation.Description = "Create a new tenant.";
                operation.Summary = "Create a new tenant.";
                operation.Responses["201"].Description = "Tenant created successfully.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });
        app.MapGet("/tenants/{tenantId}", async (Guid tenantId, IUseCase<Guid, Model.Tenant?> useCase) =>
                await useCase.Execute(tenantId)
                    .AndThen(tenant => tenant != null ? Results.Ok(tenant) : Results.NotFound()))
            .Produces<Model.Tenant>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetTenant";
                operation.Description = "Get a tenant by ID.";
                operation.Summary = "Get a tenant by ID.";
                operation.Responses["200"].Description = "Tenant details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Tenant not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });
    }

    public static void UseTenantLookupEndpoints(this WebApplication app)
    {
        var openApiTags = new List<OpenApiTag> { new() { Name = "Tenant Lookup" } };

        app.MapGet("/tenant/lookup",
                async ([FromQuery] string userId, IUseCase<string, Model.Tenant?> useCase) =>
                await useCase.Execute(userId)
                    .AndThen(tenant => tenant != null ? Results.Ok(tenant) : Results.NotFound()))
            .Produces<Model.Tenant>(StatusCodes.Status200OK, "application/json")
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "LookupTenant";
                operation.Description = "Lookup the tenant for the authenticated user.";
                operation.Summary = "Lookup the tenant for the authenticated user.";
                operation.Tags = openApiTags;
                operation.Responses["200"].Description = "Tenants associated with the the user.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Tenant not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });
    }
}

public static class ApiExtensions
{
    public static void DocumentTenantApi(this SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "1.0.0",
            Title = "Tenant Management API",
            Description =
                "API for creating, updating, deleting, and listing tenants, including a lookup feature against person identifiers.",
            TermsOfService = new Uri("https://example.com/terms"),
            Contact = new OpenApiContact
            {
                Name = "Example Contact",
                Url = new Uri("https://example.com/contact")
            },
            License = new OpenApiLicense
            {
                Name = "Example License",
                Url = new Uri("https://example.com/license")
            }
        });
        options.CustomSchemaIds(type =>
            {
                var typeMap = new Dictionary<Type, string>
                {
                    { typeof(RegisterTenant), "NewTenant" },
                    { typeof(UpdateTenant), "UpdatedTenant" },
                };
                return typeMap.GetValueOrDefault(type, type.Name);
            }
        );
    }
}