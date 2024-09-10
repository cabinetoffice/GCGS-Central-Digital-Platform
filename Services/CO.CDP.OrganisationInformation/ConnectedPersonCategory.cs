using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedPersonCategory
{
    PersonWithSignificantControl = 1,
    DirectorOrIndividualWithTheSameResponsibilities,
    AnyOtherIndividualWithSignificantInfluenceOrControl
}
