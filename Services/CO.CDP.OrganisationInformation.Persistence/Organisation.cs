using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
public class Organisation : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required Tenant Tenant { get; set; }
    public required string Name { get; set; }
    public ICollection<OrganisationIdentifier> Identifiers { get; set; } = [];
    public ICollection<OrganisationAddress> Addresses { get; set; } = [];
    public required OrganisationContactPoint ContactPoint { get; set; }
    public List<PartyRole> Roles { get; set; } = [];
    public List<Person> Persons { get; } = [];
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public SupplierInformation? SupplierInfo { get; set; }
    public BuyerInformation? BuyerInfo { get; set; }

    [Owned]
    public record OrganisationIdentifier
    {
        public int Id { get; set; }
        public required string IdentifierId;
        public required string Scheme;
        public required string LegalName;
        public string? Uri;
        public required bool Primary { get; set; }
    }

    [Owned]
    public record OrganisationAddress
    {
        public int Id { get; set; }
        public required AddressType Type { get; set; }
        public required Address Address { get; set; }
    }

    [ComplexType]
    public record OrganisationContactPoint
    {
        public string? Name;
        public required string Email;
        public string? Telephone;
        public string? Url;
    }

    [Owned]
    public record SupplierInformation
    {
        public int Id { get; set; }
        public required SupplierType SupplierType { get; set; }
        public ICollection<OperationType> OperationTypes { get; set; } = [];
        public bool? CompletedRegAddress { get; set; }
        public bool? CompletedPostalAddress { get; set; }
        public bool? CompletedVat { get; set; }
        public bool? CompletedWebsiteAddress { get; set; }
        public bool? CompletedEmailAddress { get; set; }
        public bool? CompletedQualification { get; set; }
        public bool? CompletedTradeAssurance { get; set; }
        public bool? CompletedOrganisationType { get; set; }
        public bool? CompletedLegalForm { get; set; }
        public ICollection<Qualification> Qualifications { get; set; } = [];
        public ICollection<TradeAssurance> TradeAssurances { get; set; } = [];
        public LegalForm? LegalForm { get; set; }
    }

    [Owned]
    public record BuyerInformation
    {
        public int Id { get; set; }
        //public required int OrganisationId { get; set; }        
        public string? BuyerType { get; set; }
        public ICollection<DevolvedRegulation> DevolvedRegulations { get; set; } = [];

    }

    [Owned]
    public record Qualification
    {
        public int Id { get; set; }
        //public required int SupplierInfoId { get; set; }
        public string? AwardedByPersonOrBodyName { get; set; }
        public DateTimeOffset DateAwarded { get; set; }
        public string? Name { get; set; }
    }

    [Owned]
    public record TradeAssurance
    {
        public int Id { get; set; }
        //public required int SupplierInfoId { get; set; }
        public string? AwardedByPersonOrBodyName { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTimeOffset DateAwarded { get; set; }
    }

    [Owned]
    public record LegalForm
    {
        public int Id { get; set; }
        //public required int SupplierInfoId { get; set; }
        public required string RegisteredUnderAct2006 { get; set; }
        public required string RegisteredLegalForm { get; set; }
        public required string LawRegistered { get; set; }
        public DateTimeOffset RegistrationDate { get; set; }
    }
}