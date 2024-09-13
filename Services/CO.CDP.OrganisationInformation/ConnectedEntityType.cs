using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedEntityType
{
    Organisation = 1,
    Individual,
    TrustOrTrustee
}
