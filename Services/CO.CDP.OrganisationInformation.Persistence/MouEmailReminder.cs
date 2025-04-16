using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(OrganisationId), IsUnique = true)]
public class MouEmailReminder : IEntityDate
{
    [Key]
    public int Id { get; set; }
    [ForeignKey(nameof(Organisation))]
    public int OrganisationId { get; set; }
    public DateTimeOffset ReminderSentOn { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
    public Organisation? Organisation { get; set; }
}