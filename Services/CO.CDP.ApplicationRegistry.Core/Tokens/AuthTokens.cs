namespace CO.CDP.ApplicationRegistry.Core.Tokens;

/// <summary>
/// Represents authentication tokens with their expiry times.
/// </summary>
public record AuthTokens
{
    public required string AccessToken { get; init; }

    public required DateTimeOffset AccessTokenExpiry { get; init; }

    public required string RefreshToken { get; init; }

    public required DateTimeOffset RefreshTokenExpiry { get; init; }
}

