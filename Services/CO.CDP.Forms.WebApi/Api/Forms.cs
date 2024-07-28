using CO.CDP.Functional;
using CO.CDP.Forms.WebApi.UseCase;
using CO.CDP.OrganisationInformation;
using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.Swashbuckle.SwaggerGen;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using System.Reflection;
using CO.CDP.Forms.WebApi.Model;

namespace CO.CDP.Forms.WebApi.Api;

public static class EndpointExtensions
{
    public static void UseFormsEndpoints(this WebApplication app)
    {
        app.MapGet("/forms/{formId}/sections/{sectionId}/questions", async (Guid formId, Guid sectionId, [FromQuery(Name = "organisation-id")] Guid organisationId, IUseCase<(Guid, Guid, Guid), Model.SectionQuestionsResponse?> useCase) =>
            await useCase.Execute((formId, sectionId, organisationId))
                    .AndThen(sectionQuestions => sectionQuestions != null ? Results.Ok(sectionQuestions) : Results.NotFound()))
                    .Produces<Model.SectionQuestionsResponse>(StatusCodes.Status200OK, "application/json")
                    .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
                    .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
                    .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
                    .WithOpenApi(operation =>
           {
               operation.OperationId = "GetFormSectionQuestions";
               operation.Description = "Get Form Section and Its Questions.";
               operation.Summary = "Get a form section with questions.";
               operation.Responses["200"].Description = "A section and question.";
               operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
               operation.Responses["404"].Description = "Organisation not found.";
               operation.Responses["500"].Description = "Internal server error.";
               return operation;
           });


        app.MapPut("/forms/{formId}/sections/{sectionId}/answers/{answerSetId}", async (
            Guid formId, Guid sectionId, Guid answerSetId,
            [FromQuery(Name = "organisation-id")] Guid organisationId,
            [FromBody] UpdateFormSectionAnswers updateFormSectionAnswers,
            IUseCase<(Guid formId, Guid sectionId, Guid answerSetId, Guid organisationId, List<FormAnswer> answers), bool> updateFormSectionAnswersUseCase) =>
            {
                var result = await updateFormSectionAnswersUseCase.Execute((formId, sectionId, answerSetId, organisationId, updateFormSectionAnswers.Answers ?? new List<FormAnswer>()));
                return result ? Results.NoContent() : Results.Problem();
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "PutFormSectionAnswers";
                operation.Description = "Update answers for a form section.";
                operation.Summary = "Update answers for a form section.";
                operation.Responses["204"].Description = "Answers updated.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation or answer set not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapDelete("/forms/answer-sets/{answerSetId}",
                async (Guid answerSetId, [FromQuery(Name = "organisation-id")] Guid organisationId, IUseCase<(Guid, Guid), bool> useCase) =>
                 await useCase.Execute((organisationId, answerSetId))
                    .AndThen(success => success ? Results.NoContent() : Results.NotFound()))
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "DeleteFormSectionAnswers";
                operation.Description = "Delete answers for a form section.";
                operation.Summary = "Delete answers for a form section.";
                operation.Responses["204"].Description = "Answers deleted.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Organisation or answer set not found.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });
    }
}

public static class ApiExtensions
{
    public static void DocumentFormsApi(this SwaggerGenOptions options)
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "1.0.0",
            Title = "Forms API",
            Description = "API to support dynamic forms questions and for Financial Information.",
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
        options.IncludeXmlComments(Assembly.GetExecutingAssembly(), Assembly.GetAssembly(typeof(Address)));
        options.OperationFilter<ProblemDetailsOperationFilter>(Extensions.ServiceCollectionExtensions.ErrorCodes());
        options.ConfigureBearerSecurity();
        options.ConfigureApiKeySecurity();
        options.UseAllOfToExtendReferenceSchemas();
    }
}