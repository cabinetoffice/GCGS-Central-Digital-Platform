namespace CO.CDP.OrganisationInformation.Persistence;

public class OrganisationPerson
{
    public int PersonId { get; set; }
    public int OrganisationId { get; set; }
    public string? Scopes { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}