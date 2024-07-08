using CO.CDP.Organisation.Authority.Model;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace CO.CDP.Organisation.Authority;

public interface IConfigurationService
{
    AuthorityConfiguration GetAuthorityConfiguration();

    Task<OpenIdConnectConfiguration> GetOneLoginConfiguration();
}