using System.Text.Json.Serialization;

namespace CO.CDP.OrganisationInformation;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedEntityType
{
    Organisation = 1,
    Individual,
    TrustOrTrustee
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedPersonType
{
    Individual = 1,
    TrustOrTrustee
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ConnectedOrganisationCategory
{
    RegisteredCompany = 1,
    DirectorOrTheSameResponsibilities,
    ParentOrSubsidiaryCompany,
    ACompanyYourOrganisationHasTakenOver,
    AnyOtherOrganisationWithSignificantInfluenceOrControl,
}