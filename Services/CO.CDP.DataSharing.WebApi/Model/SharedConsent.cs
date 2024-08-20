using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

public class SharedConsent
{
    public DateTime SubmittedAt { get; set; }
    public string? ShareCode { get; set; }
}