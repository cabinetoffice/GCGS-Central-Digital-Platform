using Microsoft.EntityFrameworkCore;
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
    public ICollection<Identifier> Identifiers { get; set; } = [];
    public ICollection<OrganisationAddress> Addresses { get; set; } = [];
    public required OrganisationContactPoint ContactPoint { get; set; }
    public List<PartyRole> Roles { get; set; } = [];
    public List<Person> Persons => OrganisationPersons.Select(p => p.Person).ToList();
    public List<OrganisationPerson> OrganisationPersons { get; init; } = [];
    public SupplierInformation? SupplierInfo { get; set; }
    public BuyerInformation? BuyerInfo { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

    [Owned]
    public record Identifier : IEntityDate
    {
        public int Id { get; set; }
        public required string IdentifierId;
        public required string Scheme;
        public required string LegalName;
        public string? Uri;
        public required bool Primary { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
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
    public record SupplierInformation : IEntityDate
    {
        public SupplierType? SupplierType { get; set; }
        public List<OperationType> OperationTypes { get; set; } = [];
        public bool CompletedRegAddress { get; set; }
        public bool CompletedPostalAddress { get; set; }
        public bool CompletedVat { get; set; }
        public bool CompletedWebsiteAddress { get; set; }
        public bool CompletedEmailAddress { get; set; }
        public bool CompletedQualification { get; set; }
        public bool CompletedTradeAssurance { get; set; }
        public bool CompletedOrganisationType { get; set; }
        public bool CompletedLegalForm { get; set; }
        public List<Qualification> Qualifications { get; set; } = [];
        public List<TradeAssurance> TradeAssurances { get; set; } = [];
        public LegalForm? LegalForm { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }

    [Owned]
    public record BuyerInformation : IEntityDate
    {
        public int OrganisationId { get; set; }
        public string? BuyerType { get; set; }
        public List<DevolvedRegulation> DevolvedRegulations { get; set; } = [];
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }

    [Owned]
    public record Qualification : IEntityDate
    {
        public int Id { get; set; }
        public string? AwardedByPersonOrBodyName { get; set; }
        public DateTimeOffset DateAwarded { get; set; }
        public string? Name { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }

    [Owned]
    public record TradeAssurance : IEntityDate
    {
        public int Id { get; set; }
        public string? AwardedByPersonOrBodyName { get; set; }
        public string? ReferenceNumber { get; set; }
        public DateTimeOffset DateAwarded { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }

    [Owned]
    public record LegalForm : IEntityDate
    {
        public required string RegisteredUnderAct2006 { get; set; }
        public required string RegisteredLegalForm { get; set; }
        public required string LawRegistered { get; set; }
        public DateTimeOffset RegistrationDate { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }

    public void UpdateBuyerInformation()
    {
        if (!Roles.Contains(PartyRole.Buyer))
        {
            return;
        }

        BuyerInfo ??= new BuyerInformation();
    }

    public void UpdateSupplierInformation()
    {
        if (!Roles.Contains(PartyRole.Supplier))
        {
            return;
        }

        SupplierInfo ??= new SupplierInformation();
        SupplierInfo.CompletedRegAddress = Addresses.Any(a => a.Type == AddressType.Registered);
        SupplierInfo.CompletedPostalAddress = Addresses.Any(a => a.Type == AddressType.Postal);
        SupplierInfo.CompletedVat = Identifiers.Any(i => i.Scheme == "VAT");
        SupplierInfo.CompletedQualification = SupplierInfo.Qualifications.Count > 0;
        SupplierInfo.CompletedTradeAssurance = SupplierInfo.TradeAssurances.Count > 0;
        SupplierInfo.CompletedLegalForm = SupplierInfo.LegalForm != null;
    }
}