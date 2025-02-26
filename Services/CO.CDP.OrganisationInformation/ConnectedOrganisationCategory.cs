using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedOrganisationCategory
{
    RegisteredCompany = 1,
    DirectorOrTheSameResponsibilities,
    ParentOrSubsidiaryCompany,
    ACompanyYourOrganisationHasTakenOver,
    AnyOtherOrganisationWithSignificantInfluenceOrControl
}
