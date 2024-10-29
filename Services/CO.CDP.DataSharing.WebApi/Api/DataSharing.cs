using CO.CDP.Authentication.Authorization;
using CO.CDP.DataSharing.WebApi.Model;
using CO.CDP.DataSharing.WebApi.UseCase;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation;
using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.Swashbuckle.SwaggerGen;
using CO.CDP.WebApi.Foundation;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using static CO.CDP.Authentication.Constants;

namespace CO.CDP.DataSharing.WebApi.Api;

public static class EndpointExtensions
{
    public static void UseDataSharingEndpoints(this WebApplication app)
    {
        app.MapGet("/share/data/{sharecode}",
            [OrganisationAuthorize([AuthenticationChannel.OrganisationKey])]
        async (string sharecode,
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
                    "Operation to obtain Supplier information which has been shared as part of a notice.";
                operation.Summary = "Request Supplier Submitted Information. ";
                operation.Responses["200"].Description = "Organisation Information including Form Answers.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Share code not found or the caller is not authorised to use it.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapGet("/share/data/{sharecode}/document/{documentId}",
            [OrganisationAuthorize([AuthenticationChannel.OrganisationKey])]
        async (string sharecode, string documentId,
            IUseCase<(string, string), string?> useCase) => await useCase.Execute((sharecode, documentId))
             .AndThen(url => url != null ? Results.Redirect(url) : Results.NotFound()))
            .Produces(StatusCodes.Status302Found)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetSharedDataDocumentDownloadUrl";
                operation.Description = "Operation to obtain direct download url for a document supplied within Supplier information which has been shared as part of a notice..";
                operation.Summary = "Request Document within Supplier Submitted Information.";
                operation.Responses["302"].Description = "Redirect to direct download url.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Share code not found or the caller is not authorised to use it.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapGet("/share/data/{sharecode}/file",
            [OrganisationAuthorize([AuthenticationChannel.OneLogin])]
        async (string sharecode, IUseCase<string, SharedDataFile?> useCase) =>
                 await useCase.Execute(sharecode)
                     .AndThen(res => res != null ? Results.File(res.Content, res.ContentType, res.FileName) : Results.NotFound()))
             .Produces<FileContentResult>(StatusCodes.Status200OK)
             .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
             .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
             .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
             .WithOpenApi(operation =>
             {
                 operation.OperationId = "GetSharedDataFile";
                 operation.Description = "Operation to obtain Supplier Information as file.";
                 operation.Summary = "Request Supplier Information as file.";
                 operation.Responses["200"].Description = "Supplier Information information.";
                 operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                 operation.Responses["404"].Description = "Share code not found or the caller is not authorised to use it.";
                 operation.Responses["500"].Description = "Internal server error.";
                 return operation;
             });

        app.MapPost("/share/data",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [OrganisationPersonScope.Admin, OrganisationPersonScope.Editor],
                OrganisationIdLocation.Body)]
        async (ShareRequest shareRequest, IUseCase<ShareRequest, ShareReceipt> useCase) =>
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
            });

        app.MapPost("/share/data/verify",
            [OrganisationAuthorize([AuthenticationChannel.OrganisationKey])]
        async (ShareVerificationRequest request, IUseCase<ShareVerificationRequest, ShareVerificationReceipt> useCase) =>
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
            });

        app.MapGet("/share/organisations/{organisationId}/codes",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [OrganisationPersonScope.Admin, OrganisationPersonScope.Editor, OrganisationPersonScope.Viewer],
                OrganisationIdLocation.Path,
                [PersonScope.SupportAdmin])]
        async (Guid organisationId,
            IUseCase<Guid, List<SharedConsent>?> useCase) => await useCase.Execute(organisationId)
             .AndThen(sharedCodes => sharedCodes != null ? Results.Ok(sharedCodes) : Results.NotFound()))
             .Produces<List<SharedConsent>>(StatusCodes.Status200OK, "application/json")
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
             });

        app.MapGet("/share/organisations/{organisationId}/codes/{sharecode}",
            [OrganisationAuthorize(
                [AuthenticationChannel.OneLogin],
                [OrganisationPersonScope.Admin, OrganisationPersonScope.Editor, OrganisationPersonScope.Viewer],
                OrganisationIdLocation.Path,
                [PersonScope.SupportAdmin])]
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
            });
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
        options.OperationFilter<ProblemDetailsOperationFilter>(ErrorCodes.Exception4xxMap.HttpStatusCodeErrorMap());
        options.ConfigureBearerSecurity();
        options.ConfigureApiKeySecurity();
    }
}