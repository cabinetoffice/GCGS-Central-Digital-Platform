using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedPersonType
{
    Individual = 1,
    TrustOrTrustee
}
