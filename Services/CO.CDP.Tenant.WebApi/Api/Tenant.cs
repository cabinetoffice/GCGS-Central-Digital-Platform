using CO.CDP.Common;
using CO.CDP.Tenant.WebApi.Model;
using CO.CDP.Tenant.WebApi.UseCase;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    private static Dictionary<Guid, Model.Tenant> _tenants = Enumerable.Range(1, 5)
        .Select(_ => Guid.NewGuid())
        .ToDictionary(id => id, id => new Model.Tenant
        {
            Id = id,
            Name = $"Bobby Tables {id}"
        });

    public static void UseTenantEndpoints(this WebApplication app)
    {
        app.MapGet("/tenants", () => _tenants.Values.ToArray())
            .Produces<List<Model.Tenant>>(200, "application/json")
            .Produces(401)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "ListTenants";
                operation.Description = "[STUB] A list of tenants. [STUB]";
                operation.Summary = "[STUB] A list of tenants. [STUB]";
                operation.Responses["200"].Description = "A list of tenants.";
                return operation;
            })
            .RequireAuthorization();

        app.MapPost("/tenants", async (RegisterTenant command, IUseCase<RegisterTenant, Model.Tenant> useCase) =>
                await useCase.Execute(command)
                    .AndThen(tenant =>
                        Results.Created(new Uri($"/tenants/{tenant.Id}", UriKind.Relative), tenant)
                    ))
            .Produces<Model.Tenant>(201, "application/json")
            .Produces(401)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreateTenant";
                operation.Description = "Create a new tenant.";
                operation.Summary = "Create a new tenant.";
                operation.Responses["201"].Description = "Tenant created successfully.";
                return operation;
            })
            .RequireAuthorization();

        app.MapDelete("/tenants/{tenantId}", (Guid tenantId) =>
            {
                _tenants.Remove(tenantId);
                return Results.NoContent();
            })
            .Produces(204)
            .Produces(401)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "DeleteTenant";
                operation.Description = "[STUB] Delete a tenant. [STUB]";
                operation.Summary = "[STUB] Delete a tenant. [STUB]";
                operation.Responses["204"].Description = "Tenant deleted successfully.";
                return operation;
            })
            .RequireAuthorization();

        app.MapGet("/tenants/{tenantId}", async (Guid tenantId, IUseCase<Guid, Model.Tenant?> useCase) =>
                await useCase.Execute(tenantId)
                    .AndThen(tenant => tenant != null ? Results.Ok(tenant) : Results.NotFound()))
            .Produces<Model.Tenant>(200, "application/json")
            .Produces(401)
            .Produces(404)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetTenant";
                operation.Description = "Get a tenant by ID.";
                operation.Summary = "Get a tenant by ID.";
                operation.Responses["200"].Description = "Tenant details.";
                return operation;
            })
            .RequireAuthorization();

        app.MapPut("/tenants/{tenantId}", (Guid tenantId, UpdateTenant updatedTenant) =>
            {
                _tenants[tenantId] = new Model.Tenant
                {
                    Id = tenantId,
                    Name = updatedTenant.Name
                };
                return Results.Ok(_tenants[tenantId]);
            })
            .Produces<Model.Tenant>(200, "application/json")
            .Produces(401)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateTenant";
                operation.Description = "[STUB] Update a tenant [STUB]";
                operation.Summary = "[STUB] Update a tenant [STUB]";
                operation.Responses["200"].Description = "Tenant updated successfully.";
                return operation;
            })
            .RequireAuthorization();
    }

    public static void UseTenantLookupEndpoints(this WebApplication app)
    {
        app.MapGet("/tenant/lookup",
                async ([FromQuery] string name, IUseCase<string, Model.Tenant?> useCase) =>
                await useCase.Execute(name)
                    .AndThen(tenant => tenant != null ? Results.Ok(tenant) : Results.NotFound()))
            .Produces<Model.Tenant>(200, "application/json")
            .Produces(401)
            .Produces(404)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "LookupTenant";
                operation.Description = "Lookup person by identifier.";
                operation.Summary = "Lookup person by identifier.";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Tenant Lookup" } };
                operation.Responses["200"].Description = "Tenants Associated with the the user.";
                return operation;
            })
            .RequireAuthorization();
    }

    public static void UseUserManagementEndpoints(this WebApplication app)
    {
        app.MapPost("/tenants/{tenantId}/assign-user",
                (string tenantId, [FromBody] AssignUserToOrganisation command) =>
                    new Receipt { Message = "User assigned successfully." })
            .Accepts<AssignUserToOrganisation>("application/json")
            .Produces<Receipt>(200, "application/json")
            .Produces<Error>(400, "application/json")
            .Produces(401)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "AssignUserToOrganisation";
                operation.Description = "[STUB] Assign user to an organisation. [STUB]";
                operation.Summary = "[STUB] Assign user to an organisation. [STUB]";
                operation.Tags = new List<OpenApiTag> { new() { Name = "User Management" } };
                operation.Responses["200"].Description = "User successfully assigned to the organisation.";
                return operation;
            })
            .RequireAuthorization();

        app.MapPatch("/tenants/{tenantId}/user-permissions",
                (string tenantId, [FromBody] ModifyUserPermissions command) =>
                    new Receipt { Message = "User permissions updated successfully." })
            .Produces<Receipt>(200, "application/json")
            .Produces<Error>(400, "application/json")
            .Produces(401)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "ModifyUserPermissions";
                operation.Description = "[STUB] Modify user permissions within an organisation. [STUB]";
                operation.Summary = "[STUB] Modify user permissions within an organisation. [STUB]";
                operation.Tags = new List<OpenApiTag> { new() { Name = "User Management" } };
                operation.Responses["200"].Description = "User permissions modified successfully.";
                return operation;
            })
            .RequireAuthorization(); ;
    }
}

public static class ApiExtensions
{
    public static void DocumentTenantApi(this SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "1.0.0.0",
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

        var jwtSecurityScheme = new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "GOV.UK One Login JWT Bearer token",
            Type = SecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = JwtBearerDefaults.AuthenticationScheme
            }
        };

        options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, jwtSecurityScheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement { { jwtSecurityScheme, Array.Empty<string>() } });
    }
}