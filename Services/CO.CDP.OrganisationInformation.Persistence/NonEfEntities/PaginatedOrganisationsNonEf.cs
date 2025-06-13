namespace CO.CDP.OrganisationInformation.Persistence.NonEfEntities;

public class OrganisationRawDto
{
    public int Id { get; set; }
    public Guid Guid { get; set; }
    public string? Name { get; set; }
    public OrganisationType Type { get; set; }
    public int[]? Roles { get; set; }
    public int[]? PendingRoles { get; set; }
    public DateTimeOffset? ApprovedOn { get; set; }
    public string? ReviewComment { get; set; }
    public string? ReviewedByFirstName { get; set; }
    public string? ReviewedByLastName { get; set; }
    public string? Identifiers { get; set; }
    public string? ContactPoints { get; set; }
    public string? AdminEmail { get; set; }
}