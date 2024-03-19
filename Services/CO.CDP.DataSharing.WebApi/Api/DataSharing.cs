using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CO.CDP.DataSharing.WebApi.Api;
    internal record SupplierInformation
    {
        [Required] public required Guid Id { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
        [Required] public required List<OrganisationReference> AdditionalParties { get; init; }
        [Required] public required Identifier Identifier { get; init; }
        [Required] public required List<Identifier> AdditionalIdentifiers { get; init; }
        [Required] public required Address Address { get; init; }
        [Required] public required List<ContactPoint> ContactPoints { get; init; }
        [Required] public required List<PartyRole> Roles { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Details { get; init; }
        [Required] public required SupplierInformationData SupplierInformationData { get; init; }
    }

    internal record OrganisationReference
    {
        [Required] public required Guid Id { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Name { get; init; }
    }

    internal record Identifier
    {
        [Required(AllowEmptyStrings = true)] public required string Scheme { get; init; }
        [Required] public required Guid Id { get; init; }
        public string? LegalName { get; init; }
        public string? Uri { get; init; }
    }

    internal record Address
    {
        [Required(AllowEmptyStrings = true)] public required string StreetAddress { get; init; }
        [Required(AllowEmptyStrings = true)] public required string Locality { get; init; }
        public string? Region { get; init; }
        public string? PostalCode { get; init; }
    }

    internal record ContactPoint
    {
        public string? Email { get; init; }
        public string? Telephone { get; init; }
        public string? FaxNumber { get; init; }
    }

    internal enum PartyRole
    {
        Buyer,
        ProcuringEntity,
        Supplier,
        Tenderer,
        Funder,
        Enquirer,
        Payer,
        Payee,
        ReviewBody,
        InterestedParty,
        Consortium
    }

    internal record SupplierInformationData
    {
        [Required] public required Guid FormId { get; init; }
        public List<SupplierFormAnswer> Answers { get; init; } = new List<SupplierFormAnswer>();
        public List<FormQuestion> Questions { get; init; } = new List<FormQuestion>();

    }

    internal record SupplierFormAnswer
    {
        public string? QuestionName { get; init; }
        public bool? BoolValue { get; init; }
        public double? NumericValue { get; init; }
        public DateTime? StartValue { get; init; }
        public DateTime? EndValue { get; init; }
        public string? TextValue { get; init; }
        public int? OptionValue { get; init; }
    }

    internal record FormQuestion
    {
        public QuestionTypes? Type { get; init; }
        public string? Name { get; init; }
        public string? Text { get; init; }
        public bool IsRequired { get; init; }
        public string? SectionName { get; init; }
        public List<QuestionOption> Options { get; init; } = new List<QuestionOption>();
    }

    internal enum QuestionTypes
    {
        Type1 = 1,
        Type2 = 2,
        Type3 = 3,
        Type4 = 4,
        Type5 = 5,
        Type6 = 6,
        Type7 = 7,
        Type8 = 8,
        Type9 = 9
    }

    internal record QuestionOption
    {
        [Required] public required Guid Id { get; init; }
        public string? Value { get; init; }
    }

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
                        "Operation to obtain Supplier information which has been shared as part of a notice.";
                    operation.Summary = "Request Supplier Submitted Information.";
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
                        "Operation to obtain Supplier information which has been shared as part of a notice.";
                    operation.Summary = "Create Supplier Submitted Information.";
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
