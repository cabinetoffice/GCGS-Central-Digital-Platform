using System.Text.Json.Serialization;

namespace CO.CDP.Authentication.Models;

internal sealed record TokenResponse
{
    [JsonPropertyName("access_token")]
    public required string AccessToken { get; init; }

    [JsonPropertyName("token_type")]
    public required string TokenType { get; init; }

    [JsonPropertyName("expires_in")]
    public double ExpiresIn { get; init; }

    [JsonPropertyName("refresh_token")]
    public required string RefreshToken { get; init; }

    [JsonPropertyName("refresh_expires_in")]
    public double RefreshExpiresIn { get; init; }
}
