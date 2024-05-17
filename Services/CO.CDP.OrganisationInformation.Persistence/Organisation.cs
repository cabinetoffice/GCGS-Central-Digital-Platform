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
    public required OrganisationAddress Address { get; set; }
    public required OrganisationContactPoint ContactPoint { get; set; }
    public List<int> Types { get; set; } = [];
    public List<Person> Persons { get; } = [];
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

    [Owned]
    public record OrganisationIdentifier
    {
        public int Id { get; set; }
        public string? IdentifierId;
        public required string Scheme;
        public required string Number;
        public string? LegalName;
        public string? Uri;
        public required bool Primary { get; set; }
    }

    [ComplexType]
    public record OrganisationAddress
    {
        public required string AddressLine1;
        public string? AddressLine2;
        public required string City;
        public required string PostCode;
        public string? Country;
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