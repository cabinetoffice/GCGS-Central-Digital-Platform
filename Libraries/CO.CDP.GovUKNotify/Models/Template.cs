using System.Text.Json.Serialization;

namespace CO.CDP.GovUKNotify.Models;

public class Template
{
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    [JsonPropertyName("version")]
    public required int Version { get; set; }

    [JsonPropertyName("uri")]
    public required Uri Uri { get; set; }
}