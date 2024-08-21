using System.Text.Json.Serialization;

namespace CO.CDP.DataSharing.WebApi.Model;

public class SharedConsent
{
    public DateTimeOffset SubmittedAt { get; set; }   
    public string? ShareCode { get; set; }
}