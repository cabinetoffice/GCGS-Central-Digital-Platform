using CO.CDP.OrganisationApp.Models;

namespace CO.CDP.OrganisationApp.WebApiClients;

public interface IAuthorityClient
{
    Task<AuthTokens?> GetAuthTokens(string? userUrn);

    Task RevokeRefreshToken(string? userUrn);
}