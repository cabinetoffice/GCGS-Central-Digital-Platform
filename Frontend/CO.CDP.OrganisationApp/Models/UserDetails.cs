namespace CO.CDP.OrganisationApp.Models;

public class UserDetails
{
    public required string UserUrn { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public Guid? PersonId { get; set; }

    public string? AccessToken { get; set; }
}