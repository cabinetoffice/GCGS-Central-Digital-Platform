using System.Text.Json.Serialization;

namespace CO.CDP.Organisation.WebApi.Model;
public record Review
{
    public DateTimeOffset? ApprovedOn { get; init; }
    public ReviewedBy? ReviewedBy { get; init; }
    public string? Comment { get; init; }
    public ReviewStatus Status { get; init; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ReviewStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3,
}