using CO.CDP.Functional;
using CO.CDP.Organisation.WebApi.Model;
using CO.CDP.Organisation.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.Swashbuckle.SwaggerGen;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using System.Reflection;

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
                operation.Responses["401"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Valid authentication credentials are missing in the request"] = new OpenApiExample
                    {
                        Summary = "Unauthorized",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.2"),
                            ["title"] = new OpenApiString("Unauthorized"),
                            ["status"] = new OpenApiInteger(401)
                        }
                    }
                };
                operation.Responses["500"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Internal server error"] = new OpenApiExample
                    {
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.6.1"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(500),
                            ["code"] = new OpenApiString("GENERIC_ERROR")
                        }
                    }
                };
                return operation;
            });

        app.MapPost("/organisations", async (RegisterOrganisation command, IUseCase<RegisterOrganisation, Model.Organisation> useCase) =>
              await useCase.Execute(command)
              .AndThen(organisation =>
                  organisation != null
                      ? Results.Created(new Uri($"/organisations/{organisation.Id}", UriKind.Relative), organisation)
                      : Results.Problem("Organisation could not be created due to an internal error")))

            .Produces<Model.Organisation>(StatusCodes.Status201Created, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreateOrganisation";
                operation.Description = "Create a new organisation.";
                operation.Summary = "Create a new organisation.";
                operation.Responses["201"].Description = "Organisation created successfully.";
                operation.Responses["400"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["OrganisationAlreadyExists"] = new OpenApiExample
                    {
                        Summary = "Organisation already exists",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("The organisation already exists."),
                            ["code"] = new OpenApiString("ORGANISATION_ALREADY_EXISTS")
                        }
                    },
                    ["ArgumentNull"] = new OpenApiExample
                    {
                        Summary = "Argument null",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("A required argument was null."),
                            ["code"] = new OpenApiString("ARGUMENT_NULL")
                        }
                    },
                    ["InvalidOperation"] = new OpenApiExample
                    {
                        Summary = "Invalid operation",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("The operation is invalid."),
                            ["code"] = new OpenApiString("INVALID_OPERATION")
                        }
                    },
                    ["InvalidBuyerInformationUpdateEntity"] = new OpenApiExample
                    {
                        Summary = "Invalid buyer information update entity",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("The buyer information update entity is invalid."),
                            ["code"] = new OpenApiString("INVALID_BUYER_INFORMATION_UPDATE_ENTITY")
                        }
                    },
                    ["InvalidSupplierInformationUpdateEntity"] = new OpenApiExample
                    {
                        Summary = "Invalid supplier information update entity",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("The supplier information update entity is invalid."),
                            ["code"] = new OpenApiString("INVALID_SUPPLIER_INFORMATION_UPDATE_ENTITY")
                        }
                    },
                    ["RegisterOrganisationException"] = new OpenApiExample
                    {
                        Summary = "Register organisation exception",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("Error occurred while registering organisation."),
                            ["code"] = new OpenApiString("REGISTER_ORGANISATION_EXCEPTION")
                        }
                    }
                };
                operation.Responses["401"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Valid authentication credentials are missing in the request"] = new OpenApiExample
                    {
                        Summary = "Unauthorized",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.2"),
                            ["title"] = new OpenApiString("Unauthorized"),
                            ["status"] = new OpenApiInteger(401)
                        }
                    }
                };
                operation.Responses["422"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["UnprocessableEntity"] = new OpenApiExample
                    {
                        Summary = "Unprocessable entity",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("Unprocessable Entity"),
                            ["status"] = new OpenApiInteger(422),
                            ["detail"] = new OpenApiString("Error details."),
                            ["code"] = new OpenApiString("UNPROCESSABLE_ENTITY")
                        }
                    }
                };
                operation.Responses["404"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["UnknownPersonException"] = new OpenApiExample
                    {
                        Summary = "Unknown person",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(404),
                            ["detail"] = new OpenApiString("Unknown person {GUID}."),
                            ["code"] = new OpenApiString("PERSON_DOES_NOT_EXIST")
                        }
                    }
                };
                operation.Responses["500"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Internal server error"] = new OpenApiExample
                    {
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.6.1"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(500),
                            ["code"] = new OpenApiString("GENERIC_ERROR")
                        }
                    }
                };

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
               operation.Responses["401"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
               {
                   ["Valid authentication credentials are missing in the request"] = new OpenApiExample
                   {
                       Summary = "Unauthorized",
                       Value = new OpenApiObject
                       {
                           ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.2"),
                           ["title"] = new OpenApiString("Unauthorized"),
                           ["status"] = new OpenApiInteger(401)
                       }
                   }
               };
               operation.Responses["404"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
               {
                   ["Organisation supplier information not found"] = new OpenApiExample
                   {
                       Summary = "Organisation supplier information not found",
                       Value = new OpenApiObject
                       {
                           ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                           ["title"] = new OpenApiString("An error occurred while processing your request."),
                           ["status"] = new OpenApiInteger(404),
                           ["detail"] = new OpenApiString("Supplier information not found for the organisation."),
                           ["code"] = new OpenApiString("SUPPLIER_INFO_NOT_FOUND")
                       }
                   }
               };
               operation.Responses["500"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
               {
                   ["Internal server error"] = new OpenApiExample
                   {
                       Value = new OpenApiObject
                       {
                           ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.6.1"),
                           ["title"] = new OpenApiString("An error occurred while processing your request."),
                           ["status"] = new OpenApiInteger(500),
                           ["code"] = new OpenApiString("GENERIC_ERROR")
                       }
                   }
               };

               return operation;
           });

        app.MapPatch("/organisations/{organisationId}",
            async (Guid organisationId, UpdateOrganisation updateOrganisation,
                IUseCase<(Guid, UpdateOrganisation), bool> useCase) =>
                    await useCase.Execute((organisationId, updateOrganisation))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateOrganisation";
                operation.Description = "Update Organisation.";
                operation.Summary = "Update Organisation.";
                operation.Responses["204"].Description = "Organisation updated successfully.";
                operation.Responses["400"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["InvalidUpdateOrganisationCommand"] = new OpenApiExample
                    {
                        Summary = "Invalid update organisation command",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("The update organisation command is invalid."),
                            ["code"] = new OpenApiString("INVALID_UPDATE_ORGANISATION_COMMAND")
                        }
                    },
                    ["ArgumentNull"] = new OpenApiExample
                    {
                        Summary = "Argument null",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("A required argument was null."),
                            ["code"] = new OpenApiString("ARGUMENT_NULL")
                        }
                    }
                };
                operation.Responses["401"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Valid authentication credentials are missing in the request"] = new OpenApiExample
                    {
                        Summary = "Unauthorized",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.2"),
                            ["title"] = new OpenApiString("Unauthorized"),
                            ["status"] = new OpenApiInteger(401)
                        }
                    }
                };
                operation.Responses["404"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["UnknownOrganisationException"] = new OpenApiExample
                    {
                        Summary = "Unknown organisation",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(404),
                            ["detail"] = new OpenApiString("Unknown organisation {GUID}."),
                            ["code"] = new OpenApiString("UNKNOWN_ORGANISATION")
                        }
                    }
                };
                operation.Responses["422"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["UnprocessableEntity"] = new OpenApiExample
                    {
                        Summary = "Unprocessable entity",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("Unprocessable Entity"),
                            ["status"] = new OpenApiInteger(422),
                            ["detail"] = new OpenApiString("Error details."),
                            ["code"] = new OpenApiString("UNPROCESSABLE_ENTITY")
                        }
                    }
                };
                operation.Responses["500"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Internal server error"] = new OpenApiExample
                    {
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.6.1"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(500),
                            ["code"] = new OpenApiString("GENERIC_ERROR")
                        }
                    }
                };

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
            });
        app.MapGet("/lookup",
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
                operation.Tags = new List<OpenApiTag> { new() { Name = "Organisation - Lookup" } };
                operation.Responses["200"].Description = "Organisations Associated.";
                operation.Responses["401"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Valid authentication credentials are missing in the request"] = new OpenApiExample
                    {
                        Summary = "Unauthorized",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.2"),
                            ["title"] = new OpenApiString("Unauthorized"),
                            ["status"] = new OpenApiInteger(401)
                        }
                    }
                };
                operation.Responses["404"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Organisation supplier information not found"] = new OpenApiExample
                    {
                        Summary = "Organisation supplier information not found",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(404),
                            ["detail"] = new OpenApiString("Supplier information not found for the organisation."),
                            ["code"] = new OpenApiString("SUPPLIER_INFO_NOT_FOUND")
                        }
                    }
                };
                operation.Responses["500"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Internal server error"] = new OpenApiExample
                    {
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.6.1"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(500),
                            ["code"] = new OpenApiString("GENERIC_ERROR")
                        }
                    }
                };
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
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateBuyerInformation";
                operation.Description = "Update Buyer Information.";
                operation.Summary = "Update Buyer Information.";
                operation.Responses["204"].Description = "Buyer information updated successfully.";
                operation.Responses["400"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["InvalidUpdateBuyerInformationCommand"] = new OpenApiExample
                    {
                        Summary = "Invalid update buyer information command",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("The update buyer information command is invalid."),
                            ["code"] = new OpenApiString("INVALID_BUYER_INFORMATION_UPDATE_ENTITY")
                        }
                    },
                    ["ArgumentNull"] = new OpenApiExample
                    {
                        Summary = "Argument null",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("A required argument was null."),
                            ["code"] = new OpenApiString("ARGUMENT_NULL")
                        }
                    }
                };
                operation.Responses["401"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Valid authentication credentials are missing in the request"] = new OpenApiExample
                    {
                        Summary = "Unauthorized",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.2"),
                            ["title"] = new OpenApiString("Unauthorized"),
                            ["status"] = new OpenApiInteger(401)
                        }
                    }
                };
                operation.Responses["404"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["UnknownOrganisationException"] = new OpenApiExample
                    {
                        Summary = "Unknown organisation",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(404),
                            ["detail"] = new OpenApiString("Unknown organisation {GUID}."),
                            ["code"] = new OpenApiString("UNKNOWN_ORGANISATION")
                        }
                    },
                    ["BuyerInfoNotExistException"] = new OpenApiExample
                    {
                        Summary = "Buyer information not exists",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(404),
                            ["detail"] = new OpenApiString("Buyer information does not exist for the organisation."),
                            ["code"] = new OpenApiString("BUYER_INFO_NOT_EXISTS")
                        }
                    }
                };
                operation.Responses["422"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["UnprocessableEntity"] = new OpenApiExample
                    {
                        Summary = "Unprocessable entity",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("Unprocessable Entity"),
                            ["status"] = new OpenApiInteger(422),
                            ["detail"] = new OpenApiString("Error details."),
                            ["code"] = new OpenApiString("UNPROCESSABLE_ENTITY")
                        }
                    }
                };
                operation.Responses["500"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Internal server error"] = new OpenApiExample
                    {
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.6.1"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(500),
                            ["code"] = new OpenApiString("GENERIC_ERROR")
                        }
                    }
                };
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
           .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
           .WithOpenApi(operation =>
           {
               operation.OperationId = "GetOrganisationSupplierInformation";
               operation.Description = "Get organisation supplier information by ID.";
               operation.Summary = "Get organisation supplier information by ID.";
               operation.Responses["200"].Description = "Organisation supplier information details.";
               operation.Responses["401"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
               {
                   ["Valid authentication credentials are missing in the request"] = new OpenApiExample
                   {
                       Summary = "Unauthorized",
                       Value = new OpenApiObject
                       {
                           ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.2"),
                           ["title"] = new OpenApiString("Unauthorized"),
                           ["status"] = new OpenApiInteger(401)
                       }
                   }
               };
               operation.Responses["404"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
               {
                   ["Organisation supplier information not found"] = new OpenApiExample
                   {
                       Summary = "Organisation supplier information not found",
                       Value = new OpenApiObject
                       {
                           ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                           ["title"] = new OpenApiString("An error occurred while processing your request."),
                           ["status"] = new OpenApiInteger(404),
                           ["detail"] = new OpenApiString("Supplier information not found for the organisation."),
                           ["code"] = new OpenApiString("SUPPLIER_INFO_NOT_FOUND")
                       }
                   }
               };
               operation.Responses["500"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
               {
                   ["Internal server error"] = new OpenApiExample
                   {
                       Value = new OpenApiObject
                       {
                           ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.6.1"),
                           ["title"] = new OpenApiString("An error occurred while processing your request."),
                           ["status"] = new OpenApiInteger(500),
                           ["code"] = new OpenApiString("GENERIC_ERROR")
                       }
                   }
               };
               return operation;
           });

        app.MapPatch("/{organisationId}/supplier-information",
            async (Guid organisationId, UpdateSupplierInformation supplierInformation,
                IUseCase<(Guid, UpdateSupplierInformation), bool> useCase) =>
                    await useCase.Execute((organisationId, supplierInformation))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdateSupplierInformation";
                operation.Description = "Update Supplier Information.";
                operation.Summary = "Update Supplier Information.";
                operation.Responses["204"].Description = "Supplier information updated successfully.";
                operation.Responses["400"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["InvalidUpdateSupplierInformationCommand"] = new OpenApiExample
                    {
                        Summary = "Invalid update supplier information command",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("The update supplier information command is invalid."),
                            ["code"] = new OpenApiString("INVALID_SUPPLIER_INFORMATION_UPDATE_ENTITY")
                        }
                    },
                    ["ArgumentNull"] = new OpenApiExample
                    {
                        Summary = "Argument null",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("A required argument was null."),
                            ["code"] = new OpenApiString("ARGUMENT_NULL")
                        }
                    }
                };
                operation.Responses["401"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Valid authentication credentials are missing in the request"] = new OpenApiExample
                    {
                        Summary = "Unauthorized",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.2"),
                            ["title"] = new OpenApiString("Unauthorized"),
                            ["status"] = new OpenApiInteger(401)
                        }
                    }
                };
                operation.Responses["404"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["UnknownOrganisationException"] = new OpenApiExample
                    {
                        Summary = "Unknown organisation",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(404),
                            ["detail"] = new OpenApiString("Unknown organisation {GUID}."),
                            ["code"] = new OpenApiString("UNKNOWN_ORGANISATION")
                        }
                    },
                    ["SupplierInfoNotExistException"] = new OpenApiExample
                    {
                        Summary = "Supplier information not exists",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(404),
                            ["detail"] = new OpenApiString("Supplier information does not exist for the organisation."),
                            ["code"] = new OpenApiString("SUPPLIER_INFO_NOT_EXISTS")
                        }
                    }
                };
                operation.Responses["422"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["UnprocessableEntity"] = new OpenApiExample
                    {
                        Summary = "Unprocessable entity",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("Unprocessable Entity"),
                            ["status"] = new OpenApiInteger(422),
                            ["detail"] = new OpenApiString("Error details."),
                            ["code"] = new OpenApiString("UNPROCESSABLE_ENTITY")
                        }
                    }
                };
                operation.Responses["500"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Internal server error"] = new OpenApiExample
                    {
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.6.1"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(500),
                            ["code"] = new OpenApiString("GENERIC_ERROR")
                        }
                    }
                };
                return operation;
            });

        app.MapDelete("/{organisationId}/supplier-information",
            async (Guid organisationId, [FromBody] DeleteSupplierInformation deleteSupplierInformation,
                IUseCase<(Guid, DeleteSupplierInformation), bool> useCase) =>
                    await useCase.Execute((organisationId, deleteSupplierInformation))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "DeleteSupplierInformation";
                operation.Description = "Delete Supplier Information.";
                operation.Summary = "Delete Supplier Information.";
                operation.Responses["204"].Description = "Supplier information deleted successfully.";
                operation.Responses["400"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["InvalidUpdateSupplierInformationCommand"] = new OpenApiExample
                    {
                        Summary = "Invalid update supplier information command",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("The update supplier information command is invalid."),
                            ["code"] = new OpenApiString("INVALID_SUPPLIER_INFORMATION_UPDATE_ENTITY")
                        }
                    },
                    ["ArgumentNull"] = new OpenApiExample
                    {
                        Summary = "Argument null",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                            ["title"] = new OpenApiString("Bad Request"),
                            ["status"] = new OpenApiInteger(400),
                            ["detail"] = new OpenApiString("A required argument was null."),
                            ["code"] = new OpenApiString("ARGUMENT_NULL")
                        }
                    }
                };
                operation.Responses["401"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Valid authentication credentials are missing in the request"] = new OpenApiExample
                    {
                        Summary = "Unauthorized",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.2"),
                            ["title"] = new OpenApiString("Unauthorized"),
                            ["status"] = new OpenApiInteger(401)
                        }
                    }
                };
                operation.Responses["404"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["SupplierInfoNotExistException"] = new OpenApiExample
                    {
                        Summary = "Supplier information not exists",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(404),
                            ["detail"] = new OpenApiString("Supplier information does not exist for the organisation."),
                            ["code"] = new OpenApiString("SUPPLIER_INFO_NOT_EXISTS")
                        }
                    }
                };
                operation.Responses["422"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["UnprocessableEntity"] = new OpenApiExample
                    {
                        Summary = "Unprocessable entity",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("Unprocessable Entity"),
                            ["status"] = new OpenApiInteger(422),
                            ["detail"] = new OpenApiString("Error details."),
                            ["code"] = new OpenApiString("UNPROCESSABLE_ENTITY")
                        }
                    }
                };
                operation.Responses["500"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
                {
                    ["Internal server error"] = new OpenApiExample
                    {
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.6.1"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(500),
                            ["code"] = new OpenApiString("GENERIC_ERROR")
                        }
                    }
                };
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
                    { typeof(UpdateOrganisation), "UpdatedOrganisation" }
                };
            return typeMap.GetValueOrDefault(type, type.Name);
        });
        options.IncludeXmlComments(Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(Address)));
        options.OperationFilter<ProblemDetailsOperationFilter>();
        options.ConfigureBearerSecurity();
        options.ConfigureApiKeySecurity();
        options.UseAllOfToExtendReferenceSchemas();
    }
}