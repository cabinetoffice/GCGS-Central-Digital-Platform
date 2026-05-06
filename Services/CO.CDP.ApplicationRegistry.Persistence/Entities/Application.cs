namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class Application
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public required string ClientId { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedOn { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedOn { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<ApplicationPermission> Permissions { get; set; } = new List<ApplicationPermission>();
    public ICollection<ApplicationRole> Roles { get; set; } = new List<ApplicationRole>();
}
