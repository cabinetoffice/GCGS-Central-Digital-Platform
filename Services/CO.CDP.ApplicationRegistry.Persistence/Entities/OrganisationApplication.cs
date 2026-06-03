namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class OrganisationApplication
{
    public Guid OrganisationId { get; set; }
    public Organisation Organisation { get; set; } = null!;
    public Guid ApplicationId { get; set; }
    public Application Application { get; set; } = null!;
    public DateTimeOffset EnabledAt { get; set; } = DateTimeOffset.UtcNow;
    public required string EnabledBy { get; set; }
    public bool IsEnabled { get; set; } = true;
    public DateTimeOffset? DisabledAt { get; set; }
    public string? DisabledBy { get; set; }
}
