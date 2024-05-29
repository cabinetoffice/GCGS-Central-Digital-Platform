namespace CO.CDP.OrganisationInformation.Persistence;

public class Address : IEntityDate
{
    public int Id { get; set; }
    public required string StreetAddress { get; set; }
    public string? StreetAddress2 { get; set; }
    public required string Locality { get; set; }
    public string? Region { get; set; }
    public required string PostalCode { get; set; }
    public required string CountryName { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}