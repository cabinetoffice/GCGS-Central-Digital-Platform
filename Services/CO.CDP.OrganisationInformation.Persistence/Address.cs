using CO.CDP.EntityFrameworkCore.Timestamps;

namespace CO.CDP.OrganisationInformation.Persistence;

public class Address : IEntityDate
{
    public int Id { get; set; }
    public required string StreetAddress { get; set; }
    public required string Locality { get; set; }
    public string? Region { get; set; }
    public required string PostalCode { get; set; }
    public required string CountryName { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}