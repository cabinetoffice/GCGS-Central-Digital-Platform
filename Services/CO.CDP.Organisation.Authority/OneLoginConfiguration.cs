using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using static IdentityModel.OidcConstants;

namespace CO.CDP.Organisation.Authority;

public interface IOpenIdConfiguration
{
    Task<OpenIdConnectConfiguration> Get();
}

public class OneLoginConfiguration(IConfiguration config) : IOpenIdConfiguration
{
    private OpenIdConnectConfiguration? configuration;

    public async Task<OpenIdConnectConfiguration> Get()
    {
        if (configuration == null)
        {
            var authority = config["OneLogin:Authority"]
                ?? throw new Exception("Missing configuration key: OneLogin:Authority.");

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        new Uri(new Uri(authority), Discovery.DiscoveryEndpoint).ToString(),
                        new OpenIdConnectConfigurationRetriever());

            configuration = await configurationManager.GetConfigurationAsync();
        }

        return configuration;
    }
}