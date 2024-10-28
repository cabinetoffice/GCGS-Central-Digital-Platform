using CO.CDP.EntityFrameworkCore.Timestamps;

namespace CO.CDP.OrganisationInformation.Persistence;

public class OrganisationPerson : IEntityDate
{
    public int PersonId { get; set; }
    public required Person Person { get; set; }
    public int OrganisationId { get; set; }
    public Organisation? Organisation { get; init; }
    public List<string> Scopes { get; set; } = [];

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}