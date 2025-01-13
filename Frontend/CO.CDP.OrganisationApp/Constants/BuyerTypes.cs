using CO.CDP.Localization;

namespace CO.CDP.OrganisationApp.Constants;

public static class BuyerTypeLabels
{
    public static readonly Dictionary<string, string> Labels = new()
    {
        { "CentralGovernment", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_CentralGovernment_Label },
        { "RegionalAndLocalGovernment", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_RegionalAndLocalGovernment_Label },
        { "PublicUndertaking", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_PublicUndertaking_Label },
        { "PrivateUtility", StaticTextResource.OrganisationRegistration_BuyerOrganisationType_PrivateUtility_Label }
    };
}