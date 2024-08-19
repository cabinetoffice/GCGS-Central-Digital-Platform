using System.Reflection;
using CO.CDP.Authentication.AuthorizationPolicy;
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

namespace CO.CDP.DataSharing.WebApi.Api;

public static class EndpointExtensions
{
    public static void UseDataSharingEndpoints(this WebApplication app)
    {
        app.MapGet("/share/data/{sharecode}", (string sharecode) => new SupplierInformation
        {
            Id = Guid.Parse("1e39d0ce-3abd-43c5-9f23-78c92e437f2a"),
            Name = "Acme Corporation",
            AssociatedPersons =
                [
                    new AssociatedPerson
                    {
                        Id = Guid.Parse("c16f9f7b-3f10-42db-86f8-93607b034a4c"),
                        Name = "Alice Doe",
                        Relationship = "Company Director",
                        Uri = new Uri(
                            "https://cdp.cabinetoffice.gov.uk/persons/c16f9f7b-3f10-42db-86f8-93607b034a4c"),
                        Roles = [PartyRole.Tenderer]
                    }
                ],
            AdditionalParties =
                [
                    new OrganisationReference
                    {
                        Id = Guid.Parse("f4596cdd-12e5-4f25-9db1-4312474e516f"),
                        Name = "Acme Group Ltd",
                        Roles = [PartyRole.Tenderer],
                        Uri = new Uri(
                            "https://cdp.cabinetoffice.gov.uk/organisations/f4596cdd-12e5-4f25-9db1-4312474e516f")
                    }
                ],
            AdditionalEntities =
                [
                    new OrganisationReference
                    {
                        Id = Guid.Parse("f4596cdd-12e5-4f25-9db1-4312474e516f"),
                        Name = "Acme Group Ltd",
                        Roles = [PartyRole.Tenderer],
                        Uri = new Uri(
                            "https://cdp.cabinetoffice.gov.uk/organisations/f4596cdd-12e5-4f25-9db1-4312474e516f")
                    }
                ],
            Identifier = new Identifier
            {
                Scheme = "CDP-PPON",
                Id = "1e39d0ce-3abd-43c5-9f23-78c92e437f2a",
                LegalName = "Acme Corporation Ltd",
                Uri = new Uri(
                        "https://cdp.cabinetoffice.gov.uk/organisations/1e39d0ce-3abd-43c5-9f23-78c92e437f2a")
            },
            AdditionalIdentifiers =
                [
                    new Identifier
                    {
                        Id = "06368740",
                        Scheme = "GB-COH",
                        LegalName = "Acme Corporation Ltd",
                        Uri = new Uri("http://data.companieshouse.gov.uk/doc/company/06368740")
                    }
                ],
            Address = new Address
            {
                StreetAddress = "82 St. John’s Road",
                Locality = "CHESTER",
                Region = "Lancashire",
                PostalCode = "CH43 7UR",
                CountryName = "United Kingdom",
                Type = AddressType.Registered,
            },
            ContactPoint = new ContactPoint
            {
                Name = "Procurement Team",
                Email = "info@example.com",
                Telephone = "+441234567890"
            },
            Roles = [PartyRole.Tenderer],
            Details = new Details(),
            SupplierInformationData = new SupplierInformationData
            {
                Form = new Form
                {
                    Name = "Standard Questions",
                    SubmissionState = FormSubmissionState.Submitted,
                    SubmittedAt = DateTime.Parse("2024-03-28T18:24:00.000Z"),
                    OrganisationId = Guid.Parse("1e39d0ce-3abd-43c5-9f23-78c92e437f2a"),
                    FormId = Guid.Parse("f174b921-0c58-4644-80f1-8707d8300130"),
                    FormVersionId = "20240309",
                    IsRequired = true,
                    BookingReference = "AGMT-2024-XYZ",
                    Scope = 0,
                    Type = 0
                },
                Questions =
                    [
                        new FormQuestion
                        {
                            Name = "_Steel01",
                            Type = FormQuestionType.Text,
                            Text =
                                "<span style='font-weight: bold;'>Central Government Only - UK</span><span style='font-size:16.000000000000004px'><br /></span><p><span style='font-size:13.333299999999998px'>For contracts which relate to projects/programmes (i) with a value of \u00a310 million or more; or (ii) a value of less than \u00a310 million where it is anticipated that the project will require in excess of 500 tonnes of steel; please describe the steel specific supply chain management systems, policies, standards and procedures you have in place to ensure robust supply chain management and compliance with relevant legislation.</span><span style='font-size:16.000000000000004px'></span></p><span style='font-size:13.333299999999998px'>Please provide details of previous similar projects where you have demonstrated a high level of competency and effectiveness in managing all supply chain members involved in steel supply or production to ensure a sustainable and resilient supply of steel.</span>",
                            IsRequired = false,
                            SectionName = "Steel"
                        },
                        new FormQuestion
                        {
                            Name = "_Steel02",
                            Type = FormQuestionType.Text,
                            Text =
                                "<p>Please provide all the relevant details of previous breaches of health and safety legislation in the last 5 years, applicable to the country in which you operate, on comparable projects, for both:&nbsp;<span style='font-size:13.333299999999998px'>Your organisation</span></p><span style='font-size:13.333299999999998px'>All your supply chain members involved in the production or supply of steel</span>",
                            IsRequired = true,
                            SectionName = "Steel"
                        },
                        new FormQuestion
                        {
                            Name = "_ModernSlavery01",
                            Type = FormQuestionType.Text,
                            Text =
                                "Central Government Only - Tackling Modern Slavery in Supply Chains<span style='font-size:16.000000000000004px'><br /></span><span style='font-size:13.333299999999998px'>If you are a relevant commercial organisation subject to Section 54 of the Modern Slavery Act 2015, and if your latest statement is available electronically please provide:</span><span style='font-size:16.000000000000004px'><br /></span><span style='font-size:13.333299999999998px'i. the web address,</span><span style='font-size:16.000000000000004px'><br /></span><span style='font-size:13.333299999999998px>ii. precise reference of the documents.</span><span style='font-size:16.000000000000004px'><br /></span><span style='font-size:13.333299999999998p'>iii. If your latest statement is not available electronically, please provide a copy.</span>",
                            IsRequired = true,
                            SectionName = "Modern slavery"
                        },
                        new FormQuestion
                        {
                            Name = "_CarbonNetZero01",
                            Type = FormQuestionType.Boolean,
                            Text =
                                "Please confirm that you have detailed your environmental management measures by completing and publishing a Carbon Reduction Plan which meets the required reporting standard.",
                            IsRequired = true,
                            SectionName = "Carbon Net Zero"
                        }
                    ],
                Answers =
                    [
                        new FormAnswer
                        {
                            QuestionName = "_Steel02",
                            TextValue = "Answer to question 1.",
                        },

                        new FormAnswer
                        {
                            QuestionName = "_CarbonNetZero01",
                            BoolValue = true
                        },

                        // Use GeneratePresignedUrl method from IFileHostManager to Get PresignedUrl for a file
                        //new FormAnswer
                        //{
                        //    QuestionName = "_FinancialAccountFile",
                        //    TextValue = CO.CDP.AwsServices.IFileHostManager.GeneratePresignedUrl(fileName, urlExpiryInMinutes)
                        //}
                    ]
            },
        })
            .Produces<SupplierInformation>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "GetSharedData";
                operation.Description =
                    "[STUB] Operation to obtain Supplier information which has been shared as part of a notice. [STUB]";
                operation.Summary = "[STUB] Request Supplier Submitted Information. [STUB]";
                operation.Responses["200"].Description = "Organisation Information including Form Answers.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Share code not found or the caller is not authorised to use it.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

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
            }).RequireAuthorization(Constants.OrganisationKeyPolicy);

        app.MapPost("/share/data/verify", (ShareVerificationRequest request) => Results.Ok(
                    new ShareVerificationReceipt
                    {
                        ShareCode = request.ShareCode,
                        FormVersionId = request.FormVersionId,
                        IsLatest = true
                    }
                )
            )
            .Produces<ShareVerificationReceipt>(StatusCodes.Status200OK, "application/json")
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .WithOpenApi(operation =>
            {
                operation.OperationId = "VerifySharedData";
                operation.Description =
                    "[STUB] Operation to verify if shared data is the latest version available. [STUB]";
                operation.Summary = "[STUB] Create Supplier Submitted Information. [STUB]";
                operation.Responses["200"].Description = "Share code and version verification.";
                operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                operation.Responses["404"].Description = "Share code not found or the caller is not authorised to use it.";
                operation.Responses["500"].Description = "Internal server error.";
                return operation;
            });

        app.MapGet("/share/organisations/{organisationId}/codes", async ([FromQuery(Name = "organisation-id")] Guid organisationId,
            IUseCase<Guid, List<Model.SharedConsent>?> useCase) =>
        await useCase.Execute(organisationId)
             .AndThen(sectionQuestions => sectionQuestions != null ? Results.Ok(sectionQuestions) : Results.NotFound()))
             .Produces<List<Model.SharedConsent>>(StatusCodes.Status200OK, "application/json")
             .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
             .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
             .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
             .WithOpenApi(operation =>
             {
                 operation.OperationId = "GetShareCodeList";
                 operation.Description = "Get Share Code List.";
                 operation.Summary = "Get a list of Share Code for a Organisation.";
                 operation.Responses["200"].Description = "List of Share Code.";
                 operation.Responses["401"].Description = "Valid authentication credentials are missing in the request.";
                 operation.Responses["404"].Description = "Organisation not found.";
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
        options.OperationFilter<ProblemDetailsOperationFilter>(Extensions.ServiceCollectionExtensions.ErrorCodes());
        options.ConfigureApiKeySecurity();
    }
}