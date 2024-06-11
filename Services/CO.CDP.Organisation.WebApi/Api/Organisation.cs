using System.Reflection;
using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.Swashbuckle.SwaggerGen;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace CO.CDP.Organisation.WebApi.Api;

public static class EndpointExtensions
{
    private static Dictionary<Guid, Model.Organisation> _organisations = Enumerable.Range(1, 5)
        .Select(_ => Guid.NewGuid())
        .ToDictionary(id => id, id => new Model.Organisation
        {
            Id = id,
            Identifier = new Identifier
            {
                Scheme = "CH",
                Id = $"123945123{id}",
                LegalName = "TheOrganisation",
                Uri = new Uri("https://example.com")
            },
            Name = $"Tables Limited {id}",
            AdditionalIdentifiers = [],
            Addresses = [new Address
            {
                Type = AddressType.Registered,
                StreetAddress = $"Green Lane {id}",
                StreetAddress2 = "",
                Locality = "London",
                Region = "",
                PostalCode = "BR8 7AA",
                CountryName = "United Kingdom"
            }],
            ContactPoint = new ContactPoint
            {
                Name = "Bobby Tables",
                Email = $"bobby+{id}@example.com",
                Telephone = "07925123123",
                Url = new Uri("https://example.com")
            },
            Roles = [PartyRole.Supplier],
        });

    public static void UseOrganisationEndpoints(this WebApplication app)
    {
        app.MapGet("/organisations",
                async ([FromQuery] string userUrn, IUseCase<string, IEnumerable<Model.Organisation>> useCase) =>
                await useCase.Execute(userUrn)
                    .AndThen(organisations => Results.Ok(organisations)))
            .Produces<List<Model.Organisation>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "ListOrganisations";
                operation.Description = "Get a list of organisations.";
                operation.Summary = "Get a list of organisations.";
                operation.Responses["200"].Description = "A list of organisations.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });
        app.MapPost("/organisations", async (RegisterOrganisation command, IUseCase<RegisterOrganisation, Model.Organisation> useCase) =>
              await useCase.Execute(command)
              .AndThen(organisation =>
                  organisation != null
                      ? Results.Created(new Uri($"/organisations/{organisation.Id}", UriKind.Relative), organisation)
                      : Results.Problem("Organisation could not be created due to an internal error"))
        )
        .Produces<Model.Organisation>(StatusCodes.Status201Created, "application/json")
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi(operation =>
        {
            operation.OperationId = "CreateOrganisation";
            operation.Description = "Create a new organisation.";
            operation.Summary = "Create a new organisation.";
            operation.Responses["201"].Description = "Organisation created successfully.";
            operation.Responses["400"].Description = "Bad request.";
            operation.Responses["422"].Description = "Unprocessable entity.";
            operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
            operation.Responses["500"].Description = "Internal server error.";
            return operation;
        });
        app.MapGet("/organisations/{organisationId}", async (Guid organisationId, IUseCase<Guid, Model.Organisation?> useCase) =>
               await useCase.Execute(organisationId)
                   .AndThen(organisation => organisation != null ? Results.Ok(organisation) : Results.NotFound()))
           .Produces<Model.Organisation>(StatusCodes.Status200OK, "application/json")
           .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
           .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
           .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
           .WithOpenApi(operation =>
           {
               operation.OperationId = "GetOrganisation";
               operation.Description = "Get an organisation by ID.";
               operation.Summary = "Get an organisation by ID.";
               operation.Responses["200"].Description = "Organisation details.";
               operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
               operation.Responses["404"].Description = "Organisation not found.";
               operation.Responses["500"].Description = "Internal server error.";
               return operation;
           });
        app.MapPut("/organisations/{organisationId}",
                (Guid organisationId, UpdateOrganisation updatedOrganisation) =>
                {
                    if (!_organisations.ContainsKey(organisationId))
                    {
                        return Results.NotFound();
                    }
                    _organisations[organisationId] = new Model.Organisation
                    {
                        Id = organisationId,
                        Identifier = updatedOrganisation.Identifier.AsView(),
                        Name = updatedOrganisation.Name,
                        AdditionalIdentifiers = updatedOrganisation.AdditionalIdentifiers.AsView(),
                        Addresses = updatedOrganisation.Addresses.AsView(),
                        ContactPoint = updatedOrganisation.ContactPoint.AsView(),
                        Roles = updatedOrganisation.Roles ?? [],
                    };
                    return Results.Ok(_organisations[organisationId]);
                })
            .Produces<Model.Organisation>(200, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateOrganisation";
                operation.Description = "[STUB] Update a organisation [STUB]";
                operation.Summary = "[STUB] Update a organisation [STUB]";
                operation.Responses["200"].Description = "Organisation updated.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPatch("/organisations/{organisationId}/buyerinformation",
            async (Guid organisationId, UpdateBuyerInformation byuerInformation,
                IUseCase<(Guid, UpdateBuyerInformation), bool> useCase) =>
            {
                await useCase.Execute((organisationId, byuerInformation))
                   .AndThen(_ => Results.Ok());
            });
    }
    public static void UseOrganisationLookupEndpoints(this WebApplication app)
    {
        app.MapGet("/organisation/me", () => Results.Ok(_organisations.First().Value))
            .Produces<List<Model.Organisation>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "MyOrganisation";
                operation.Description = "[STUB] The organisation details of the organisation the API key was issued for. [STUB]";
                operation.Summary = "[STUB] The organisation details of the organisation the API key was issued for. [STUB]";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation Lookup" } };
                operation.Responses["200"].Description = "Organisation details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation matching the API key was not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });
        app.MapGet("/organisation/lookup",
                async ([FromQuery] string name, IUseCase<string, Model.Organisation?> useCase) =>
                await useCase.Execute(name)
                    .AndThen(organisation => organisation != null ? Results.Ok(organisation) : Results.NotFound()))
            .Produces<Model.Organisation>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "LookupOrganisation";
                operation.Description = "Find an organisation.";
                operation.Summary = "Find an organisation.";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation Lookup" } };
                operation.Responses["200"].Description = "Organisations Associated.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation not found.";
                operation.Responses["500"].Description = "Internal server error.";
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
            Version = "1.0.0",
            Title = "Organisation Management API",
            Description = "API for creating, updating, deleting, and listing organisations, including a lookup feature against organisation identifiers.",
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
                    { typeof(RegisterOrganisation), "NewOrganisation" },
                    { typeof(UpdateOrganisation), "UpdatedOrganisation" },
                };
            return typeMap.GetValueOrDefault(type, type.Name);
        }
       );
        options.IncludeXmlComments(Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(Address)));
        options.OperationFilter<ProblemDetailsOperationFilter>();
        options.ConfigureBearerSecurity();
        options.ConfigureApiKeySecurity();
    }

}