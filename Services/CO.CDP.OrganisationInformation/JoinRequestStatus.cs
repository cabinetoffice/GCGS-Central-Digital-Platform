using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum JoinRequestStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}