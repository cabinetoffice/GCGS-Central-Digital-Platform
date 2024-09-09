using System.Text.Json.Serialization;

namespace GovukNotify.Models;

public class EmailNotificationResquest
{
    [JsonPropertyName("email_address")]
    public required string EmailAddress { get; set; }

    [JsonPropertyName("template_id")]
    public required string TemplateId { get; set; }

    [JsonPropertyName("personalisation")]
    public Dictionary<string, string>? Personalisation { get; set; }

    [JsonPropertyName("reference")]
    public string? Reference { get; set; }

    [JsonPropertyName("one_click_unsubscribe_url ")]
    public string? OneClickUnsubscribeUrl { get; set; }

    [JsonPropertyName("email_reply_to_id")]
    public Guid? EmailReplyToId { get; set; }
}