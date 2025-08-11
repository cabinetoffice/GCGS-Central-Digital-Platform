using CO.CDP.OrganisationInformation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Address = CO.CDP.OrganisationInformation.Address;

namespace CO.CDP.Organisation.WebApi.Model;

public record RegisterOrganisation
{
    /// <example>"Acme Corporation"</example>
    [Required(AllowEmptyStrings = true)] public required string Name { get; init; }

    public required OrganisationType Type { get; set; } = OrganisationType.Organisation;

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
    RemoveIdentifier,
    Address,
    OrganisationName,
    OrganisationEmail,
    RegisteredAddress,
    AddRoles,
    AddAsBuyerRole
}

public record OrganisationInfo
{
    public string? OrganisationName { get; set; }
    public List<OrganisationIdentifier>? AdditionalIdentifiers { get; init; }
    public OrganisationContactPoint? ContactPoint { get; init; }
    public List<OrganisationAddress>? Addresses { get; init; }
    public OrganisationIdentifier? IdentifierToRemove { get; init; }
    public List<PartyRole>? Roles { get; init; }
    public BuyerInformation? BuyerInformation { get; init; }

}

public record OrganisationIdentifier
{
    /// <example>"GB-PPON"</example>
    [Required(AllowEmptyStrings = false)]
    public required string Scheme { get; init; }

    /// <example>"5a360be7-e1d3-4214-9f72-0e1d6b57b85d"</example>
    public string? Id { get; init; }

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

    /// <example>"CHESTER"</example>
    [Required(AllowEmptyStrings = false)]
    public required string Locality { get; init; }

    /// <example>"Lancashire"</example>
    public string? Region { get; init; }

    /// <example>"CH43 7UR"</example>
    public string? PostalCode { get; init; }

    /// <example>"United Kingdom"</example>
    public required string CountryName { get; init; }

    /// <example>"GB"</example>
    public required string Country { get; init; }
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

public record AssignOrganisationIdentifier
{
    public required Guid OrganisationId { get; init; }
    public required OrganisationIdentifier Identifier { get; init; }
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
    LegalForm,
    OperationType,
    CompletedConnectedPerson,
    CompletedVat
}

public record SupplierInfo
{
    public SupplierType? SupplierType { get; set; }

    public LegalForm? LegalForm { get; set; }
    public List<OperationType>? OperationTypes { get; set; }
}

public record RegisterConnectedEntity
{
    public required ConnectedEntityType EntityType { get; init; }
    public bool HasCompanyHouseNumber { get; init; }
    public string? CompanyHouseNumber { get; init; }
    public string? OverseasCompanyNumber { get; init; }

    public CreateConnectedOrganisation? Organisation { get; init; }
    public CreateConnectedIndividualTrust? IndividualOrTrust { get; init; }
    public ICollection<Address> Addresses { get; init; } = [];

    public DateTimeOffset? RegisteredDate { get; init; }
    public string? RegisterName { get; init; }

    public DateTimeOffset? EndDate { get; init; }

    public bool Deleted { set; get; }
}

public record UpdateConnectedEntity
{
    public string? Id { get; init; }
    public required ConnectedEntityType EntityType { get; init; }
    public bool HasCompanyHouseNumber { get; init; }
    public string? CompanyHouseNumber { get; init; }
    public string? OverseasCompanyNumber { get; init; }

    public UpdateConnectedOrganisation? Organisation { get; init; }
    public UpdateConnectedIndividualTrust? IndividualOrTrust { get; init; }
    public ICollection<Address> Addresses { get; init; } = [];

    public DateTimeOffset? RegisteredDate { get; init; }
    public string? RegisterName { get; init; }

