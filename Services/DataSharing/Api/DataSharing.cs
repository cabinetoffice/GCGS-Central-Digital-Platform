using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DataSharing.Api
{
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
    }

    public static class EndpointExtensions
    {
        public static void UseDataSharingEndpoints(this WebApplication app)
        {
            app.MapGet("/share/data/{sharecode}", (string sharecode) => new SupplierInformation
                {
                    Id = Guid.NewGuid(),
                    Name = "Tables Ltd",
                    AdditionalParties = [],
                    Identifier = new Identifier
                    {
                        Scheme = "CH",
                        Id = Guid.NewGuid(),
                    },
                    AdditionalIdentifiers = [],
                    Address = new Address()
                    {
                        StreetAddress = $"42 Green Lane",
                        Locality = "London",
                        Region = "Kent",
                        PostalCode = "BR8 7AA"
                    },
                    ContactPoints = [new ContactPoint()],
                    Roles = [PartyRole.Buyer],
                    Details = "Details.",
                    SupplierInformationData = new SupplierInformationData(),
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