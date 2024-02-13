using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Tenant.Api
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

    internal record TenantContactInfo
    {
        [Required(AllowEmptyStrings = true)] public required string Email { get; init; }

        [Required(AllowEmptyStrings = true)] public required string Phone { get; init; }
    }

    public static class EndpointExtensions
    {
        public static void UseTenantEndpoints(this WebApplication app)
        {
            app.MapGet("/tenants", () =>
                {
                    return Enumerable.Range(1, 5).Select(index =>
                        new Tenant
                        {
                            Id = index.ToString(),
                            Name = $"Bobby Tables {index}",
                            ContactInfo = new TenantContactInfo
                            {
                                Email = $"bobby{index}@example.com",
                                Phone = $"0555 123 95{index}"
                            }
                        }
                    ).ToArray();
                })
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
                        Id = Random.Shared.Next(1, 5).ToString(),
                        Name = newTenant.Name,
                        ContactInfo = newTenant.ContactInfo
                    };
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