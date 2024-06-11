using System.Text.Json.Serialization;

namespace CO.CDP.Organisation.Authority.Model;

public class JsonWebKey
{
    [JsonPropertyName("alg")]
    public required string Alg { get; init; }

    [JsonPropertyName("kid")]
    public required string Kid { get; init; }

    [JsonPropertyName("kty")]
    public required string Kty { get; init; }

    [JsonPropertyName("use")]
    public required string Use { get; init; }

    [JsonPropertyName("n")]
    public required string N { get; init; }

    [JsonPropertyName("e")]
    public required string E { get; init; }
}