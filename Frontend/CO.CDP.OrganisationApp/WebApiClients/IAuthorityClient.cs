using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.WebApiClients;

public interface IAuthorityClient
{
    Task<(bool newToken, AuthTokens tokens)> GetAuthTokens(AuthTokens? authToken);
}