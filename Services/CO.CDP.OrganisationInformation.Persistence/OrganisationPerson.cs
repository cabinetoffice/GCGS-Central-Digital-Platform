namespace CO.CDP.OrganisationInformation.Persistence;

public class OrganisationPerson : IEntityDate
{
    public int PersonId { get; set; }
    public required Person Person { get; init; }
    public int OrganisationId { get; set; }
    public required Organisation Organisation { get; init; }
    public List<string> Scopes { get; init; } = [];

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}