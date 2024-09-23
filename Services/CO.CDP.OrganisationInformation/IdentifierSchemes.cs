namespace CO.CDP.OrganisationInformation;
public static class IdentifierSchemes
{
    public static IDictionary<string, string> SchemesToEndpointUris
    {
        get
        {
            return new Dictionary<string, string>()
            {
                { "GB-COH", "https://find-and-update.company-information.service.gov.uk/company/{0}"},
                { "GB-CHC", "https://register-of-charities.charitycommission.gov.uk/charity-search/-/charity-details/{0}"},
                { "GB-SC", "https://www.oscr.org.uk/about-charities/search-the-register/charity-details?number={0}"},
                { "GB-NIC", "https://www.charitycommissionni.org.uk/charity-details/?regId={0}"},
                { "GB-NHS", "https://odsportal.digital.nhs.uk/Organisation/OrganisationDetails?organisationId={0}"},
                { "GB-UKPRN", "https://www.ukrlp.co.uk"},
                { "GB-MPR", "https://mutuals.fca.org.uk/Search/Society/{0}"},
                { "GG-RCE", "https://portal.guernseyregistry.com/e-commerce/company/{0}"},
                { "JE-FSC", "https://www.jerseyfsc.org/registry/registry-entities/entity/{0}"},
                { "IM-CR", "https://services.gov.im/ded/services/companiesregistry/companysearch.iom"}
            };
        }
    }

    public static Uri? GetRegistryUri(string hostUrl, string? scheme, string? identifierId)
    {
        if (scheme == null || identifierId == null || (scheme != "GB-PPON" && !SchemesToEndpointUris.ContainsKey(scheme)))
        {
            return default;
        }

        var uriString = scheme != "GB-PPON" ?
            string.Format(SchemesToEndpointUris[scheme], identifierId) :
            string.Format("{0}/organisations/{1}", hostUrl, identifierId);

        return new Uri(uriString);
    }
}