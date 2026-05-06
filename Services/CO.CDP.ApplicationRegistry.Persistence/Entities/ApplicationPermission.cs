namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class ApplicationPermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public required string Name { get; set; }
    public string? Description { get; set; }
}
