namespace CO.CDP.ApplicationRegistry.App.Models;

public record UserDetails
{
    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string? Email { get; init; }

    public string? Phone { get; init; }

    public Guid? PersonId { get; init; }
}
