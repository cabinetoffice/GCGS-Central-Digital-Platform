namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class CategoryPermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public ReportCategory Category { get; set; } = null!;
    public Guid OrganisationTypeId { get; set; }
    public required string PermissionLevel { get; set; }
    public Guid GrantedBy { get; set; }
    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;
}
