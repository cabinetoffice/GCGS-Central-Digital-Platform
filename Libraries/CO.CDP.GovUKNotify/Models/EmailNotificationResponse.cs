using System.Text.Json.Serialization;

namespace GovukNotify.Models;

public class EmailNotificationResponse
{
    [JsonPropertyName("id")]
    public required Guid Id { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("content")]
    public required EmailResponseContent Content { get; set; }

    [JsonPropertyName("uri")]
    public required Uri Uri { get; set; }

    [JsonPropertyName("template")]
    public required Template Template { get; set; }
}

public class EmailResponseContent
{
    [JsonPropertyName("subject")]
    public required string Subject { get; set; }

    [JsonPropertyName("body")]
    public required string Body { get; set; }

    [JsonPropertyName("from_email")]
    public required string FromEmail { get; set; }
}