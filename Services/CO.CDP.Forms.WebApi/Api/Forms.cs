using System.Reflection;
using CO.CDP.Functional;
using CO.CDP.OrganisationInformation;
using CO.CDP.Swashbuckle.Filter;
using CO.CDP.Swashbuckle.Security;
using CO.CDP.Swashbuckle.SwaggerGen;
using CO.CDP.Forms.WebApi.Model;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace CO.CDP.Forms.WebApi.Api;

public static class EndpointExtensions
{
     private static Dictionary<Guid, (FormSection section, List<FormQuestion> questions)> _sections = Enumerable.Range(1, 5)
            .Select(_ => Guid.NewGuid())
            .ToDictionary(id => id, id =>
            {
                var sectionId = Guid.NewGuid();
                return (new FormSection
                {
                    Id = sectionId,
                    Title = "Financial Information",
                    AllowsMultipleAnswerSets = true,
                },
                new List<FormQuestion>
                {
                    new FormQuestion
                    {
                        Id = Guid.NewGuid(),
                        Title = "Upload accounts or statements for your 2 most recent financial years.",
                        Description = "If you do not have 2 years, you can upload your most recent financial year. You will need to enter the financial year end date.",
                        Type = FormQuestionType.NoInput,
                        IsRequired = true,
                        Options = new FormQuestionOptions()
                    },
                    new FormQuestion
                    {
                        Id = Guid.NewGuid(),
                        Title = "Were your accounts audited?",
                        Type = FormQuestionType.YesOrNo,
                        IsRequired = true,
                        Options = new FormQuestionOptions()
                    },
                    new FormQuestion
                    {
                        Id = Guid.NewGuid(),
                        Title = "Upload your accounts",
                        Description = "Upload your most recent 2 financial years. If you do not have 2, upload your most recent financial year.",
                        Type = FormQuestionType.FileUpload,
                        IsRequired = true,
                        Options = new FormQuestionOptions()
                    },
                    new FormQuestion
                    {
                        Id = Guid.NewGuid(),
                        Title = "What is the financial year end date for the information you uploaded?",
                        Type = FormQuestionType.Date,
                        IsRequired = true,
                        Options = new FormQuestionOptions()
                    }
                });
            });

    public static void UseFormsEndpoints(this WebApplication app)
    {
        app.MapGet("/forms/{formId}/sections/{sectionId}/questions",
                (Guid formId, Guid sectionId) =>
                {
                    var (section, questions) = _sections.Values.First();
                    var response = new SectionQuestionsResponse
                    {
                        Section = section,
                        Questions = questions.ToList()
                    };

                    return Results.Ok(response);
                })
           .Produces<SectionQuestionsResponse>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
           .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
           .WithOpenApi(operation =>
           {
               operation.OperationId = "[STUB] GetFormSectionQuestions [STUB]";
               operation.Description = "[STUB] Get Form Section and Its Questions. [STUB]";
               operation.Summary = "Get a form section with questions.";
               operation.Responses["200"].Description = "A section and question.";
               operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
               operation.Responses["404"].Description = "Organisation not found.";
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
            Title = "Organisation Management API",
            Description = "API for creating, updating, deleting, and listing organisations, including a lookup feature against organisation identifiers.",
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