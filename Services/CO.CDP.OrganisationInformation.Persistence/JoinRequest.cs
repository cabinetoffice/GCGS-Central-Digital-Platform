using CO.CDP.EntityFrameworkCore.Timestamps;
using Microsoft.EntityFrameworkCore;

namespace CO.CDP.OrganisationInformation.Persistence;

[Index(nameof(Guid), IsUnique = true)]
public class JoinRequest : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }
    public Organisation? Organisation { get; set; }
    public int? OrganisationId { get; set; }
    public Person? Person { get; set; }
    public int? PersonId { get; set; }
    public required JoinRequestStatus Status { get; set; }
    public DateTimeOffset? ReviewedOn { get; set; }
    public Person? ReviewedBy { get; set; }
    public int? ReviewedById { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}