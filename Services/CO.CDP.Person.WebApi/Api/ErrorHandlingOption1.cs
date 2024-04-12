using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.ComponentModel.DataAnnotations;

namespace CO.CDP.Person.WebApi.Api.ErrorSpikeOne;
internal record Person
{
    [Required(AllowEmptyStrings = true)] public required string Id { get; init; }
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    public required int Age { get; init; }
    [EmailAddress] public required string Email { get; init; }
}

internal record NewPerson
{
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    public required int Age { get; init; }
    [EmailAddress] public required string Email { get; init; }
}

internal record UpdatedPerson
{
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    public required int Age { get; init; }
    [EmailAddress] public required string Email { get; init; }
}

public static class EndpointExtensions
{

    public static RouteHandlerBuilder ProducesProblemDetails(this RouteHandlerBuilder builder, int statusCode)
    {
        return builder.Produces<ProblemDetails>(statusCode);
    }

    private static Dictionary<string, Person> _persons = Enumerable.Range(1, 5)
        .ToDictionary(index => index.ToString(), index => new Person
        {
            Id = index.ToString(),
            Name = $"Sussan Tables {index}",
            Age = 40 + index,
            Email = "sussan@example.com"
        });

    public static void UsePersonEndpoints(this WebApplication app)
    {
        app.MapGet("/persons", () => _persons.Values.ToArray())
            .Produces<List<Person>>(200, "application/json")
            .WithOpenApi(operation =>
            {
                operation.OperationId = "ListPersons";
                operation.Description = "A list of persons.";
                operation.Summary = "A list of persons.";
                operation.Responses["200"].Description = "A list of persons.";
                return operation;
            });
        app.MapPost("/persons", ([FromBody] NewPerson newPerson) =>
            {
                var person = new Person
                {
                    Id = $"{_persons.Count + 1}",
                    Name = newPerson.Name,
                    Age = newPerson.Age,
                    Email = newPerson.Email
                };
                _persons[person.Id] = person;
                return Results.Created(new Uri($"/persons/{person.Id}"), person);
            })
            .Produces<List<Person>>(201, "application/json")
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreatePerson";
                operation.Description = "Create a new person.";
                operation.Summary = "Create a new person.";
                operation.Responses["201"].Description = "Person created.";
                return operation;
            });
        app.MapGet("/persons/{personId}", (String personId) =>
            {
                if (!_persons.TryGetValue(personId, out var person))
                {
                    return Results.Problem(
                        title: "Not Found",
                        detail: $"No person found with ID {personId}.",
                        statusCode: 404);
                }
                return Results.Ok(person);
            })
            .Produces<Person>(200, "application/json")
            .ProducesProblemDetails(404)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetPerson";
                operation.Description = "Get a person by ID.";
                operation.Summary = "Get a person by ID.";
                operation.Responses["200"].Description = "Person details.";
                operation.Responses["404"].Description = "No person found with the specified ID.";
                return operation;
            });
        app.MapDelete("/persons/{personId}", (String personId) =>
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
      app.MapPut("/persons/{personId}", (String personId, UpdatedPerson updatedPerson) =>
        {
            if (!_persons.ContainsKey(personId))
            {
                return Results.Problem(
                    title: "Not Found",
                    detail: $"No person found with ID {personId} to update.",
                    statusCode: 404);
            }
            try
            {
                _persons[personId] = new Person
                {
                    Id = personId,
                    Name = updatedPerson.Name,
                    Age = updatedPerson.Age,
                    Email = updatedPerson.Email
                };
                return Results.Ok(_persons[personId]);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Error Updating Person",
                    detail: $"An error occurred while updating the person: {ex.Message}",
                    statusCode: 500);
            }
        })
        .Produces<Person>(200, "application/json")
        .ProducesProblemDetails(404)
        .ProducesProblemDetails(500)
        .WithOpenApi(operation =>
        {
            operation.OperationId = "UpdatePerson";
            operation.Description = "Update a person";
            operation.Summary = "Update a person";
            operation.Responses["200"].Description = "Person updated.";
            operation.Responses["404"].Description = "No person found with the specified ID to update.";
            operation.Responses["500"].Description = "An error occurred while updating the person.";
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
            Description = "",
        });
    }
}
