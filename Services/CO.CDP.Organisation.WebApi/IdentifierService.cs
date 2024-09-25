namespace CO.CDP.Organisation.WebApi;
public class IdentifierService(IConfiguration configuration) : IIdentifierService
{
    public IDictionary<string, string> SchemesToEndpointUris
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

    public string? GetRegistryUri(string? scheme, string? identifierId)
    {
        if (scheme == null || identifierId == null || (scheme != "GB-PPON" && !SchemesToEndpointUris.ContainsKey(scheme)))
        {
            return default;
        }

        if (scheme == "GB-PPON")
        {
            var hostUrl = configuration["OrganisationApiHostUrl"] ?? throw new Exception("Missing configuration key: OrganisationApiHostUrl.");

            return string.Format("{0}/organisations/{1}", hostUrl, identifierId);
        }

        return string.Format(SchemesToEndpointUris[scheme], identifierId);
    }
}