using System.ComponentModel.DataAnnotations;

namespace Tenant.Api
{
    internal record Tenant
    {
        [Required(AllowEmptyStrings = true)]
        public required string Id { get; init; }
    
        [Required(AllowEmptyStrings = true)]
        public required string Name { get; init; }
    }
    
    public static class EndpointExtensions
    {
        public static void AddTenantEndpoints(this WebApplication app)
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
}