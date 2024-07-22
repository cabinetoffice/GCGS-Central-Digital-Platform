using CO.CDP.OrganisationInformation.Persistence;

namespace CO.CDP.EntityVerification.Persistence;

public class Identifier : IEntityDate
{
    public int Id { get; set; }
    public required string Scheme { get; set; }
    public required string LegalName { get; set; }
    public Uri? Uri { get; set; }
    public required Ppon Ppon { get; set; }
    public int PponId { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }

    public static ICollection<Identifier> GetPersistenceIdentifiers(IEnumerable<Events.Identifier> evIds, Ppon newPpon)
    {
        List<Identifier> ids = [];

        foreach (var e in evIds)
        {
            ids.Add(new Identifier { Ppon = newPpon,
                LegalName = e.LegalName,
                Scheme = e.Scheme,
                Uri = e.Uri
            });
        }

        return ids;
    }

}
