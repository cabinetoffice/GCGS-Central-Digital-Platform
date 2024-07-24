using CO.CDP.OrganisationInformation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CO.CDP.Organisation.WebApi.Model;

public record RegisterOrganisation
{
    /// <example>"Acme Corporation"</example>
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

    /// <example>"d230dbc1-b273-4e0e-8f58-d94f2ab3c096"</example>
    [Required] public required Guid PersonId { get; init; }

    public required OrganisationIdentifier Identifier { get; init; }

    public List<OrganisationIdentifier>? AdditionalIdentifiers { get; init; }

    public List<OrganisationAddress> Addresses { get; init; } = [];

    public required OrganisationContactPoint? ContactPoint { get; init; }

    public required List<PartyRole> Roles { get; init; }
}

public record UpdateBuyerInformation
{
    public required BuyerInformationUpdateType Type { get; init; }

    public required BuyerInformation BuyerInformation { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BuyerInformationUpdateType
{
    BuyerOrganisationType,
    DevolvedRegulation
}

public record BuyerInformation
{
    public string? BuyerType { get; init; }

    public List<DevolvedRegulation>? DevolvedRegulations { get; init; }
}

public record UpdateOrganisation
{
    public required OrganisationUpdateType Type { get; init; }

    public required OrganisationInfo Organisation { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrganisationUpdateType
{
    AdditionalIdentifiers,
    ContactPoint,
    Address
}

public record OrganisationInfo
{
    public List<OrganisationIdentifier>? AdditionalIdentifiers { get; init; }
    public OrganisationContactPoint? ContactPoint { get; init; }
    public List<OrganisationAddress>? Addresses { get; init; }
}

public record OrganisationIdentifier
{
    /// <example>"CDP-PPON"</example>
    [Required(AllowEmptyStrings = false)]
    public required string Scheme { get; init; }

    /// <example>"5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public required string Id { get; init; }

    /// <example>"Acme Corporation Ltd."</example>
    public required string LegalName { get; init; }
}

public record OrganisationAddress
{
    [Required]
    public required AddressType Type { get; init; }

    /// <example>"82 St. John’s Road"</example>
    [Required(AllowEmptyStrings = false)]
    public required string StreetAddress { get; init; }

    /// <example>"Green Tower"</example>
    public string? StreetAddress2 { get; init; }

    /// <example>"CHESTER"</example>
    [Required(AllowEmptyStrings = false)]
    public required string Locality { get; init; }

    /// <example>"Lancashire"</example>
    public string? Region { get; init; }

    /// <example>"CH43 7UR"</example>
    public required string PostalCode { get; init; }

    /// <example>"United Kingdom"</example>
    public required string CountryName { get; init; }
}

public record OrganisationContactPoint
{
    /// <example>"Procurement Team"</example>
    public string? Name { get; init; }

    /// <example>"procurement@example.com"</example>
    [Required(AllowEmptyStrings = false)]
    public required string Email { get; init; }

    /// <example>"079256123321"</example>
    public string? Telephone { get; init; }

    /// <example>"https://example.com"</example>
    public string? Url { get; init; }
}

public record UpdateSupplierInformation
{
    public required SupplierInformationUpdateType Type { get; init; }

    public required SupplierInfo SupplierInformation { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupplierInformationUpdateType
{
    SupplierType,
    CompletedWebsiteAddress,
    CompletedEmailAddress,
    TradeAssurance,
    LegalForm,
    OperationType,
    Qualification,
    CompletedConnectedPerson
}

public record SupplierInfo
{
    public SupplierType? SupplierType { get; set; }

    public TradeAssurance? TradeAssurance { get; set; }

    public LegalForm? LegalForm { get; set; }
    public List<OperationType>? OperationTypes { get; set; }
    public Qualification? Qualification { get; set; }
}

public record DeleteSupplierInformation
{
    public required SupplierInformationDeleteType Type { get; init; }

    public Guid? TradeAssuranceId { get; init; }
    public Guid? QualificationId { get; init; }
}

public record RegisterConnectedEntity
{
    public required ConnectedEntityType EntityType { get; init; }
    public bool HasCompnayHouseNumber { get; init; }
    public string? CompanyHouseNumber { get; init; }
    public string? OverseasCompanyNumber { get; init; }

    public CreateConnectedOrganisation? Organisation { get; init; }
    public CreateConnectedIndividualTrust? IndividualOrTrust { get; init; }
    public ICollection<Address> Addresses { get; init; } = [];

    public DateTimeOffset? RegisteredDate { get; init; }
    public string? RegisterName { get; init; }

    public DateTimeOffset? StartDate { get; init; }
    public DateTimeOffset? EndDate { get; init; }
}

public record CreateConnectedIndividualTrust
{
    public required ConnectedIndividualAndTrustCategory Category { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public ICollection<ControlCondition> ControlCondition { get; set; } = [];
    public ConnectedPersonType ConnectedType { get; set; }
    public Guid? PersonId { get; set; }
}

public record CreateConnectedOrganisation
{
    public required ConnectedOrganisationCategory Category { get; set; }
    public required string Name { get; set; }
    public DateTimeOffset? InsolvencyDate { get; set; }
    public string? RegisteredLegalForm { get; set; }
    public string? LawRegistered { get; set; }
    public ICollection<ControlCondition> ControlCondition { get; set; } = [];
    public Guid? OrganisationId { get; set; }
}

public record ConnectedEntityLookup
{
    public required string Name { get; init; }
    public required Guid EntityId { get; init; }
    public required Uri Uri { get; init; }
}

public record DeleteConnectedEntity
{
    public required DateTimeOffset EndDate { get; init; }
}

public record OrganisationQuery
{
    public string? Name { get; }
    public string? Identifier { get; }
    public OrganisationQuery(string? name = null, string? identifier = null)
    {
        Name = name;
        Identifier = identifier;
    }

    public bool TryGetIdentifier(out string scheme, out string id)
    {
        if (!string.IsNullOrEmpty(Identifier))
        {
            var parts = Identifier.Split(':');
            if (parts.Length == 2)
            {
                scheme = parts[0];
                id = parts[1];
                return true;
            }
        }

        scheme = string.Empty;
        id = string.Empty;
        return false;
    }

    public bool TryGetName(out string name)
    {
        if (!string.IsNullOrEmpty(Name))
        {
            name = Name;
            return true;
        }

        name = string.Empty;
        return false;
    }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupplierInformationDeleteType
{
    TradeAssurance,
    Qualification

}

public static class MappingExtensions
{
    public static Identifier AsView(this OrganisationIdentifier command) =>
        new()
        {
            Scheme = command.Scheme,
            Id = command.Id,
            LegalName = command.LegalName,
            Uri = null
        };

    public static List<Identifier> AsView(this List<OrganisationIdentifier>? command) =>
        command?.Select(i => i.AsView()).ToList() ?? [];

    public static Address AsView(this OrganisationAddress command) =>
        new()
        {
            Type = command.Type,
            StreetAddress = command.StreetAddress,
            StreetAddress2 = command.StreetAddress2,
            Locality = command.Locality,
            Region = command.Region,
            PostalCode = command.PostalCode,
            CountryName = command.CountryName
        };

    public static List<Address> AsView(this List<OrganisationAddress> command) =>
        command.Select(i => i.AsView()).ToList() ?? [];

    public static ContactPoint AsView(this OrganisationContactPoint? command) =>
        command != null ? new()
        {
            Name = command.Name ?? "",
            Email = command.Email,
            Telephone = command.Telephone,
            Url = command.Url != null ? new Uri(command.Url) : null
        } : new ContactPoint();
}