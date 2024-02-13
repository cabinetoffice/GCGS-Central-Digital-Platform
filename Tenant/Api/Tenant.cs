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
                            Name = $"Bobby Tables {index}"
                        }
                    ).ToArray();
                })
                .Produces<List<Tenant>>(200, "application/json")
                .WithName("listTenants")
                .WithSummary("A list of tenants.")
                .WithDescription("A list of tenants.")
                .WithOpenApi();
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