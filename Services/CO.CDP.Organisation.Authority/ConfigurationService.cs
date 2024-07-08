using CO.CDP.Organisation.Authority.Model;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using static IdentityModel.OidcConstants;

namespace CO.CDP.Organisation.Authority;

public class ConfigurationService(IConfiguration config) : IConfigurationService
{
    private AuthorityConfiguration? authorityConfig;
    private OpenIdConnectConfiguration? oneLoginConfig;

    public AuthorityConfiguration GetAuthorityConfiguration()
    {
        if (authorityConfig == null)
        {
            var issuer = config["Issuer"] ?? throw new Exception("Missing configuration key: Issuer.");
            var privateKey = config["PrivateKey"] ?? throw new Exception("Missing configuration key: PrivateKey.");

            var rsaPrivate = RSA.Create();
            rsaPrivate.ImportFromPem(privateKey);
            var rsaPrivateKey = new RsaSecurityKey(rsaPrivate);

            var rsaPublic = RSA.Create();
            rsaPublic.ImportFromPem(rsaPrivate.ExportRSAPublicKeyPem());
            var rsaPublicParams = rsaPublic.ExportParameters(false);

            authorityConfig = new AuthorityConfiguration
            {
                Issuer = issuer,
                RsaPrivateKey = rsaPrivateKey,
                RsaPublicParams = rsaPublicParams
            };
        }

        return authorityConfig;
    }

    public async Task<OpenIdConnectConfiguration> GetOneLoginConfiguration()
    {
        if (oneLoginConfig == null)
        {
            var authority = config["OneLogin:Authority"]
                ?? throw new Exception("Missing configuration key: OneLogin:Authority.");

            var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        new Uri(new Uri(authority), Discovery.DiscoveryEndpoint).ToString(),
                        new OpenIdConnectConfigurationRetriever());

            oneLoginConfig = await configurationManager.GetConfigurationAsync();
        }

        return oneLoginConfig;
    }
}