    public DateTimeOffset? EndDate { get; init; }
    public bool Deleted { set; get; }
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
    public string? ResidentCountry { get; set; }
}

public record UpdateConnectedIndividualTrust
{
    public required ConnectedIndividualAndTrustCategory Category { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTimeOffset? DateOfBirth { get; set; }
    public string? Nationality { get; set; }
    public ICollection<ControlCondition> ControlCondition { get; set; } = [];
    public ConnectedPersonType ConnectedType { get; set; }
    public Guid? PersonId { get; set; }
    public string? ResidentCountry { get; set; }

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

public record UpdateConnectedOrganisation
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
    public required ConnectedEntityType EntityType { get; init; }
    public DateTimeOffset? EndDate { get; init; }
    public bool Deleted { get; set; }
    public bool IsInUse { get; set; }
    public Guid? FormGuid { get; set; }
    public Guid? SectionGuid { get; set; }
}

public record RemovePersonFromOrganisation
{
    public required Guid PersonId { get; set; }
}

public record InvitePersonToOrganisation
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required List<string> Scopes { get; init; }
}

public record ProvideFeedbackAndContact
{
    public required string FeedbackAbout { get; init; }
    public required string SpecificPage { get; init; }
    public required string Feedback { get; init; }
    public required string Name { get; init; }
    public required string Email { get; init; }
    public required string Subject { get; init; }
}

public record ContactUs
{
    public required string Name { get; init; }
    public required string EmailAddress { get; init; }
    public required string OrganisationName { get; init; }
    public required string Message { get; init; }
}


public record UpdateInvitedPersonToOrganisation
{
    public required List<string> Scopes { get; init; }
}

public record UpdatePersonToOrganisation
{
    public required List<string> Scopes { get; init; }
}

public record UpdateJoinRequest
{
    public required OrganisationJoinRequestStatus status { get; init; }
    public required Guid ReviewedBy { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum SupportOrganisationUpdateType
{
    Review,
    AdditionalIdentifiers
}

public record SupportUpdateOrganisation
{
    public required SupportOrganisationUpdateType Type { get; init; }

    public required SupportOrganisationInfo Organisation { get; init; }
}

public record SupportOrganisationInfo
{
    public Guid? ReviewedById { get; set; }
    public Boolean? Approved { get; set; }
    public string? Comment { get; set; }
    public List<OrganisationIdentifier>? AdditionalIdentifiers { get; set; }
}

public record PaginatedOrganisationQuery
{
    public PartyRole? Role { get; }
    public PartyRole? PendingRole { get; }
    public int Limit { get; }
    public int Skip { get; }
    public string? SearchText { get; }

    public PaginatedOrganisationQuery(int limit, int skip, string? role = null, string? pendingRole = null, string? searchText = null)
    {
        Limit = limit;
        Skip = skip;
        SearchText = searchText;

        if (role != null)
        {
            Role = (PartyRole)Enum.Parse(typeof(PartyRole), role, true);
        }

        if (pendingRole != null)
        {
            PendingRole = (PartyRole)Enum.Parse(typeof(PartyRole), pendingRole, true);
        }
    }
}

public record OrganisationTypeQuery
{
    public PartyRole? Role { get; }
    public PartyRole? PendingRole { get; }
    public string? SearchText { get; }

    public OrganisationTypeQuery(string? role = null, string? pendingRole = null, string? searchText = null)
    {
        if (role != null)
        {
            Role = (PartyRole)Enum.Parse(typeof(PartyRole), role, true);
        }

        if (pendingRole != null)
        {
            PendingRole = (PartyRole)Enum.Parse(typeof(PartyRole), pendingRole, true);
        }

        SearchText = searchText;
    }
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

public record OrganisationSearchQuery
{
    public string Name { get; }
    public PartyRole? Role { get; }
    public int? Limit { get; }
    public double Threshold { get; }
    public OrganisationSearchQuery(string name, int? limit, double? threshold, string? role = null)
    {
        Name = name;

        if (role != null)
        {
            Role = (PartyRole)Enum.Parse(typeof(PartyRole), role, true);
        }

        Limit = limit;

        if (threshold.HasValue)
        {
            Threshold = threshold.Value;
        }
    }
}

public record OrganisationSearchByPponQuery
{
    public string SearchText { get; }
    public int? Limit { get; }
    public int Skip { get; }
    public string OrderBy { get; } = "rel";
    public double Threshold { get; }

    public OrganisationSearchByPponQuery(string searchText, int? limit, int skip, string orderBy, double? threshold)
    {
        SearchText = searchText;
        Limit = limit;
        Skip = skip;
        if (!string.IsNullOrEmpty(orderBy))
        {
            OrderBy = orderBy;
        }

        if (threshold.HasValue)
        {
            Threshold = threshold.Value;
        }
    }
}

public record OrganisationsByOrganisationEmailQuery
{
    public string Email { get; }
    public PartyRole? Role { get; }
    public int? Limit { get; }
    public OrganisationsByOrganisationEmailQuery(string email, int? limit, string? role = null)
    {
        Email = email;

        if (role != null)
        {
            Role = (PartyRole)Enum.Parse(typeof(PartyRole), role, true);
        }

        Limit = limit;
    }
}

public record OrganisationsByAdminEmailQuery
{
    public string Email { get; }
    public PartyRole? Role { get; }
    public int? Limit { get; }
    public OrganisationsByAdminEmailQuery(string email, int? limit, string? role = null)
    {
        Email = email;

        if (role != null)
        {
            Role = (PartyRole)Enum.Parse(typeof(PartyRole), role, true);
        }

        Limit = limit;
    }
}

public record RegisterAuthenticationKey
{
    public required string Name { get; set; }
    public required string Key { get; set; }
    public Guid OrganisationId { get; set; }
}

public record AuthenticationKey
{
    public required string Name { get; set; }
    public bool? Revoked { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset? RevokedOn { get; set; }
}

public record CreateOrganisationJoinRequest
{
    public Guid PersonId { get; init; }
}

public record SignMouRequest
{
    public required Guid MouId { get; set; }
    public required Guid CreatedById { get; set; }
    public required string Name { get; set; }
    public required string JobTitle { get; set; }
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
            Locality = command.Locality,
            Region = command.Region,
            PostalCode = command.PostalCode,
            CountryName = command.CountryName,
            Country = command.Country
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

public record AddOrganisationParty
{
    public required Guid OrganisationPartyId { get; init; }

    public required OrganisationRelationship OrganisationRelationship { get; init; }

    public string? ShareCode { get; init; }
}

public record UpdateOrganisationParty
{
    public required Guid OrganisationPartyId { get; init; }

    public string? ShareCode { get; init; }
}

public record RemoveOrganisationParty
{
    public required Guid OrganisationPartyId { get; init; }
}

public record GetAnnouncementQuery
{
    public required string Page { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum OrganisationRelationship
{
    Consortium
}