namespace CO.CDP.DataSharing.WebApi;

public class ConfigurationService(IConfiguration config) : IConfigurationService
{
    public string GetOrganisationApiHostUrl()
    {
        return config["OrganisationApiHostUrl"] ?? throw new Exception("Missing configuration key: OrganisationApiHostUrl.");
    }
}