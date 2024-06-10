using System.Text.Json.Serialization;

namespace CO.CDP.Organisation.Authority.Model;

public class OpenIdConfiguration
{
    [JsonPropertyName("issuer")]
    public required string Issuer { get; init; }

    [JsonPropertyName("token_endpoint")]
    public required string TokenEndpoint { get; init; }

    [JsonPropertyName("jwks_uri")]
    public required string JwksUri { get; init; }

    [JsonPropertyName("response_types_supported")]
    public required ICollection<string> ResponseTypesSupported { get; init; }

    [JsonPropertyName("scopes_supported")]
    public required ICollection<string> ScopesSupported { get; init; }

    [JsonPropertyName("token_endpoint_auth_methods_supported")]
    public required ICollection<string> TokenEndpointAuthMethodsSupported { get; init; }

    [JsonPropertyName("token_endpoint_auth_signing_alg_values_supported")]
    public required ICollection<string> TokenEndpointAuthSigningAlgValuesSupported { get; init; }

    [JsonPropertyName("grant_types_supported")]
    public required ICollection<string> GrantTypesSupported { get; init; }

    [JsonPropertyName("subject_types_supported")]
    public required ICollection<string> SubjectTypesSupported { get; init; }

    [JsonPropertyName("claim_types_supported")]
    public required ICollection<string> ClaimTypesSupported { get; init; }

    [JsonPropertyName("claims_supported")]
    public required ICollection<string> ClaimsSupported { get; init; }
}