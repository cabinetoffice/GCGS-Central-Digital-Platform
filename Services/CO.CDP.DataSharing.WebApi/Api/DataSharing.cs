using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation;
using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.Swashbuckle.SwaggerGen;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Net.Mime;
using System.Reflection;

namespace CO.CDP.DataSharing.WebApi.Api;

public static class EndpointExtensions
{
    public static void UseDataSharingEndpoints(this WebApplication app)
    {
        app.MapGet("/share/data/{sharecode}", async (string sharecode,
            IUseCase<string, SupplierInformation?> useCase) => await useCase.Execute(sharecode)
             .AndThen(supplierInformation => supplierInformation != null ? Results.Ok(supplierInformation) : Results.NotFound()))
            .Produces<SupplierInformation>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetSharedData";
                operation.Description =
                    "Operation to obtain Supplier information which has been shared as part of a notice. ";
                operation.Summary = "Request Supplier Submitted Information. ";
                operation.Responses["200"].Description = "Organisation Information including Form Answers.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Share code not found or the caller is not authorised to use it.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            })
            .RequireAuthorization(Authentication.AuthorizationPolicy.Constants.OrganisationApiKeyPolicy);

        app.MapGet("/share/data/{sharecode}/pdf",
             async (string sharecode, IUseCase<string, byte[]?> useCase) =>
                 await useCase.Execute(sharecode)
                     .AndThen(res => res != null ? Results.File(res, MediaTypeNames.Application.Pdf, $"{sharecode}.pdf") : Results.NotFound()))
             .Produces<byte[]?>(StatusCodes.Status200OK, MediaTypeNames.Application.Pdf)
             .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
             .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
             .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
             .WithOpenApi(operation =>
             {
                 operation.OperationId = "GetSharedDataPdf";
                 operation.Description =
                     "Operation to obtain Supplier Information as Pdf.";
                 operation.Summary = "Request Supplier Information as Pdf.";
                 operation.Responses["200"].Description = "Supplier Information in Pdf format.";
                 operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                 operation.Responses["404"].Description = "Share code not found or the caller is not authorised to use it.";
                 operation.Responses["500"].Description = "Internal server error.";
                 return operation;
             })
             .RequireAuthorization(Authentication.AuthorizationPolicy.Constants.OneLoginPolicy);

        app.MapPost("/share/data", async (ShareRequest shareRequest, IUseCase<ShareRequest, ShareReceipt> useCase) =>
                await useCase.Execute(shareRequest)
                    .AndThen(shareReceipt => Results.Ok(shareReceipt)))
            .Produces<ShareReceipt>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "CreateSharedData";
                operation.Description =
                    "Operation to obtain Supplier information which has been shared as part of a notice.";
                operation.Summary = "Create Supplier Submitted Information.";
                operation.Responses["200"].Description = "Organisation Information created.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            })
            .RequireAuthorization(Authentication.AuthorizationPolicy.Constants.OneLoginPolicy);

        app.MapPost("/share/data/verify", async (ShareVerificationRequest request, IUseCase<ShareVerificationRequest, ShareVerificationReceipt> useCase) =>
                await useCase.Execute(request)
                    .AndThen(shareVerificationReceipt => shareVerificationReceipt != null ? Results.Ok(shareVerificationReceipt) : Results.NotFound()))
            .Produces<ShareVerificationReceipt>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "VerifySharedData";
                operation.Description =
                    "Operation to verify if shared data is the latest version available.";
                operation.Summary = "Create Supplier Submitted Information.";
                operation.Responses["200"].Description = "Share code and version verification.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Share code not found or the caller is not authorised to use it.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            })
            .RequireAuthorization(Authentication.AuthorizationPolicy.Constants.OrganisationApiKeyPolicy);

        app.MapGet("/share/organisations/{organisationId}/codes", async (Guid organisationId,
            IUseCase<Guid, List<Model.SharedConsent>?> useCase) => await useCase.Execute(organisationId)
             .AndThen(sharedCodes => sharedCodes != null ? Results.Ok(sharedCodes) : Results.NotFound()))
             .Produces<List<Model.SharedConsent>>(StatusCodes.Status200OK, "application/json")
             .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
             .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
             .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
             .WithOpenApi(operation =>
             {
                 operation.OperationId = "GetShareCodeList";
                 operation.Description = "Get Share Code List.";
                 operation.Summary = "Get a List of Share Code for an Organisation.";
                 operation.Responses["200"].Description = "List of Share Code.";
                 operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                 operation.Responses["404"].Description = "Share Codes not found.";
                 operation.Responses["500"].Description = "Internal server error.";
                 return operation;
             })
            .RequireAuthorization(Authentication.AuthorizationPolicy.Constants.OneLoginPolicy);

        app.MapGet("/share/organisations/{organisationId}/codes/{sharecode}",
                async (Guid organisationId, string shareCode, IUseCase<(Guid, string), SharedConsentDetails?> useCase)
                    => await useCase.Execute((organisationId, shareCode))
            .AndThen(sharedCodeDetails => sharedCodeDetails != null ? Results.Ok(sharedCodeDetails) : Results.NotFound()))
            .Produces<Model.SharedConsentDetails?>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetShareCodeDetails";
                operation.Description = "Get Share Code details.";
                operation.Summary = "Get Share Code details for an Organisation and a Share code.";
                operation.Responses["200"].Description = "Share Code Details.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Share Code Details not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            })
            .RequireAuthorization(Authentication.AuthorizationPolicy.Constants.OneLoginPolicy);
    }
}

public static class ApiExtensions
{
    public static void DocumentDataSharingApi(this SwaggerGenOptions options, IConfigurationManager configuration)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = configuration.GetValue("Version", "dev"),
            Title = "Data Sharing API",
            Description = "",
        });
        options.IncludeXmlComments(Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(Address)));
        options.OperationFilter<ProblemDetailsOperationFilter>(Extensions.ServiceCollectionExtensions.ErrorCodes());
        options.ConfigureBearerSecurity();
        options.ConfigureApiKeySecurity();
    }
}