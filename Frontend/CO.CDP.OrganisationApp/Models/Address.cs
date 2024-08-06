namespace CO.CDP.OrganisationApp.Models;

public class Address
{
    public required string AddressLine1 { get; set; }

    public required string TownOrCity { get; set; }

    public required string Postcode { get; set; }

    public required string Country { get; set; }

    public override string ToString()
    {
        return $"{AddressLine1}, {TownOrCity}, {Postcode}, {Country}";
    }
}