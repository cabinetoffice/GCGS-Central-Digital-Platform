using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.Persistence.OrganisationInformation;

[Index(nameof(Guid), IsUnique = true)]
[Index(nameof(Name), IsUnique = true)]
public class Organisation
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public required string Name { get; set; }
    public required OrganisationIdentifier Identifier { get; set; }
    public List<OrganisationIdentifier> AdditionalIdentifiers { get; set; } = [];
    public required OrganisationAddress Address { get; set; }
    public required OrganisationContactPoint ContactPoint { get; set; }
    public required List<int> Roles { get; set; } = [];

    [ComplexType]
    public record OrganisationIdentifier
    {
        public required string Id;
        public required string Scheme;
        public required string LegalName;
        public required string Uri;
    }

    [ComplexType]
    public record OrganisationAddress
    {
        public required string StreetAddress;
        public required string Locality;
        public required string Region;
        public required string PostalCode;
        public required string CountryName;
    }

    [ComplexType]
    public record OrganisationContactPoint
    {
        public required string Name;
        public required string Email;
        public required string Telephone;
        public required string FaxNumber;
        public required string Url;
    }
}