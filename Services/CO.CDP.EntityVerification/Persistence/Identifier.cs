using CO.CDP.EntityFrameworkCore.Timestamps;

namespace CO.CDP.EntityVerification.Persistence;

public class Identifier : IEntityDate
{
    public int Id { get; set; }
    public required string IdentifierId { get; set; }
    public required string Scheme { get; set; }
    public required string LegalName { get; set; }
    public Uri? Uri { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

    public static ICollection<Identifier> GetPersistenceIdentifiers(IEnumerable<Events.Identifier> evIds, Ppon newPpon)
    {
        List<Identifier> ids = [];

        foreach (var e in evIds)
        {
            ids.Add(new Identifier {
                IdentifierId = e.Id,
                LegalName = e.LegalName,
                Scheme = e.Scheme,
                Uri = e.Uri
            });
        }

        return ids;
    }

}
