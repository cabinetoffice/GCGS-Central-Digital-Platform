using CO.CDP.OrganisationInformation;
using Microsoft.Extensions.Configuration;

public class ConfigurationService(IConfiguration config) : IConfigurationService
{
    public string GetOrganisationApiHostUrl()
    {
        return config["OrganisationApiHostUrl"] ?? throw new Exception("Missing configuration key: OrganisationApiHostUrl.");
    }
}