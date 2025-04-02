using CO.CDP.OrganisationInformation.Persistence.Forms;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static CO.CDP.OrganisationInformation.Persistence.ConnectedEntity;

namespace CO.CDP.OrganisationInformation.Persistence;

public record OrganisationSnapshot
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(SharedConsent))]
    public int SharedConsentId { get; set; }
    public required string Name { get; set; }
    public SharedConsent? SharedConsent { get; set; }
}

public record AddressSnapshot
{
    [Key]
    public int Id { get; set; }
    public required string StreetAddress { get; set; }
    public required string Locality { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public required string CountryName { get; set; }
    public required string Country { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public int MappingId { get; set; }
}

public record OrganisationAddressSnapshot
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(SharedConsent))]
    public int SharedConsentId { get; set; }
    public required AddressType Type { get; set; }
    public required AddressSnapshot Address { get; set; }

    public SharedConsent? SharedConsent { get; set; }
}

public record IdentifierSnapshot
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(SharedConsent))]
    public int SharedConsentId { get; set; }
    public string? IdentifierId { get; set; }
    public required string Scheme { get; set; }
    public required string LegalName { get; set; }
    public string? Uri { get; set; }
    public required bool Primary { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

    public SharedConsent? SharedConsent { get; set; }
}

public record ContactPointSnapshot
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(SharedConsent))]
    public int SharedConsentId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? Url { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

    public SharedConsent? SharedConsent { get; set; }
}

public record SupplierInformationSnapshot
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(SharedConsent))]
    public int SharedConsentId { get; set; }
    public SupplierType? SupplierType { get; set; }
    public List<OperationType> OperationTypes { get; set; } = [];
    public bool CompletedRegAddress { get; set; }
    public bool CompletedPostalAddress { get; set; }
    public bool CompletedVat { get; set; }
    public bool CompletedWebsiteAddress { get; set; }
    public bool CompletedEmailAddress { get; set; }
    public bool CompletedOperationType { get; set; }
    public bool CompletedLegalForm { get; set; }
    public bool CompletedConnectedPerson { get; set; }
    public LegalFormSnapshot? LegalFormSnapshot { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

    public SharedConsent? SharedConsent { get; set; }
}

public record LegalFormSnapshot
{
    public required bool RegisteredUnderAct2006 { get; set; }
    public required string RegisteredLegalForm { get; set; }
    public required string LawRegistered { get; set; }
    public required DateTimeOffset RegistrationDate { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public record ConnectedEntitySnapshot
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(SharedConsent))]
    public int SharedConsentId { get; set; }
    public required Guid Guid { get; set; }
    public required ConnectedEntityType EntityType { get; set; }
    public bool HasCompanyHouseNumber { get; set; }
    public string? CompanyHouseNumber { get; set; }
    public string? OverseasCompanyNumber { get; set; }
    public ConnectedOrganisationSnapshot? Organisation { get; set; }
    public ConnectedIndividualTrustSnapshot? IndividualOrTrust { get; set; }
    public ICollection<ConnectedEntityAddressSnapshot> Addresses { get; set; } = [];
    public DateTimeOffset? RegisteredDate { get; set; }
    public string? RegisterName { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public int MappingId { get; set; }

    public SharedConsent? SharedConsent { get; set; }

    [Owned]
    public record ConnectedEntityAddressSnapshot
    {
        [Key]
        public int Id { get; set; }
        public required AddressType Type { get; set; }
        public required AddressSnapshot Address { get; set; }
    }

    [Owned]
    public record ConnectedIndividualTrustSnapshot
    {
        public required ConnectedEntityIndividualAndTrustCategoryType Category { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public DateTimeOffset? DateOfBirth { get; set; }
        public string? Nationality { get; set; }
        public List<ControlCondition> ControlCondition { get; set; } = [];
        public ConnectedPersonType ConnectedType { get; set; }
        public Guid? PersonId { get; set; }
        public string? ResidentCountry { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }

    [Owned]
    public record ConnectedOrganisationSnapshot
    {
        public required ConnectedOrganisationCategory Category { get; set; }
        public required string Name { get; set; }
        public DateTimeOffset? InsolvencyDate { get; set; }
        public string? RegisteredLegalForm { get; set; }
        public string? LawRegistered { get; set; }
        public List<ControlCondition> ControlCondition { get; set; } = [];
        public Guid? OrganisationId { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }
}