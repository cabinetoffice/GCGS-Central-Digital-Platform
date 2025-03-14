using CO.CDP.Authentication.Authorization;
using CO.CDP.Functional;
using CO.CDP.Person.WebApi.Model;
using CO.CDP.Person.WebApi.UseCase;
using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.WebApi.Foundation;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace CO.CDP.Person.WebApi.Api;

public static class EndpointExtensions
{
    public static void UsePersonEndpoints(this WebApplication app)
    {
        app.MapPost("/persons", async (RegisterPerson command, IUseCase<RegisterPerson, Model.Person> useCase) =>
         await useCase.Execute(command)
                .AndThen(person =>
                    Results.Created(new Uri($"/persons/{person.Id}", UriKind.Relative), person))
         )
        .Produces<Model.Person>(201, "application/json")
        .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
        .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
        .Produces<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)
        .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
        .WithOpenApi(operation =>
        {
            operation.OperationId = "CreatePerson";
            operation.Description = "Create a new person.";
            operation.Summary = "Create a new person.";
            operation.Responses["201"].Description = "Person created successfully.";
            operation.Responses["400"].Description = "Bad request.";
            operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
            operation.Responses["404"].Description = "Person not found.";
            operation.Responses["422"].Description = "Unprocessable entity.";
            operation.Responses["500"].Description = "Internal server error.";
            return operation;
        });

        app.MapGet("/persons/{personId}",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin])]
        async (Guid personId, IUseCase<Guid, Model.Person?> useCase) =>
                await useCase.Execute(personId)
                    .AndThen(person => person != null ? Results.Ok(person) : Results.NotFound()))
            .Produces<Model.Person>(200, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetPerson";
                operation.Description = "Get a person by ID.";
                operation.Summary = "Get a person by ID.";
                operation.Responses["200"].Description = "Person details.";
                operation.Responses["404"].Description = "Person not found.";
                return operation;
            });

        app.MapPost("/persons/{personId}/claim-person-invite",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin])]
        async (Guid personId, ClaimPersonInvite command, IUseCase<(Guid, ClaimPersonInvite), bool> useCase) =>
                    await useCase.Execute((personId, command))
                        .AndThen(_ => Results.NoContent())
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "ClaimPersonInvite";
                operation.Description = "Claims a person invite.";
                operation.Summary = "Claims a person invite.";
                operation.Responses["204"].Description = "Person invite claimed successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Person invite not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapPatch("/persons/{personId}",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin])]
            async (Guid personId, UpdatePerson updatePerson,
                IUseCase<(Guid, UpdatePerson), bool> useCase) =>
                    await useCase.Execute((personId, updatePerson))
                        .AndThen(_ => Results.NoContent()))
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "UpdatePerson";
                operation.Description = "Update Person.";
                operation.Summary = "Update Person.";
                operation.Responses["204"].Description = "Person updated successfully.";
                operation.Responses["400"].Description = "Bad request.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Person not found.";
                operation.Responses["422"].Description = "Unprocessable entity.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapDelete("/persons/{personId}", (Guid personId) =>
        {
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
            async ([FromQuery] string? urn, [FromQuery] string ? email, IUseCase<LookupPerson, Model.Person?> useCase) => {
                if (string.IsNullOrWhiteSpace(urn) && string.IsNullOrWhiteSpace(email))
                {
                    return Results.BadRequest("Either Urn or Email must be provided.");
                }

                return await useCase.Execute(new LookupPerson(urn, email))
                    .AndThen(persons => persons != null ? Results.Ok(persons) : Results.NotFound());
            })
            .Produces<Model.Person>(200, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
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
    public static void DocumentPersonApi(this SwaggerGenOptions options, IConfigurationManager configuration)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = configuration.GetValue("Version", "dev"),
            Title = "Person API",
            Description = "API for creating, updating, deleting, and listing persons."
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
        options.OperationFilter<ProblemDetailsOperationFilter>(ErrorCodes.Exception4xxMap.HttpStatusCodeErrorMap());
        options.ConfigureBearerSecurity();
        options.ConfigureApiKeySecurity();
        options.UseAllOfToExtendReferenceSchemas();
    }
}