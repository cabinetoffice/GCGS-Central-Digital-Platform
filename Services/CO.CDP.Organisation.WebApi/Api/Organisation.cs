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
using System.Data.SqlTypes;
using System.Reflection;
using AuthorizationPolicyConstants = CO.CDP.Authentication.AuthorizationPolicy.Constants;

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
                      : Results.Problem("Organisation could not be created due to an internal error")))
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

        app.MapPatch("/organisations/{organisationId}",
            async (Guid organisationId, UpdateOrganisation updateOrganisation,
                IUseCase<(Guid, UpdateOrganisation), bool> useCase) =>
                    await useCase.Execute((organisationId, updateOrganisation))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateOrganisation";
                operation.Description = "Update Organisation.";
                operation.Summary = "Update Organisation.";
                operation.Responses["204"].Description = "Organisation updated successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });
    }

    public static RouteGroupBuilder UseOrganisationLookupEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/me", () => Results.Ok(_organisations.First().Value))
            .Produces<List<Model.Organisation>>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "MyOrganisation";
                operation.Description = "[STUB] The organisation details of the organisation the API key was issued for. [STUB]";
                operation.Summary = "[STUB] The organisation details of the organisation the API key was issued for. [STUB]";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation - Lookup" } };
                operation.Responses["200"].Description = "Organisation details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation matching the API key was not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            }).RequireAuthorization(AuthorizationPolicyConstants.OrganisationKeyPolicy);

        app.MapGet("/lookup",
             async ([FromQuery] string? name, [FromQuery] string? identifier, IUseCase<OrganisationQuery, Model.Organisation?> useCase) =>
                 await useCase.Execute(new OrganisationQuery(name, identifier))
                    .AndThen(organisation => organisation != null ? Results.Ok(organisation) : Results.NotFound()))
         .Produces<Model.Organisation>(StatusCodes.Status200OK, "application/json")
         .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
         .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
         .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
         .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
         .WithOpenApi(operation =>
         {
             operation.OperationId = "LookupOrganisation";
             operation.Description = "Find an organisation by name or identifier.";
             operation.Summary = "Find an organisation by name or identifier.";
             operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation - Lookup" } };
             operation.Responses["200"].Description = "Organisation details.";
             operation.Responses["400"].Description = "Bad request.";
             operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
             operation.Responses["404"].Description = "Organisation not found.";
             operation.Responses["500"].Description = "Internal server error.";
             return operation;
         });

        return app;
    }

    public static RouteGroupBuilder UseBuyerInformationEndpoints(this RouteGroupBuilder app)
    {
        app.MapPatch("/{organisationId}/buyer-information",

            async (Guid organisationId, UpdateBuyerInformation buyerInformation,
                IUseCase<(Guid, UpdateBuyerInformation), bool> useCase) =>

                await useCase.Execute((organisationId, buyerInformation))
                   .AndThen(_ => Results.NoContent())
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateBuyerInformation";
                operation.Description = "Update Buyer Information.";
                operation.Summary = "Update Buyer Information.";
                operation.Responses["204"].Description = "Buyer information updated successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });
        return app;
    }

    public static RouteGroupBuilder UseSupplierInformationEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/{organisationId}/supplier-information",
            async (Guid organisationId, IUseCase<Guid, SupplierInformation?> useCase) =>
               await useCase.Execute(organisationId)
                   .AndThen(supplier => supplier != null ? Results.Ok(supplier) : Results.NotFound()))
           .Produces<SupplierInformation>(StatusCodes.Status200OK, "application/json")
           .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
           .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
           .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
           .WithOpenApi(operation =>
           {
               operation.OperationId = "GetOrganisationSupplierInformation";
               operation.Description = "Get organisation supplier information by ID.";
               operation.Summary = "Get organisation supplier information by ID.";
               operation.Responses["200"].Description = "Organisation supplier information details.";
               operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
               operation.Responses["404"].Description = "Organisation supplier information not found.";
               operation.Responses["422"].Description = "Unprocessable entity.";
               operation.Responses["500"].Description = "Internal server error.";
               return operation;
           });

        app.MapPatch("/{organisationId}/supplier-information",
            async (Guid organisationId, UpdateSupplierInformation supplierInformation,
                IUseCase<(Guid, UpdateSupplierInformation), bool> useCase) =>
                    await useCase.Execute((organisationId, supplierInformation))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateSupplierInformation";
                operation.Description = "Update Supplier Information.";
                operation.Summary = "Update Supplier Information.";
                operation.Responses["204"].Description = "Supplier information updated successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation supplier information not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapDelete("/{organisationId}/supplier-information",
            async (Guid organisationId, [FromBody] DeleteSupplierInformation deleteSupplierInformation,
                IUseCase<(Guid, DeleteSupplierInformation), bool> useCase) =>
                    await useCase.Execute((organisationId, deleteSupplierInformation))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "DeleteSupplierInformation";
                operation.Description = "Delete Supplier Information.";
                operation.Summary = "Delete Supplier Information.";
                operation.Responses["204"].Description = "Supplier information deleted successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation supplier information not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        return app;
    }

    public static RouteGroupBuilder UseConnectedEntityEndpoints(this RouteGroupBuilder app)
    {
        app.MapGet("/{organisationId}/connected-entities",
            async (Guid organisationId, IUseCase<Guid, IEnumerable<ConnectedEntityLookup>> useCase) =>
               await useCase.Execute(organisationId)
                   .AndThen(entities => entities != null ? Results.Ok(entities) : Results.NotFound()))
           .Produces<List<ConnectedEntityLookup>>(StatusCodes.Status200OK, "application/json")
           .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
           .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
           .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
           .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
           .WithOpenApi(operation =>
           {
               operation.OperationId = "GetConnectedEntities";
               operation.Description = "Get connected entities summary by Organisation ID.";
               operation.Summary = "Get connected entities information by Organisation ID.";
               operation.Responses["200"].Description = "Connected entities details.";
               operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
               operation.Responses["404"].Description = "Connected entities information not found.";
               operation.Responses["422"].Description = "Unprocessable entity.";
               operation.Responses["500"].Description = "Internal server error.";
               return operation;
           });

        app.MapGet("/{organisationId}/connected-entities/{connectedEntityId}",
            async (Guid organisationId, Guid connectedEntityId, IUseCase<(Guid, Guid), ConnectedEntity?> useCase) =>
               await useCase.Execute((organisationId, connectedEntityId))
                   .AndThen(entity => entity != null ? Results.Ok(entity) : Results.NotFound()))
            .Produces<ConnectedEntity>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetConnectedEntity";
                operation.Description = "Get connected entity by ID.";
                operation.Summary = "Get connected entity by ID.";
                operation.Responses["200"].Description = "Connected entity details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Connected entity not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPost("/{organisationId}/connected-entities",
            async (Guid organisationId, RegisterConnectedEntity updateConnectedEntity,
                IUseCase<(Guid, RegisterConnectedEntity), bool> useCase) =>

                await useCase.Execute((organisationId, updateConnectedEntity))
                    .AndThen(_ => Results.NoContent())
            )
            .Produces(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreateConnectedEntity";
                operation.Description = "Create a new connected entity.";
                operation.Summary = "Create a new connected entity.";
                operation.Responses["201"].Description = "Connected entity created successfully.";
                operation.Responses["204"].Description = "Connected entity created successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Connected entity not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        return app;
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
        options.OperationFilter<ProblemDetailsOperationFilter>(Extensions.ServiceCollectionExtensions.ErrorCodes());
        options.ConfigureBearerSecurity();
        options.ConfigureApiKeySecurity();
        options.UseAllOfToExtendReferenceSchemas();
    }
}