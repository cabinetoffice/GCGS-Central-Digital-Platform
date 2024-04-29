using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace CO.CDP.Person.WebApi.Api;

public static class EndpointExtensions
{
    private static Dictionary<Guid, Model.Person> _persons = Enumerable.Range(1, 5)
        .Select(_ => Guid.NewGuid())
        .ToDictionary(index => index, index => new Model.Person
        {
            Id = index,
            Name = $"Sussan Tables {index}",
            Age = 40,
            Email = "sussan@example.com"
        });

    public static void UsePersonEndpoints(this WebApplication app)
    {
        app.MapGet("/persons", () => _persons.Values.ToArray())
            .Produces<List<Model.Person>>(200, "application/json")
            .WithOpenApi(operation =>
            {
                operation.OperationId = "ListPersons";
                operation.Description = "A list of persons.";
                operation.Summary = "A list of persons.";
                operation.Responses["200"].Description = "A list of persons.";
                return operation;
            });
        app.MapPost("/persons", async (RegisterPerson command, IUseCase<RegisterPerson, Model.Person> useCase) =>
            await useCase.Execute(command)
                .AndThen(person =>
                    person != null
                        ? Results.Created(new Uri($"/persons/{person.Id}", UriKind.Relative), person)
                        : Results.Problem("Person could not be created due to an internal error"))
                )
        .Produces<Model.Person>(201, "application/json")
        .ProducesProblem(500)
        .WithOpenApi(operation =>
        {
            operation.OperationId = "CreatePerson";
            operation.Description = "Create a new person.";
            operation.Summary = "Create a new person.";
            operation.Responses["201"].Description = "Person created successfully.";
            return operation;
        });

        app.MapGet("/persons/{personId}", (Guid personId) =>
        {
            try
            {
                return Results.Ok(_persons[personId]);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        })
            .Produces<Model.Person>(200, "application/json")
            .Produces(404)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetPersons";
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
                        Name = updatedPerson.Name,
                        Age = updatedPerson.Age,
                    };
                    return Results.Ok(_persons[personId]);
                })
            .Produces<Model.Person>(200, "application/json")
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdatePerson";
                operation.Description = "Update a person";
                operation.Summary = "Update a person";
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
                operation.Description = "Delete a person.";
                operation.Summary = "Delete a person.";
                operation.Responses["204"].Description = "Person deleted.";
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
            Version = "1.0.0.0",
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
    }
}