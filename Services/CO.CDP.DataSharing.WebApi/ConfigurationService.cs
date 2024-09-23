namespace CO.CDP.DataSharing.WebApi;

public class ConfigurationService(IConfiguration config) : IConfigurationService
{
    public string GetOrganisationsApiUrl()
    {
        return config["OrganisationsApiUrl"] ?? throw new Exception("Missing configuration key: OrganisationsApiUrl.");
    }
}