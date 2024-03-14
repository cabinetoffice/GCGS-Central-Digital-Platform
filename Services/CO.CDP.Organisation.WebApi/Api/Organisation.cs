using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CO.CDP.Organisation.WebApi.Api
{
    internal record Organisation
    {
        [Required(AllowEmptyStrings = true)] public required string Id { get; init; }
        [Required] public required Identifier Identifier { get; init; }
        [Required] public required List<Identifier> AdditionalIdentifiers { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
        [Required] public required Address Address { get; init; }
        [Required] public required ContactPoint ContactPoint { get; init; }
        [Required] public required List<Role> Roles { get; init; }
    }

    internal record NewOrganisation
    {
        [Required] public required Identifier Identifier { get; init; }
        [Required] public required List<Identifier> AdditionalIdentifiers { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
        [Required] public required Address Address { get; init; }
        [Required] public required ContactPoint ContactPoint { get; init; }
        [Required] public required List<Role> Roles { get; init; }
    }

    internal record UpdatedOrganisation
    {
        [Required] public required Identifier Identifier { get; init; }
        [Required] public required List<Identifier> AdditionalIdentifiers { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
        [Required] public required Address Address { get; init; }
        [Required] public required ContactPoint ContactPoint { get; init; }
        [Required] public required List<Role> Roles { get; init; }
    }

    internal record Identifier
    {
        [Required(AllowEmptyStrings = true)] public required string Scheme { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Id { get; init; }
        public string? LegalName { get; init; }
        public string? Uri { get; init; }
    }

    internal record Address
    {
        [Required(AllowEmptyStrings = true)] public required string StreetAddress { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Locality { get; init; }
        public string? Region { get; init; }
        public string? PostalCode { get; init; }
        [Required(AllowEmptyStrings = true)] public required string CountryName { get; init; }

    }

    internal record ContactPoint
    {
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Email { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Telephone { get; init; }
        [Required(AllowEmptyStrings = true)] public required string FaxNumber { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Url { get; init; }
    }

    internal enum Role
    {
        ProcuringEntity,
        Supplier,
        Tenderer,
        Funder,
        Enquirer
    }

    public static class EndpointExtensions
    {
        private static Dictionary<string, Organisation> _organisations = Enumerable.Range(1, 5)
            .ToDictionary(index => index.ToString(), index => new Organisation
            {
                Id = index.ToString(),
                Identifier = new Identifier
                {
                    Scheme = "CH",
                    Id = $"123945123{index}",
                },
                Name = $"Tables Limited {index}",
                AdditionalIdentifiers = [],
                Address = new Address
                {
                    StreetAddress = $"Green Lane {index}",
                    Locality = "London",
                    Region = "Kent",
                    CountryName = "United Kingdom",
                    PostalCode = "BR8 7AA"
                },
                ContactPoint = new ContactPoint
                {
                    Name = "Bobby Tables",
                    Email = $"bobby+{index}@example.com",
                    Telephone = "07925123123",
                    FaxNumber = "07925123999",
                    Url = "https://example.com"
                },
                Roles = [Role.Supplier],
            });

        public static void UseOrganisationEndpoints(this WebApplication app)
        {
            app.MapGet("/organisations", () => _organisations.Values.ToArray())
                .Produces<List<Organisation>>(200, "application/json")
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "ListOrganisations";
                    operation.Description = "A list of organisations.";
                    operation.Summary = "A list of organisations.";
                    operation.Responses["200"].Description = "A list of organisations.";
                    return operation;
                });
            app.MapPost("/organisations", (NewOrganisation newOrganisation) =>
                {
                    var organisation = new Organisation
                    {
                        Id = (_organisations.Count + 1).ToString(),
                        Identifier = newOrganisation.Identifier,
                        Name = newOrganisation.Name,
                        AdditionalIdentifiers = newOrganisation.AdditionalIdentifiers,
                        Address = newOrganisation.Address,
                        ContactPoint = newOrganisation.ContactPoint,
                        Roles = newOrganisation.Roles,
                    };
                    _organisations.Add(organisation.Id, organisation);
                    return Results.Created(new Uri($"/organisations/{organisation.Id}"), organisation);
                })
                .Produces<Organisation>(201, "application/json")
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "CreateOrganisation";
                    operation.Description = "Create a new organisation.";
                    operation.Summary = "Create a new organisation.";
                    operation.Responses["201"].Description = "Organisation created.";
                    return operation;
                });
            app.MapGet("/organisations/{organisationId}", (String organisationId) =>
                {
                    try
                    {
                        return Results.Ok(_organisations[organisationId]);
                    }
                    catch (KeyNotFoundException _)
                    {
                        return Results.NotFound();
                    }
                })
                .Produces<Organisation>(200, "application/json")
                .Produces(404)
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "GetOrganisation";
                    operation.Description = "Get a organisation by ID.";
                    operation.Summary = "Get a organisation by ID.";
                    operation.Responses["200"].Description = "Organisation details.";
                    return operation;
                });
            app.MapPut("/organisations/{organisationId}",
                    (String organisationId, UpdatedOrganisation updatedOrganisation) =>
                    {
                        _organisations[organisationId] = new Organisation
                        {
                            Id = organisationId,
                            Identifier = updatedOrganisation.Identifier,
                            Name = updatedOrganisation.Name,
                            AdditionalIdentifiers = updatedOrganisation.AdditionalIdentifiers,
                            Address = updatedOrganisation.Address,
                            ContactPoint = updatedOrganisation.ContactPoint,
                            Roles = updatedOrganisation.Roles,
                        };
                        return Results.Ok(_organisations[organisationId]);
                    })
                .Produces<Organisation>(200, "application/json")
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "UpdateOrganisation";
                    operation.Description = "Update a organisation";
                    operation.Summary = "Update a organisation";
                    operation.Responses["200"].Description = "Organisation updated.";
                    return operation;
                });
            app.MapDelete("/organisations/{organisationId}", (String organisationId) =>
                {
                    _organisations.Remove(organisationId);
                    return Results.NoContent();
                })
                .Produces(204)
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "DeleteOrganisation";
                    operation.Description = "Delete a organisation.";
                    operation.Summary = "Delete a organisation.";
                    operation.Responses["204"].Description = "Organisation deleted.";
                    return operation;
                });
        }
    }

    public static class ApiExtensions
    {
        public static void DocumentOrganisationApi(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "1.0.0.0",
                Title = "Organisation API",
                Description = "",
            });
        }
    }
}