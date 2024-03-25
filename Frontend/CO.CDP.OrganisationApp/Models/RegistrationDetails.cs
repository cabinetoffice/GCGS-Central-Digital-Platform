namespace CO.CDP.OrganisationApp.Models;

public class RegistrationDetails
{
    public Guid TenantId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }
}