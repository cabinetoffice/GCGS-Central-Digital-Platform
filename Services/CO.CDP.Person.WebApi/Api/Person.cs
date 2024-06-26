using CO.CDP.Functional;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using CO.CDP.Swashbuckle.Security;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

namespace CO.CDP.Person.WebApi.Api;

public static class EndpointExtensions
{
    private static Dictionary<Guid, Model.Person> _persons = Enumerable.Range(1, 5)
        .Select(_ => Guid.NewGuid())
        .ToDictionary(index => index, index => new Model.Person
        {
            Id = index,
            FirstName = $"Sussan Tables {index}",
            LastName = "LN",
            Email = "sussan@example.com"
        });

    public static void UsePersonEndpoints(this WebApplication app)
    {
        app.MapPost("/persons", async (RegisterPerson command, IUseCase<RegisterPerson, Model.Person> useCase) =>
         await useCase.Execute(command)
                .AndThen(person =>
                    Results.Created(new Uri($"/persons/{person.Id}", UriKind.Relative), person))
         )
        .Produces<Model.Person>(StatusCodes.Status201Created, "application/json")
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .WithOpenApi(operation =>
        {
            operation.OperationId = "CreatePerson";
            operation.Description = "Create a new person.";
            operation.Summary = "Create a new person.";
            operation.Responses["201"].Description = "Person created successfully.";
            operation.Responses["400"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
            {
                ["UnknownPersonException"] = new OpenApiExample
                {
                    Summary = "Duplicate person",
                    Value = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.6.1"),
                        ["title"] = new OpenApiString("Duplicate person"),
                        ["status"] = new OpenApiInteger(400),
                        ["code"] = new OpenApiString("PERSON_ALREADY_EXISTS")
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
            operation.Responses["500"].Content["application/json"].Examples = new Dictionary<string, OpenApiExample>
            {
                ["UnknownPersonException"] = new OpenApiExample
                {
                    Summary = "Unknown person",
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

        app.MapGet("/persons/{personId}", async (Guid personId, IUseCase<Guid, Model.Person?> useCase) =>
                await useCase.Execute(personId)
                    .AndThen(person => person != null ? Results.Ok(person) : Results.NotFound()))
            .Produces<Model.Person>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetPerson";
                operation.Description = "Get a person by ID.";
                operation.Summary = "Get a person by ID.";
                operation.Responses["200"].Description = "Person details.";
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
                    ["The requested person was not found"] = new OpenApiExample
                    {
                        Summary = "The requested person was not found",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(404),
                            ["detail"] = new OpenApiString("The requested person was not found"),
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

        app.MapPut("/persons/{personId}",
                (Guid personId, UpdatePerson updatedPerson) =>
                {
                    _persons[personId] = new Model.Person
                    {
                        Id = personId,
                        Email = updatedPerson.Email,
                        FirstName = updatedPerson.FirstName,
                        LastName = updatedPerson.LastName,
                    };
                    return Results.Ok(_persons[personId]);
                })
            .Produces<Model.Person>(200, "application/json")
            .Produces(404)
            .Produces(400)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdatePerson";
                operation.Description = "[STUB] Update a person [STUB]";
                operation.Summary = "[STUB] Update a person [STUB]";
                operation.Responses["200"].Description = "Person updated.";
                return operation;
            });
        app.MapDelete("/persons/{personId}", (Guid personId) =>
        {
            _persons.Remove(personId);
            return Results.NoContent();
        })
            .Produces(204)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "DeletePerson";
                operation.Description = "[STUB] Delete a person. [STUB]";
                operation.Summary = "[STUB] Delete a person. [STUB]";
                operation.Responses["204"].Description = "Person deleted.";
                return operation;
            });
    }

    public static void UsePersonLookupEndpoints(this WebApplication app)
    {
        app.MapGet("/persons/lookup",
                async ([FromQuery] string urn, IUseCase<string, Model.Person?> useCase) =>
                await useCase.Execute(urn)
                    .AndThen(persons => persons != null ? Results.Ok(persons) : Results.NotFound()))
            .Produces<Model.Person>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "LookupPerson";
                operation.Description = "Lookup person by user principal.";
                operation.Summary = "Lookup person by user principal.";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Person Lookup" } };
                operation.Responses["200"].Description = "Person Associated.";
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
                    ["The requested person was not found"] = new OpenApiExample
                    {
                        Summary = "The requested person was not found",
                        Value = new OpenApiObject
                        {
                            ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc4918#section-11.2"),
                            ["title"] = new OpenApiString("An error occurred while processing your request."),
                            ["status"] = new OpenApiInteger(404),
                            ["detail"] = new OpenApiString("The requested person was not found"),
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
    }
}

public static class ApiExtensions
{
    public static void DocumentPersonApi(this SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "1.0.0",
            Title = "Person API",
            Description = "API for creating, updating, deleting, and listing persons.",
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
                    { typeof(RegisterPerson), "NewPerson" },
                    { typeof(UpdatePerson), "UpdatedPerson" },
                };
            return typeMap.GetValueOrDefault(type, type.Name);
        });
        options.ConfigureBearerSecurity();
    }
}