namespace CO.CDP.OrganisationInformation.Persistence;

public class AuthenticationKey : IEntityDate
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required string Key { get; set; }
    public Organisation? Organisation { get; set; }
    public List<string> Scopes { get; set; } = [];
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}