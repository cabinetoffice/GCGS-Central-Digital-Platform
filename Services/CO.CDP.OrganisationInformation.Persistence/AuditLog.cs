using System.ComponentModel.DataAnnotations;
using CO.CDP.EntityFrameworkCore.Timestamps;

namespace CO.CDP.OrganisationInformation.Persistence;

public class AuditLog : IEntityDate
{
    public int Id { get; set; }
    public required Guid Guid { get; set; }

    [MaxLength(100)]
    public required string EntityType { get; set; }

    public required int EntityId { get; set; }

    [MaxLength(50)]
    public required string Action { get; set; }

    [MaxLength(200)]
    public string? PropertyName { get; set; }

    [MaxLength(4000)]
    public string? OldValue { get; set; }

    [MaxLength(4000)]
    public string? NewValue { get; set; }

    [MaxLength(256)]
    public required string UserId { get; set; }

    public DateTimeOffset Timestamp { get; set; }

    public DateTimeOffset CreatedOn { get; set; }
    public DateTimeOffset UpdatedOn { get; set; }
}
