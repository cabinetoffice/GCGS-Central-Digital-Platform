using CO.CDP.EntityFrameworkCore.Timestamps;

namespace CO.CDP.OrganisationInformation.Persistence;

public class OrganisationPerson : IEntityDate
{
    public int PersonId { get; set; }
    public required Person Person { get; init; }
    public int OrganisationId { get; set; }
    public required Organisation Organisation { get; init; }
    public List<string> Scopes { get; init; } = [];

    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}