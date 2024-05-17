using System.Reflection;
using CO.CDP.DataSharing.WebApi.Model;
using DotSwashbuckle.AspNetCore.SwaggerGen;
using Microsoft.OpenApi.Models;

namespace CO.CDP.DataSharing.WebApi.Api
{
    public static class EndpointExtensions
    {
        public static void UseDataSharingEndpoints(this WebApplication app)
        {
            app.MapGet("/share/data/{sharecode}", (string sharecode) => new SupplierInformation
            {
                Id = Guid.NewGuid(),
                Name = "Tables Ltd",
                AdditionalParties = new List<OrganisationReference>{
                    new OrganisationReference
                    {
                        Id = Guid.NewGuid(),
                         Name = "Org 1"
                    },
                    new OrganisationReference
                    {
                        Id = Guid.NewGuid(),
                        Name = "Org 2"
                    }
                },
                Identifier = new Identifier
                {
                    Scheme = "CH",
                    Id = Guid.NewGuid(),
                    LegalName = "Tables Incorporated",
                    Uri = "https://tablesinc.com"
                },
                AdditionalIdentifiers = new List<Identifier>
                {
                    new Identifier {
                        Id = Guid.NewGuid(),
                        Scheme = "GB-COH",
                        LegalName = "Tables Ltd",
                        Uri = "https://tables.co.uk" }
                },
                Address = new Address
                {
                    StreetAddress = $"42 Green Lane",
                    Locality = "London",
                    Region = "Kent",
                    PostalCode = "BR8 7AA"
                },
                ContactPoints = new List<ContactPoint>
                    {
                        new ContactPoint {
                            Email = "info@tables.com",
                            Telephone = "+441234567890" }
                    },
                Roles = new List<PartyRole> {
                    PartyRole.Supplier
                },
                Details = "Details.",
                SupplierInformationData = new SupplierInformationData
                {
                    FormId = Guid.NewGuid(),
                    Questions = new List<FormQuestion>
                        {
                            new FormQuestion {
                                Name = "Question 1",
                                Text = "What is your answer?",
                                IsRequired = true
                            }
                        },
                    Answers = new List<SupplierFormAnswer>
                        {
                            new SupplierFormAnswer {
                                QuestionName = "Question 1",
                                TextValue = "Answer 1"
                            }
                        }
                },
            })
                .Produces<SupplierInformation>(200, "application/json")
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "GetSharedData";
                    operation.Description =
                        "[STUB] Operation to obtain Supplier information which has been shared as part of a notice. [STUB]";
                    operation.Summary = "[STUB] Request Supplier Submitted Information. [STUB]";
                    operation.Responses["200"].Description = "Organisation Information including Form Answers.";
                    return operation;
                });



            app.MapPost("/share/data", (SupplierInformationData shareRequest) =>
            {
                var shareCode = Guid.NewGuid().ToString();

                return Results.Ok(new { ShareCode = shareCode, Expiry = DateTime.UtcNow.AddDays(7) });
            }).Produces(StatusCodes.Status200OK)
                .WithOpenApi(operation =>
                {
                    operation.OperationId = "CreateSharedData";
                    operation.Description =
                        "[STUB] Operation to obtain Supplier information which has been shared as part of a notice. [STUB]";
                    operation.Summary = "[STUB] Create Supplier Submitted Information. [STUB]";
                    operation.Responses["200"].Description = "Organisation Information created.";
                    return operation;
                });
        }
    }

    public static class ApiExtensions
    {
        public static void DocumentDataSharingApi(this SwaggerGenOptions options)
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "1.0.0.0",
                Title = "Data Sharing API",
                Description = "",
            });
        }
    }
}