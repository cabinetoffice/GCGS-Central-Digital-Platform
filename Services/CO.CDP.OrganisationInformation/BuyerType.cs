using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum BuyerType
{
    CentralGovernment = 1,
    RegionalAndLocalGovernment,
    PublicUndertaking,
    PrivateUtility
}