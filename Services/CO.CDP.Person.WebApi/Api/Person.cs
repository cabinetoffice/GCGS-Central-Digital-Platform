using CO.CDP.Functional;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
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
        .Produces<Model.Person>(201, "application/json")
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithOpenApi(operation =>
        {
            operation.OperationId = "CreatePerson";
            operation.Description = "Create a new person.";
            operation.Summary = "Create a new person.";
            operation.Responses["201"].Description = "Person created successfully.";
            operation.Responses["400"].Description = "Bad request.";
            operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
            operation.Responses["422"].Description = "Unprocessable entity.";
            operation.Responses["500"].Description = "Internal server error.";
            return operation;
        });

        app.MapGet("/persons/{personId}", async (Guid personId, IUseCase<Guid, Model.Person?> useCase) =>
                await useCase.Execute(personId)
                    .AndThen(person => person != null ? Results.Ok(person) : Results.NotFound()))
            .Produces<Model.Person>(200, "application/json")
            .Produces(404)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetPerson";
                operation.Description = "Get a person by ID.";
                operation.Summary = "Get a person by ID.";
                operation.Responses["200"].Description = "Person details.";
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
            .Produces<Model.Person>(200, "application/json")
            .Produces(404)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "LookupPerson";
                operation.Description = "Lookup person by user principal.";
                operation.Summary = "Lookup person by user principal.";
                operation.Tags = new List<OpenApiTag> { new() { Name = "Person Lookup" } };
                operation.Responses["200"].Description = "Person Associated.";
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
        }
       );
    }
}