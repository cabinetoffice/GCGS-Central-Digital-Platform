namespace CO.CDP.OrganisationApp.Models;

public class UserDetails
{
    public required string UserUrn { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public Guid? PersonId { get; set; }

    public AuthTokens? AuthTokens { get; set; }
}

public record AuthTokens
{
    public required string AccessToken { get; init; }

    public required DateTime AccessTokenExpiry { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTime RefreshTokenExpiry { get; init; }
}