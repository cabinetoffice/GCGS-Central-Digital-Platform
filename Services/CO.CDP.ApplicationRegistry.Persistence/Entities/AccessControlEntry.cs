namespace CO.CDP.ApplicationRegistry.Persistence.Entities;

public class AccessControlEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReportId { get; set; }
    public Guid UserPrincipal { get; set; }
    public Guid OrganisationId { get; set; }
    public Guid GrantedBy { get; set; }
    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? RevokedAt { get; set; }
}
