using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.OrganisationInformation.Persistence.Forms;

[Index(nameof(Guid), IsUnique = true)]
public class MouSignatures : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }

    [ForeignKey(nameof(Organisation))]
    public required int OrganisationId { get; set; }
    public required Organisation Organisation { get; set; }

    [ForeignKey(nameof(Person))]
    public required int PersionId { get; set; }
    public required Person Person { get; set; }

    public required string JobTitle { get; set; }

    [ForeignKey(nameof(Mou))]
    public required int MouId { get; set; }
    public required Mou Mou { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}