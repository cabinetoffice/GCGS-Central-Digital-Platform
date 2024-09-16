using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ControlCondition
{
    None,
    OwnsShares,
    HasVotingRights,
    CanAppointOrRemoveDirectors,
    HasOtherSignificantInfluenceOrControl
}
