using System.Text.Json.Serialization;
using CO.CDP.OrganisationInformation.Serialisation;

namespace CO.CDP.OrganisationInformation;

/// <summary>
/// Based on OCDS <a href="https://standard.open-contracting.org/latest/en/schema/codelists/#party-role">PartyRole</a>.
/// </summary>
[JsonConverter(typeof(LowerCamelCaseEnumConverter<JoinRequestStatus>))]
public enum JoinRequestStatus
{
    Pending = 1,
    Accepted = 2,
    Rejected = 3
}