namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class ReportCategoryAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReportId { get; set; }
    public Guid CategoryId { get; set; }
    public ReportCategory Category { get; set; } = null!;
    public Guid AssignedBy { get; set; }
    public DateTimeOffset AssignedAt { get; set; } = DateTimeOffset.UtcNow;
}
