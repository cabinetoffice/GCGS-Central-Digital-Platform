using System.Text.Json.Serialization;

namespace CO.CDP.Organisation.Authority.Model;

public class JsonWebKeySet
{
    [JsonPropertyName("keys")]
    public required IList<JsonWebKey> Keys { get; init; }
}