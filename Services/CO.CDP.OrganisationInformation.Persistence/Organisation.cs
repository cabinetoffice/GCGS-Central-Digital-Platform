using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
    public required OrganisationType Type { get; set; } = OrganisationType.Organisation;
    public IList<Identifier> Identifiers { get; set; } = [];
    public ICollection<OrganisationAddress> Addresses { get; set; } = [];
    public ICollection<ContactPoint> ContactPoints { get; set; } = [];
    public List<PartyRole> Roles { get; init; } = [];
    public List<PartyRole> PendingRoles { get; init; } = [];
    public List<Person> Persons => OrganisationPersons.Select(p => p.Person).ToList();
    public List<OrganisationPerson> OrganisationPersons { get; set; } = [];
    public SupplierInformation? SupplierInfo { get; set; }
    public BuyerInformation? BuyerInfo { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public DateTimeOffset? ApprovedOn { get; set; }
    public Person? ReviewedBy { get; set; }
    public int? ReviewedById { get; set; }

    [MaxLength(10000)]
    public string? ReviewComment { get; set; }

    public void UpdateBuyerInformation()
    {
        if (!Roles.Contains(PartyRole.Buyer) && !PendingRoles.Contains(PartyRole.Buyer))
        {
            return;
        }

        BuyerInfo ??= new BuyerInformation { Id = Id };
    }

    public void UpdateSupplierInformation()
    {
        if (!Roles.Contains(PartyRole.Tenderer) || Type != OrganisationType.Organisation)
        {
            return;
        }

        SupplierInfo ??= new SupplierInformation { Id = Id };
        SupplierInfo.CompletedRegAddress =
            Addresses.Any(a => a.Type == AddressType.Registered) || SupplierInfo.CompletedRegAddress;
        SupplierInfo.CompletedPostalAddress =
            Addresses.Any(a => a.Type == AddressType.Postal) || SupplierInfo.CompletedPostalAddress;
        SupplierInfo.CompletedVat = Identifiers.Any(i => i.Scheme == "VAT") || SupplierInfo.CompletedVat;
        SupplierInfo.CompletedLegalForm = SupplierInfo.LegalForm != null || SupplierInfo.CompletedLegalForm;
        SupplierInfo.CompletedEmailAddress =
            !string.IsNullOrWhiteSpace(ContactPoints.FirstOrDefault()?.Email) || SupplierInfo.CompletedEmailAddress;
    }
}

public record Identifier : IEntityDate
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Organisation))]
    public int OrganisationId { get; set; }
    public Organisation? Organisation { get; set; }

    public string? IdentifierId { get; set; }
    public required string Scheme { get; set; }
    public required string LegalName { get; set; }
    public string? Uri { get; set; }
    public required bool Primary { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public record OrganisationAddress
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Organisation))]
    public int OrganisationId { get; set; }
    public Organisation? Organisation { get; set; }
    public required AddressType Type { get; set; }
    public required Address Address { get; set; }
}

public record ContactPoint : IEntityDate
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Organisation))]
    public int OrganisationId { get; set; }
    public Organisation? Organisation { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Telephone { get; set; }
    public string? Url { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}

public record SupplierInformation : IEntityDate
{
    [Key]
    [ForeignKey(nameof(Organisation))]
    public int Id { get; set; }
    public Organisation? Organisation { get; set; }
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
    public LegalForm? LegalForm { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

}

public record BuyerInformation : IEntityDate
{
    [Key]
    [ForeignKey(nameof(Organisation))]
    public int Id { get; set; }
    public Organisation? Organisation { get; set; }
    public string? BuyerType { get; set; }
    public List<DevolvedRegulation> DevolvedRegulations { get; set; } = [];
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
