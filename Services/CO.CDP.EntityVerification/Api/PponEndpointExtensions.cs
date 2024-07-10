using CO.CDP.Swashbuckle.Filter;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace CO.CDP.EntityVerification.Api;

public static class PponEndpointExtensions
{
    public static void UsePponEndpoints(this WebApplication app)
    {
        app.MapGet("/identifiers/{identifier}", (string _) => Task.FromResult(Results.NotFound()))
            .Produces<string>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetIdentifiers";
                operation.Description = "Get related identifiers.";
                operation.Summary = "Get related identifiers.";
                operation.Responses["200"].Description = "List of identifiers.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Identifier not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });
    }
}

public static class PponApiExtensions
{
    public static void DocumentPponApi(this SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "1.0.0",
            Title = "PPON Service API",
            Description = "API for organisation identifier queries.",
        });
        options.OperationFilter<ProblemDetailsOperationFilter>(Extensions.ServiceCollectionExtensions.ErrorCodes());
    }
}