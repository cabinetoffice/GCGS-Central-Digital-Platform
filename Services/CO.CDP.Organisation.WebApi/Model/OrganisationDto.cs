using CO.CDP.OrganisationInformation;

namespace CO.CDP.Organisation.WebApi.Model;
public class OrganisationDto
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public OrganisationType Type { get; set; }
    public List<PartyRole> Roles { get; set; } = new();
    public List<PartyRole> PendingRoles { get; set; } = new();
    public DateTimeOffset? ApprovedOn { get; set; }
    public string? ReviewComment { get; set; }
    public string? ReviewedByFirstName { get; set; }
    public string? ReviewedByLastName { get; set; }
    public List<string> Identifiers { get; set; } = new();
    public List<string> ContactPoints { get; set; } = new();
    public string? AdminEmail { get; set; }
}
