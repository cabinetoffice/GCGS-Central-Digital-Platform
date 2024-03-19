using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using CO.CDP.Common.Auth;

namespace CO.CDP.Login.WebApi.Api;
public static class EndpointExtensions
{

    public static void UseLoginEndpoints(this WebApplication app)
    {
        app.MapGet("/fake-onelogin/", () =>
        {
            var sub = "urn:fdc:gov.uk:2022:56P4CMsGh_-2sVIB2nsNU7mcLZYhYw=";
            var email = "test@example.com";
            var emailVerified = true;
            var phoneNumber = "01406946277";
            var phoneNumberVerified = true;
            var updatedAt = 1311280970L;

            var oneLoginResponce = new OneLoginResponce(sub, email, emailVerified, phoneNumber, phoneNumberVerified, updatedAt);

            return Results.Ok(oneLoginResponce);
        })
            .Produces(StatusCodes.Status200OK)
            .Produces(404)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetLogin";
                operation.Description = "Get a fake login payload.";
                operation.Summary = "Get a fake login payload.";
                operation.Responses["200"].Description = "OneLogin payload.";
                return operation;
            });
    }
}

public static class ApiExtensions
{
    public static void DocumentLoginApi(this SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "1.0.0.0",
            Title = "Login API",
            Description = "",
        });
    }
}