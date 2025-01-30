using CO.CDP.EntityFrameworkCore.Timestamps;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.OrganisationInformation.Persistence.Forms;

public class SharedConsentConsortium : IEntityDate
{
    public int Id { get; set; }

    [ForeignKey(nameof(ParentSharedConsent))]
    public required int ParentSharedConsentId { get; set; }
    public SharedConsent? ParentSharedConsent { get; set; }

    [ForeignKey(nameof(ChildSharedConsent))]
    public required int ChildSharedConsentId { get; set; }
    public SharedConsent? ChildSharedConsent { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}