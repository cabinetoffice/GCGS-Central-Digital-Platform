using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CO.CDP.Tenant.WebApi.Api
{
    internal record Tenant
    {
        [Required(AllowEmptyStrings = true)] public required string Id { get; init; }

        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

        [Required(AllowEmptyStrings = true)] public required TenantContactInfo ContactInfo { get; init; }
    }

    internal record NewTenant
    {
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

        [Required(AllowEmptyStrings = true)] public required TenantContactInfo ContactInfo { get; init; }
    }

    internal record UpdatedTenant
    {
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

        [Required(AllowEmptyStrings = true)] public required TenantContactInfo ContactInfo { get; init; }
    }

    internal record TenantContactInfo
    {
        [Required(AllowEmptyStrings = true), EmailAddress]
        public required string Email { get; init; }

        [Required(AllowEmptyStrings = true)] public required string Phone { get; init; }
    }

    internal record TenantLookupCriteria
    {
        [Required(AllowEmptyStrings = true)] public required string UserPrinciple { get; init; }

        [Required(AllowEmptyStrings = true)] public required string OpenId { get; init; }
    }

    internal record TenantLookupResponse
    {
        [Required(AllowEmptyStrings = true)] public required string UserPrinciple { get; init; }
        [Required] public required UserDetails UserDetails { get; init; }
        [Required] public required List<UserInTenants> UserTenants { get; init; }
    }

    internal record UserDetails
    {
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

        [EmailAddress, Required(AllowEmptyStrings = true)]
        public required string Email { get; init; }
    }

    internal record UserInTenants
    {
        [Required(AllowEmptyStrings = true)] public required string Id { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    }

    internal record AssignUserToOrganisation
    {
        [Required(AllowEmptyStrings = true)] public required string UserId { get; init; }
        [Required(AllowEmptyStrings = true)] public required string OrganisationId { get; init; }
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
        private static Dictionary<string, Tenant> _tenants = Enumerable.Range(1, 5)
            .ToDictionary(index => index.ToString(), index => new Tenant
            {
                Id = index.ToString(),
                Name = $"Bobby Tables {index}",
                ContactInfo = new TenantContactInfo
                {
                    Email = $"bobby{index}@example.com",
                    Phone = $"0555 123 95{index}"
                }
            });

        public static void UseTenantEndpoints(this WebApplication app)
        {
            app.MapGet("/tenants", () => _tenants.Values.ToArray())
                .Produces<List<Tenant>>(200, "application/json")
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "ListTenants";
                    operation.Description = "A list of tenants.";
                    operation.Summary = "A list of tenants.";
                    operation.Responses["200"].Description = "A list of tenants.";
                    return operation;
                });
            app.MapPost("/tenants", (NewTenant newTenant) =>
                {
                    var tenant = new Tenant
                    {
                        Id = (_tenants.Count + 1).ToString(),
                        Name = newTenant.Name,
                        ContactInfo = newTenant.ContactInfo
                    };
                    _tenants.Add(tenant.Id, tenant);
                    return Results.Created(new Uri($"/tenants/{tenant.Id}"), tenant);
                })
                .Produces<Tenant>(201, "application/json")
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "CreateTenant";
                    operation.Description = "Create a new tenant.";
                    operation.Summary = "Create a new tenant.";
                    operation.Responses["201"].Description = "Tenant created successfully.";
                    return operation;
                });
            app.MapDelete("/tenants/{tenantId}", (String tenantId) =>
                {
                    _tenants.Remove(tenantId);
                    return Results.NoContent();
                })
                .Produces(204)
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "DeleteTenant";
                    operation.Description = "Delete a tenant.";
                    operation.Summary = "Delete a tenant.";
                    operation.Responses["204"].Description = "Tenant deleted successfully.";
                    return operation;
                });
            app.MapGet("/tenants/{tenantId}", (String tenantId) =>
                {
                    try
                    {
                        return Results.Ok(_tenants[tenantId]);
                    }
                    catch (KeyNotFoundException _)
                    {
                        return Results.NotFound();
                    }
                })
                .Produces<Tenant>(200, "application/json")
                .Produces(404)
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "GetTenant";
                    operation.Description = "Get a tenant by ID.";
                    operation.Summary = "Get a tenant by ID.";
                    operation.Responses["200"].Description = "Tenant details.";
                    return operation;
                });
            app.MapPut("/tenants/{tenantId}", (String tenantId, UpdatedTenant updatedTenant) =>
                {
                    _tenants[tenantId] = new Tenant
                    {
                        Id = tenantId,
                        Name = updatedTenant.Name,
                        ContactInfo = updatedTenant.ContactInfo
                    };
                    return Results.Ok(_tenants[tenantId]);
                })
                .Produces<Tenant>(200, "application/json")
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "UpdateTenant";
                    operation.Description = "Update a tenant";
                    operation.Summary = "Update a tenant";
                    operation.Responses["200"].Description = "Tenant updated successfully.";
                    return operation;
                });
        }

        public static void UseTenantLookupEndpoints(this WebApplication app)
        {
            app.MapPost("/tenant/lookup",
                    ([FromBody] TenantLookupCriteria criteria) => new TenantLookupResponse
                    {
                        UserPrinciple = criteria.UserPrinciple,
                        UserDetails = new UserDetails
                        {
                            Name = "Bobby Tables",
                            Email = "bob@example.com"
                        },
                        UserTenants = _tenants.Select(tenant => new UserInTenants
                        {
                            Id = tenant.Value.Id,
                            Name = tenant.Value.Name
                        }).ToList()
                    })
                .Accepts<TenantLookupCriteria>("application/json")
                .Produces<TenantLookupResponse>(200, "application/json")
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "LookupTenant";
                    operation.Description = "Lookup person by identifier.";
                    operation.Summary = "Lookup person by identifier.";
                    operation.Tags = new List<OpenApiTag> { new() { Name = "Tenant Lookup" } };
                    operation.Responses["200"].Description = "Tenants Associated with the the user.";
                    return operation;
                });
        }

        public static void UseUserManagementEndpoints(this WebApplication app)
        {
            app.MapPost("/tenants/{tenantId}/assign-user",
                    (string tenantId, [FromBody] AssignUserToOrganisation command) =>
                        new Receipt { Message = "User assigned successfully." })
                .Accepts<AssignUserToOrganisation>("application/json")
                .Produces<Receipt>(200, "application/json")
                .Produces<Error>(400, "application/json")
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "AssignUserToOrganisation";
                    operation.Description = "Assign user to an organisation.";
                    operation.Summary = "Assign user to an organisation.";
                    operation.Tags = new List<OpenApiTag> { new() { Name = "User Management" } };
                    operation.Responses["200"].Description = "User successfully assigned to the organisation.";
                    return operation;
                });
            app.MapPatch("/tenants/{tenantId}/user-permissions",
                    (string tenantId, [FromBody] ModifyUserPermissions command) =>
                        new Receipt { Message = "User permissions updated successfully." })
                .Produces<Receipt>(200, "application/json")
                .Produces<Error>(400, "application/json")
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "ModifyUserPermissions";
                    operation.Description = "Modify user permissions within an organisation.";
                    operation.Summary = "Modify user permissions within an organisation.";
                    operation.Tags = new List<OpenApiTag> { new() { Name = "User Management" } };
                    operation.Responses["200"].Description = "User permissions modified successfully.";
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
        }
    }
}