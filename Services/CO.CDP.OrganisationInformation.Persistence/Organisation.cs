using System.ComponentModel.DataAnnotations;
using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
public class Organisation : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required Tenant Tenant { get; set; }
    public required string Name { get; set; }
    public IList<Identifier> Identifiers { get; set; } = [];
    public ICollection<OrganisationAddress> Addresses { get; set; } = [];
    public ICollection<ContactPoint> ContactPoints { get; set; } = [];
    public List<PartyRole> Roles { get; set; } = [];
    public List<Person> Persons => OrganisationPersons.Select(p => p.Person).ToList();
    public List<OrganisationPerson> OrganisationPersons { get; init; } = [];
    public SupplierInformation? SupplierInfo { get; set; }
    public BuyerInformation? BuyerInfo { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public DateTimeOffset? ApprovedOn { get; set; }
    public Person? ReviewedBy { get; set; }
    public int? ReviewedById { get; set; }

    [MaxLength(10000)]
    public string? ReviewComment { get; set; }

    [Owned]
    [Index(nameof(IdentifierId), [nameof(Scheme)], IsUnique = true)]
    public record Identifier : IEntityDate
    {
        public int Id { get; set; }
        public string? IdentifierId { get; set; }
        public required string Scheme { get; set; }
        public required string LegalName { get; set; }
        public string? Uri { get; set; }
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

    [Owned]
    public record ContactPoint : IEntityDate
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Telephone { get; set; }
        public string? Url { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
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
        public bool CompletedOperationType { get; set; }
        public bool CompletedLegalForm { get; set; }
        public bool CompletedConnectedPerson { get; set; }
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
    [Index(nameof(Guid), IsUnique = true)]
    public record Qualification : IEntityDate
    {
        public int Id { get; set; }
        public required Guid Guid { get; set; }
        public required string AwardedByPersonOrBodyName { get; set; }
        public required DateTimeOffset DateAwarded { get; set; }
        public required string Name { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }

    [Owned]
    [Index(nameof(Guid), IsUnique = true)]
    public record TradeAssurance : IEntityDate
    {
        public int Id { get; set; }
        public required Guid Guid { get; set; }
        public required string AwardedByPersonOrBodyName { get; set; }
        public required string ReferenceNumber { get; set; }
        public required DateTimeOffset DateAwarded { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }

    [Owned]
    public record LegalForm : IEntityDate
    {
        public required bool RegisteredUnderAct2006 { get; set; }
        public required string RegisteredLegalForm { get; set; }
        public required string LawRegistered { get; set; }
        public required DateTimeOffset RegistrationDate { get; set; }
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
        if (!Roles.Contains(PartyRole.Tenderer))
        {
            return;
        }

        SupplierInfo ??= new SupplierInformation();
        SupplierInfo.CompletedRegAddress =
            Addresses.Any(a => a.Type == AddressType.Registered) || SupplierInfo.CompletedRegAddress;
        SupplierInfo.CompletedPostalAddress =
            Addresses.Any(a => a.Type == AddressType.Postal) || SupplierInfo.CompletedPostalAddress;
        SupplierInfo.CompletedVat = Identifiers.Any(i => i.Scheme == "VAT") || SupplierInfo.CompletedVat;
        SupplierInfo.CompletedQualification =
            SupplierInfo.Qualifications.Count > 0 || SupplierInfo.CompletedQualification;
        SupplierInfo.CompletedTradeAssurance =
            SupplierInfo.TradeAssurances.Count > 0 || SupplierInfo.CompletedTradeAssurance;
        SupplierInfo.CompletedLegalForm = SupplierInfo.LegalForm != null || SupplierInfo.CompletedLegalForm;
        SupplierInfo.CompletedEmailAddress =
            !string.IsNullOrWhiteSpace(ContactPoints.FirstOrDefault()?.Email) || SupplierInfo.CompletedEmailAddress;
    }
}