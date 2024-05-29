using CO.CDP.Common.Enums;
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
    public ICollection<OrganisationIdentifier> Identifiers { get; set; } = [];
    public ICollection<OrganisationAddress> Addresses { get; set; } = [];
    public required OrganisationContactPoint ContactPoint { get; set; }
    public List<PartyRole> Roles { get; set; } = [];
    public List<Person> Persons { get; } = [];
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

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
}